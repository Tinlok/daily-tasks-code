#!/bin/bash
# git-flow.sh - 增强版 Git 工作流脚本
# 功能：分支管理、智能 commit、PR 创建、代码质量检查
# 用法：./git-flow.sh <command> [options]
#
# 命令：
#   branch create <name> [base]  - 从 base 创建特性分支
#   branch switch <name>         - 切换分支（自动 stash）
#   branch cleanup               - 清理已合并的分支
#   commit [message]             - 智能 commit（自动生成或自定义消息）
#   pr create [title] [body]     - 创建 PR
#   pr list                      - 列出当前仓库的 PR
#   quality                      - 运行代码质量检查
#   status                       - 查看当前工作区状态摘要

set -euo pipefail

# ============================================================
# 配置
# ============================================================
REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null)
if [[ -z "$REPO_ROOT" ]]; then
    echo "❌ 错误：不在 Git 仓库中"
    exit 1
fi

cd "$REPO_ROOT"

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m'

# ============================================================
# 工具函数
# ============================================================

log_info()    { echo -e "${BLUE}ℹ️  $1${NC}"; }
log_ok()      { echo -e "${GREEN}✅ $1${NC}"; }
log_warn()    { echo -e "${YELLOW}⚠️  $1${NC}"; }
log_error()   { echo -e "${RED}❌ $1${NC}"; exit 1; }
log_section() { echo -e "\n${BOLD}${CYAN}━━━ $1 ━━━${NC}\n"; }

# 确认操作
confirm() {
    local msg="$1"
    echo -e "${YELLOW}$msg [y/N]${NC}"
    read -r response
    [[ "$response" =~ ^[Yy]$ ]]
}

# 检查依赖
check_dep() {
    local cmd="$1"
    local name="${2:-$cmd}"
    if ! command -v "$cmd" &>/dev/null; then
        log_warn "$name 未安装，部分功能不可用"
        return 1
    fi
    return 0
}

# ============================================================
# 智能分支名规范化
# ============================================================
normalize_branch_name() {
    local input="$1"
    # 转小写，空格转连字符，移除特殊字符
    echo "$input" | tr '[:upper:]' '[:lower:]' | tr ' ' '-' | sed 's/[^a-z0-9._-]//g'
}

# ============================================================
# 分支管理
# ============================================================

cmd_branch_create() {
    local name="${1:?用法：git-flow.sh branch create <name> [base]}"
    local base="${2:-main}"
    local branch_name

    branch_name="feature/$(normalize_branch_name "$name")"

    # 检查分支是否已存在
    if git show-ref --verify --quiet "refs/heads/$branch_name"; then
        log_error "分支 '$branch_name' 已存在"
    fi

    # 检查 base 分支是否存在
    if ! git show-ref --verify --quiet "refs/heads/$base" && \
       ! git show-ref --verify --quiet "refs/remotes/origin/$base"; then
        log_error "基础分支 '$base' 不存在"
    fi

    # 确保 base 是最新的
    log_info "拉取最新的 $base ..."
    git fetch origin "$base:$base" 2>/dev/null || true

    log_info "从 $base 创建分支 $branch_name ..."
    git checkout -b "$branch_name" "$base"
    log_ok "分支已创建并切换到 $branch_name"
}

cmd_branch_switch() {
    local name="${1:?用法：git-flow.sh branch switch <name>}"

    # 支持模糊匹配：如果输入不完整，尝试匹配
    local matched_branch=""
    if git show-ref --verify --quiet "refs/heads/$name"; then
        matched_branch="$name"
    else
        # 尝试模糊匹配
        matched_branch=$(git branch --list "*$name*" --format='%(refname:short)' | head -1)
        if [[ -z "$matched_branch" ]]; then
            log_error "找不到匹配 '$name' 的分支"
        fi
    fi

    # 自动 stash 未提交的更改
    local stash_needed=false
    if ! git diff --quiet || ! git diff --cached --quiet; then
        log_warn "检测到未提交的更改，自动 stash ..."
        git stash push -m "auto-stash: switching to $matched_branch"
        stash_needed=true
    fi

    git checkout "$matched_branch"

    if $stash_needed; then
        log_info "恢复 stash ..."
        git stash pop 2>/dev/null && log_ok "Stash 已恢复" || log_warn "Stash 恢复失败（可能有冲突），使用 git stash list 查看"
    fi

    log_ok "已切换到 $matched_branch"
}

