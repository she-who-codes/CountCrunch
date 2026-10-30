using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PuzzleDatabase",
    menuName = "Pumpkin Game/Puzzle Database"
)]
public class PuzzleDatabase : ScriptableObject
{
    [Header("All Puzzles")]
    public List<PuzzleData> puzzles =
        new List<PuzzleData>();

    public List<PuzzleData> GetPuzzlesForDifficulty(
        int minimumDifficulty,
        int maximumDifficulty)
    {
        List<PuzzleData> results =
            new List<PuzzleData>();

        foreach (PuzzleData puzzle in puzzles)
        {
            if (puzzle == null)
                continue;

            if (puzzle.difficulty >= minimumDifficulty &&
                puzzle.difficulty <= maximumDifficulty)
            {
                results.Add(puzzle);
            }
        }

        return results;
    }
}