#if UNITY_EDITOR
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

        // Check for changes in the inspector
        if (GUI.changed)
        {
            progressBar.currentProgress = Mathf.Clamp(progressBar.currentProgress, 0f, progressBar.timeTillCompletion);

            // Update the scene view for visual feedback
            SceneView.RepaintAll();
        }
    }
}
#endif