cmd_branch_cleanup() {
    log_section "清理已合并分支"

    local current_branch
    current_branch=$(git branch --show-current)
    local default_branch="${1:-main}"

    # 获取已合并到 default_branch 的分支（排除当前分支和 protected 分支）
    local merged_branches
    merged_branches=$(git branch --merged "origin/$default_branch" --format='%(refname:short)' \
        | grep -v "^\*\?$current_branch$" \
        | grep -v "^main$" \
        | grep -v "^master$" \
        | grep -v "^develop$")

    if [[ -z "$merged_branches" ]]; then
        log_ok "没有需要清理的分支"
        return 0
    fi

    echo -e "${YELLOW}以下分支已合并到 $default_branch：${NC}"
    echo "$merged_branches"
    echo ""

    if ! confirm "确认删除以上分支？"; then
        log_info "已取消"
        return 0
    fi

    echo "$merged_branches" | while read -r branch; do
        [[ -z "$branch" ]] && continue
        # 先删除远程分支
        git push origin --delete "$branch" 2>/dev/null && log_ok "已删除远程分支 $branch" || true
        # 再删除本地分支
        git branch -d "$branch" && log_ok "已删除本地分支 $branch" || log_warn "无法删除本地分支 $branch"
    done

    # 清理远程已删除的跟踪分支
    git fetch --prune 2>/dev/null
    log_ok "分支清理完成"
}

# ============================================================
# 智能 Commit
# ============================================================

generate_commit_message() {
    local staged_files
    staged_files=$(git diff --cached --name-only)

    if [[ -z "$staged_files" ]]; then
        log_error "没有已暂存的文件。先运行 git add"
    fi

    # 分析变更类型
    local has_new=false has_modified=false has_deleted=false
    local has_test=false has_docs=false has_config=false has_src=false
    local ext_summary=""

    while IFS= read -r file; do
        [[ -z "$file" ]] && continue
        case "$(git diff --cached --diff-filter=A -- "$file" | head -1)" in
            "") has_modified=true ;;
            *)  has_new=true ;;
        esac

        case "$file" in
            *test*|*spec*)   has_test=true ;;
            *README*|*doc*|*md) has_docs=true ;;
            *.yaml|*.yml|*.json|*.toml|*.cfg|*.conf) has_config=true ;;
        esac

        # 提取扩展名
        local ext="${file##*.}"
        if [[ -n "$ext" && "$ext" != "$file" ]]; then
            ext_summary+="$ext "
        fi
    done <<< "$staged_files"

    # 生成类型
    local commit_type="feat"
    $has_docs && commit_type="docs"
    $has_test && commit_type="test"
    $has_config && commit_type="chore"
    $has_modified && ! $has_new && ! $has_docs && ! $has_test && ! $has_config && commit_type="update"

    # 统计
    local file_count=$(echo "$staged_files" | wc -l | tr -d ' ')
    local insertions=0 deletions=0
    local stats
    stats=$(git diff --cached --numstat 2>/dev/null)
    while IFS=$'\t' read -r ins del _; do
        [[ -z "$ins" ]] && continue
        insertions=$((insertions + ${ins:-0}))
        deletions=$((deletions + ${del:-0}))
    done <<< "$stats"

    # 生成消息
    local msg="$commit_type: "
    if $has_new; then
        msg+="add ${file_count} file(s)"
    else
        msg+="update ${file_count} file(s)"
    fi

    msg+=" (+${insertions}/-${deletions})"

    if [[ -n "$ext_summary" ]]; then
        msg+=" [$(echo "$ext_summary" | tr ' ' '\n' | sort -u | tr '\n' ' ' | sed 's/ $//')]"
    fi

    echo "$msg"
}

