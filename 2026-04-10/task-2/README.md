# Unity 2D 增强角色控制器

一个功能完整的 Unity 2D 平台角色控制器，包含冲刺、攀爬、跳跃等高级移动机制。

## 功能特性

### 核心移动系统
- **平滑移动**: 可配置的加速度/减速度曲线
- **空中控制**: 独立的空中移动参数
- **摩擦力系统**: 自然的停止手感

### 冲刺系统 (Dash)
- 短距离快速移动
- 可配置冲刺速度和持续时间
- 空中冲刺次数限制
- 冲刺冷却机制
- 冲刺残影视觉效果
- 冲刺时可选忽略重力

### 攀爬系统 (Climb)
- 梯子/藤蔓等垂直攀爬
- 墙壁攀爬（体力系统）
- 边缘检测和自动攀爬
- 体力消耗和恢复机制

### 跳跃系统 (Jump)
- 可变跳跃高度（按住跳更高）
- 土狼时间 (Coyote Time) - 离地后仍可跳跃的短暂时间
- 跳跃缓冲 (Jump Buffer) - 提前按键的容错时间
- 多段跳（二段跳、三段跳等）
- 蹬墙跳

### 墙壁交互 (Wall)
- 墙壁滑行
- 蹬墙跳（带输入锁定）
- 墙壁攀爬

### 下蹲系统 (Crouch)
- 动态调整碰撞体大小
- 下蹲移动
- 头顶空间检测

## 文件结构

```
Scripts/
├── PlayerController2D.cs       # 核心控制器
├── PlayerInputManager.cs       # 输入管理器
├── PlayerCameraController.cs   # 相机跟随控制器
├── PlayerAudioController.cs    # 音效控制器
└── ClimbableSurface.cs         # 可攀爬表面组件
```

## 快速开始

### 1. 设置玩家对象

创建一个 2D 对象并添加以下组件：

```
GameObject (Player)
├── Rigidbody2D
│   ├── Gravity Scale: 3
│   ├── Constraints: Freeze Rotation Z
│   └── Collision Detection: Continuous
├── BoxCollider2D
│   ├── Size: (0.8, 1.8)
│   └── Offset: (0, 0)
├── SpriteRenderer
└── PlayerController2D (脚本)
```

### 2. 配置图层

在 Unity 的 Layer 设置中创建以下图层：
- `Ground` - 用于地面检测
- `Wall` - 用于墙壁检测
- `Climbable` - 用于可攀爬表面

### 3. 设置输入

在 Unity Input Manager 中添加以下输入轴：

| 名称 | 类型 | 按钮/轴 |
|------|------|---------|
| Horizontal | Key/Axis | A/D, Left/Right Arrow |
| Vertical | Key/Axis | W/S, Up/Down Arrow |
| Jump | Button | Space |
| Dash | Button | Left Shift |
| Crouch | Button | Left Ctrl |

### 4. 配置层级设置

在 PlayerController2D 组件中：
- **Ground Layer**: 设置为 Ground 图层
- **Wall Layer**: 设置为 Wall 图层
- **Climbable Layer**: 设置为 Climbable 图层

### 5. 设置相机

1. 创建一个 Camera 对象
2. 添加 `PlayerCameraController` 脚本
3. 将玩家对象拖拽到 Target 字段

## 参数说明

### 移动参数

| 参数 | 说明 | 推荐值 |
|------|------|--------|
| Walk Speed | 行走速度 | 5 |
| Run Speed | 奔跑速度 | 8 |
| Ground Acceleration | 地面加速度 | 15 |
| Ground Deceleration | 地面减速度 | 20 |
| Air Control Factor | 空中控制系数 | 0.6 |

### 冲刺参数

| 参数 | 说明 | 推荐值 |
|------|------|--------|
| Dash Speed | 冲刺速度 | 20 |
| Dash Duration | 冲刺持续时间 | 0.15 |
| Dash Cooldown | 冲刺冷却时间 | 0.5 |
| Air Dash Count | 空中冲刺次数 | 1 |

### 攀爬参数

| 参数 | 说明 | 推荐值 |
|------|------|--------|
| Climb Speed | 攀爬速度 | 4 |
| Climb Stamina | 最大体力 | 10 |
| Stamina Regen Rate | 体力恢复速度 | 2 |

### 跳跃参数

| 参数 | 说明 | 推荐值 |
|------|------|--------|
| Jump Force | 跳跃力度 | 14 |
| Jump Hold Force | 按住跳跃附加力 | 3 |
| Coyote Time | 土狼时间 | 0.15 |
| Jump Buffer Time | 跳跃缓冲时间 | 0.1 |
| Max Jump Count | 最大跳跃次数 | 2 |

## 使用示例

### 创建可攀爬的梯子

