using UnityEngine;
using UnityEditor;

public enum EditMode
{
    Platform,
    Trap,
    Collectible,
    Delete
}

public class LevelEditor : MonoBehaviour
{
    public EditMode currentMode = EditMode.Platform;
    public GameObject platformPrefab;
    public GameObject trapPrefab;
    public GameObject collectiblePrefab;
    public float gridSize = 1f;
    public bool showGrid = true;
    public LayerName targetLayer = LayerName.Default;
    public LayerName obstacleLayer = LayerName.Default;
    public LayerName collectibleLayer = LayerName.Default;
    
    private Camera editorCamera;
    private LevelData levelData;
    private bool isInitialized = false;

    private void Start()
    {
        InitializeEditor();
    }

    private void InitializeEditor()
    {
        if (isInitialized) return;
        
        // Set up camera
        editorCamera = Camera.main;
        if (editorCamera == null)
        {
            Debug.LogError("No main camera found in the scene!");
            return;
        }
        
        // Initialize level data
        levelData = new LevelData();
        levelData.name = "New Level";
        
        // Set up layers
        SetupLayers();
        
        isInitialized = true;
        Debug.Log("Level Editor initialized successfully!");
    }

    private void SetupLayers()
    {
        // Set up object layers for collision detection
        // This can be customized based on your project needs
        int targetLayerValue = LayerMask.NameToLayer("Target");
        int obstacleLayerValue = LayerMask.NameToLayer("Obstacle");
        int collectibleLayerValue = LayerMask.NameToLayer("Collectible");
        
        if (targetLayerValue == -1)
        {
            targetLayerValue = 8; // Default custom layer
        }
        if (obstacleLayerValue == -1)
        {
            obstacleLayerValue = 9; // Default custom layer
        }
        if (collectibleLayerValue == -1)
        {
            collectibleLayerValue = 10; // Default custom layer
        }
        
        targetLayer = (LayerName)targetLayerValue;
        obstacleLayer = (LayerName)obstacleLayerValue;
        collectibleLayer = (LayerName)collectibleLayerValue;
    }

    private void Update()
    {
        if (!isInitialized) return;
        
        HandleEditorInput();
        DrawGrid();
    }

