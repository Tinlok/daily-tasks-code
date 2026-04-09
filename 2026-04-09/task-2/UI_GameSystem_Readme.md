# Unity 游戏基础UI系统

这个项目实现了一个完整的Unity游戏基础UI系统，包括主菜单、暂停菜单和设置界面。

## 功能特性

### 1. 主菜单系统 (MainMenuController.cs)
- 新游戏按钮 - 创建新游戏，清除所有存档数据
- 继续游戏按钮 - 加载最近的存档（如果没有存档则禁用）
- 设置按钮 - 进入设置界面
- 退出游戏按钮 - 退出游戏（编辑器中停止播放）

### 2. 暂停菜单系统 (PauseMenuController.cs)
- ESC键快捷暂停/继续
- 继续游戏按钮 - 恢复游戏时间
- 重新开始按钮 - 重新加载当前关卡
- 返回主菜单按钮 - 退出到主菜单
- 设置按钮 - 进入暂停中的设置界面

### 3. 设置菜单系统 (SettingsMenuController.cs)
- 视频设置：分辨率、画质、全屏、垂直同步、亮度
- 音频设置：主音量、音乐音量、音效音量、静音
- 控制设置：控制方案选择、按键绑定
- 语言设置：多语言支持

### 4. 统一UI管理器 (UIManager.cs)
- 集中管理所有UI面板的显示/隐藏
- 游戏状态UI：分数、生命值、关卡、血条
- 场景管理功能
- UI动画效果（淡入淡出）

## 文件结构

```
2026-04-09/task-2/
├── MainMenuController.cs      # 主菜单控制器
├── PauseMenuController.cs     # 暂停菜单控制器  
├── SettingsMenuController.cs # 设置菜单控制器
├── UIManager.cs              # 统一UI管理器
├── README.md                 # 说明文档
└── Unity scenes/             # 需要创建的Unity场景（示例）
    ├── MainMenu.unity        # 主菜单场景
    ├── Game.unity            # 游戏场景
    └── Settings.unity       # 设置场景
```

## 安装和使用说明

### 1. 创建Unity场景
- 创建三个空场景：MainMenu、Game、Settings
- 将相应的预制体或UI对象添加到场景中

### 2. 设置UI对象
为每个场景配置相应的UI对象：

**MainMenu场景：**
- MainMenuController组件
- 主菜单面板（包含按钮）
- 设置面板（可选）

**Game场景：**
- UIManager组件
- 游戏UI面板（分数、生命值等）
- PauseMenuPanel（暂停菜单）
- 挂载PauseMenuController的对象

**Settings场景：**
- SettingsMenuController组件
- 所有设置相关的UI元素

### 3. 配置UI元素
在Inspector中为每个控制器设置相应的UI元素引用：
- Buttons
- Texts
- Sliders
- Toggles
- Dropdowns
- Images

### 4. 场景配置
确保所有场景都正确设置：
- 设置场景构建设置（File > Build Settings）
- 添加场景到构建设置列表
- 设置正确的场景名称

## 使用示例

### 主菜单使用
```csharp
// 在主菜单按钮的OnClick事件中调用
MainMenuController mainMenuController = FindObjectOfType<MainMenuController>();
mainMenuController.OnNewGame();    // 开始新游戏
mainMenuController.OnContinue();   // 继续游戏
mainMenuController.OnSettings();   // 打开设置
mainMenuController.OnQuit();       // 退出游戏
```

### 暂停菜单使用
```csharp
// 在游戏脚本中检测ESC键
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        PauseMenuController pauseMenu = FindObjectOfType<PauseMenuController>();
        pauseMenu.PauseGame();
    }
}

// 在暂停菜单按钮的OnClick事件中调用
PauseMenuController pauseMenuController = FindObjectOfType<PauseMenuController>();
pauseMenuController.ResumeGame();    // 继续游戏
pauseMenuController.RestartLevel();  // 重新开始关卡
pauseMenuController.ReturnToMainMenu(); // 返回主菜单
```

### UI管理器使用
```csharp
// 显示主菜单
UIManager.Instance.ShowMainMenu();

// 显示游戏UI
UIManager.Instance.ShowGameUI();

// 显示暂停菜单
UIManager.Instance.ShowPauseMenu();

// 更新游戏UI
UIManager.Instance.UpdateScore(100);
UIManager.Instance.UpdateLives(3);
UIManager.Instance.UpdateLevel(2);
UIManager.Instance.UpdateHealth(0.8f);
```

### 设置菜单使用
```csharp
// 在设置按钮的OnClick事件中调用
SettingsMenuController settingsController = FindObjectOfType<SettingsMenuController>();
settingsController.OpenSettings();

// 返回按钮
settingsController.OnBackPressed();
```

## 扩展功能

### 1. 添加新的UI面板
- 在UIManager中添加新的面板引用
- 创建对应的显示/隐藏方法
- 添加动画效果支持

### 2. 自定义设置项
- 在SettingsMenuController中添加新的设置控件
- 扩展SaveSettings和LoadSettings方法
- 添加ApplySettings方法来应用新设置

### 3. 多语言支持
- 创建语言资源文件
- 在SettingsMenuController中实现语言切换逻辑
- 使用Localization工具包（可选）

### 4. 主题支持
- 创建主题配置文件
- 实现主题切换功能
- 支持颜色方案和字体设置

## 性能优化

### 1. 对象池
- 为频繁创建/销毁的UI对象使用对象池
- 减少GC压力

### 2. 异步加载
- 使用Addressables异步加载UI资源
- 减少启动时间

### 3. 图集优化
- 将小图标打包成图集
- 减少Draw Call

## 兼容性

- Unity 2020.3 或更高版本
- 支持PC和移动平台
- 支持主流输入设备（键盘、鼠标、手柄）

## 注意事项

1. 确保所有场景都有正确的Canvas组件
2. UI对象需要有合适的Canvas Group组件以支持透明度动画
3. 音频设置需要配合AudioMixer使用以获得更好的效果
4. 控制绑定功能需要实现完整的输入映射系统

## 故障排除

### 常见问题
1. UI面板不显示 - 检查Canvas和UI组件设置
2. 按钮无响应 - 检查Button组件和OnClick事件
3. 场景切换卡顿 - 使用异步加载场景
4. 设置不保存 - 确保PlayerPrefs.Save()被调用

### 调试建议
1. 使用Unity Profiler分析UI性能
2. 使用Debug.Log检查设置保存状态
3. 手动测试所有UI流程
4. 检查控制台是否有错误信息

---

*这个UI系统是一个基础框架，可以根据具体游戏需求进行扩展和定制。*