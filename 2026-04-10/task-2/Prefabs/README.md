# Prefabs 设置指南

## 玩家预制体 (Player.prefab)

### 组件配置

#### Rigidbody2D
```
Body Type: Dynamic
Mass: 1
Linear Drag: 0
Angular Drag: 0.05
Gravity Scale: 3
Material: None
Simulated: ✓
Use Full Kinematic Contacts: ✗
Collision Detection: Continuous
Constraints: Freeze Rotation Z
```

#### BoxCollider2D
```
Size: (0.8, 1.8)
Offset: (0, 0)
Edge Radius: 0.05
Density: 1
Is Trigger: ✗
Used By Effector: ✗
Used By Composite: ✗
```

#### SpriteRenderer
```
Sprite: [玩家精灵]
Color: White
Material: Default - Sprite (Default)
Drawing Mode: Simple
Mask Interaction: None
Sprite Sort Point: Center
```

#### PlayerController2D
```
[基础移动]
Walk Speed: 5
Run Speed: 8
Ground Acceleration: 15
Ground Deceleration: 20
Air Control Factor: 0.6
Air Acceleration: 10
Air Deceleration: 5
Friction: 10

[冲刺设置]
Dash Speed: 20
Dash Duration: 0.15
Dash Cooldown: 0.5
Air Dash Count: 1
Dash End Lag: 0.1
Dash Ignore Gravity: ✓

[攀爬设置]
Climb Speed: 4
Climbable Layer: Climbable
Climb Gravity Scale: 0.1
Climb Stamina: 10
Stamina Regen Rate: 2
Stamina Exhausted Cooldown: 2

[跳跃设置]
Jump Force: 14
Jump Hold Force: 3
Jump Hold Duration: 0.2
Coyote Time: 0.15
Jump Buffer Time: 0.1
Max Jump Count: 2
Double Jump Force: 12

[墙壁交互]
Wall Slide Speed: 2
Wall Jump Horizontal Force: 10
Wall Jump Vertical Force: 12
Wall Jump Input Lock Time: 0.2
Wall Layer: Wall

[下蹲设置]
Crouch Speed: 2.5
Crouch Collider Size: (0.8, 1)
Crouch Collider Offset: (0, -0.4)
Stand Collider Size: (0.8, 1.8)
Stand Collider Offset: (0, 0)

[检测设置]
Ground Layer: Ground
Ground Check Size: (0.6, 0.1)
Ground Check Offset: (0, -0.9)
Wall Check Size: (0.1, 0.8)
Wall Check Offset: (0.5, 0.2)
Ledge Check Offset: (0.4, -0.5)
Ledge Check Radius: 0.2

[视觉效果]
Afterimage Interval: 0.02
Afterimage Lifetime: 0.3
Afterimage Color: (1, 1, 1, 0.5)
```

## 相机预制体 (PlayerCamera.prefab)

### 组件配置

#### Camera
```
Clear Flags: Solid Color
Background: (0.1, 0.1, 0.15, 1)
Culling Mask: Everything
Orthographic: ✓
Orthographic Size: 5
Depth: -1
Rendering Path: Use Player Settings
Target Texture: None
Occlusion Culling: ✓
HDR: ✗
MSAA: Disabled
Dynamic Resolution: ✗
Target Display: 0
```

#### PlayerCameraController
```
[目标设置]
Target: [玩家对象引用]
Follow Offset: (0, 0, -10)

[平滑设置]
Position Smooth Speed: 3
Use Damping: ✓
Damping Factor: 0.15

[死区设置]
Use Dead Zone: ✓
Dead Zone Size: (2, 1.5)

[视界预测]
Use Look Ahead: ✓
Look Ahead Distance: 2
Look Ahead Smooth Speed: 5

[边界限制]
Use Bounds: ✗
Bounds Min: (-50, -20)
Bounds Max: (50, 20)

[缩放设置]
Allow Zoom: ✓
Base Zoom: 5
Zoom Smooth Speed: 3

[冲刺效果]
Dash FOV Increase: 1
Dash FOV Speed: 5

[屏幕震动]
Shake Intensity: 0.5
Shake Duration: 0.3
Shake Decay: 5
```

