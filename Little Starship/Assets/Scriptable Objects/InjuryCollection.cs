using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Injury Collection")]
public class InjuryCollection : ScriptableObject
{
    public string displayedName;    // What will be displayed in the UI
    public BodyRegion[] fullBodyCollection; // Array to hold all body regions
    public List<BodyRegion> presetAffectedRegions; // List of preset regions affected by the medical emergency
    public List<BodyRegion> randomAffectedRegions; // List of possible regions affected by the medical emergency
    public int desiredRandomRegions; // Number of random regions needed
    public float stabilizationTime; // Time to stabilize all regions in this injury collection

    // Called when the script is loaded or a value changes in the inspector
    private void OnEnable()
    {
        InitializeFullBodyCollection(); // Initialize the full body collection
        InitializeInjuredRegions();     // Initialize the injured regions
    }

    private void InitializeFullBodyCollection()
    {
        // Get all BodyRegion ScriptableObjects from the Resources/BodyRegions folder
        BodyRegion[] bodyRegions = Resources.LoadAll<BodyRegion>("Body Regions");

        // Initialize the fullBodyCollection array with the size of the RegionType enum
        fullBodyCollection = new BodyRegion[System.Enum.GetValues(typeof(BodyRegion.RegionType)).Length];

        foreach (BodyRegion bodyRegion in bodyRegions)
        {
            int index = (int)bodyRegion.bodyRegionType;
            if (index >= 0 && index < fullBodyCollection.Length)
            {
                fullBodyCollection[index] = bodyRegion;
            }
            else
            {
                Debug.LogWarning($"Invalid body region type index: {index} for region: {bodyRegion.name}");
            }
        }
    }

    private void InitializeInjuredRegions()
    {
        if (presetAffectedRegions == null)
        {
            presetAffectedRegions = new List<BodyRegion>();
        }

        if (randomAffectedRegions != null && randomAffectedRegions.Count > 0)
        {
            ChooseFromRandomRegions(); // Choose from random regions and add to the preset list
        }

        HashSet<BodyRegion.RegionType> uniqueRegionTypes = new HashSet<BodyRegion.RegionType>();

        for (int i = presetAffectedRegions.Count - 1; i >= 0; i--)
        {
            BodyRegion region = presetAffectedRegions[i];
            if (!uniqueRegionTypes.Add(region.bodyRegionType))
            {
                presetAffectedRegions.RemoveAt(i); // Remove duplicate region
            }
        }

        foreach (BodyRegion region in presetAffectedRegions)
        {
            region.regionInjuryStatus = InjuryStatus.Injured; // Set the injury status for the preset region
            region.stabilizationTime = stabilizationTime; // Set the stabilization time for the preset region
            int index = (int)region.bodyRegionType;
            if (index >= 0 && index < fullBodyCollection.Length)
            {
                fullBodyCollection[index] = region; // Set the region in the full body collection to the preset region
            }
            else
            {
                Debug.LogWarning($"Invalid body region type index: {index} for region: {region.name}");
            }
        }
    }

    private void ChooseFromRandomRegions()
    {
        if (presetAffectedRegions == null)
        {
            presetAffectedRegions = new List<BodyRegion>();
        }

        HashSet<int> selectedIndices = new HashSet<int>(); // Ensure unique indices
        HashSet<BodyRegion.RegionType> uniqueRegionTypes = new HashSet<BodyRegion.RegionType>();

        while (selectedIndices.Count < desiredRandomRegions && selectedIndices.Count < randomAffectedRegions.Count)
        {
            int randomIndex = Random.Range(0, randomAffectedRegions.Count); // Choose a random index
            BodyRegion randomRegion = randomAffectedRegions[randomIndex];

            if (uniqueRegionTypes.Add(randomRegion.bodyRegionType))
            {
                selectedIndices.Add(randomIndex);
            }
        }

        foreach (int index in selectedIndices)
        {
            BodyRegion randomRegion = randomAffectedRegions[index];
            randomRegion.regionInjuryStatus = InjuryStatus.Injured; // Set the injury status for the random region
            randomRegion.stabilizationTime = stabilizationTime; // Set the stabilization time for the random region
            if (!presetAffectedRegions.Contains(randomRegion))
            {
                presetAffectedRegions.Add(randomRegion); // Add the chosen region to the preset list
            }
        }
    }
}
