using UnityEngine;

public class LayerCollisionSound2D : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip collisionSound;

    [Header("Collision Filter")]
    [Tooltip("Select the layer(s) that should trigger this sound effect.")]
    [SerializeField] private LayerMask targetLayer;

    private void Awake()
    {
        // Fallback: automatically get the AudioSource component if not manually assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    // Use this if your objects physically bounce off each other (Solid Colliders)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object we hit belongs to the target layer mask
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            PlaySound();
        }
    }

    // Use this instead if one of your colliders has "Is Trigger" checked
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object we entered belongs to the target layer mask
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            PlaySound();
        }
    }

    private void PlaySound()
    {
        if (audioSource != null && collisionSound != null)
        {
            // PlayOneShot allows multiple instances of the sound to overlap cleanly
            audioSource.PlayOneShot(collisionSound);
        }
    }
}
