using UnityEngine;

public enum TrapType
{
    Spike,
    Laser,
    FallingRock
}

public class Trap : MonoBehaviour
{
    public TrapType type;
    public float damage = 10f;
    public float activationDelay = 1f;
    public float cooldown = 3f;
    
    private bool isActive = false;
    private bool isPlayerInRange = false;
    private float activationTimer;
    private float cooldownTimer;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private Collider2D trapCollider;
    
    // Laser specific
    public LineRenderer laserLine;
    public float laserLength = 5f;
    public float laserDamagePerSecond = 5f;
    
    // Falling rock specific
    private Rigidbody2D rb;
    public float fallSpeed = 5f;
    public bool hasFallen = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trapCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        
        SetupTrapType();
    }

    private void Update()
    {
        if (!isActive)
        {
            CheckPlayerProximity();
        }
        
        switch (type)
        {
            case TrapType.Spike:
                UpdateSpikeTrap();
                break;
            case TrapType.Laser:
                UpdateLaserTrap();
                break;
            case TrapType.FallingRock:
                UpdateFallingRock();
                break;
        }
    }

    private void SetupTrapType()
    {
        switch (type)
        {
            case TrapType.Spike:
                // Setup spike trap
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.gray;
                }
                break;
            case TrapType.Laser:
                // Setup laser trap
                if (laserLine == null)
                {
                    laserLine = gameObject.AddComponent<LineRenderer>();
                    laserLine.startWidth = 0.1f;
                    laserLine.endWidth = 0.1f;
                    laserLine.colorGradient = new Gradient();
                    laserLine.colorGradient.colorKeys = new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.red, 1f) };
                }
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.red;
                }
                break;
            case TrapType.FallingRock:
                // Setup falling rock
                if (rb == null)
                {
                    rb = gameObject.AddComponent<Rigidbody2D>();
                }
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.brown;
                }
                break;
        }
    }

    private void CheckPlayerProximity()
    {
        // Simple proximity check - can be expanded with raycasts for more precise detection
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2f);
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                if (activationTimer < activationDelay)
                {
                    activationTimer += Time.deltaTime;
                }
                else if (!isActive)
                {
                    ActivateTrap();
                }
                return;
            }
        }
        
        // Reset if player leaves range
        if (activationTimer > 0)
        {
            activationTimer -= Time.deltaTime;
        }
    }

    private void ActivateTrap()
    {
        isActive = true;
        
        switch (type)
        {
            case TrapType.Spike:
                Debug.Log("Spike trap activated!");
                // Play spike animation
                break;
            case TrapType.Laser:
                Debug.Log("Laser trap activated!");
                laserLine.enabled = true;
                break;
            case TrapType.FallingRock:
                Debug.Log("Falling rock activated!");
                rb.velocity = new Vector2(0, -fallSpeed);
                hasFallen = true;
                break;
        }
    }

    private void UpdateSpikeTrap()
    {
        if (isActive && isPlayerInRange && trapCollider != null)
        {
            // Apply damage to player
            PlayerHealth player = GetPlayerInRange();
            if (player != null)
            {
                player.TakeDamage(damage * Time.deltaTime);
            }
            
            // Cooldown logic
            if (cooldownTimer < cooldown)
            {
                cooldownTimer += Time.deltaTime;
            }
            else
            {
                isActive = false;
                cooldownTimer = 0f;
            }
        }
    }

    private void UpdateLaserTrap()
    {
        if (isActive && laserLine != null)
        {
            // Update laser line
            laserLine.SetPosition(0, transform.position);
            laserLine.SetPosition(1, transform.position + new Vector3(laserLength, 0, 0));
            
            // Check for player in laser path
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, laserLength);
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                PlayerHealth player = hit.collider.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(laserDamagePerSecond * Time.deltaTime);
                }
            }
        }
    }

    private void UpdateFallingRock()
    {
        if (hasFallen && rb != null)
        {
            // Check if rock hit ground or player
            if (rb.velocity.y <= 0)
            {
                // Rock hit ground - destroy after a delay
                Destroy(gameObject, 1f);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerInRange = true;
            
            if (type == TrapType.FallingRock && hasFallen)
            {
                // Apply damage once on impact
                PlayerHealth player = other.gameObject.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private PlayerHealth GetPlayerInRange()
    {
        // Find player in range and return health component
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return collider.GetComponent<PlayerHealth>();
            }
        }
        
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection range for editor visualization
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
        
        // Draw laser range for laser traps
        if (type == TrapType.Laser)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(laserLength, 0, 0));
        }
    }
}