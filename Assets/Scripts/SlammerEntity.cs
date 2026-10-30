using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SlammerEntity : MonoBehaviour
{
    [Header("Active Equipment Configuration")]
    [SerializeField] private SlammerData activeData;

    [Header("Physics Settings Baselines")]
    [Tooltip("Mass of a standard common slammer (Weight stat value = 15).")]
    [SerializeField] private float baseMassMultiplier = 0.1f;
    [Tooltip("Maximum scale applied to torque force calculations.")]
    [SerializeField] private float maxTorqueForce = 15f;

    private HashSet<int> struckMilkCapInstanceIDs = new HashSet<int>();

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;
    private Vector3 spawnPoint;
    private Vector2 lastVelocityBeforeImpact;
  
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnPoint = transform.position;

        ApplySlammerData();
    }
    // Expose a public setter so the GameManager can dynamically set where it belongs
    public void SetInitialSpawnPoint(Vector3 newSpawnPosition)
    {
        spawnPoint = newSpawnPosition;
        transform.position = newSpawnPosition;
    }

    // Expose a public method allowing the GameManager to change data parameters on the fly
    public void ApplyCustomSlammerData(SlammerData dataProfile)
    {
        activeData = dataProfile;
        ApplySlammerData(); // Fires your existing 1-100 attribute layout mapper script
    }

    private void FixedUpdate()
    {
        // Cache velocity right before collision to use for kinetic energy math in OnCollisionEnter2D
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            lastVelocityBeforeImpact = rb.linearVelocity;
        }
    }

    /// <summary>
    /// Translates 1-100 attributes from the ScriptableObject into concrete Unity 2D physical parameters
    /// </summary>
    public void ApplySlammerData()
    {
        if (activeData == null || rb == null || col == null) return;

        // 1. Weight -> Rigidbody2D Mass (e.g., Weight of 50 = 5kg mass)
        rb.mass = activeData.stats.weight * baseMassMultiplier;
        rb.bodyType = RigidbodyType2D.Static; // Keep frozen until throw execution

        // 2. Stability -> Rigidbody2D Linear/Angular Drag (High stability resists minor deflections)
        //rb.linearDrag = Mathf.Lerp(1.0f, 0.05f, activeData.stats.stability / 100f);
        rb.angularDamping = Mathf.Lerp(1.0f, 0.05f, activeData.stats.stability / 100f);

        // 3. Bounce -> Dynamic Runtime Physics Material Creation
        PhysicsMaterial2D customMat = new PhysicsMaterial2D($"Mat_{activeData.slammerName}");
        customMat.bounciness = activeData.stats.bounce / 100f;
        customMat.friction = 0.2f; // Keep it slick so it slides under milkcaps
        col.sharedMaterial = customMat;

        // 4. Identity Visuals
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = activeData.slammerSprite;
        }
    }

    /// <summary>
    /// Delivers the slammer down onto the table.
    /// </summary>
    public void ThrowSlammer(Vector2 directTargetVector, float strikePower)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Accuracy math: Drill down user input drift using accuracy stat
        float maxSpreadAngle = Mathf.Lerp(15f, 0f, activeData.stats.accuracy / 100f);
        float appliedSpread = Random.Range(-maxSpreadAngle, maxSpreadAngle);
        Vector2 finalDirection = Quaternion.Euler(0, 0, appliedSpread) * directTargetVector.normalized;

        // Apply raw linear velocity force
        float forceOutput = strikePower * (1f + (activeData.stats.flipPower / 100f));
        rb.AddForce(finalDirection * forceOutput, ForceMode2D.Impulse);

        // Apply Spin math -> Physical Torque Force
        float calculatedTorque = (activeData.stats.spin / 100f) * maxTorqueForce * (Random.value > 0.5f ? 1f : -1f);
        rb.AddTorque(calculatedTorque, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Resets our hit guard tracker. Call this inside your existing ResetPosition() method!
    /// </summary>
    public void ResetSlammerHitGuard()
    {
        struckMilkCapInstanceIDs.Clear();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out ControlledMilkCapPhysics milkCap))
        {
            int capID = milkCap.gameObject.GetInstanceID();

            // CONSTRAINT 1: Single-Interaction Guard. Skip if we already smashed this cap this turn.
            if (struckMilkCapInstanceIDs.Contains(capID))
            {
                Debug.Log($"🛡️ Slammer Constraint: Ignoring late repeat contact with [{collision.gameObject.name}].");
                return;
            }

            // CONSTRAINT 2: Velocity Speed Filter. 
            // If the slammer is merely rolling slowly on the floor, it cannot disrupt or change outcomes.
            if (lastVelocityBeforeImpact.sqrMagnitude < 9f) // Adjust 9f (3 units/sec) to fit your game's speed
            {
                Debug.Log($"🛡️ Slammer Constraint: Interaction blocked. Slammer velocity too low to count as a legal hit.");
                return;
            }

            // Lock this cap in as legally struck for the rest of the active turn
            struckMilkCapInstanceIDs.Add(capID);

            Vector2 impactDirection = (collision.transform.position - transform.position).normalized;
            float energyTransferred = lastVelocityBeforeImpact.magnitude * (rb.mass * 1.2f);
            Vector2 scatterVector = impactDirection * energyTransferred;

            // Roll Critical Chance or assign outcomes safely
            bool isCriticalFlip = Random.Range(0, 100) < activeData.stats.criticalChance;
            if (isCriticalFlip)
            {
                milkCap.desiredOutcome = ControlledMilkCapPhysics.LandingOutcome.ForceHeads;
            }
            else if (activeData.overridesLandingOutcome)
            {
                milkCap.desiredOutcome = activeData.specialOutcomeOverride;
            }

            // Execute the clean launch sequence
            milkCap.LaunchCapCustom(scatterVector);

            if (activeData.specialEffect != SlammerEffectType.None)
            {
                ExecuteSpecialEffect(activeData.specialEffect, collision.contacts[0].point);
            }
        }
    }

    /// <summary>
    /// Evaluates special environmental effects across the playing grid based on Ability Power
    /// </summary>
    private void ExecuteSpecialEffect(SlammerEffectType effect, Vector2 impactPoint)
    {
        float effectRadius = Mathf.Lerp(1f, 5f, activeData.stats.abilityPower / 100f);
        //Debug.Log($"💥 Slammer Ability: Triggered [{effect}] with an effect radius of {effectRadius} units.");

        switch (effect)
        {
            case SlammerEffectType.HeavyImpact:
                // Proactively add explosive physics push to all nearby rigidbodies
                Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(impactPoint, effectRadius);
                foreach (var obj in objectsInRange)
                {
                    if (obj.gameObject != gameObject && obj.TryGetComponent(out Rigidbody2D targetRb))
                    {
                        Vector2 forceDirection = ((Vector2)obj.transform.position - impactPoint).normalized;
                        targetRb.AddForce(forceDirection * (activeData.stats.abilityPower * 0.2f), ForceMode2D.Impulse);
                    }
                }
                break;

            case SlammerEffectType.Freeze:
                // Logic would go here to tell nearby caps to reduce their bounce/scatter variables
                break;

                // Add additional custom math blocks here for your remaining 22 effects!
        }
    }

    public void ResetPosition()
    {
        rb.bodyType = RigidbodyType2D.Static;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = spawnPoint;
        transform.rotation = Quaternion.identity;
    }
}
