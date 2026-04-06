using UnityEngine;

public enum PlatformType
{
    Static,
    Moving,
    FollowPlayer
}

public class Platform : MonoBehaviour
{
    public PlatformType type;
    public float moveSpeed = 2f;
    public float moveDistance = 5f;
    public float moveDelay = 0f;
    
    private Vector3 originalPosition;
    private bool movingRight = true;
    private float moveTimer;
    private bool isFollowing = false;

    private void Start()
    {
        originalPosition = transform.position;
        
        if (type == PlatformType.FollowPlayer)
        {
            isFollowing = true;
            // Start follow player logic
            StartFollowingPlayer();
        }
    }

    private void Update()
    {
        if (type == PlatformType.Moving && !isFollowing)
        {
            MovePlatform();
        }
        
        if (isFollowing)
        {
            FollowPlayer();
        }
    }

    private void MovePlatform()
    {
        moveTimer += Time.deltaTime;
        
        if (moveTimer >= moveDelay)
        {
            float newX = transform.position.x;
            
            if (movingRight)
            {
                newX += moveSpeed * Time.deltaTime;
                if (newX >= originalPosition.x + moveDistance)
                {
                    newX = originalPosition.x + moveDistance;
                    movingRight = false;
                }
            }
            else
            {
                newX -= moveSpeed * Time.deltaTime;
                if (newX <= originalPosition.x - moveDistance)
                {
                    newX = originalPosition.x - moveDistance;
                    movingRight = true;
                }
            }
            
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }
    }

    private void StartFollowingPlayer()
    {
        // Initialize following behavior
        // This could be expanded to track specific player instances
        Debug.Log("Platform started following player behavior");
    }

    private void FollowPlayer()
    {
        // TODO: Implement player following logic
        // This would track player movement and adjust platform accordingly
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Make player child of platform when standing on it
            if (other.contacts[0].normal.y > 0.5f)
            {
                other.transform.SetParent(transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Remove player as child when leaving platform
            other.transform.SetParent(null);
        }
    }
}