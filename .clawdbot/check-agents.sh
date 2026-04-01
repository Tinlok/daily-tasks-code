#!/bin/bash
# check-agents.sh - Deterministic monitoring for agent swarms
# Checks tmux sessions, PR status, and CI status every 10 minutes
# Usage: ./check-agents.sh

set -e

REPO=$(git rev-parse --show-toplevel)
TASKS_FILE="$REPO/.clawdbot/active-tasks.json"
MAX_RETRIES=3

# Load environment variables FIRST
if [[ -f "$REPO/.clawdbot/.env" ]]; then
    export $(grep -v '^#' "$REPO/.clawdbot/.env" | xargs)
fi

# Now define WEBHOOK_URL after loading .env
WEBHOOK_URL="${FEISHU_WEBHOOK_URL:-}"

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1"
}

# Send Feishu notification
send_feishu_notification() {
    local title=$1
    local content=$2
    local emoji=$3

    if [[ -z "$WEBHOOK_URL" ]]; then
        log "⚠️  No Feishu webhook configured, skipping notification"
        return
    fi

    local message="$emoji $title\n\n$content"

    curl -s -X POST "$WEBHOOK_URL" \
        -H "Content-Type: application/json" \
        -d "{\"msg_type\": \"text\", \"content\": {\"text\": \"$message\"}}" > /dev/null 2>&1

    log "📤 Sent Feishu notification: $title"
}

# Update task status
update_task() {
    local task_id=$1
    local update=$2

    jq "(.tasks[] | select(.id == \"$task_id\")) += $update" "$TASKS_FILE" > "${TASKS_FILE}.tmp"
    mv "${TASKS_FILE}.tmp" "$TASKS_FILE"
}

# Check if task was just marked as done (first time completion)
is_first_completion() {
    local task=$1
    local previous_status=$(echo "$task" | jq -r '.status // "running"')
    local new_status=$(echo "$2" | jq -r '.status')

    [[ "$previous_status" != "done" && "$new_status" == "done" ]]
}

# Main monitoring loop
log "🔍 Starting agent swarm check..."

TASK_COUNT=$(jq '.tasks | length' "$TASKS_FILE")

if [[ "$TASK_COUNT" -eq 0 ]]; then
    log "✓ No active tasks, nothing to check"
    exit 0
fi

log "📊 Checking $TASK_COUNT active task(s)..."

