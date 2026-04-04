# PlayerController2D 增强版 - 任务 2

## 任务概述
扩展现有的 PlayerController2D，添加冲刺+攀爬机制，完善移动手感。

## 实现的功能

### 1. 冲刺机制
- **功能**：按住 Shift 键进行冲刺
- **效果**：
  - 移动速度提升 60%
  - 加速度和减速度优化
  - 消耗耐力值
  - 耐力耗尽时自动停止冲刺
- **耐力系统**：
  - 最大耐力值：100
  - 冲刺消耗：20/秒
  - 耐力恢复：10/秒
  - 停止冲刺后延迟1秒开始恢复

### 2. 攀爬机制
- **功能**：在特定墙面上进行攀爬
- **效果**：
  - 按 W/S 键控制上下攀爬
  - 持续消耗耐力
  - 水平移动速度减半
  - 离开墙面或耐力耗尽时停止攀爬
- **攀爬检测**：
  - 使用射线检测可攀爬表面
  - 需要自定义 ClimbableSurface 组件标记

### 3. 移动手感优化
- **物理效果**：
  - 加速度和减速度系统
  - 地面和空气摩擦力
  - 跳跃高度控制
- **状态系统**：
  - 站立、行走、冲刺、攀爬、跳跃、下落
  - 完整的状态机支持
- **动画系统**：
  - 根据移动状态播放相应动画
  - 支持朝向翻转
  - 速度和状态参数同步

### 4. 额外功能
- **Coyote Time**：离开地面后仍可跳跃一段时间
- **跳跃缓冲**：按跳跃键后仍可跳跃
- **跳跃控制**：按住跳跃键跳得更高
- **二段跳**：支持空中的额外跳跃
- **事件系统**：各种动作都触发相应事件

## 文件说明

### 核心文件
- `PlayerController2D_Enhanced.cs` - 主要的玩家控制器
- `ClimbableSurface.cs` - 可攀爬表面标记
- `GroundCheck.cs` - 地面检测组件

### 依赖组件
- `Rigidbody2D` - 2D刚体
- `Animator` - 动画控制器
- `PlayerState` - 玩家状态数据
- `PlayerInput` - 玩家输入处理
- `PlayerStats` - 玩家属性（生命值、耐力等）

## 使用方法

### 1. 设置场景
1. 创建一个 2D 玩家对象
2. 添加 `PlayerController2D_Enhanced` 组件
3. 创建一个空的子对象作为地面检测点
4. 添加 `GroundCheck` 组件到玩家对象

### 2. 配置地面
1. 创建地面对象
2. 添加 `Collider2D` 组件
3. 设置图层为地面层
4. 在 `GroundCheck` 中配置地面检测

### 3. 配置可攀爬表面
1. 创建墙面对象
2. 添加 `ClimbableSurface` 组件
3. 设置攀爬参数
4. 配置碰撞体为触发器

### 4. 设置输入系统
- 使用 Unity 的输入系统
- 水平移动：A/D 或 左/右箭头
- 垂直移动：W/S 或 上/下箭头
- 跳跃：空格键
- 冲刺：左/右 Shift 键

### 5. 设置动画
1. 创建动画控制器
2. 添加相应动画状态
3. 配置动画参数：
   - `IsGrounded` - 是否在地面上
   - `IsMoving` - 是否在移动
   - `IsSprinting` - 是否在冲刺
   - `IsClimbing` - 是否在攀爬
   - `MoveSpeed` - 移动速度
   - `VerticalVelocity` - 垂直速度
   - `FacingDirection` - 朝向

## 参数配置

### 移动参数
- `moveSpeed` - 基础移动速度
- `acceleration` - 加速度
- `deceleration` - 减速度
- `groundFriction` - 地面摩擦力
- `airFriction` - 空气摩擦力
- `maxFallSpeed` - 最大下落速度
- `gravityScale` - 重力缩放

