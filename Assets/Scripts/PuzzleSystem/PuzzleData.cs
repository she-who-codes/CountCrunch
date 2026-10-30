using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    Rectangle,
    Triangle,
    Circle
}
public enum PumpkinType
{
    Orange,
    Green,
    Purple
}
public enum BlockRole
{
    Base,
    Wall,
    Roof,
    Support,
    Filler
}

[System.Serializable]
public class BlockData
{
    public BlockType type;

    public BlockRole role;

    public Vector2 position;

    public float rotation;

    public Vector2 scale = Vector2.one;

    public bool critical;
}

[CreateAssetMenu(
    fileName = "New Puzzle",
    menuName = "Pumpkin Game/Puzzle"
)]
public class PuzzleData : ScriptableObject
{
    [Header("Puzzle Information")]
    public int puzzleID;

    [Range(1, 100)]
    public int difficulty = 1;

    public string puzzleName;

    [Header("Pumpkin")]
    public PumpkinType pumpkinType;

    [Header("Pumpkin")]
    public Vector2 pumpkinPosition;

    [Header("Blocks")]
    public List<BlockData> blocks = new List<BlockData>();
}