using UnityEngine;

public class PumpkinDestructible : MonoBehaviour
{
    [Header("Destruction Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float breakForceThreshold = 4f;
    [SerializeField] private GameObject explosionParticlesPrefab;

    private bool hasBeenSmashed = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasBeenSmashed) return;

        // Check if the hit object's layer is included in our groundLayer mask
        // This handles bitwise comparison to securely identify the object type
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // Calculate kinetic stress from the relative impact velocity magnitudes
            if (collision.relativeVelocity.magnitude > breakForceThreshold)
            {
                SmashPumpkin();
            }
        }
    }

    private void SmashPumpkin()
    {
        hasBeenSmashed = true;

        if (explosionParticlesPrefab != null)
        {
            Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
        }

        CatapultGameManager.Instance.OnPumpkinSmashed();
        Destroy(gameObject); // Remove pumpkin physics model from current stack scene space
    }
}