### 跳跃参数
- `jumpForce` - 跳跃力度
- `jumpBufferTime` - 跳跃缓冲时间
- `coyoteTime` - Coyote Time 时间
- `maxJumps` - 最大跳跃次数
- `jumpCutMultiplier` - 跳跃切割倍数
- `jumpHoldMultiplier` - 跳跃保持倍数

### 冲刺参数
- `sprintSpeedMultiplier` - 冲刺速度倍数
- `sprintAcceleration` - 冲刺加速度
- `sprintDeceleration` - 冲刺减速度
- `sprintStaminaCost` - 冲刺耐力消耗
- `sprintStaminaRegen` - 冲刺耐力恢复
- `maxStamina` - 最大耐力值

### 攀爬参数
- `climbSpeed` - 攀爬速度
- `climbStaminaCost` - 攀爬耐力消耗
- `climbDistance` - 攀爬距离
- `climbableLayers` - 可攀爬图层
- `climbRaycastDistance` - 攀爬射线检测距离

## 事件系统

### 可用事件
- `onJump` - 跳跃时触发
- `onLand` - 着陆时触发
- `onStartSprint` - 开始冲刺时触发
- `onEndSprint` - 结束冲刺时触发
- `onStartClimb` - 开始攀爬时触发
- `onEndClimb` - 结束攀爬时触发
- `onStaminaChanged` - 耐力值变化时触发

### 事件使用示例
```csharp
public class PlayerEffects : MonoBehaviour
{
    private PlayerController2D_Enhanced playerController;
    
    private void Start()
    {
        playerController = GetComponent<PlayerController2D_Enhanced>();
        
        // 订阅事件
        playerController.onJump += OnPlayerJump;
        playerController.onStartSprint += OnStartSprint;
        playerController.onStaminaChanged += OnStaminaChanged;
    }
    
    private void OnPlayerJump()
    {
        // 播放跳跃音效
        AudioManager.Play("Jump");
        
        // 创建粒子效果
        ParticleSystem jumpEffect = Instantiate(JumpEffect, transform.position);
        jumpEffect.Play();
    }
    
    private void OnStartSprint()
    {
        // 开始冲刺音效
        AudioManager.Play("SprintStart");
    }
    
    private void OnStaminaChanged()
    {
        // 更新耐力条UI
        StaminaBar.UpdateStamina(playerController.GetCurrentStamina());
    }
}
```

## 性能优化

### 1. 物理优化
- 使用插值刚体运动
- 连续碰撞检测
- 优化的摩擦力计算

### 2. 内存优化
- 对象池化（如果需要）
- 事件监听器管理

### 3. 渲染优化
- 动画状态机优化
- 条件渲染

## 调试功能

### 1. 调试可视化
- 地面检测范围显示
- 攀爬检测射线显示
- 状态指示器

### 2. 日志输出
- 状态变化日志
- 耐力变化日志
- 错误和警告日志

## 扩展建议

### 1. 新功能扩展
- 冲刺攻击
- 空中冲刺
- 墙面跳跃
- 滑翔
- 冲刺蓄力

### 2. 视觉效果
- 冲刺尾迹效果
- 攀爬粒子效果
- 状态变化过渡动画

### 3. 音效系统
- 步行音效
- 跳跃音效
- 冲刺音效
- 攀爬音效

### 4. 输入系统
- 自定义按键绑定
- 手柄支持
- 触摸屏支持

## 已知问题

1. **攀爬检测**：在某些复杂墙面可能检测不准确
2. **物理冲突**：快速移动时可能与碰撞体产生穿透
3. **动画过渡**：状态切换时动画过渡可能不够平滑

## 更新日志

### v1.0.0 (2026-04-04)
- 初始版本发布
- 实现基础冲刺功能
- 实现攀爬功能
- 优化移动手感
- 添加完整事件系统

## 许可证
MIT License - 可以自由使用和修改