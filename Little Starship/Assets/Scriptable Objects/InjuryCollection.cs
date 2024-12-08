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
        // Load all BodyRegion ScriptableObjects from the Resources/BodyRegions folder
        BodyRegion[] bodyRegions = Resources.LoadAll<BodyRegion>("Body Regions");

        // Use a dictionary to ensure unique RegionType entries
        Dictionary<BodyRegion.RegionType, BodyRegion> uniqueBodyRegions = new Dictionary<BodyRegion.RegionType, BodyRegion>();

        foreach (BodyRegion bodyRegion in bodyRegions)
        {
            if (!uniqueBodyRegions.ContainsKey(bodyRegion.bodyRegionType))
            {
                uniqueBodyRegions.Add(bodyRegion.bodyRegionType, bodyRegion);
            }
        }

        // Initialize the fullBodyCollection array with the unique BodyRegion instances
        fullBodyCollection = new BodyRegion[uniqueBodyRegions.Count];
        uniqueBodyRegions.Values.CopyTo(fullBodyCollection, 0);
    }

    private void InitializeInjuredRegions()
    {
        if (presetAffectedRegions == null)
        {
            presetAffectedRegions = new List<BodyRegion>();
        }

        if (presetAffectedRegions.Count == 0 && randomAffectedRegions != null && randomAffectedRegions.Count > 0)
        {
            ChooseFromRandomRegions(); // Choose from random regions and add to the preset list
        }

        foreach (BodyRegion region in presetAffectedRegions)
        {
            fullBodyCollection[(int)region.bodyRegionType] = region; // Set the region in the full body collection to the preset region
        }
    }

    private void ChooseFromRandomRegions()
    {
        if (presetAffectedRegions == null)
        {
            presetAffectedRegions = new List<BodyRegion>();
        }

        HashSet<int> selectedIndices = new HashSet<int>(); // Ensure unique indices

        while (selectedIndices.Count < desiredRandomRegions && selectedIndices.Count < randomAffectedRegions.Count)
        {
            int randomIndex = Random.Range(0, randomAffectedRegions.Count); // Choose a random index
            selectedIndices.Add(randomIndex);
        }

        foreach (int index in selectedIndices)
        {
            BodyRegion randomRegion = randomAffectedRegions[index];
            if (!presetAffectedRegions.Contains(randomRegion))
            {
                presetAffectedRegions.Add(randomRegion); // Add the chosen region to the preset list
            }
        }
    }
}
