using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
public class PuzzleEditorWindow : EditorWindow
{
    private PuzzleDatabase database;

    private PuzzleBuilder builder;

    private BlockType selectedBlockType =
        BlockType.Rectangle;

    private BlockRole selectedRole =
        BlockRole.Base;

    private bool placing = false;

    private GameObject prefabToPlace;

    [MenuItem("Tools/Pumpkin Puzzle Builder")]
    /*public static void ShowWindow()
    {
        PuzzleEditorWindow window =
            GetWindow<PuzzleEditorWindow>(
                "Puzzle Builder"
            );

        window.minSize =
            new Vector2(300, 450);
    }*/
    public static void ShowWindow()
    {
        PuzzleEditorWindow window =
            GetWindow<PuzzleEditorWindow>(
                "Puzzle Builder"
            );

        window.minSize =
            new Vector2(300, 450);

        window.FindPuzzleBuilder();
    }

    private void OnEnable()
    {
        FindPuzzleBuilder();

        EditorSceneManager.sceneOpened +=
            OnSceneOpened;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -=
            OnSceneGUI;
        EditorSceneManager.sceneOpened -=
            OnSceneOpened;
    }

    private void OnSceneOpened(
        UnityEngine.SceneManagement.Scene scene,
        OpenSceneMode mode)
    {
        FindPuzzleBuilder();

        Repaint();
    }

