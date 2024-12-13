#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HealthBar))]
public class HealthBarEditor : Editor
{
    public override void OnInspectorGUI() // Override the default inspector GUI
    {
        //HealthBar healthBar = (HealthBar)target; // Get the HealthBar component from the target object
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>(); // Find the PlayerHealth component in the scene
        if (playerHealth == null) // Check if the PlayerHealth component is found in the scene
        {
            EditorGUILayout.HelpBox("PlayerHealth component not found in the scene.", MessageType.Error); // Display an error message in the inspector
            return; // Exit the inspector GUI
        }

        DrawDefaultInspector(); // Draw default inspector

        if (GUI.changed) // Check for changes in the inspector
        {
            SceneView.RepaintAll(); // Update the scene view for visual feedback
        }
    }
}
#endif