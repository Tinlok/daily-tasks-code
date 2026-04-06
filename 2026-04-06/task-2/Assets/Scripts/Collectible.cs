using UnityEngine;

public enum CollectibleType
{
    Coin,
    Gem,
    PowerUp
}

public class Collectible : MonoBehaviour
{
    public CollectibleType type;
    public int value = 1;
    public float rotationSpeed = 100f;
    public float floatSpeed = 2f;
    public float floatHeight = 0.5f;
    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.position;
    }

    private void Update()
    {
        // Rotation animation
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        // Floating animation
        transform.position = originalPosition + new Vector3(0, Mathf.Sin(Time.time * floatSpeed) * floatHeight, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        // Add to score based on type
        switch (type)
        {
            case CollectibleType.Coin:
                value = 1;
                break;
            case CollectibleType.Gem:
                value = 5;
                break;
            case CollectibleType.PowerUp:
                value = 0; // Power-ups don't give score, they give abilities
                break;
        }

        // Notify game manager about collection
        GameManager.Instance?.CollectItem(this);

        // Play collection effect
        PlayCollectionEffect();

        // Destroy this collectible
        Destroy(gameObject);
    }

    private void PlayCollectionEffect()
    {
        // TODO: Instantiate particle effect
        // TODO: Play collection sound
    }
}