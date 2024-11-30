using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CircularProgressBar))]
public class CircularProgressBarEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CircularProgressBar progressBar = (CircularProgressBar)target;

        // Draw default inspector
        DrawDefaultInspector();

        // Update the progress bar on changes
        if (GUI.changed)
        {
            progressBar.currentProgress = Mathf.Clamp(progressBar.currentProgress, 0f, progressBar.timeTillCompletion);
            EditorApplication.QueuePlayerLoopUpdate(); // Update the scene safely
            SceneView.RepaintAll(); // Ensure the Scene view reflects the changes
        }
    }
}