    private void FindPuzzleBuilder()
    {
        builder =
            FindFirstObjectByType<PuzzleBuilder>();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        GUILayout.Label(
            "PUMPKIN PUZZLE BUILDER",
            EditorStyles.boldLabel
        );

        GUILayout.Space(10);

        builder =
            (PuzzleBuilder)EditorGUILayout.ObjectField(
                "Builder",
                builder,
                typeof(PuzzleBuilder),
                true
            );

        if (builder == null)
        {
            EditorGUILayout.HelpBox(
                "Create a GameObject with a PuzzleBuilder component and assign it here.",
                MessageType.Info
            );

            return;
        }

        DrawPumpkinSelection();

        GUILayout.Space(15);

        DrawPuzzleSettings();

        GUILayout.Space(15);

        DrawBlockButtons();

        GUILayout.Space(15);

        DrawRoleSelection();

        GUILayout.Space(15);

        DrawUtilityButtons();

        GUILayout.Space(20);

        DrawDatabaseTools();
    }
    private void DrawPumpkinSelection()
    {
        GUILayout.Label(
            "Pumpkin",
            EditorStyles.boldLabel
        );

        builder.pumpkinType =
            (PumpkinType)EditorGUILayout.EnumPopup(
                "Type",
                builder.pumpkinType
            );

        if (GUILayout.Button(
            "PLACE PUMPKIN",
            GUILayout.Height(30)))
        {
            PlacePumpkin();
        }
    }
    private void DrawDatabaseTools()
    {
        GUILayout.Label(
            "Puzzle Database",
            EditorStyles.boldLabel
        );

        database =
            (PuzzleDatabase)EditorGUILayout.ObjectField(
                "Database",
                database,
                typeof(PuzzleDatabase),
                false
            );

        if (database == null)
        {
            EditorGUILayout.HelpBox(
                "Assign your PuzzleDatabase asset.",
                MessageType.Info
            );

            return;
        }

        if (GUILayout.Button(
            "REFRESH PUZZLE DATABASE",
            GUILayout.Height(40)))
        {
            RefreshPuzzleDatabase();
        }

        GUILayout.Space(5);

        EditorGUILayout.LabelField(
            "Puzzles Found:",
            database.puzzles.Count.ToString()
        );
    }
    private void RefreshPuzzleDatabase()
    {
        if (database == null)
        {
            Debug.LogError(
                "No PuzzleDatabase assigned."
            );

            return;
        }

        string[] guids =
            AssetDatabase.FindAssets(
                "t:PuzzleData",
                new[] { "Assets/Puzzles" }
            );

        List<PuzzleData> foundPuzzles =
            new List<PuzzleData>();

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
                foundPuzzles.Add(puzzle);
            }
        }

        foundPuzzles.Sort(
            (a, b) =>
                a.puzzleID.CompareTo(b.puzzleID)
        );

        Undo.RecordObject(
            database,
            "Refresh Puzzle Database"
        );

        database.puzzles =
            foundPuzzles;

        EditorUtility.SetDirty(database);

        AssetDatabase.SaveAssets();

        Debug.Log(
            $"Puzzle Database refreshed. " +
            $"Found {foundPuzzles.Count} puzzles."
        );
    }
    // =========================================================
    // PUZZLE SETTINGS
    // =========================================================

    private void DrawPuzzleSettings()
    {
        GUILayout.Label(
            "Puzzle Settings",
            EditorStyles.boldLabel
        );

        builder.puzzleName =
            EditorGUILayout.TextField(
                "Puzzle Name",
                builder.puzzleName
            );

        builder.difficulty =
            EditorGUILayout.IntSlider(
                "Difficulty",
                builder.difficulty,
                1,
                100
            );
    }

    // =========================================================
    // BLOCK BUTTONS
    // =========================================================

    private void DrawBlockButtons()
    {
        GUILayout.Label(
            "Add Blocks",
            EditorStyles.boldLabel
        );

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("RECTANGLE"))
        {
            StartPlacement(
                BlockType.Rectangle
            );
        }

        if (GUILayout.Button("TRIANGLE"))
        {
            StartPlacement(
                BlockType.Triangle
            );
        }

        if (GUILayout.Button("CIRCLE"))
        {
            StartPlacement(
                BlockType.Circle
            );
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        if (GUILayout.Button(
            "PUMPKIN",
            GUILayout.Height(30)))
        {
            PlacePumpkin();
        }

        if (placing)
        {
            EditorGUILayout.HelpBox(
                $"Placing {selectedBlockType}. Click in the Scene.",
                MessageType.Info
            );
        }
    }

    // =========================================================
    // ROLE
    // =========================================================

    private void DrawRoleSelection()
    {
        GUILayout.Label(
            "Block Role",
            EditorStyles.boldLabel
        );

        selectedRole =
            (BlockRole)EditorGUILayout.EnumPopup(
                "Role",
                selectedRole
            );
    }

    // =========================================================
    // UTILITIES
    // =========================================================

    private void DrawUtilityButtons()
    {
       if (GUILayout.Button(
            "DELETE SELECTED",
            GUILayout.Height(30)))
        {
            DeleteSelected();
        }

        GUILayout.Space(5);

        if (GUILayout.Button(
            "CLEAR PUZZLE",
            GUILayout.Height(30)))
        {
            ClearPuzzle();
        }

        /* GUILayout.Space(15);

        if (GUILayout.Button(
            "VALIDATE PUZZLE",
            GUILayout.Height(35)))
        {
            Debug.Log(
                "Puzzle validation coming next."
            );
        }

        GUILayout.Space(5);

        if (GUILayout.Button(
            "SAVE PUZZLE",
            GUILayout.Height(40)))
        {
            Debug.Log(
                "Puzzle saving coming next."
            );
        }*/
    }

    // =========================================================
    // PLACEMENT
    // =========================================================

    private void StartPlacement(
        BlockType type)
    {
        selectedBlockType = type;

        switch (type)
        {
            case BlockType.Rectangle:
                prefabToPlace =
                    builder.rectanglePrefab;
                break;

            case BlockType.Triangle:
                prefabToPlace =
                    builder.trianglePrefab;
                break;

            case BlockType.Circle:
                prefabToPlace =
                    builder.circlePrefab;
                break;
        }

        if (prefabToPlace == null)
        {
            Debug.LogError(
                $"No prefab assigned for {type}."
            );

            return;
        }

        placing = true;

        SceneView.duringSceneGui -=
            OnSceneGUI;

        SceneView.duringSceneGui +=
            OnSceneGUI;

        SceneView.RepaintAll();
    }

    // =========================================================
    // SCENE VIEW
    // =========================================================

    private void OnSceneGUI(
        SceneView sceneView)
    {
        if (!placing)
            return;

        Event e = Event.current;

        if (e.type != EventType.MouseDown ||
            e.button != 0 ||
            e.alt)
        {
            return;
        }

        Ray ray =
            HandleUtility.GUIPointToWorldRay(
                e.mousePosition
            );

        Plane plane =
            new Plane(
                Vector3.forward,
                Vector3.zero
            );

        if (!plane.Raycast(
            ray,
            out float distance))
        {
            return;
        }

        Vector3 worldPosition =
            ray.GetPoint(distance);

        CreateBlock(worldPosition);

        e.Use();
    }

    // =========================================================
    // CREATE BLOCK
    // =========================================================

    private void CreateBlock(
        Vector3 position)
    {
        GameObject block =
            (GameObject)PrefabUtility.InstantiatePrefab(
                prefabToPlace
            );

        Undo.RegisterCreatedObjectUndo(
            block,
            "Create Puzzle Block"
        );

        block.transform.position =
            position;

        block.transform.SetParent(
            builder.puzzleRoot
        );

        PuzzleBlock puzzleBlock =
            block.GetComponent<PuzzleBlock>();

        if (puzzleBlock == null)
        {
            puzzleBlock =
                block.AddComponent<PuzzleBlock>();
        }

        puzzleBlock.type =
            selectedBlockType;

        puzzleBlock.role =
            selectedRole;

        Selection.activeGameObject =
            block;

        placing = false;

        SceneView.duringSceneGui -=
            OnSceneGUI;

        SceneView.RepaintAll();
    }

    // =========================================================
    // PUMPKIN
    // =========================================================

    /*private void PlacePumpkin()
    {
        if (builder.pumpkinPrefab == null)
        {
            Debug.LogError(
                "Pumpkin prefab has not been assigned."
            );

            return;
        }

        GameObject pumpkin =
            (GameObject)PrefabUtility.InstantiatePrefab(
                builder.pumpkinPrefab
            );

        Undo.RegisterCreatedObjectUndo(
            pumpkin,
            "Create Pumpkin"
        );

        pumpkin.transform.SetParent(
            builder.puzzleRoot
        );

        pumpkin.transform.position =
            Vector3.zero;

        builder.pumpkin =
            pumpkin.transform;

        Selection.activeGameObject =
            pumpkin;
    }*/
    private void PlacePumpkin()
    {
        GameObject prefab = null;

        switch (builder.pumpkinType)
        {
            case PumpkinType.Orange:
                prefab =
                    builder.orangePumpkinPrefab;
                break;

            case PumpkinType.Green:
                prefab =
                    builder.greenPumpkinPrefab;
                break;

            case PumpkinType.Purple:
                prefab =
                    builder.purplePumpkinPrefab;
                break;
        }

        if (prefab == null)
        {
            Debug.LogError(
                $"No prefab assigned for {builder.pumpkinType} pumpkin."
            );

            return;
        }

        // Remove existing pumpkin if there is one.
        if (builder.pumpkin != null)
        {
            Undo.DestroyObjectImmediate(
                builder.pumpkin.gameObject
            );

            builder.pumpkin = null;
        }

        GameObject pumpkin =
            (GameObject)PrefabUtility.InstantiatePrefab(
                prefab
            );

        Undo.RegisterCreatedObjectUndo(
            pumpkin,
            "Create Pumpkin"
        );

        pumpkin.transform.SetParent(
            builder.puzzleRoot
        );

        pumpkin.transform.position =
            Vector3.zero;

        builder.pumpkin =
            pumpkin.transform;

        Selection.activeGameObject =
            pumpkin;
    }

    // =========================================================
    // DELETE
    // =========================================================

    private void DeleteSelected()
    {
        if (Selection.activeGameObject == null)
            return;

        GameObject selected =
            Selection.activeGameObject;

        if (selected.transform.parent !=
            builder.puzzleRoot)
        {
            Debug.LogWarning(
                "Selected object is not part of this puzzle."
            );

            return;
        }

        Undo.DestroyObjectImmediate(
            selected
        );
    }

    // =========================================================
    // CLEAR
    // =========================================================

    private void ClearPuzzle()
    {
        if (builder.puzzleRoot == null)
            return;

        Undo.RegisterFullObjectHierarchyUndo(
            builder.puzzleRoot.gameObject,
            "Clear Puzzle"
        );

        while (
            builder.puzzleRoot.childCount > 0)
        {
            Undo.DestroyObjectImmediate(
                builder.puzzleRoot.GetChild(0)
                    .gameObject
            );
        }

        builder.pumpkin = null;
    }

   /* private void OnDisable()
    {
        SceneView.duringSceneGui -=
            OnSceneGUI;
    }*/
}