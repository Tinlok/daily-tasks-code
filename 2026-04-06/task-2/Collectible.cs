using UnityEngine;

/// <summary>
/// 收集物类 - 支持多种收集物类型
/// </summary>
public class Collectible : MonoBehaviour
{
    [Header("收集物属性")]
    public CollectibleType collectibleType = CollectibleType.Coin;
    public int value = 1;
    public bool isCollected = false;
    public float floatSpeed = 2f;
    public float floatHeight = 0.5f;
    
    [Header("视觉")]
    public Color normalColor = Color.yellow;
    public Color collectedColor = Color.gray;
    
    [Header("效果")]
    public ParticleSystem collectEffect;
    public AudioClip collectSound;
    
    private MeshRenderer meshRenderer;
    private AudioSource audioSource;
    private float startY;
    private float floatTimer;
    
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        audioSource = GetComponent<AudioSource>();
        startY = transform.position.y;
        floatTimer = 0f;
        
        if (meshRenderer != null)
        {
            meshRenderer.material.color = normalColor;
        }
        
        // 初始化视觉效果
        InitializeVisualEffects();
    }
    
    void Update()
    {
        if (!isCollected)
        {
            // 浮动动画
            FloatAnimation();
        }
    }
    
    void FloatAnimation()
    {
        floatTimer += Time.deltaTime * floatSpeed;
        float yOffset = Mathf.Sin(floatTimer) * floatHeight;
        transform.position = new Vector3(transform.position.x, startY + yOffset, transform.position.z);
        
        // 旋转动画
        transform.Rotate(Vector3.up * 50f * Time.deltaTime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }
    
    public void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        Debug.Log($"收集了 {collectibleType}，价值: {value}");
        
        // 触发收集效果
        CollectEffects();
        
        // 禁用渲染和碰撞
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
        
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // 延迟后销毁
        Invoke("DestroyCollectible", 1f);
    }
    
    void CollectEffects()
    {
        // 播放收集音效
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        // 播放粒子效果
        if (collectEffect != null)
        {
            collectEffect.Play();
        }
        
        // 显示分数提示
        ShowScorePopup();
    }
    
    void ShowScorePopup()
    {
        GameObject popup = new GameObject("ScorePopup");
        popup.transform.position = transform.position + Vector3.up;
        
        TextMesh textMesh = popup.AddComponent<TextMesh>();
        textMesh.text = $"+{value}";
        textMesh.color = Color.yellow;
        textMesh.fontSize = 20;
        textMesh.alignment = TextAlignment.Center;
        
        // 添加浮动动画
        LeanTween.moveY(popup, popup.transform.position.y + 1f, 1f);
        LeanTween.alphaText(textMesh, 0f, 1f).setOnComplete(() => {
            Destroy(popup);
        });
    }
    
    void DestroyCollectible()
    {
        Destroy(gameObject);
    }
    
    void InitializeVisualEffects()
    {
        // 根据收集物类型设置不同的视觉效果
        switch (collectibleType)
        {
            case CollectibleType.Coin:
                InitializeCoinEffect();
                break;
            case CollectibleType.Gem:
                InitializeGemEffect();
                break;
            case CollectibleType.PowerUp:
                InitializePowerUpEffect();
                break;
        }
    }
    
    void InitializeCoinEffect()
    {
        // 创建硬币效果
        GameObject coinModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coinModel.transform.parent = transform;
        coinModel.transform.localPosition = Vector3.zero;
        coinModel.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
        coinModel.GetComponent<Renderer>().material.color = Color.yellow;
    }
    
    void InitializeGemEffect()
    {
        // 创建宝石效果
        GameObject gemModel = GameObject.CreatePrimitive(PrimitiveType.Octahedron);
        gemModel.transform.parent = transform;
        gemModel.transform.localPosition = Vector3.zero;
        gemModel.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        gemModel.GetComponent<Renderer>().material.color = new Color(0f, 1f, 1f);
    }
    
    void InitializePowerUpEffect()
    {
        // 创建能量提升效果
        GameObject powerUpModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        powerUpModel.transform.parent = transform;
        powerUpModel.transform.localPosition = Vector3.zero;
        powerUpModel.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        powerUpModel.GetComponent<Renderer>().material.color = Color.magenta;
        
        // 添加发光效果
        Light light = powerUpModel.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = Color.magenta;
        light.intensity = 2f;
        light.range = 2f;
    }
    
    // 设置收集物类型
    public void SetCollectibleType(CollectibleType type)
    {
        collectibleType = type;
        UpdateVisualEffects();
    }
    
    // 设置收集物价值
    public void SetValue(int amount)
    {
        value = amount;
    }
    
    // 重置收集物状态
    public void Reset()
    {
        isCollected = false;
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
            meshRenderer.material.color = normalColor;
        }
        
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
        
        startY = transform.position.y;
        floatTimer = 0f;
    }
    
    // 更新视觉效果
    void UpdateVisualEffects()
    {
        if (meshRenderer != null)
        {
            switch (collectibleType)
            {
                case CollectibleType.Coin:
                    meshRenderer.material.color = Color.yellow;
                    break;
                case CollectibleType.Gem:
                    meshRenderer.material.color = Color.cyan;
                    break;
                case CollectibleType.PowerUp:
                    meshRenderer.material.color = Color.magenta;
                    break;
            }
        }
    }
}

// 收集物管理器（用于跟踪游戏中的所有收集物）
public class CollectibleManager : MonoBehaviour
{
    [Header("收集物统计")]
    public int totalCoinsCollected = 0;
    public int totalGemsCollected = 0;
    public int totalPowerUpsCollected = 0;
    public int totalScore = 0;
    
    [Header("UI")]
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI coinsText;
    public TMPro.TextMeshProUGUI gemsText;
    public TMPro.TextMeshProUGUI powerUpsText;
    
    void Update()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {totalScore}";
        
        if (coinsText != null)
            coinsText.text = $"Coins: {totalCoinsCollected}";
        
        if (gemsText != null)
            gemsText.text = $"Gems: {totalGemsCollected}";
        
        if (powerUpsText != null)
            powerUpsText.text = $"Power-ups: {totalPowerUpsCollected}";
    }
    
    public void RegisterCollectible(CollectibleType type, int value)
    {
        switch (type)
        {
            case CollectibleType.Coin:
                totalCoinsCollected++;
                break;
            case CollectibleType.Gem:
                totalGemsCollected++;
                break;
            case CollectibleType.PowerUp:
                totalPowerUpsCollected++;
                break;
        }
        
        totalScore += value;
        Debug.Log($"收集统计更新 - {type}: +{value}, 总分: {totalScore}");
    }
}