using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PogGameManager;

public class NPCController : MonoBehaviour
{
    [Header("AI Configuration")]
    [SerializeField] private float thinkingDelay = 1.2f;
    [SerializeField] private float strikePowerBase = 15f;

    // These fields are now completely managed and populated automatically via code injection
    private PogGameManager gameManager;
    private SlammerEntity npcSlammer;

    /// <summary>
    /// Automatically populates all required fields cleanly at runtime without manual inspector binding.
    /// </summary>
    public void ConfigureNPCReferences(PogGameManager manager, SlammerEntity spawnedSlammer)
    {
        gameManager = manager;
        npcSlammer = spawnedSlammer;
        //Debug.Log("🤖 NPC Controller: All pertinent dependencies injected successfully.");
    }

    public void ExecuteAITurn(List<ControlledMilkCapPhysics> targets)
    {
        if (targets == null || targets.Count == 0 || npcSlammer == null) return;
        StartCoroutine(PerformSlammerStrike(targets));
    }

    private IEnumerator PerformSlammerStrike(List<ControlledMilkCapPhysics> targets)
    {
        yield return new WaitForSeconds(thinkingDelay);

        Vector2 targetCenter = CalculateStackCenter(targets);
        Vector2 slammerPos = npcSlammer.transform.position;
        Vector2 throwDirection = (targetCenter - slammerPos).normalized;

        npcSlammer.ThrowSlammer(throwDirection, strikePowerBase);

        if (gameManager != null)
        {
            gameManager.UpdateState(GameState.Slamming);
        }
    }

    private Vector2 CalculateStackCenter(List<ControlledMilkCapPhysics> targets)
    {
        Vector2 combinedPositions = Vector2.zero;
        int validCount = 0;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                combinedPositions += (Vector2)targets[i].transform.position;
                validCount++;
            }
        }
        return validCount > 0 ? combinedPositions / validCount : Vector2.zero;
    }
}
