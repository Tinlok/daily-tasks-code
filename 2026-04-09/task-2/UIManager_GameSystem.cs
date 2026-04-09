using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("UI Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text levelText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image crosshair;

    private static UIManager instance;
    public static UIManager Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeUI();
        HideAllPanels();
        ShowMainMenu();
    }

    private void InitializeUI()
    {
        // 初始化游戏UI
        if (scoreText != null)
        {
            scoreText.text = "Score: 0";
        }

        if (livesText != null)
        {
            livesText.text = "Lives: 3";
        }

        if (levelText != null)
        {
            levelText.text = "Level: 1";
        }

        if (healthBar != null)
        {
            healthBar.value = 1f;
        }

        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(false);
        }
    }

    // 主菜单相关方法
    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        Time.timeScale = 0f; // 暂停游戏时间
    }

    public void HideMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }

    // 游戏UI相关方法
    public void ShowGameUI()
    {
        HideAllPanels();
        if (gameUIPanel != null)
        {
            gameUIPanel.SetActive(true);
        }
        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(true);
        }
        Time.timeScale = 1f; // 恢复游戏时间
    }

    public void HideGameUI()
    {
        if (gameUIPanel != null)
        {
            gameUIPanel.SetActive(false);
        }
        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(false);
        }
    }

    // 暂停菜单相关方法
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
        Time.timeScale = 0f; // 暂停游戏时间
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    // 设置面板相关方法
    public void ShowSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void HideSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // 游戏结束相关方法
    public void ShowGameOver()
    {
        HideAllPanels();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f; // 暂停游戏时间
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // 胜利相关方法
    public void ShowVictory()
    {
        HideAllPanels();
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        Time.timeScale = 0f; // 暂停游戏时间
    }

    public void HideVictory()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    // 隐藏所有面板
    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    // 更新游戏UI数据
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + lives.ToString();
        }
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + level.ToString();
        }
    }

    public void UpdateHealth(float health)
    {
        if (healthBar != null)
        {
            healthBar.value = Mathf.Clamp01(health);
        }
    }

    // 场景切换方法
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f; // 恢复时间缩放
        SceneManager.LoadScene(sceneName);
    }

    public void RestartCurrentScene()
    {
        Time.timeScale = 1f; // 恢复时间缩放
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // 临时显示消息（用于游戏中的提示信息）
    public void ShowMessage(string message, float duration = 2f)
    {
        StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    private System.Collections.IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        // 这里可以创建一个临时的消息UI
        Debug.Log("Message: " + message);
        yield return new WaitForSecondsRealtime(duration);
    }

    // 动画效果相关方法
    public void FadeIn(GameObject target, float duration = 0.5f)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        StartCoroutine(FadeCoroutine(canvasGroup, 0f, 1f, duration));
    }

    public void FadeOut(GameObject target, float duration = 0.5f)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        StartCoroutine(FadeCoroutine(canvasGroup, 1f, 0f, duration));
    }

    private System.Collections.IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }
}