cmd_commit() {
    local custom_msg="$1"

    # 检查是否有暂存的文件
    if git diff --cached --quiet; then
        log_info "没有已暂存的文件，尝试添加所有变更 ..."
        if git diff --quiet; then
            log_error "没有需要提交的变更"
        fi

        if ! confirm "暂存所有变更文件？"; then
            log_error "已取消。请手动 git add 后重试"
        fi
        git add -A
    fi

    if [[ -n "$custom_msg" ]]; then
        local commit_msg="$custom_msg"
    else
        local commit_msg
        commit_msg=$(generate_commit_message)
        log_info "自动生成的 commit 消息："
        echo -e "  ${BOLD}$commit_msg${NC}"
        echo ""
        if ! confirm "使用此消息？(输入 n 可自定义)"; then
            echo -e "${CYAN}请输入 commit 消息：${NC}"
            read -r commit_msg
            [[ -z "$commit_msg" ]] && log_error "消息不能为空"
        fi
    fi

    git commit -m "$commit_msg"
    log_ok "提交成功：$commit_msg"
}

# ============================================================
# PR 管理
# ============================================================

cmd_pr_create() {
    local title="${1:-}"
    local body="${2:-}"

    # 检查 gh CLI
    if ! check_dep gh "GitHub CLI"; then
        log_error "需要安装 GitHub CLI：brew install gh"
    fi

    # 检查是否已推送
    local current_branch
    current_branch=$(git branch --show-current)

    if [[ "$current_branch" == "main" || "$current_branch" == "master" ]]; then
        log_error "不能从 $current_branch 创建 PR"
    fi

    local remote_exists
    remote_exists=$(git ls-remote --heads origin "$current_branch" 2>/dev/null)

    if [[ -z "$remote_exists" ]]; then
        log_info "推送分支到远程 ..."
        git push -u origin "$current_branch" || log_error "推送失败"
    fi

    # 自动生成 PR 信息
    if [[ -z "$title" ]]; then
        # 基于分支名和 commit 历史生成标题
        title=$(echo "$current_branch" | sed 's/^[^/]*\///' | tr '-' ' ')
        title="$(tr '[:lower:]' '[:upper:]' <<< ${title:0:1})${title:1}"  # 首字母大写
    fi

    if [[ -z "$body" ]]; then
        body="## 变更摘要

$(git log origin/main..HEAD --oneline 2>/dev/null || git log HEAD --oneline -10)

---
_Automated PR created by git-flow.sh_"
    fi

    log_info "创建 PR：$title"
    gh pr create --title "$title" --body "$body"
    log_ok "PR 创建成功"
}

cmd_pr_list() {
    if ! check_dep gh "GitHub CLI"; then
        log_error "需要安装 GitHub CLI：brew install gh"
    fi

    log_section "Pull Requests"
    gh pr list --limit 20 \
        --json number,title,headRefName,state,updatedAt \
        --template '{{range .}}{{printf "#%v [%v] %s (updated: %s)\n" .number .state .title .updatedAt}}{{end}}'
}

# ============================================================
# 代码质量检查
# ============================================================

