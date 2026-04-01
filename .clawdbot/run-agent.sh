#!/bin/bash
# run-agent.sh - Spawn a coding agent in tmux session with worktree isolation
# Usage: ./run-agent.sh <task-id> <task-date> <branch-name> "<prompt>" [model]

set -e

TASK_ID=$1
TASK_DATE=$2
BRANCH_NAME=$3
PROMPT=$4
MODEL=${5:-"claude-opus-4.5"}
REPO=$(git rev-parse --show-toplevel)
WORKTREE_PATH="$REPO.worktrees/$TASK_ID"
TMUX_SESSION="agent-$TASK_ID"

# Check dependencies
command -v git >/dev/null 2>&1 || { echo "Error: git not found"; exit 1; }
command -v tmux >/dev/null 2>&1 || { echo "Error: tmux not found"; exit 1; }
command -v claude >/dev/null 2>&1 || { echo "Error: claude CLI not found"; exit 1; }

# Validate inputs
if [[ -z "$TASK_ID" || -z "$TASK_DATE" || -z "$BRANCH_NAME" || -z "$PROMPT" ]]; then
    echo "Usage: $0 <task-id> <task-date> <branch-name> \"<prompt>\" [model]"
    exit 1
fi

# Check if task already exists
if tmux has-session -t "$TMUX_SESSION" 2>/dev/null; then
    echo "Error: tmux session $TMUX_SESSION already exists"
    exit 1
fi

# Create worktree
echo "Creating worktree at $WORKTREE_PATH on branch $BRANCH_NAME..."
git worktree add -b "$BRANCH_NAME" "$WORKTREE_PATH" main 2>/dev/null || \
git worktree add -b "$BRANCH_NAME" "$WORKTREE_PATH" origin/main

# Create task directory structure
TASK_DIR="$WORKTREE_PATH/$TASK_DATE/task-$TASK_ID"
mkdir -p "$TASK_DIR"
echo "Task directory created: $TASK_DIR"

# Register task in active-tasks.json
TASK_JSON=$(cat "$REPO/.clawdbot/active-tasks.json")
REPO_NAME=$(basename "$REPO")
NEW_TASK=$(jq -n \
  --arg id "$TASK_ID" \
  --arg date "$TASK_DATE" \
  --arg tmuxSession "$TMUX_SESSION" \
  --arg agent "claude" \
  --arg branch "$BRANCH_NAME" \
  --arg worktree "$WORKTREE_PATH" \
  --arg taskDir "$TASK_DIR" \
  --arg prompt "$PROMPT" \
  --arg repoName "$REPO_NAME" \
  --argjson startedAt "$(date +%s)000" \
  '{
    id: $id,
    date: $date,
    tmuxSession: $tmuxSession,
    agent: $agent,
    description: $prompt,
    repo: $repoName,
    worktree: $worktree,
    taskDir: $taskDir,
    branch: $branch,
    startedAt: $startedAt,
    status: "running",
    notifyOnComplete: true
  }')

UPDATED_TASKS=$(echo "$TASK_JSON" | jq ".tasks += [$NEW_TASK]")
echo "$UPDATED_TASKS" > "$REPO/.clawdbot/active-tasks.json"

echo "Starting Claude Code agent in tmux session $TMUX_SESSION..."

# Prepare the command
CMD="claude --model $MODEL --dangerously-skip-permissions -p \"$PROMPT

Project Context:
This is a daily task repository. Task directory: $TASK_DIR

Code style guidelines:
- Unity C# code: Use C# 9.0+ features, follow Unity best practices
- Shell scripts: Use bash, add proper error handling
- Add comments and documentation
- Follow clean code principles

Task output requirements:
1. Put all code in: $TASK_DIR
2. Create appropriate README if needed
3. Test your code locally
4. Commit with message: '[$TASK_DATE] Task #$TASK_ID: <task-title>'
5. Push to branch: $BRANCH_NAME
6. Create PR with title: '[$TASK_DATE] Task #$TASK_ID: <task-title>'

Definition of Done:
1. All code committed to $TASK_DIR
2. Create PR using 'gh pr create --fill'
3. Update task registry in .clawdbot/active-tasks.json when done\""

# Start tmux session and run the command
tmux new-session -d -s "$TMUX_SESSION" -c "$WORKTREE_PATH" "$CMD"

echo "✓ Agent started successfully"
echo "  - Task ID: $TASK_ID"
echo "  - Date: $TASK_DATE"
echo "  - Branch: $BRANCH_NAME"
echo "  - Worktree: $WORKTREE_PATH"
echo "  - Task Dir: $TASK_DIR"
echo "  - Tmux session: $TMUX_SESSION"
echo "  - Model: $MODEL"
echo ""
echo "Monitor: tmux attach -t $TMUX_SESSION"
echo "Send command: tmux send-keys -t $TMUX_SESSION '<command>' Enter"
