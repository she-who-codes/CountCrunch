using UnityEngine;
using System.Collections.Generic;

public class MilkCapStackManager : MonoBehaviour
{
    [Header("Stack Design Layout")]
    [SerializeField] private List<ControlledMilkCapPhysics> capPrefabs;
    [SerializeField] private Transform stackStartingPoint;

    [Tooltip("Side-view stack thickness. Lower values keep the caps physically tight for better slammer impact transfer.")]
    [SerializeField] private float verticalVisualOffset = 0.05f;

    // Internal list to track the active instances created at match start
    private List<ControlledMilkCapPhysics> spawnedCaps = new List<ControlledMilkCapPhysics>();

    /// <summary>
    /// Generates the initial stack at the center point when the match initializes.
    /// </summary>
    /// <summary>
    /// Generates the initial stack at the center point when the match initializes.
    /// </summary>
    public List<ControlledMilkCapPhysics> BuildInitialStack()
    {
        // Clear any old leftover references safely
        ClearTable();

        if (capPrefabs == null || capPrefabs.Count == 0)
        {
            Debug.LogWarning("⚠️ MilkCapStackManager: No cap prefabs assigned in the inspector!");
            return spawnedCaps;
        }

        // --- FULL & COMPLETE LOOP STARTS HERE ---
        for (int i = 0; i < capPrefabs.Count; i++)
        {
            if (capPrefabs[i] == null) continue;

            // 1. Calculate the physics position, offsetting along the Y axis to make a neat column
            Vector3 spawnPosition = stackStartingPoint.position + new Vector3(0f, i * verticalVisualOffset, 0f);
            ControlledMilkCapPhysics newCap = Instantiate(capPrefabs[i], spawnPosition, Quaternion.identity);

            // 2. Adjust sorting layers so higher pogs visually render on top of lower ones
            if (newCap.TryGetComponent(out SpriteRenderer sr))
            {
                sr.sortingOrder = 2 + i;
            }

            // 3. Explicitly force your cap to be static so it doesn't tip over before being slammed
            if (newCap.TryGetComponent(out Rigidbody2D rb))
            {
                rb.bodyType = RigidbodyType2D.Static;
            }

            spawnedCaps.Add(newCap);
        } // --- LOOP CLOSES SAFELY HERE ---

        //Debug.Log($"📚 Stack Manager: Successfully built a column of {spawnedCaps.Count} milkcaps.");
        return spawnedCaps;
    }

    /// Gathers surviving caps from the GameManager and pulls them back into a clean stack.
    /// </summary>
    /// <summary>
    /// Gathers surviving caps from the GameManager and pulls them back into a clean stack.
    /// </summary>
    public void RestackRemainingPogs(List<ControlledMilkCapPhysics> remainingCaps)
    {
        // Sync our reference list with the current survivors remaining on table
        spawnedCaps = new List<ControlledMilkCapPhysics>(remainingCaps);

        for (int i = 0; i < spawnedCaps.Count; i++)
        {
            if (spawnedCaps[i] == null) continue;

            // 1. Freeze physics and completely kill momentum before teleportation
            if (spawnedCaps[i].TryGetComponent(out Rigidbody2D rb))
            {
                rb.bodyType = RigidbodyType2D.Static;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // 2. Align them perfectly back onto the starting baseline stack position
            Vector3 stackHeightY = stackStartingPoint.position + new Vector3(0f, i * verticalVisualOffset, 0f);
            spawnedCaps[i].transform.position = stackHeightY;
            spawnedCaps[i].transform.rotation = Quaternion.identity;

            // 3. Fix rendering depth priorities for the remaining pieces
            if (spawnedCaps[i].TryGetComponent(out SpriteRenderer sr))
            {
                sr.sortingOrder = 2 + i;
            }
        }

        //Debug.Log($"📚 Stack Manager: Re-stacked {spawnedCaps.Count} surviving milkcaps.");
    }



    /// Utility cleanup method to purge objects when reloading a scene or leaving matches.
    /// </summary>
    public void ClearTable()
    {
        for (int i = spawnedCaps.Count - 1; i >= 0; i--)
        {
            if (spawnedCaps[i] != null)
            {
                Destroy(spawnedCaps[i].gameObject);
            }
        }
        spawnedCaps.Clear();
    }
}
