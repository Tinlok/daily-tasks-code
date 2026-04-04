# PlayerController2D Enhanced - 使用说明

## 新增系统

### 1. 冲刺系统 (Dash)
- **触发**: Input Manager 配置 "Dash" 按钮（建议绑定 Left Shift）
- **机制**: AnimationCurve 控制速度衰减，冲刺结束保留 40% 动量，支持空中冲刺
- **冷却**: 0.8s，可通过 `DashCooldownNormalized` 属性查询进度

### 2. 攀爬系统 (Climb)
- **触发**: Input Manager 配置 "Climb" 按钮（建议绑定 Left Alt）
- **机制**: 接触墙壁按住 Climb 攀爬，消耗体力（默认3秒），体力耗尽自动松手
- **蹬墙跳**: 攀爬中按 Jump 蹬墙跳，带水平推力

### 3. 手感优化
- velocity-based 移动（非线性映射）
- 着陆压扁效果（Landing Squash）
- 重坠落（fallGravityMultiplier）
- 坡道辅助
- 全系统输入缓冲

## Input Manager 配置
| Name | Positive Button |
|------|----------------|
| Dash | left shift |
| Climb | left alt |

## Layer 配置
- `ClimbLayer`: 指定给可攀爬物体

## 事件接口
```csharp
controller.OnDashStart/End
controller.OnClimbStart/End
controller.OnGrabLedge
controller.OnStaminaDepleted
controller.OnLanded
```