1. 创建一个空 GameObject
2. 添加 `BoxCollider2D`（设置为 Trigger）
3. 添加 `ClimbableSurface` 脚本
4. 设置图层为 `Climbable`

```csharp
// 代码示例：动态创建可攀爬表面
GameObject ladder = new GameObject("Ladder");
ladder.layer = LayerMask.NameToLayer("Climbable");

var collider = ladder.AddComponent<BoxCollider2D>();
collider.isTrigger = true;
collider.size = new Vector2(1, 5);

ladder.AddComponent<ClimbableSurface>();
```

### 访问控制器状态

```csharp
PlayerController2D player = GetComponent<PlayerController2D>();

// 检查状态
bool isGrounded = player.IsGrounded;
bool isDashing = player.IsDashing;
bool isClimbing = player.IsClimbing;
Vector2 velocity = player.Velocity;

// 外部控制
player.ResetDash();           // 重置冲刺
player.RestoreStamina(5f);    // 恢复体力
player.AddForce(Vector2.up * 10); // 施加力
```

### 相机控制

```csharp
PlayerCameraController camera = GetComponent<PlayerCameraController>();

// 缩放控制
camera.SetZoom(8f);           // 设置缩放
camera.ApplyDashZoom(true);   // 冲刺缩放

// 屏幕震动
camera.Shake(1f, 0.5f);       // 震动强度1，持续0.5秒

// 边界控制
camera.SetBounds(new Vector2(-50, -20), new Vector2(50, 20));
camera.ClearBounds();         // 清除边界
```

## 动画集成

控制器会更新 Animator 的以下参数：

| 参数名 | 类型 | 说明 |
|--------|------|------|
| VelocityX | Float | 水平速度 |
| VelocityY | Float | 垂直速度 |
| IsGrounded | Bool | 是否在地面上 |
| IsCrouching | Bool | 是否下蹲 |
| IsDashing | Bool | 是否冲刺 |
| IsClimbing | Bool | 是否攀爬 |
| IsWallSliding | Bool | 是否墙壁滑行 |
| StaminaPercent | Float | 体力百分比 |

## 最佳实践

### 性能优化

1. **使用对象池**: 对于频繁生成的对象（如残影、粒子）
2. **减少物理检测**: 合理设置检测间隔
3. **使用 sqrMagnitude**: 当比较距离平方足够时

### 移动手感调优

1. **土狼时间**: 0.1-0.2 秒提供最佳手感
2. **跳跃缓冲**: 0.1 秒足以覆盖大多数输入延迟
3. **空中控制**: 0.5-0.7 的系数提供平衡的控制感
4. **摩擦力**: 与减速度配合，确保停止自然

### 设计建议

1. **冲刺**: 冲刺距离 = 速度 × 时间
   - 快速冲刺: 高速度、短时间
   - 长距离冲刺: 低速度、长时间

2. **体力系统**: 
   - 攀爬速度与体力消耗成正比
   - 确保有足够的恢复时间

3. **相机跟随**:
   - 死区大小约为屏幕 1/6
   - 视界预测距离 2-3 单位

## 故障排除

### 穿过地面

- 增加 `Ground Check Size`
- 调整 `Ground Check Offset`
- 检查 `Rigidbody2D` 碰撞检测模式

### 跳跃手感不佳

- 调整 `Coyote Time` (0.1-0.2)
- 调整 `Jump Buffer Time` (0.05-0.15)
- 检查 `Jump Hold Force` 和 `Jump Hold Duration`

### 冲刺不工作

- 检查 `CanDash()` 返回值
- 确认 `Air Dash Count` 设置正确
- 验证 `Dash Cooldown` 不为 0

### 攀爬不工作

- 确认对象在 `Climbable` 图层
- 检查 `ClimbableSurface` 组件存在
- 验证 Collider 设置为 Trigger

## 扩展开发

### 添加新移动能力

```csharp
// 在 PlayerController2D 中添加
[Header("新能力")]
[SerializeField] private float newAbilityValue = 10f;

private void HandleNewAbility()
{
    // 实现逻辑
}

private void Update()
{
    // 在现有方法后调用
    HandleNewAbility();
}
```

### 事件系统集成

```csharp
// 定义事件
public event System.Action OnDashStart;
public event System.Action OnDashEnd;

// 触发事件
private IEnumerator PerformDash()
{
    OnDashStart?.Invoke();
    // ... 冲刺逻辑
    OnDashEnd?.Invoke();
}
```

## 版本历史

- **v1.0.0** (2026-04-10)
  - 初始版本
  - 冲刺、攀爬、跳跃系统
  - 相机跟随和音效控制

## 许可

MIT License

## 贡献

欢迎提交 Issue 和 Pull Request！
