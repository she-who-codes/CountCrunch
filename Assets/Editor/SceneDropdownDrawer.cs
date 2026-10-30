#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[CustomPropertyDrawer(typeof(SceneDropdownAttribute))]
public class SceneDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use [SceneDropdown] only on strings.");
            return;
        }

        // Gather all scene names from the Build Settings
        List<string> sceneNames = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                // Extract just the file name without ".unity"
                string name = Path.GetFileNameWithoutExtension(scene.path);
                sceneNames.Add(name);
            }
        }

        if (sceneNames.Count == 0)
        {
            EditorGUI.LabelField(position, label.text, "No active scenes in Build Settings!");
            return;
        }

        // Find which index matches the currently saved string value
        int currentIndex = sceneNames.IndexOf(property.stringValue);
        if (currentIndex < 0) currentIndex = 0; // Default to first scene if not found

        // Draw the dropdown popup
        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, sceneNames.ToArray());

        // Update the string property value if the user changes the selection
        property.stringValue = sceneNames[newIndex];
    }
}
#endif
