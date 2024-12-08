using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Injury Collection")]
public class InjuryCollection : ScriptableObject
{
    public string displayedName;    // What will be displayed in the UI
    public BodyRegion[] fullBodyCollection; // Array to hold all body regions
    public List<BodyRegion> presetAffectedRegions; // list of preset regions affected by the medical emergency
    public List<BodyRegion> randomAffectedRegions; // list of possible regions affected by the medical emergency
    public int desiredRandomRegions; // Number of random regions needed
    public float stabilizationTime; // Time to stabilize all regions in this injury collection

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        InitializeFullBodyCollection(); // Initialize the full body collection
        InitializeInjuredRegions(); // Initialize the injured regions on Awake
    }

    // Start is called before the first frame update
    void Start()
    {
        DebugFullBodyCollection(); // Debug the full body collection
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void InitializeFullBodyCollection()
    {
        BodyRegion.RegionType[] regionTypes = (BodyRegion.RegionType[])System.Enum.GetValues(typeof(BodyRegion.RegionType));
        fullBodyCollection = new BodyRegion[regionTypes.Length];

        for (int i = 0; i < regionTypes.Length; i++)
        {
            BodyRegion bodyRegion = ScriptableObject.CreateInstance<BodyRegion>();
            bodyRegion.bodyRegionType = regionTypes[i];
            fullBodyCollection[i] = bodyRegion;
        }
    }

    private void InitializeInjuredRegions() // Initialize the injured regions
    {
        if (presetAffectedRegions != null && presetAffectedRegions.Count > 0) // Check if there are preset regions
        {
            return; // If there are preset regions, return
        }

        if (randomAffectedRegions != null && randomAffectedRegions.Count > 0) // Check if there are random regions
        {
            ChooseFromRandomRegions(); // If there are random regions, choose from them and add to the preset list
        }

        foreach (BodyRegion region in presetAffectedRegions) // Loop through the preset regions
        {
            fullBodyCollection[(int)region.bodyRegionType] = region; // Set the region in the full body collection to the preset region
        }
    }

    private void ChooseFromRandomRegions()
    {
        HashSet<int> selectedIndices = new HashSet<int>(); // Use a hash set to ensure unique indices

        while (selectedIndices.Count < desiredRandomRegions && selectedIndices.Count < randomAffectedRegions.Count) // Loop until the desired number of regions are chosen
        {
            int randomIndex = Random.Range(0, randomAffectedRegions.Count); // Choose a random index
            selectedIndices.Add(randomIndex); // Add the index to the hash set
        }

        foreach (int index in selectedIndices) // Loop through the selected indices
        {
            if (presetAffectedRegions.Contains(randomAffectedRegions[index])) // Check if the chosen region is already in the preset list
            {
                continue; // Skip if the region is already in the preset list
            }
            else // If the region is not in the preset list
            {
                presetAffectedRegions.Add(randomAffectedRegions[index]); // Add the chosen regions to the emergency's preset list
            }
        }
    }

    private void DebugFullBodyCollection()
    {
        foreach (BodyRegion region in fullBodyCollection)
        {
            Debug.Log($"{region.bodyRegionType}: {region.regionInjuryStatus}, {region.stabilizationTime}");
        }
    }
}
