using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主菜单控制器 - 处理游戏主菜单的显示和交互
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Animation")]
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private float fadeInDuration = 1.0f;
    
    private void Awake()
    {
        // 绑定按钮事件
        startButton.onClick.AddListener(OnStartGame);
        settingsButton.onClick.AddListener(OnOpenSettings);
        quitButton.onClick.AddListener(OnQuitGame);
        
        // 初始淡入效果
        StartCoroutine(FadeInMenu());
    }
    
    /// <summary>
    /// 淡入菜单动画
    /// </summary>
    private System.Collections.IEnumerator FadeInMenu()
    {
        float elapsedTime = 0f;
        menuCanvasGroup.alpha = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            menuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        menuCanvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// 开始游戏
    /// </summary>
    private void OnStartGame()
    {
        Debug.Log("开始游戏");
        // 这里可以添加场景切换逻辑
        // SceneManager.LoadScene("GameScene");
    }
    
    /// <summary>
    /// 打开设置菜单
    /// </summary>
    private void OnOpenSettings()
    {
        Debug.Log("打开设置菜单");
        // 通知UI管理器切换到设置菜单
        UIManager.Instance.ShowSettingsMenu();
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    private void OnQuitGame()
    {
        Debug.Log("退出游戏");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}