    private void HandleEditorInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Vector3 worldPosition = GetWorldMousePosition();
            if (currentMode != EditMode.Delete)
            {
                PlaceObject(worldPosition);
            }
            else
            {
                DeleteObject(worldPosition);
            }
        }
    }

    private Vector3 GetWorldMousePosition()
    {
        Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
        float distance;
        
        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);
            return SnapToGrid(worldPosition);
        }
        
        return Vector3.zero;
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        position.x = Mathf.Round(position.x / gridSize) * gridSize;
        position.y = Mathf.Round(position.y / gridSize) * gridSize;
        return position;
    }

    private void PlaceObject(Vector3 position)
    {
        GameObject prefab = GetPrefabForMode(currentMode);
        if (prefab == null)
        {
            Debug.LogWarning($"No prefab assigned for {currentMode} mode");
            return;
        }
        
        GameObject placedObject = Instantiate(prefab, position, Quaternion.identity);
        string objectName = $"{currentMode}_{placedObject.GetInstanceID()}";
        placedObject.name = objectName;
        
        // Set layer based on object type
        SetLayerForObject(placedObject, currentMode);
        
        // Add to level data
        LevelObjectData objectData = new LevelObjectData
        {
            id = placedObject.GetInstanceID(),
            type = currentMode,
            position = position,
            rotation = Quaternion.identity,
            scale = Vector3.one,
            layer = GetLayerForMode(currentMode)
        };
        
        levelData.objects.Add(objectData);
        
        Debug.Log($"{currentMode} placed at {position}");
    }

    private void DeleteObject(Vector3 position)
    {
        // Find objects near the click position
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.5f);
        
        foreach (var collider in colliders)
        {
            GameObject obj = collider.gameObject;
            int instanceID = obj.GetInstanceID();
            
            // Remove from level data
            levelData.objects.RemoveAll(o => o.id == instanceID);
            
            // Destroy the object
            Destroy(obj);
            Debug.Log($"Object deleted at {position}");
            break; // Only delete one object per click
        }
    }

    private GameObject GetPrefabForMode(EditMode mode)
    {
        switch (mode)
        {
            case EditMode.Platform:
                return platformPrefab;
            case EditMode.Trap:
                return trapPrefab;
            case EditMode.Collectible:
                return collectiblePrefab;
            default:
                return null;
        }
    }

    private void SetLayerForObject(GameObject obj, EditMode mode)
    {
        switch (mode)
        {
            case EditMode.Platform:
                obj.layer = (int)obstacleLayer;
                break;
            case EditMode.Trap:
                obj.layer = (int)obstacleLayer;
                break;
            case EditMode.Collectible:
                obj.layer = (int)collectibleLayer;
                break;
            default:
                obj.layer = (int)targetLayer;
                break;
        }
    }

    private LayerName GetLayerForMode(EditMode mode)
    {
        switch (mode)
        {
            case EditMode.Platform:
                return obstacleLayer;
            case EditMode.Trap:
                return obstacleLayer;
            case EditMode.Collectible:
                return collectibleLayer;
            default:
                return targetLayer;
        }
    }

    private void DrawGrid()
    {
        if (!showGrid) return;
        
        // Draw grid lines in the editor
        Gizmos.color = Color.gray;
        
        float gridStartX = -50f;
        float gridEndX = 50f;
        float gridStartY = -50f;
        float gridEndY = 50f;
        
        for (float x = gridStartX; x <= gridEndX; x += gridSize)
        {
            Gizmos.DrawLine(new Vector3(x, gridStartY, 0), new Vector3(x, gridEndY, 0));
        }
        
        for (float y = gridStartY; y <= gridEndY; y += gridSize)
        {
            Gizmos.DrawLine(new Vector3(gridStartX, y, 0), new Vector3(gridEndX, y, 0));
        }
    }

    // Save and Load functionality
    public void SaveLevel(string path)
    {
        if (levelData == null)
        {
            Debug.LogError("No level data to save!");
            return;
        }
        
        string jsonData = JsonUtility.ToJson(levelData, true);
        System.IO.File.WriteAllText(path, jsonData);
        Debug.Log($"Level saved to {path}");
    }

    public void LoadLevel(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError($"Level file not found: {path}");
            return;
        }
        
        string jsonData = System.IO.File.ReadAllText(path);
        levelData = JsonUtility.FromJson<LevelData>(jsonData);
        
        // Clear existing objects
        ClearLevelObjects();
        
        // Recreate objects from data
        RecreateLevelObjects();
        
        Debug.Log($"Level loaded from {path}");
    }

    private void ClearLevelObjects()
    {
        // Destroy all existing objects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Platform_") || 
                obj.name.Contains("Trap_") || 
                obj.name.Contains("Collectible_"))
            {
                Destroy(obj);
            }
        }
    }

    private void RecreateLevelObjects()
    {
        foreach (LevelObjectData objData in levelData.objects)
        {
            GameObject prefab = GetPrefabForMode(objData.type);
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, objData.position, objData.rotation);
                obj.name = $"{objData.type}_{objData.id}";
                obj.transform.localScale = objData.scale;
                SetLayerForObject(obj, objData.type);
            }
        }
    }

    public void ClearLevel()
    {
        ClearLevelObjects();
        levelData.objects.Clear();
        Debug.Log("Level cleared");
    }
}

[System.Serializable]
public class LevelData
{
    public string name;
    public List<LevelObjectData> objects = new List<LevelObjectData>;
}

[System.Serializable]
public class LevelObjectData
{
    public int id;
    public EditMode type;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public LayerName layer;
}

public enum LayerName
{
    Default = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2,
    Water = 4,
    UI = 5,
    Target = 8,
    Obstacle = 9,
    Collectible = 10
}