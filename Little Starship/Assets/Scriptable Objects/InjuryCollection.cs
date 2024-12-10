using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Injury Collection")]
public class InjuryCollection : ScriptableObject // ScriptableObject class for injury collection
{
    public string displayedName;    // What will be displayed in the UI

    public BodyRegion[] injuredBodyCollection; // Array to hold all body regions
    public List<BodyRegion> presetAffectedRegions; // List of preset regions affected by the medical emergency
    public List<BodyRegion> randomAffectedRegions; // List of possible regions affected by the medical emergency
    
    public int desiredRandomRegions; // Number of random regions needed
    public float stabilizationTime = 0.0f; // Time to stabilize one region in this injury collection
    
    public string stabilizedRegionTotal = "0/0"; // Total number of stabilized regions
    public float progressSum = 0.0f; // Sum of progress for all regions in this injury collection

    public bool isStabilized = false; // Flag to check if the injury collection is stabilized

    private void OnEnable() // Called when the script is loaded or a value changes in the inspector
    {
        InitializeInjuredBodyCollection(); // Initialize injured body collection
    }

    private void InitializeInjuredBodyCollection() // Initialize the injured body collection with default values
    {
        FullBodyCollection fullBodyCollection = Resources.Load<FullBodyCollection>("Full Body Collection"); // Load the full body collection
        
        if (fullBodyCollection == null) // Check if the full body collection resource is found
        {
            Debug.LogError("Full Body Collection resource not found.");
            return;
        }

        BodyRegion[] fullBodyArray = fullBodyCollection.bodyRegionsArray; // Get the array of body regions from the full body collection

        if (fullBodyArray == null || fullBodyArray.Length == 0) // Check if the full body collection is empty or null
        {
            Debug.LogError("Full Body Collection is empty or null.");
            return;
        }

        injuredBodyCollection = new BodyRegion[fullBodyArray.Length]; // Initialize the injured body collection array with the size of the full body collection array

        for (int i = 0; i < fullBodyArray.Length; i++) // Loop through the full body array and initialize the injured body collection
        {
            BodyRegion currentRegion = fullBodyArray[i]; // Get the current body region from the full body array
            
            if (currentRegion == null) // Check if the current region is null
            {
                Debug.LogError("Body region is null at index " + i);
                continue;
            }

            BodyRegion newRegion = ScriptableObject.CreateInstance<BodyRegion>(); // Create a new body region object for the injured body collection
            newRegion.bodyRegionType = currentRegion.bodyRegionType; // Set the body region type of the new region to the current region type
            newRegion.regionInjuryStatus = InjuryStatus.Unharmed; // Set the injury status of the new region to unharmed (default)
            newRegion.stabilizationTime = 0f; // Set the stabilization time of the new region to 0 (default)
            newRegion.stabilizationProgress = 0f; // Set the stabilization progress of the new region to 0 (default)
            injuredBodyCollection[i] = newRegion; // Add the new region to the injured body collection array
        }
    }

    public void GetProgressSum() // Update the progress sum for this injury collection
    {
        progressSum = 0.0f; // Reset the progress sum to 0

        foreach (BodyRegion region in injuredBodyCollection) // Loop through all body regions in the injured body collection
        {
            progressSum += region.stabilizationProgress; // Add the stabilization progress of each region to the progress sum
        }
    }

    public void UpdateStabilizedRegionCount() // Update the stabilized region count for this injury collection
    {
        int totalStabilized = 0; 
        int totalInjured = 0;

        foreach (BodyRegion region in injuredBodyCollection) // Loop through all body regions in the injured body collection
        {
            if (region.regionInjuryStatus == InjuryStatus.Stabilized) // Check if the region is stabilized
            {
                totalStabilized++; // Increment the total stabilized count if the region is stabilized
            }
            if (region.regionInjuryStatus != InjuryStatus.Unharmed) // Check if the region is not unharmed
            {
                totalInjured++; // Increment the total injured count if the region is not unharmed
            }
        }
        stabilizedRegionTotal = totalStabilized + "/" + totalInjured; // Update the stabilized region total string to a ratio of stabilized to non-unharmed regions

        if (totalStabilized == totalInjured) // Check if all regions are stabilized
        {
            isStabilized = true; // Set the isStabilized flag to true
        }
    }

}
