#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MedicalEmergency))]
public class MedicalEmergencyEditor : Editor
{
    SerializedProperty desiredRandomRegionsProp;

    private void OnEnable()
    {
        desiredRandomRegionsProp = serializedObject.FindProperty(nameof(MedicalEmergency.desiredRandomRegions));
    }

    public override void OnInspectorGUI()
    {
        MedicalEmergency emergency = (MedicalEmergency)target;

        // Render all fields except 'desiredRandomRegions'
        SerializedProperty property = serializedObject.GetIterator();
        property.NextVisible(true); // Move to the first property
        while (property.NextVisible(false)) // Iterate over all serialized properties
        {
            if (property.name != nameof(MedicalEmergency.desiredRandomRegions))
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }

        // Add the custom slider for 'desiredRandomRegions'
        if (emergency.randomAffectedRegions != null && emergency.randomAffectedRegions.Count > 0)
        {
            EditorGUILayout.IntSlider(
                desiredRandomRegionsProp,
                1,
                emergency.randomAffectedRegions.Count,
                new GUIContent("Desired Random Regions")
            );
        }
        else
        {
            EditorGUILayout.HelpBox("Random Affected Regions list is empty or null.", MessageType.Warning);
        }

        // Apply any changes made to serialized properties
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