## 可攀爬表面预制体

### 梯子 (Ladder.prefab)

```
GameObject: Ladder
├── Transform
│   ├── Position: (0, 0, 0)
│   ├── Rotation: (0, 0, 0)
│   └── Scale: (1, 1, 1)
├── BoxCollider2D
│   ├── Is Trigger: ✓
│   ├── Size: (1, 5)
│   └── Offset: (0, 0)
├── SpriteRenderer (可选)
│   ├── Sprite: [梯子精灵]
│   └── Sorting Layer: Default
└── ClimbableSurface
    ├── Climb Speed Multiplier: 1
    ├── Allow Horizontal Climb: ✗
    └── Stamina Drain Multiplier: 1
```

### 藤蔓 (Vine.prefab)

```
GameObject: Vine
├── Transform
├── BoxCollider2D (Trigger)
├── SpriteRenderer
└── ClimbableSurface
    ├── Climb Speed Multiplier: 0.7  // 藤蔓爬得慢一点
    ├── Allow Horizontal Climb: ✗
    └── Stamina Drain Multiplier: 1.2  // 藤蔓消耗更多体力
```

## 场景设置建议

### 图层配置

在 Edit -> Project Settings -> Tags and Layers 中添加：

| Layer | 编号 | 用途 |
|-------|------|------|
| Default | 0 | 默认对象 |
| Ground | 6 | 地面、平台 |
| Wall | 7 | 墙壁 |
| Climbable | 8 | 梯子、藤蔓 |
| Player | 9 | 玩家 |
| Enemy | 10 | 敌人 |

### 物理设置 (Physics2D)

在 Edit -> Project Settings -> Physics 2D 中：

```
Material: [可选 - 创建摩擦力材质]
├── Friction: 0.6
└── Bounciness: 0

Gravity: (0, -9.81)

Default Material: None

Velocity Iterations: 8
Position Iterations: 3
```

### 输入管理器 (Input Manager)

在 Edit -> Project Settings -> Input Manager 中添加：

| 名称 | 轴 | 正向按钮 | 负向按钮 | 死区 | 灵敏度 | 类型 |
|------|-----|----------|----------|------|--------|------|
| Horizontal | X轴 | d | a | 0.001 | 1000 | Key/MouseButton |
| Vertical | Y轴 | w | s | 0.001 | 1000 | Key/MouseButton |
| Jump | - | space | - | 0.001 | 1000 | Key/MouseButton |
| Dash | - | left shift | - | 0.001 | 1000 | Key/MouseButton |
| Crouch | - | left ctrl | - | 0.001 | 1000 | Key/MouseButton |
| Interact | - | e | - | 0.001 | 1000 | Key/MouseButton |
| Pause | - | escape | - | 0.001 | 1000 | Key/MouseButton |

## 动画控制器设置

### Animator Parameters

创建以下参数：

| 名称 | 类型 | 说明 |
|------|------|------|
| VelocityX | Float | 水平速度 |
| VelocityY | Float | 垂直速度 |
| IsGrounded | Bool | 地面状态 |
| IsCrouching | Bool | 下蹲状态 |
| IsDashing | Bool | 冲刺状态 |
| IsClimbing | Bool | 攀爬状态 |
| IsWallSliding | Bool | 墙壁滑行 |
| StaminaPercent | Float | 体力百分比 |

### 推荐动画状态机结构

```
[Entry]
    ↓
[Idle] ←→ [Run] (基于 VelocityX)
    ↓
[Jump] (当 !IsGrounded)
    ↓
[Fall] (当 VelocityY < 0)
    ↓
[Land] (当 IsGrounded 变为 true)

[Wall Slide] (当 IsWallSliding)

[Climb Idle] ←→ [Climb Move] (当 IsClimbing)

[Dash] (当 IsDashing)

[Crouch Idle] ←→ [Crouch Walk] (当 IsCrouching)
```