jq -c '.tasks[]' "$TASKS_FILE" | while read -r task; do
    TASK_ID=$(echo "$task" | jq -r '.id')
    TASK_DATE=$(echo "$task" | jq -r '.date')
    TMUX_SESSION=$(echo "$task" | jq -r '.tmuxSession')
    BRANCH=$(echo "$task" | jq -r '.branch')
    RETRY_COUNT=$(echo "$task" | jq -r '.retryCount // 0')
    STATUS=$(echo "$task" | jq -r '.status')
    TASK_DIR=$(echo "$task" | jq -r '.taskDir')
    PROMPT=$(echo "$task" | jq -r '.description')

    log "→ Checking task: $TASK_ID (status: $STATUS, retries: $RETRY_COUNT/$MAX_RETRIES)"

    # Skip completed or failed tasks
    if [[ "$STATUS" == "done" || "$STATUS" == "failed" ]]; then
        log "  ⏭️  Task already $STATUS, skipping"
        continue
    fi

    # Check 1: Is tmux session alive?
    if ! tmux has-session -t "$TMUX_SESSION" 2>/dev/null; then
        log "  ❌ Tmux session $TMUX_SESSION not found"

        # Check if there's a PR created for this task
        PR_INFO=$(gh pr list --head "$BRANCH" --json number,state,title,url --jq '.[0]' 2>/dev/null || echo "")

        if [[ -n "$PR_INFO" && "$PR_INFO" != "null" ]]; then
            PR_NUMBER=$(echo "$PR_INFO" | jq -r '.number')
            PR_STATE=$(echo "$PR_INFO" | jq -r '.state')
            PR_TITLE=$(echo "$PR_INFO" | jq -r '.title')

            log "  ✓ Found PR #$PR_NUMBER ($PR_STATE): $PR_TITLE"

            # Check CI status
            CI_STATUS=$(gh pr checks "$PR_NUMBER" --json name,state --jq 'map(select(.state == "SUCCESS" or .state == "FAILURE")) | if all(.state == "SUCCESS") then "success" elif any(.state == "FAILURE") then "failure" else "pending" end' 2>/dev/null || echo "")

            if [[ "$CI_STATUS" == "success" ]]; then
                log "  ✅ CI passed, marking task as done (loose mode)"

                # Check if this is the first completion
                if is_first_completion "$task" '{"status": "done"}'; then
                    log "  📤 First completion detected, sending notification..."

                    # Trigger code review (reference only)
                    log "  🔍 Running code review (reference only)..."
                    if "$REPO/.clawdbot/review-pr.sh" "$PR_NUMBER"; then
                        log "  ✅ Code review completed"
                    else
                        log "  ⚠️  Code review failed, but task marked as done (loose mode)"
                    fi

                    # Update task status
                    update_task "$TASK_ID" '{"status": "done", "completedAt": "'$(date +%s)000'", "pr": '"$PR_NUMBER"', "checks": {"prCreated": true, "ciPassed": true}}'

                    # Send notification
                    send_feishu_notification \
                        "PR #$PR_NUMBER CI Passed - Task Complete" \
                        "$PR_TITLE\nBranch: $BRANCH\nCI: ✅\nCode Review: ✅ (参考)\n\n任务已完成！\n查看 PR: https://github.com/Tinlok/daily-tasks-code/pull/$PR_NUMBER" \
                        "✅"
                else
                    log "  ℹ️  Task already done, skipping notification"
                    update_task "$TASK_ID" '{"status": "done", "completedAt": "'$(date +%s)000'", "pr": '"$PR_NUMBER"', "checks": {"prCreated": true, "ciPassed": true}}'
                fi

                continue
            elif [[ "$CI_STATUS" == "failure" ]]; then
                log "  ❌ CI failed"

                if [[ "$RETRY_COUNT" -lt "$MAX_RETRIES" ]]; then
                    log "  🔄 Retrying ($RETRY_COUNT/$MAX_RETRIES)..."
                    update_task "$TASK_ID" "{\"retryCount\": $((RETRY_COUNT + 1)), \"status\": \"retrying\"}"

                    send_feishu_notification \
                        "CI Failed - Manual Intervention Needed" \
                        "PR #$PR_NUMBER failed CI checks.\nBranch: \`$BRANCH\`\nTask: \`$TASK_ID\`\nDate: \`$TASK_DATE\`\n\n请 review 并重启。" \
                        "⚠️"
                else
                    log "  ❌ Max retries reached, marking as failed"
                    update_task "$TASK_ID" '{"status": "failed", "failedAt": "'$(date +%s)000'", "reason": "Max retries reached after CI failure"}'

                    send_feishu_notification \
                        "Task Failed: $TASK_ID" \
                        "Max retries ($MAX_RETRIES) reached after CI failure.\nBranch: \`$BRANCH\`\nPR: #$PR_NUMBER\n\nManual intervention required." \
                        "💀"
                fi
                continue
            else
                log "  ⏳ CI in progress or pending..."
                continue
            fi
        else
            # No PR found, tmux session dead - this is a failure
            log "  ❌ No PR found and tmux session dead"

            if [[ "$RETRY_COUNT" -lt "$MAX_RETRIES" ]]; then
                log "  🔄 Would retry here (need prompt re-injection)"
                update_task "$TASK_ID" "{\"retryCount\": $((RETRY_COUNT + 1)), \"status\": \"needs-restart\"}"
            else
                log "  ❌ Max retries reached, marking as failed"
                update_task "$TASK_ID" '{"status": "failed", "failedAt": "'$(date +%s)000'", "reason": "Agent died without creating PR"}'

                send_feishu_notification \
                    "Agent Died: $TASK_ID" \
                    "Agent session died without creating a PR.\nMax retries reached.\nTask: \`$TASK_ID\`\nDate: \`$TASK_DATE\`\n\nManual intervention required." \
                    "💀"
            fi
            continue
        fi
    fi

    # Tmux session is alive, check PR status
    PR_INFO=$(gh pr list --head "$BRANCH" --json number,state,title,url --jq '.[0]' 2>/dev/null || echo "")

    if [[ -n "$PR_INFO" && "$PR_INFO" != "null" ]]; then
        PR_NUMBER=$(echo "$PR_INFO" | jq -r '.number')
        PR_STATE=$(echo "$PR_INFO" | jq -r '.state')

        if [[ "$PR_STATE" == "OPEN" ]]; then
            log "  ✓ PR #$PR_NUMBER is open, checking CI..."
            CI_STATUS=$(gh pr checks "$PR_NUMBER" --json name,state --jq 'map(select(.state == "SUCCESS" or .state == "FAILURE")) | if all(.state == "SUCCESS") then "success" elif any(.state == "FAILURE") then "failure" else "pending" end' 2>/dev/null || echo "")

            case "$CI_STATUS" in
                success)
                    log "  ✅ CI passed, marking task as done (loose mode)"

                    # Check if this is the first completion
                    if is_first_completion "$task" '{"status": "done"}'; then
                        log "  📤 First completion detected, sending notification..."

                        # Trigger code review (reference only)
                        log "  🔍 Running code review (reference only)..."
                        if "$REPO/.clawdbot/review-pr.sh" "$PR_NUMBER"; then
                            log "  ✅ Code review completed"
                        else
                            log "  ⚠️  Code review failed, but task marked as done (loose mode)"
                        fi

                        # Update task status
                        update_task "$TASK_ID" '{"status": "done", "completedAt": "'$(date +%s)000'", "pr": '"$PR_NUMBER"', "checks": {"prCreated": true, "ciPassed": true}}'

                        # Send notification
                        send_feishu_notification \
                            "PR #$PR_NUMBER CI Passed - Task Complete" \
                            "$PR_TITLE\nBranch: $BRANCH\nCI: ✅\nCode Review: ✅ (参考)\n\n任务已完成！\n查看 PR: https://github.com/Tinlok/daily-tasks-code/pull/$PR_NUMBER" \
                            "✅"
                    else
                        log "  ℹ️  Task already done, skipping notification"
                        update_task "$TASK_ID" '{"status": "done", "completedAt": "'$(date +%s)000'", "pr": '"$PR_NUMBER"', "checks": {"prCreated": true, "ciPassed": true}}'
                    fi
                    ;;
                failure)
                    log "  ❌ CI failed"
                    update_task "$TASK_ID" '{"status": "ci-failed", "ciFailedAt": "'$(date +%s)000'"}'

                    if [[ "$RETRY_COUNT" -lt "$MAX_RETRIES" ]]; then
                        log "  🔄 Would retry here"
                    else
                        send_feishu_notification \
                            "CI Failed: PR #$PR_NUMBER" \
                            "Max retries reached.\nBranch: \`$BRANCH\`\nTask: \`$TASK_ID\`\nDate: \`$TASK_DATE\`\n\nManual intervention required." \
                            "💀"
                    fi
                    ;;
                pending|in_progress|*)
                    log "  ⏳ CI in progress: $CI_STATUS"
                    ;;
            esac
        else
            log "  ℹ️  PR state: $PR_STATE"
        fi
    else
        log "  ℹ️  No PR yet, agent still working..."
    fi
done

log "✓ Agent swarm check complete"