cmd_quality() {
    log_section "代码质量检查"

    local has_errors=false

    # 1. 检查是否有未暂存的更改（警告）
    if ! git diff --quiet; then
        log_warn "有未暂存的更改（以下检查基于暂存区/HEAD）"
    fi

    # 2. 基础检查
    log_info "运行基础检查 ..."

    # 尾部空格
    local trailing=$(git diff --cached --name-only 2>/dev/null | xargs grep -l ' $' 2>/dev/null || true)
    if [[ -n "$trailing" ]]; then
        log_warn "尾部空格：$(echo "$trailing" | tr '\n' ', ')"
    else
        log_ok "无尾部空格问题"
    fi

    # 行尾符
    local crlf=$(git diff --cached --name-only 2>/dev/null | xargs file 2>/dev/null | grep CRLF || true)
    if [[ -n "$crlf" ]]; then
        log_warn "CRLF 行尾符：$(echo "$crlf" | awk -F: '{print $1}' | tr '\n' ', ')"
    else
        log_ok "行尾符正常 (LF)"
    fi

    # 大文件检查
    local large_files=$(git diff --cached --name-only 2>/dev/null | while read -r f; do
        [[ -z "$f" ]] && continue
        size=$(wc -c < "$f" 2>/dev/null || echo 0)
        [[ "$size" -gt 1048576 ]] && echo "$f (${size}B)"
    done)

    if [[ -n "$large_files" ]]; then
        log_warn "大文件 (>1MB)：$large_files"
    else
        log_ok "无过大文件"
    fi

    # 3. Shell 脚本检查
    local shell_files=$(git diff --cached --name-only 2>/dev/null | grep '\.sh$' || true)
    if [[ -n "$shell_files" ]]; then
        log_info "检查 Shell 脚本 ..."

        if check_dep shellcheck "ShellCheck"; then
            while IFS= read -r f; do
                [[ -z "$f" ]] && continue
                if shellcheck -s bash "$f" 2>/dev/null; then
                    log_ok "ShellCheck 通过: $f"
                else
                    log_warn "ShellCheck 有警告: $f"
                    has_errors=true
                fi
            done <<< "$shell_files"
        else
            log_warn "跳过 Shell 脚本检查（shellcheck 未安装）"
        fi
    fi

    # 4. 语言特定检查
    local py_files=$(git diff --cached --name-only 2>/dev/null | grep '\.py$' || true)
    if [[ -n "$py_files" ]]; then
        log_info "检查 Python 文件 ..."

        if check_dep ruff "ruff"; then
            if echo "$py_files" | xargs ruff check 2>/dev/null; then
                log_ok "ruff 检查通过"
            else
                log_warn "ruff 发现问题"
                has_errors=true
            fi
        fi
    fi

    local cs_files=$(git diff --cached --name-only 2>/dev/null | grep '\.cs$' || true)
    if [[ -n "$cs_files" ]]; then
        log_info "检测到 C# 文件：$(echo "$cs_files" | tr '\n' ', ')"
        log_warn "C# lint 需要 dotnet format / EditorConfig（项目级配置）"
    fi

    # 5. JSON/YAML 格式检查
    local json_files=$(git diff --cached --name-only 2>/dev/null | grep '\.json$' || true)
    if [[ -n "$json_files" ]]; then
        log_info "验证 JSON 文件 ..."
        while IFS= read -r f; do
            [[ -z "$f" ]] && continue
            if python3 -c "import json; json.load(open('$f'))" 2>/dev/null; then
                log_ok "JSON 有效: $f"
            else
                log_warn "JSON 无效: $f"
                has_errors=true
            fi
        done <<< "$json_files"
    fi

    # 6. Commit 消息规范检查（最新 commit）
    local last_msg
    last_msg=$(git log -1 --format='%s')
    if [[ ! "$last_msg" =~ ^(feat|fix|docs|style|refactor|test|chore|update|add|remove|bump|init|ci|perf|build|revert)(\(.+\))?: ]]; then
        log_warn "最新 commit 消息不符合 Conventional Commits 规范：$last_msg"
    else
        log_ok "Commit 消息规范合规：$last_msg"
    fi

    echo ""
    if $has_errors; then
        log_warn "质量检查完成（有问题需要关注）"
    else
        log_ok "所有质量检查通过 🎉"
    fi
}

# ============================================================
# 状态摘要
# ============================================================

