using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class LevelEditorUI : MonoBehaviour
{
    public Canvas editorCanvas;
    public GameObject toolPanel;
    public GameObject infoPanel;
    
    // Tool buttons
    public Button platformButton;
    public Button trapButton;
    public Button collectibleButton;
    public Button deleteButton;
    public Button saveButton;
    public Button loadButton;
    public Button clearButton;
    
    // UI Text elements
    public Text currentModeText;
    public Text objectCountText;
    public Text levelNameText;
    public InputField levelNameInput;
    
    // Prefab references
    public GameObject platformPrefab;
    public GameObject trapPrefab;
    public GameObject collectiblePrefab;
    
    private LevelEditor levelEditor;
    private int objectCount = 0;
    private string currentLevelName = "New Level";

    private void Start()
    {
        InitializeUI();
        SetupEventListeners();
    }

    private void InitializeUI()
    {
        // Find and set up level editor
        levelEditor = FindObjectOfType<LevelEditor>();
        if (levelEditor == null)
        {
            Debug.LogError("LevelEditor not found in the scene!");
            return;
        }
        
        // Set up canvas
        if (editorCanvas == null)
        {
            editorCanvas = FindObjectOfType<Canvas>();
            if (editorCanvas == null)
            {
                GameObject canvasObj = new GameObject("EditorCanvas");
                canvasObj.AddComponent<Canvas>();
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                editorCanvas = canvasObj.GetComponent<Canvas>();
            }
        }
        
        // Set up panels
        if (toolPanel == null)
        {
            toolPanel = CreateToolPanel();
        }
        
        if (infoPanel == null)
        {
            infoPanel = CreateInfoPanel();
        }
        
        // Initialize UI elements
        UpdateUI();
    }

    private GameObject CreateToolPanel()
    {
        GameObject panel = new GameObject("ToolPanel");
        panel.transform.SetParent(editorCanvas.transform);
        panel.AddComponent<RectTransform>();
        
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(10, -10);
        rectTransform.sizeDelta = new Vector2(200, 300);
        
        // Background
        Image backgroundImage = panel.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Add buttons
        CreateToolButton(panel, "Platform", new Vector2(0, 250), () => SelectMode(EditMode.Platform));
        CreateToolButton(panel, "Trap", new Vector2(0, 200), () => SelectMode(EditMode.Trap));
        CreateToolButton(panel, "Collectible", new Vector2(0, 150), () => SelectMode(EditMode.Collectible));
        CreateToolButton(panel, "Delete", new Vector2(0, 100), () => SelectMode(EditMode.Delete));
        CreateToolButton(panel, "Save", new Vector2(0, 50), () => SaveLevel());
        CreateToolButton(panel, "Load", new Vector2(0, 0), () => LoadLevel());
        CreateToolButton(panel, "Clear", new Vector2(0, -50), () => ClearLevel());
        
        return panel;
    }

    private GameObject CreateToolButton(GameObject parent, string name, Vector2 position, System.Action onClick)
    {
        GameObject buttonObj = new GameObject(name + "Button");
        buttonObj.transform.SetParent(parent.transform);
        
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(180, 40);
        
        // Button background
        Image backgroundImage = buttonObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        
        RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
        textRectTransform.anchorMin = new Vector2(0, 0);
        textRectTransform.anchorMax = new Vector2(1, 1);
        textRectTransform.offsetMin = Vector2.zero;
        textRectTransform.offsetMax = Vector2.zero;
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = name;
        textComponent.color = Color.white;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.fontSize = 14;
        
        // Button functionality
        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(() => onClick());
        
        return buttonObj;
    }

    private GameObject CreateInfoPanel()
    {
        GameObject panel = new GameObject("InfoPanel");
        panel.transform.SetParent(editorCanvas.transform);
        
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(-10, -10);
        rectTransform.sizeDelta = new Vector2(300, 150);
        
        // Background
        Image backgroundImage = panel.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Mode text
        CreateInfoText(panel, "Current Mode:", new Vector2(0, 100), TextAnchor.MiddleLeft);
        currentModeText = CreateInfoText(panel, "Platform", new Vector2(80, 100), TextAnchor.MiddleLeft);
        
        // Object count text
        CreateInfoText(panel, "Objects:", new Vector2(0, 70), TextAnchor.MiddleLeft);
        objectCountText = CreateInfoText(panel, "0", new Vector2(60, 70), TextAnchor.MiddleLeft);
        
        // Level name text
        CreateInfoText(panel, "Level Name:", new Vector2(0, 40), TextAnchor.MiddleLeft);
        levelNameText = CreateInfoText(panel, currentLevelName, new Vector2(80, 40), TextAnchor.MiddleLeft);
        
        // Level name input
        GameObject inputObj = new GameObject("LevelNameInput");
        inputObj.transform.SetParent(panel.transform);
        
        RectTransform inputRectTransform = inputObj.AddComponent<RectTransform>();
        inputRectTransform.anchorMin = new Vector2(0, 0.15f);
        inputRectTransform.anchorMax = new Vector2(1, 0.45f);
        inputRectTransform.offsetMin = new Vector2(10, 0);
        inputRectTransform.offsetMax = new Vector2(-10, 0);
        
        InputField inputField = inputObj.AddComponent<InputField>();
        inputField.text = currentLevelName;
        inputField.onValueChanged.AddListener(OnLevelNameChanged);
        
        // Input field background
        Image inputBackground = inputObj.AddComponent<Image>();
        inputBackground.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        return panel;
    }

    private Text CreateInfoText(GameObject parent, string text, Vector2 position, TextAnchor alignment)
    {
        GameObject textObj = new GameObject("InfoText");
        textObj.transform.SetParent(parent.transform);
        
        RectTransform rectTransform = textObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(200, 30);
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.color = Color.white;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.alignment = alignment;
        textComponent.fontSize = 12;
        
        return textComponent;
    }

    private void SetupEventListeners()
    {
        if (platformButton != null) platformButton.onClick.AddListener(() => SelectMode(EditMode.Platform));
        if (trapButton != null) trapButton.onClick.AddListener(() => SelectMode(EditMode.Trap));
        if (collectibleButton != null) collectibleButton.onClick.AddListener(() => SelectMode(EditMode.Collectible));
        if (deleteButton != null) deleteButton.onClick.AddListener(() => SelectMode(EditMode.Delete));
        if (saveButton != null) saveButton.onClick.AddListener(SaveLevel);
        if (loadButton != null) loadButton.onClick.AddListener(LoadLevel);
        if (clearButton != null) clearButton.onClick.AddListener(ClearLevel);
        
        if (levelNameInput != null)
        {
            levelNameInput.onValueChanged.AddListener(OnLevelNameChanged);
        }
    }

    private void SelectMode(EditMode mode)
    {
        if (levelEditor != null)
        {
            levelEditor.currentMode = mode;
            currentModeText.text = mode.ToString();
            Debug.Log($"Selected mode: {mode}");
        }
    }

    private void SaveLevel()
    {
        if (levelEditor != null)
        {
            // Create save path
            string savePath = Application.dataPath + "/Levels/" + currentLevelName + ".json";
            
            // Create directory if it doesn't exist
            System.IO.Directory.CreateDirectory(Application.dataPath + "/Levels/");
            
            // Save level
            levelEditor.SaveLevel(savePath);
            
            // Update level name in level data
            if (levelEditor.levelData != null)
            {
                levelEditor.levelData.name = currentLevelName;
            }
            
            Debug.Log($"Level saved: {savePath}");
        }
    }

    private void LoadLevel()
    {
        if (levelEditor != null)
        {
            // Open file dialog in editor
            string path = EditorUtility.OpenFilePanel("Load Level", Application.dataPath + "/Levels/", "json");
            
            if (!string.IsNullOrEmpty(path))
            {
                levelEditor.LoadLevel(path);
                currentLevelName = System.IO.Path.GetFileNameWithoutExtension(path);
                levelNameText.text = currentLevelName;
                if (levelNameInput != null)
                {
                    levelNameInput.text = currentLevelName;
                }
            }
        }
    }

    private void ClearLevel()
    {
        if (levelEditor != null)
        {
            levelEditor.ClearLevel();
            objectCount = 0;
            UpdateUI();
        }
    }

    private void OnLevelNameChanged(string newName)
    {
        currentLevelName = newName;
        if (levelNameText != null)
        {
            levelNameText.text = currentLevelName;
        }
    }

    private void UpdateUI()
    {
        if (currentModeText != null)
        {
            currentModeText.text = levelEditor != null ? levelEditor.currentMode.ToString() : "N/A";
        }
        
        if (objectCountText != null)
        {
            objectCountText.text = objectCount.ToString();
        }
        
        if (levelNameText != null)
        {
            levelNameText.text = currentLevelName;
        }
    }

    private void Update()
    {
        // Update object count
        if (levelEditor != null && levelEditor.levelData != null)
        {
            int newCount = levelEditor.levelData.objects.Count;
            if (newCount != objectCount)
            {
                objectCount = newCount;
                UpdateUI();
            }
        }
    }
}