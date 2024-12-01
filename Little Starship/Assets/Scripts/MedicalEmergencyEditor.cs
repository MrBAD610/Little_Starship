using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MedicalEmergency))]
public class MedicalEmergencyEditor : Editor
{
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
            emergency.desiredRandomRegions = EditorGUILayout.IntSlider(
                "Desired Random Regions",
                emergency.desiredRandomRegions,
                1,
                emergency.randomAffectedRegions.Count
            );
        }

        // Apply any changes made to serialized properties
        serializedObject.ApplyModifiedProperties();
    }
}
