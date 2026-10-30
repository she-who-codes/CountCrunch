using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PuzzleBuilder : MonoBehaviour
{
    [Header("Puzzle Information")]
    public string puzzleName = "Puzzle_001";

    [Range(1, 100)]
    public int difficulty = 1;

    [Header("Puzzle Root")]
    public Transform puzzleRoot;

    //[Header("Pumpkin")]
    [HideInInspector]
    public Transform pumpkin;


    [Header("Selected Pumpkin")]
    public PumpkinType pumpkinType =
    PumpkinType.Orange;

    [Header("Pumpkin Prefabs")]
    public GameObject orangePumpkinPrefab;
    public GameObject greenPumpkinPrefab;
    public GameObject purplePumpkinPrefab;

    [Header("Prefabs")]
    public GameObject rectanglePrefab;
    public GameObject trianglePrefab;
    public GameObject circlePrefab;

    [Header("Save Location")]
    public string saveFolder = "Assets/Puzzles";

#if UNITY_EDITOR

    public int GetNextPuzzleID()
    {
        if (!AssetDatabase.IsValidFolder(saveFolder))
        {
            return 1;
        }

        string[] guids =
            AssetDatabase.FindAssets(
                "t:PuzzleData",
                new[] { saveFolder }
            );

        int highestID = 0;

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            PuzzleData puzzle =
                AssetDatabase.LoadAssetAtPath<PuzzleData>(
                    path
                );

            if (puzzle != null)
            {
                highestID =
                    Mathf.Max(
                        highestID,
                        puzzle.puzzleID
                    );
            }
        }

        return highestID + 1;
    }

    public void SetNextPuzzleName()
    {
        int nextID = GetNextPuzzleID();

        puzzleName =
            $"Puzzle_{nextID:000}";

        EditorUtility.SetDirty(this);
    }

#endif
}