cmd_status() {
    log_section "工作区状态"

    local current_branch
    current_branch=$(git branch --show-current)
    local ahead_behind
    ahead_behind=$(git rev-list --left-right --count "@{upstream}...HEAD" 2>/dev/null || echo "0	0")
    local ahead=$(echo "$ahead_behind" | awk '{print $2}')
    local behind=$(echo "$ahead_behind" | awk '{print $1}')

    echo -e "${BOLD}分支：${NC}$current_branch"
    echo -e "${BOLD}远程：${NC}领先 $ahead / 落后 $behind"
    echo ""

    # 未暂存更改
    local changed
    changed=$(git diff --stat 2>/dev/null | tail -1)
    if [[ -n "$changed" ]]; then
        echo -e "${YELLOW}未暂存更改：${NC}$changed"
    else
        echo -e "${GREEN}工作区干净（未暂存）${NC}"
    fi

    # 已暂存更改
    local staged
    staged=$(git diff --cached --stat 2>/dev/null | tail -1)
    if [[ -n "$staged" ]]; then
        echo -e "${YELLOW}已暂存更改：${NC}$staged"
    else
        echo -e "${GREEN}暂存区干净${NC}"
    fi

    # 未跟踪文件
    local untracked
    untracked=$(git ls-files --others --exclude-standard | wc -l | tr -d ' ')
    if [[ "$untracked" -gt 0 ]]; then
        echo -e "${YELLOW}未跟踪文件：${NC}${untracked} 个"
    fi

    # Stash 列表
    local stash_count
    stash_count=$(git stash list 2>/dev/null | wc -l | tr -d ' ')
    if [[ "$stash_count" -gt 0 ]]; then
        echo -e "${CYAN}Stash：${NC}${stash_count} 个"
    fi

    echo ""

    # 最近 commits
    echo -e "${BOLD}最近提交：${NC}"
    git log --oneline -5 2>/dev/null
}

# ============================================================
# 帮助
# ============================================================

cmd_help() {
    cat << 'EOF'
🚀 git-flow.sh - 增强版 Git 工作流脚本

用法: git-flow.sh <command> [options]

分支管理:
  branch create <name> [base]   从 base 创建 feature/<name> 分支
  branch switch <name>           切换分支（自动 stash/恢复）
  branch cleanup                 清理已合并的分支

提交:
  commit [message]               智能 commit（自动分析变更）
                                  不传 message 则自动生成

PR:
  pr create [title] [body]       创建 PR（自动推送、生成标题）
  pr list                        列出 PR

质量:
  quality                        运行代码质量检查
                                  (shellcheck, ruff, JSON 验证等)

其他:
  status                         工作区状态摘要
  help                           显示帮助

示例:
  git-flow.sh branch create "user login"
  git-flow.sh branch switch login
  git-flow.sh commit
  git-flow.sh quality
  git-flow.sh pr create "feat: add user login"
  git-flow.sh branch cleanup
EOF
}

# ============================================================
# 主入口
# ============================================================

main() {
    local command="${1:-help}"
    shift || true

    case "$command" in
        branch)
            local sub="${1:-help}"
            shift || true
            case "$sub" in
                create)  cmd_branch_create "$@" ;;
                switch)  cmd_branch_switch "$@" ;;
                cleanup) cmd_branch_cleanup "$@" ;;
                *)       echo "用法：git-flow.sh branch {create|switch|cleanup} [options]" ;;
            esac
            ;;
        commit)  cmd_commit "$@" ;;
        pr)
            local sub="${1:-help}"
            shift || true
            case "$sub" in
                create) cmd_pr_create "$@" ;;
                list)   cmd_pr_list "$@" ;;
                *)      echo "用法：git-flow.sh pr {create|list} [options]" ;;
            esac
            ;;
        quality) cmd_quality ;;
        status)  cmd_status ;;
        help|--help|-h) cmd_help ;;
        *) log_error "未知命令 '$command'。运行 git-flow.sh help 查看帮助" ;;
    esac
}

main "$@"
