#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerHealth))]
public class PlayerHealthEditor : Editor
{
    public override void OnInspectorGUI() // Override the default inspector GUI
    {
        PlayerHealth playerHealth = (PlayerHealth)target; // Get the PlayerHealth component from the target object
        DrawDefaultInspector(); // Draw default inspector
        if (GUI.changed) // Check for changes in the inspector
        {
            HealthBar[] healthBars = FindObjectsOfType<HealthBar>(); // Find all HealthBar components in the scene
            foreach (HealthBar healthBar in healthBars) // Iterate over all HealthBar components
            {
                healthBar.SetHealth(playerHealth.GetHealthNormalized()); // Update the health bar when the player's health changes
            }
            SceneView.RepaintAll(); // Update the scene view for visual feedback
        }
    }
}
#endif