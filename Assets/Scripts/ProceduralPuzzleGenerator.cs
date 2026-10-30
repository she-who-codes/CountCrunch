using UnityEngine;

public class ProceduralPuzzleGenerator : MonoBehaviour
{
    public enum PuzzleType { Stack, Grid, Pyramid }

    [Header("Prefabs")]
    [SerializeField] private GameObject haybalePrefab;
    [SerializeField] private GameObject pumpkinPrefab;

    [Header("Structure Dimensions")]
    [SerializeField] private int minWidth = 2;
    [SerializeField] private int maxWidth = 4;
    [SerializeField] private int minHeight = 2;
    [SerializeField] private int maxHeight = 5;

    [Header("Spacing Setup")]
    [SerializeField] private Vector2 itemSpacing = new Vector2(1.2f, 0.8f); // Match your sprite size dimensions

    [Header("Randomization Modifiers")]
    [Range(0f, 0.4f)][SerializeField] private float structuralHoleChance = 0.15f; // Chance a haybale is skipped to make gaps
    [Range(0f, 1f)][SerializeField] private float pumpkinSpawnChance = 0.7f; // Chance of a pumpkin spawning on top blocks

    public void CleanAndGenerate()
    {
        // Clear out any previous layout remnants safely
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Randomly select structural puzzle style
        PuzzleType chosenStyle = (PuzzleType)Random.Range(0, 3);
        int targetWidth = Random.Range(minWidth, maxWidth + 1);
        int targetHeight = Random.Range(minHeight, maxHeight + 1);

        switch (chosenStyle)
        {
            case PuzzleType.Stack:
                BuildStack(targetHeight);
                break;
            case PuzzleType.Grid:
                BuildGrid(targetWidth, targetHeight);
                break;
            case PuzzleType.Pyramid:
                BuildPyramid(targetHeight);
                break;
        }
    }

    private void BuildStack(int height)
    {
        for (int y = 0; y < height; y++)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, y * itemSpacing.y, 0);
            SpawnHaybale(spawnPos, y == height - 1); // Only try to put pumpkins on the absolute top block
        }
    }

    private void BuildGrid(int width, int height)
    {
        // Offset starting position to center the structure around the generator object
        float startXOffset = -((width - 1) * itemSpacing.x) / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Inject random structurally sound pockets/holes in lower levels
                if (y < height - 1 && Random.value < structuralHoleChance) continue;

                Vector3 spawnPos = transform.position + new Vector3(startXOffset + (x * itemSpacing.x), y * itemSpacing.y, 0);

                // Pumpkin eligible if it's the top row
                bool isTopRow = (y == height - 1);
                SpawnHaybale(spawnPos, isTopRow);
            }
        }
    }

    private void BuildPyramid(int height)
    {
        for (int y = 0; y < height; y++)
        {
            int blocksInRow = height - y;
            float startXOffset = -((blocksInRow - 1) * itemSpacing.x) / 2f;

            for (int x = 0; x < blocksInRow; x++)
            {
                Vector3 spawnPos = transform.position + new Vector3(startXOffset + (x * itemSpacing.x), y * itemSpacing.y, 0);

                // In pyramids, every block on the outer steps can technically hold a pumpkin safely
                bool isExposedStep = (y == height - 1) || (x == 0) || (x == blocksInRow - 1);
                SpawnHaybale(spawnPos, isExposedStep);
            }
        }
    }

    private void SpawnHaybale(Vector3 position, bool eligibleForPumpkin)
    {
        GameObject bale = Instantiate(haybalePrefab, position, Quaternion.identity, transform);

        if (eligibleForPumpkin && Random.value < pumpkinSpawnChance)
        {
            // Spawn the pumpkin sitting directly on top of this haybale element
            Vector3 pumpkinPos = position + new Vector3(0, itemSpacing.y, 0);
            Instantiate(pumpkinPrefab, pumpkinPos, Quaternion.identity, transform);
        }
    }
}
