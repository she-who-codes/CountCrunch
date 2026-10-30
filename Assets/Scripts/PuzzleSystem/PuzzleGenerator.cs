using UnityEngine;

public class PuzzleSpawner : MonoBehaviour
{
    [Header("Puzzle Database")]
    [SerializeField] private PuzzleDatabase database;

    [Header("Difficulty")]
    [SerializeField] private int minimumDifficulty = 1;
    [SerializeField] private int maximumDifficulty = 100;

    [Header("Prefabs")]
    [SerializeField] private GameObject rectanglePrefab;
    [SerializeField] private GameObject trianglePrefab;
    [SerializeField] private GameObject circlePrefab;
    [Header("Pumpkin Prefabs")]
    [SerializeField] private GameObject orangePumpkinPrefab;
    [SerializeField] private GameObject greenPumpkinPrefab;
    [SerializeField] private GameObject purplePumpkinPrefab;

    [Header("Testing")]
    [SerializeField] private PuzzleData testPuzzle;

    private PuzzleData currentPuzzle;

    private void Start()
    {
        SpawnRandomPuzzle();
    }

    // =========================================================
    // RANDOM PUZZLE
    // =========================================================

    public void SpawnRandomPuzzle()
    {
        if (database == null)
        {
            Debug.LogError(
                "Puzzle Database has not been assigned."
            );

            return;
        }

        var availablePuzzles =
            database.GetPuzzlesForDifficulty(
                minimumDifficulty,
                maximumDifficulty
            );

        if (availablePuzzles.Count == 0)
        {
            Debug.LogError(
                "No puzzles found for this difficulty range."
            );

            return;
        }

        int randomIndex =
            Random.Range(
                0,
                availablePuzzles.Count
            );

        currentPuzzle =
            availablePuzzles[randomIndex];

        SpawnPuzzle(currentPuzzle);
    }

    // =========================================================
    // SPECIFIC PUZZLE
    // =========================================================

    public void SpawnPuzzle(PuzzleData puzzle)
    {
        if (puzzle == null)
        {
            Debug.LogError(
                "Cannot spawn a null puzzle."
            );

            return;
        }

        ClearCurrentPuzzle();

        currentPuzzle = puzzle;

        SpawnPumpkin();
        SpawnBlocks();
    }

    // =========================================================
    // PUMPKIN
    // =========================================================
    private GameObject GetPumpkinPrefab(
    PumpkinType type)
    {
        switch (type)
        {
            case PumpkinType.Orange:
                return orangePumpkinPrefab;

            case PumpkinType.Green:
                return greenPumpkinPrefab;

            case PumpkinType.Purple:
                return purplePumpkinPrefab;

            default:
                return null;
        }
    }
    private void SpawnPumpkin()
    {
        GameObject prefab =
            GetPumpkinPrefab(
                currentPuzzle.pumpkinType
            );

        if (prefab == null)
        {
            Debug.LogError(
                $"Missing prefab for {currentPuzzle.pumpkinType} pumpkin."
            );

            return;
        }

        Instantiate(
            prefab,
            currentPuzzle.pumpkinPosition,
            Quaternion.identity,
            transform
        );
    }
    // =========================================================
    // BLOCKS
    // =========================================================

    private void SpawnBlocks()
    {
        foreach (
            BlockData block in currentPuzzle.blocks)
        {
            GameObject prefab =
                GetPrefab(block.type);

            if (prefab == null)
            {
                Debug.LogWarning(
                    $"Missing prefab for {block.type}"
                );

                continue;
            }

            GameObject spawnedBlock =
                Instantiate(
                    prefab,
                    block.position,
                    Quaternion.Euler(
                        0f,
                        0f,
                        block.rotation
                    ),
                    transform
                );

            spawnedBlock.transform.localScale =
                block.scale;
        }
    }

    // =========================================================
    // PREFAB LOOKUP
    // =========================================================

    private GameObject GetPrefab(
        BlockType type)
    {
        switch (type)
        {
            case BlockType.Rectangle:
                return rectanglePrefab;

            case BlockType.Triangle:
                return trianglePrefab;

            case BlockType.Circle:
                return circlePrefab;

            default:
                return null;
        }
    }

    // =========================================================
    // CLEAR
    // =========================================================

    public void ClearCurrentPuzzle()
    {
        for (
            int i = transform.childCount - 1;
            i >= 0;
            i--)
        {
            Destroy(
                transform.GetChild(i).gameObject
            );
        }
    }

    // =========================================================
    // TEST SPECIFIC PUZZLE
    // =========================================================

    public void SpawnTestPuzzle()
    {
        if (testPuzzle == null)
        {
            Debug.LogError(
                "No test puzzle assigned."
            );

            return;
        }

        SpawnPuzzle(testPuzzle);
    }
}