using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Projectile2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private Vector2 inheritedScale;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Ensure the collider is set as a trigger so it doesn't push the player
        if (TryGetComponent(out CircleCollider2D col))
        {
            col.isTrigger = true;
        }
        inheritedScale = transform.localScale;
        // Automatically destroy bullet if it doesn't hit anything
        Destroy(gameObject, lifeTime);
    }

    // Called automatically by PlayerMovement when instantiated
    public void Launch(float direction)
    {
        // Vector2.right * 1 moves right, Vector2.right * -1 moves left
        rb.linearVelocity = Vector2.right * direction * speed;

        // Optional: Flip the visual bullet sprite to face the travel direction
        if (direction < 0)
        {
            transform.localScale = new Vector3(-inheritedScale.x, inheritedScale.y, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object hit belongs to your target layers (like Enemies or Walls)
        if (((1 << collision.gameObject.layer) & damageableLayers) != 0)
        {
            // Handle dealing damage here (e.g., collision.GetComponent<Enemy>().TakeDamage())

            Destroy(gameObject); // Destroy the bullet on impact
        }
    }
}
