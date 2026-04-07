using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UI管理器 - 负责管理游戏中的所有UI面板和界面切换
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;
    [SerializeField] private GameObject gameHudPanel;
    
    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.3f;
    
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 初始化UI
        InitializeUI();
    }
    
    private void Start()
    {
        // 根据当前场景显示相应的UI
        UpdateSceneUI();
    }
    
    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitializeUI()
    {
        // 确保所有面板初始状态为隐藏
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);
        if (gameHudPanel != null) gameHudPanel.SetActive(false);
        
        // 获取场景名称
        string currentScene = SceneManager.GetActiveScene().name;
        
        // 根据场景显示相应的UI
        switch (currentScene)
        {
            case "MainMenu":
                ShowMainMenu();
                break;
            case "Game":
                ShowGameHud();
                break;
            default:
                // 默认显示主菜单
                ShowMainMenu();
                break;
        }
    }
    
    /// <summary>
    /// 更新场景UI
    /// </summary>
    private void UpdateSceneUI()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        switch (currentScene)
        {
            case "MainMenu":
                ShowMainMenu();
                break;
            case "Game":
                ShowGameHud();
                break;
        }
    }
    
    /// <summary>
    /// 显示主菜单
    /// </summary>
    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            StartCoroutine(FadeInPanel(mainMenuPanel));
        }
    }
    
    /// <summary>
    /// 显示暂停菜单
    /// </summary>
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            StartCoroutine(FadeInPanel(pauseMenuPanel));
        }
    }
    
    /// <summary>
    /// 显示设置菜单
    /// </summary>
    public void ShowSettingsMenu()
    {
        // 隐藏当前面板，显示设置菜单
        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
        {
            StartCoroutine(SwitchPanel(mainMenuPanel, settingsMenuPanel));
        }
        else if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            StartCoroutine(SwitchPanel(pauseMenuPanel, settingsMenuPanel));
        }
        else if (gameHudPanel != null && gameHudPanel.activeSelf)
        {
            StartCoroutine(SwitchPanel(gameHudPanel, settingsMenuPanel));
        }
    }
    
    /// <summary>
    /// 显示游戏HUD
    /// </summary>
    public void ShowGameHud()
    {
        HideAllPanels();
        if (gameHudPanel != null)
        {
            gameHudPanel.SetActive(true);
            StartCoroutine(FadeInPanel(gameHudPanel));
        }
    }
    
    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);
        if (gameHudPanel != null) gameHudPanel.SetActive(false);
    }
    
    /// <summary>
    /// 淡入面板
    /// </summary>
    private System.Collections.IEnumerator FadeInPanel(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        
        float elapsedTime = 0f;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// 切换面板
    /// </summary>
    private System.Collections.IEnumerator SwitchPanel(GameObject fromPanel, GameObject toPanel)
    {
        // 淡出当前面板
        CanvasGroup fromCanvasGroup = fromPanel.GetComponent<CanvasGroup>();
        if (fromCanvasGroup == null)
        {
            fromCanvasGroup = fromPanel.AddComponent<CanvasGroup>();
        }
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fromCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        
        fromPanel.SetActive(false);
        
        // 淡入新面板
        if (toPanel != null)
        {
            toPanel.SetActive(true);
            yield return StartCoroutine(FadeInPanel(toPanel));
        }
    }
    
    /// <summary>
    /// 场景加载完成后的回调
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateSceneUI();
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// 清理UI
    /// </summary>
    public void CleanupUI()
    {
        Instance = null;
        Destroy(gameObject);
    }
}