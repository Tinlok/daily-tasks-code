#!/bin/bash
# review-pr.sh - Code review PR using Claude Code + Gemini
# Usage: ./review-pr.sh <pr-number> [--claude-only | --gemini-only]

set -e

# Load env
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
source "$SCRIPT_DIR/.env"

PR_NUMBER=$1
REVIEW_MODE="${2:-all}"

if [[ -z "$PR_NUMBER" ]]; then
    echo "Usage: $0 <pr-number> [--claude-only | --gemini-only | all]"
    exit 1
fi

echo "🔍 Starting code review for PR #$PR_NUMBER..."

# Get PR details
PR_TITLE=$(gh pr view "$PR_NUMBER" --json title --jq '.title')
PR_BRANCH=$(gh pr view "$PR_NUMBER" --json headRefName --jq '.headRefName')
PR_AUTHOR=$(gh pr view "$PR_NUMBER" --json author --jq '.author.login')

echo "📋 PR: #$PR_NUMBER"
echo "   Title: $PR_TITLE"
echo "   Branch: $PR_BRANCH"
echo "   Author: $PR_AUTHOR"
echo ""

# Get diff
gh pr diff "$PR_NUMBER" > /tmp/pr-diff-$PR_NUMBER.patch
DIFF_LINES=$(wc -l < /tmp/pr-diff-$PR_NUMBER.patch)

echo "📊 Changes: $DIFF_LINES lines"
echo ""

PROMPT="请用中文审查这个 Pull Request PR #$PR_NUMBER，标题：\"$PR_TITLE\"。

PR 作者：$PR_AUTHOR
分支：$PR_BRANCH

代码变更（diff）：
$(cat /tmp/pr-diff-$PR_NUMBER.patch)

审查标准：
1. **代码质量**：代码是否整洁、易读，符合最佳实践？
2. **逻辑**：是否有逻辑错误、边界情况或竞态条件？
3. **安全性**：是否有安全漏洞或潜在问题？
4. **性能**：是否有性能问题或优化机会？
5. **Unity 最佳实践**：是否遵循 Unity 编码规范？（如果是 Unity 代码）
6. **C# 最佳实践**：是否遵循 C# 最佳实践和现代语法？
7. **Shell 脚本最佳实践**：脚本是否有适当的错误处理？（如果是 Shell 脚本）

对于发现的每个问题：
- 指出具体的文件和行号
- 清楚解释问题
- 提供具体的修复建议
- 评级严重性：严重 / 高 / 中 / 低
- 评级置信度：高 / 中 / 低

请按以下格式输出审查结果：

## 总结
[代码质量的简要总结]

## 问题
### [严重性] 问题 1
- 文件：filename.cs:行号
- 描述：[详细解释]
- 建议：[具体修复方案]
- 置信度：[高/中/低]

... 更多问题 ...

## 结论
[整体评价：批准 / 批准但需修改 / 需要修改]

注意：本审查仅供参考，不阻塞任务完成。"

# --- Claude Code Review ---
if [[ "$REVIEW_MODE" == "all" || "$REVIEW_MODE" == "--claude-only" ]]; then
    echo "🤖 Reviewing with Claude Code..."

    CLAUDE_MODEL="${CLAUDE_MODEL:-claude-opus-4.5}"
    REVIEW_OUTPUT=$(claude --model "$CLAUDE_MODEL" \
        --dangerously-skip-permissions \
        -p "$PROMPT")

    echo "📤 Posting Claude Code review to PR #$PR_NUMBER..."
    echo "**🤖 Claude Code 审查 ($CLAUDE_MODEL)**" > /tmp/review-claude-$PR_NUMBER.txt
    echo "" >> /tmp/review-claude-$PR_NUMBER.txt
    echo "$REVIEW_OUTPUT" >> /tmp/review-claude-$PR_NUMBER.txt
    gh pr comment "$PR_NUMBER" --body-file /tmp/review-claude-$PR_NUMBER.txt
    echo "✅ Claude Code review posted"

    rm -f /tmp/review-claude-$PR_NUMBER.txt
    echo ""
fi

# --- Gemini Review ---
if [[ "$REVIEW_MODE" == "all" || "$REVIEW_MODE" == "--gemini-only" ]]; then
    if [[ -z "$GEMINI_API_KEY" ]]; then
        echo "⚠️  GEMINI_API_KEY not set in .env, skipping Gemini review"
    else
        echo "💎 Reviewing with Gemini..."

        # Escape prompt for JSON
        ESCAPED_PROMPT=$(echo "$PROMPT" | python3 -c 'import json,sys; print(json.dumps(sys.stdin.read()))')

        GEMINI_OUTPUT=$(curl -s --max-time 120 \
            ${HTTPS_PROXY:+--proxy "$HTTPS_PROXY"} \
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=$GEMINI_API_KEY" \
            -H "Content-Type: application/json" \
            -d "{\"contents\":[{\"parts\":[{\"text\":$ESCAPED_PROMPT}]}],\"generationConfig\":{\"temperature\":0.2}}")

        # Extract text from response
        REVIEW_TEXT=$(echo "$GEMINI_OUTPUT" | python3 -c '
import json, sys
data = json.load(sys.stdin)
try:
    print(data["candidates"][0]["content"]["parts"][0]["text"])
except (KeyError, IndexError):
    print("❌ Gemini API 返回错误:", json.dumps(data, ensure_ascii=False))
')

        if [[ "$REVIEW_TEXT" == ❌* ]]; then
            echo "$REVIEW_TEXT"
        else
            echo "📤 Posting Gemini review to PR #$PR_NUMBER..."
            echo "**💎 Gemini 审查 (gemini-2.5-flash)**" > /tmp/review-gemini-$PR_NUMBER.txt
            echo "" >> /tmp/review-gemini-$PR_NUMBER.txt
            echo "$REVIEW_TEXT" >> /tmp/review-gemini-$PR_NUMBER.txt
            gh pr comment "$PR_NUMBER" --body-file /tmp/review-gemini-$PR_NUMBER.txt
            echo "✅ Gemini review posted"

            rm -f /tmp/review-gemini-$PR_NUMBER.txt
        fi
        echo ""
    fi
fi

# Cleanup
rm -f /tmp/pr-diff-$PR_NUMBER.patch

echo "📋 Summary:"
if [[ "$REVIEW_MODE" == "all" || "$REVIEW_MODE" == "--claude-only" ]]; then
    echo "   ✅ Claude Code reviewed"
fi
if [[ "$REVIEW_MODE" == "all" || "$REVIEW_MODE" == "--gemini-only" ]]; then
    if [[ -n "$GEMINI_API_KEY" ]]; then
        echo "   ✅ Gemini reviewed"
    else
        echo "   ⚠️  Gemini skipped (no API key)"
    fi
fi
echo "   Reviewed files: $(gh pr view "$PR_NUMBER" --json files --jq '.files | length')"
echo "   Changes: $DIFF_LINES lines"
