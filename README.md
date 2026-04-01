# Daily Tasks Code Repository

这是每日任务的代码产出仓库，按日期和任务号组织。

## 目录结构

```
daily-tasks-code/
├── .clawdbot/                    # Agent Swarm 管理
│   ├── run-agent.sh               # 启动编码 Agent
│   ├── check-agents.sh            # 监控 Agent
│   ├── review-pr.sh               # 双模型评审（Claude + Gemini）
│   ├── active-tasks.json          # 任务注册表
│   └── .env                     # 环境变量
├── .github/workflows/             # CI/CD 配置
│   └── ci-check.yml             # 代码检查工作流
└── [年份]/[月份]/[日期]/         # 按日期组织的代码
    ├── task-1/                   # 任务1的代码
    ├── task-2/                   # 任务2的代码
    └── ...
```

## 任务分类

| 任务类型 | 目录前缀 | 示例 |
|---------|-----------|------|
| 调研 | `task-1-research/` | 市场分析、竞品调研 |
| 开发 | `task-2-dev/` | Unity 代码、功能实现 |
| 学习 | `task-3-study/` | 学习笔记、代码示例 |
| 创意 | `task-4-creative/` | 游戏设计、GDD |
| 技术 | `task-5-tech/` | 自动化脚本、工具 |

## Agent Swarm 工作流程

```
1. AI 完成任务
   ↓
2. 创建分支并提交代码
   - 分支：feature/2026-04-01-task-2
   - 提交："[2026-04-01] Task #2: 任务标题"
   ↓
3. 推送到 GitHub
   ↓
4. 创建 PR
   - 标题："[2026-04-01] Task #2: 任务标题"
   ↓
5. CI/CD 自动运行
   - C# 代码格式检查
   - Shell 脚本检查
   ↓
6. CI 通过 → 触发双模型评审
   - Claude Code 评审
   - Gemini 评审
   ↓
7. 评审通过 → 标记任务完成
   - 更新 kanban.md 状态
   - 发送飞书通知
```

## 初始化

### 1. 设置环境变量

```bash
cd /Users/tinlok/Documents/GitHub/daily-tasks-code/.clawdbot
cp .env.example .env
# 编辑 .env，填入你的飞书 Webhook URL
```

### 2. 推送到 GitHub

```bash
cd /Users/tinlok/Documents/GitHub/daily-tasks-code
git remote add origin https://github.com/Tinlok/daily-tasks-code.git
git push -u origin main
```

### 3. 登录 GitHub

```bash
gh auth login
```

## 使用方法

### 启动一个编码 Agent

```bash
cd /Users/tinlok/Documents/GitHub/daily-tasks-code
./.clawdbot/run-agent.sh <task-id> <task-date> <branch-name> "<prompt>"
```

**示例：**

```bash
# Unity 代码任务
./.clawdbot/run-agent.sh \
  2 \
  2026-04-01 \
  feature/2026-04-01-task-2 \
  "使用Unity+AI工具构建2D平台跳跃原型（基础移动+跳跃机制）。要求：二段跳、土狼时间、跳跃缓冲。"
```

### 监控 Agent 状态

```bash
# 查看所有 tmux 会话
tmux ls

# 附加到 Agent 会话
tmux attach -t agent-2

# 发送指令给 Agent（无需终止）
tmux send-keys -t agent-2 "Stop. Focus on PlayerController.cs file first." Enter
```

### 检查任务状态

```bash
# 查看任务注册表
cat .clawdbot/active-tasks.json

# 手动运行监控脚本（测试用）
./.clawdbot/check-agents.sh

# 查看 PR
gh pr list
```

## Definition of Done

任务完成标准：
- ✅ PR 已创建
- ✅ CI 通过（代码格式检查 + Shell 脚本检查）
- ✅ 代码审查通过（Claude Code + Gemini，仅供参考）
- ✅ Kanban.md 状态更新为"✅完成"
- ✅ 飞书通知发送

## 代码规范

### Unity C# 代码
- 使用 C# 9.0+ 特性
- 遵循 Unity 最佳实践
- 为公共 API 添加 XML 文档注释
- 分离关注点（逻辑与 Unity 生命周期方法分离）
- 使用适当的命名约定（PascalCase 用于方法/属性，camelCase 用于局部变量）

### Shell 脚本
- 使用 bash
- 添加适当的错误处理（`set -e`）
- 添加注释和文档
- 遵循 Google Shell Style Guide

### Git 提交
- 格式：`[YYYY-MM-DD] Task #N: 任务标题`
- 示例：`[2026-04-01] Task #2: 使用Unity+AI工具构建2D平台跳跃原型`

## 清理

### 清理已完成的任务

每日清理孤立的 worktree：
```bash
cd /Users/tinlok/Documents/GitHub/daily-tasks-code
git worktree prune
```

### 清理过期任务

编辑 `.clawdbot/active-tasks.json`，移除已完成（`status: "done"`）超过 7 天的任务。

## 故障排查

### Agent 没有响应？

```bash
# 检查 tmux 会话是否存活
tmux ls

# 查看完整日志
tmux capture-pane -t agent-<task-id> -p | tail -100
```

### CI 失败？

```bash
# 查看 GitHub Actions 日志
gh run list
gh run view <run-id>

# 查看特定 PR 的检查状态
gh pr checks <pr-number>
```

### 飞书通知没有收到？

```bash
# 检查环境变量
cat .clawdbot/.env

# 手动测试 Webhook
curl -X POST "$FEISHU_WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -d '{"msg_type":"text","content":{"text":"Test from daily-tasks-code"}}'
```

## 技术栈

- **编排**: OpenClaw (sessions_spawn + cron)
- **编码 Agent**: Claude Code CLI
- **版本控制**: Git + Git Worktree
- **会话管理**: tmux
- **PR 管理**: GitHub CLI (gh)
- **CI/CD**: GitHub Actions
- **通知**: 飞书自定义机器人

## 下一步

1. 创建第一个测试任务，跑通完整流程
2. 根据实际需求调整 CI 配置
3. 添加更多编码 Agent（Codex 等）
4. 集成 Unity Cloud Build 用于完整构建测试
