using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(PuzzleBuilder))]
public class PuzzleBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        PuzzleBuilder builder =
            (PuzzleBuilder)target;

         GUI.backgroundColor = Color.green;

         if (GUILayout.Button(
             "SAVE PUZZLE",
             GUILayout.Height(40)))
         {
             SavePuzzle(builder);
         }

        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);

        if (GUILayout.Button(
            "CLEAR PUZZLE",
            GUILayout.Height(30)))
        {
            ClearPuzzle(builder);
        }
    }

    /*private void SavePuzzle(PuzzleBuilder builder)
    {
        PuzzleData puzzle =
            ScriptableObject.CreateInstance<PuzzleData>();

        puzzle.puzzleID =
    GetNextPuzzleID();

        puzzle.difficulty =
            builder.difficulty;

        Transform root =
            builder.puzzleRoot;

        if (root == null)
        {
            Debug.LogError(
                "Puzzle Root has not been assigned."
            );

            return;
        }

        if (builder.pumpkin != null)
        {
            puzzle.pumpkinPosition =
                builder.pumpkin.localPosition;
        }

        foreach (
            Transform child in root)
        {
            PuzzleBlock block =
                child.GetComponent<PuzzleBlock>();

            if (block == null)
                continue;

            BlockData data =
                new BlockData();

            data.type =
                block.type;

            data.role =
                block.role;

            data.position =
                child.localPosition;

            data.rotation =
                child.localEulerAngles.z;

            data.scale =
                child.localScale;

            data.critical =
                block.critical;

            puzzle.blocks.Add(data);
        }

        string folder =
            builder.saveFolder;

        if (!AssetDatabase.IsValidFolder(folder))
        {
            CreateFolderRecursive(folder);
        }

        string safeName =
            MakeSafeFileName(
                builder.puzzleName
            );

        string path =
            $"{folder}/{safeName}.asset";

        AssetDatabase.CreateAsset(
            puzzle,
            path
        );

        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject =
            puzzle;

        Debug.Log(
            $"Puzzle saved: {path}"
        );
    }
    */

    private void SavePuzzle(PuzzleBuilder builder)
    {
        if (builder.puzzleRoot == null)
        {
            Debug.LogError(
                "Puzzle Root has not been assigned."
            );

            return;
        }

        int puzzleID =
            builder.GetNextPuzzleID();

        PuzzleData puzzle =
            ScriptableObject.CreateInstance<PuzzleData>();

        puzzle.puzzleID =
            puzzleID;

        puzzle.puzzleName =
            $"Puzzle_{puzzleID:000}";

        puzzle.difficulty =
            builder.difficulty;

        // ---------------------------------------------------------
        // PUMPKIN
        // ---------------------------------------------------------

      
        if (builder.pumpkin != null)
        {
            puzzle.pumpkinType =
                builder.pumpkinType;

            puzzle.pumpkinPosition =
                builder.pumpkin.localPosition;
        }

        // ---------------------------------------------------------
        // BLOCKS
        // ---------------------------------------------------------

        foreach (
            Transform child in builder.puzzleRoot)
        {
            PuzzleBlock block =
                child.GetComponent<PuzzleBlock>();

            if (block == null)
                continue;

            BlockData data =
                new BlockData();

            data.type =
                block.type;

            data.role =
                block.role;

            data.position =
                child.localPosition;

            data.rotation =
                child.localEulerAngles.z;

            data.scale =
                child.localScale;

            data.critical =
                block.critical;

            puzzle.blocks.Add(data);
        }

        // ---------------------------------------------------------
        // CREATE FOLDER
        // ---------------------------------------------------------

        string folder =
            builder.saveFolder;

        if (!AssetDatabase.IsValidFolder(folder))
        {
            CreateFolderRecursive(folder);
        }

        // ---------------------------------------------------------
        // FILE NAME
        // ---------------------------------------------------------

        string fileName =
            $"Puzzle_{puzzleID:000}.asset";

        string path =
            $"{folder}/{fileName}";

        // ---------------------------------------------------------
        // SAVE
        // ---------------------------------------------------------

        AssetDatabase.CreateAsset(
            puzzle,
            path
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Select newly created puzzle
        EditorUtility.FocusProjectWindow();

        Selection.activeObject =
            puzzle;

        Debug.Log(
            $"Saved {puzzle.puzzleName} " +
            $"to {path}"
        );

        // ---------------------------------------------------------
        // PREPARE BUILDER FOR NEXT PUZZLE
        // ---------------------------------------------------------

        builder.puzzleName =
            $"Puzzle_{puzzleID + 1:000}";

        EditorUtility.SetDirty(builder);

        AssetDatabase.SaveAssets();
    }
    private int GetNextPuzzleID()
    {
        string[] guids =
            AssetDatabase.FindAssets(
                "t:PuzzleData",
                new[] { "Assets/Puzzles" }
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
    private void ClearPuzzle(
        PuzzleBuilder builder)
    {
        Transform root =
            builder.puzzleRoot;

        if (root == null)
            return;

        while (root.childCount > 0)
        {
            DestroyImmediate(
                root.GetChild(0).gameObject
            );
        }
    }

    private void CreateFolderRecursive(
        string path)
    {
        string[] folders =
            path.Split('/');

        string current = folders[0];

        for (int i = 1;
             i < folders.Length;
             i++)
        {
            string next =
                current + "/" + folders[i];

            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(
                    current,
                    folders[i]
                );
            }

            current = next;
        }
    }

    private string MakeSafeFileName(
        string fileName)
    {
        foreach (
            char c in Path.GetInvalidFileNameChars())
        {
            fileName =
                fileName.Replace(
                    c.ToString(),
                    "_"
                );
        }

        return fileName;
    }
}