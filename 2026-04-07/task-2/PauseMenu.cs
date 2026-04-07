using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 暂停菜单控制器 - 处理游戏暂停时的菜单显示和交互
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitToMenuButton;
    [SerializeField] private Button quitGameButton;
    
    [Header("Animation")]
    [SerializeField] private CanvasGroup pauseCanvasGroup;
    [SerializeField] private float fadeInDuration = 0.3f;
    
    private bool isPaused = false;
    
    private void Awake()
    {
        // 绑定按钮事件
        resumeButton.onClick.AddListener(OnResume);
        restartButton.onClick.AddListener(OnRestart);
        settingsButton.onClick.AddListener(OnOpenSettings);
        quitToMenuButton.onClick.AddListener(OnQuitToMenu);
        quitGameButton.onClick.AddListener(OnQuitGame);
        
        // 初始隐藏菜单
        pauseCanvasGroup.alpha = 0f;
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;
    }
    
    private void Update()
    {
        // 监听ESC键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                OnResume();
            }
            else
            {
                OnPause();
            }
        }
    }
    
    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void OnPause()
    {
        isPaused = true;
        Time.timeScale = 0f; // 暂停游戏时间
        
        // 显示暂停菜单
        StartCoroutine(FadeInPauseMenu());
        
        // 防止玩家在暂停时移动
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.SetActive(false);
        }
    }
    
    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void OnResume()
    {
        isPaused = false;
        Time.timeScale = 1f; // 恢复游戏时间
        
        // 隐藏暂停菜单
        StartCoroutine(FadeOutPauseMenu());
        
        // 重新激活玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.SetActive(true);
        }
    }
    
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    private void OnRestart()
    {
        Debug.Log("重新开始游戏");
        Time.timeScale = 1f;
        // 这里可以添加重新开始逻辑
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// 打开设置菜单
    /// </summary>
    private void OnOpenSettings()
    {
        Debug.Log("打开设置菜单");
        UIManager.Instance.ShowSettingsMenu();
    }
    
    /// <summary>
    /// 退出到主菜单
    /// </summary>
    private void OnQuitToMenu()
    {
        Debug.Log("退出到主菜单");
        Time.timeScale = 1f;
        // 这里可以添加场景切换逻辑
        // SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    private void OnQuitGame()
    {
        Debug.Log("退出游戏");
        Time.timeScale = 1f;
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// 淡入暂停菜单
    /// </summary>
    private System.Collections.IEnumerator FadeInPauseMenu()
    {
        float elapsedTime = 0f;
        pauseCanvasGroup.interactable = true;
        pauseCanvasGroup.blocksRaycasts = true;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            pauseCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        pauseCanvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// 淡出暂停菜单
    /// </summary>
    private System.Collections.IEnumerator FadeOutPauseMenu()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            pauseCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        pauseCanvasGroup.alpha = 0f;
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;
    }
}