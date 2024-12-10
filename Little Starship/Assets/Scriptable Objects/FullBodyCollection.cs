using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Full Body Collection", menuName = "Full Body Collection")]
public class FullBodyCollection : ScriptableObject
{
    public BodyRegion[] bodyRegionsArray; // Array to hold all body regions

    private void OnEnable()
    {
        InitializeFullBodyCollection(); // Initialize all body regions on enable
    }

    public void InitializeFullBodyCollection()
    {
        // Get all BodyRegion ScriptableObjects from the Resources/BodyRegions folder
        BodyRegion[] bodyRegions = Resources.LoadAll<BodyRegion>("Body Regions");
        // Initialize the injuredBodyCollection array with the size of the RegionType enum
        bodyRegionsArray = new BodyRegion[System.Enum.GetValues(typeof(BodyRegion.RegionType)).Length];
        foreach (BodyRegion bodyRegion in bodyRegions)
        {
            int index = (int)bodyRegion.bodyRegionType;
            if (index >= 0 && index < bodyRegionsArray.Length)
            {
                bodyRegionsArray[index] = bodyRegion;
            }
            else
            {
                Debug.LogWarning($"Invalid body region type index: {index} for region: {bodyRegion.name}");
            }
        }
    }
}
