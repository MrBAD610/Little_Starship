using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Injury Collection")]
public class InjuryCollection : ScriptableObject
{
    public string displayedName;    // What will be displayed in the UI
    public BodyRegion[] injuredBodyCollection; // Array to hold all body regions
    public List<BodyRegion> presetAffectedRegions; // List of preset regions affected by the medical emergency
    public List<BodyRegion> randomAffectedRegions; // List of possible regions affected by the medical emergency
    public int desiredRandomRegions; // Number of random regions needed
    public float progressSum = 0.0f; // Sum of progress for all regions in this injury collection
    public string stabilizedRegionTotal = "0/0"; // Total number of stabilized regions
    public float stabilizationTime = 0.0f; // Time to stabilize one region in this injury collection

    // Called when the script is loaded or a value changes in the inspector
    private void OnEnable()
    {
        InitializeInjuredBodyCollection(); // Initialize injured body collection
        GetProgressSum(); // Calculate the sum of progress for all regions
    }

    // Initialize the injured body collection with default values
    private void InitializeInjuredBodyCollection()
    {
        FullBodyCollection fullBodyCollection = Resources.Load<FullBodyCollection>("Full Body Collection");
        if (fullBodyCollection == null)
        {
            Debug.LogError("Full Body Collection resource not found.");
            return;
        }

        BodyRegion[] fullBodyArray = fullBodyCollection.bodyRegionsArray;
        if (fullBodyArray == null || fullBodyArray.Length == 0)
        {
            Debug.LogError("Full Body Collection is empty or null.");
            return;
        }

        injuredBodyCollection = new BodyRegion[fullBodyArray.Length];

        for (int i = 0; i < fullBodyArray.Length; i++)
        {
            BodyRegion currentRegion = fullBodyArray[i];
            if (currentRegion == null)
            {
                Debug.LogError("Body region is null at index " + i);
                continue;
            }

            BodyRegion newRegion = ScriptableObject.CreateInstance<BodyRegion>();
            newRegion.bodyRegionType = currentRegion.bodyRegionType;
            newRegion.regionInjuryStatus = InjuryStatus.Unharmed;
            newRegion.stabilizationTime = 0f;
            newRegion.stabilizationProgress = 0f;
            injuredBodyCollection[i] = newRegion;
        }
    }

    public void ResetInjuryCollection()
    {
        for (int i = 0; i < injuredBodyCollection.Length; i++)
        {
            injuredBodyCollection[i].regionInjuryStatus = InjuryStatus.Unharmed;
            injuredBodyCollection[i].stabilizationProgress = 0f;
        }
    }

    public void GetProgressSum()
    {
        progressSum = 0.0f;
        foreach (BodyRegion region in injuredBodyCollection)
        {
            progressSum += region.stabilizationProgress;
        }
    }

    public void UpdateStabilizedRegionTotal()
    {
        int totalStabilized = 0;
        int totalInjured = 0;
        foreach (BodyRegion region in injuredBodyCollection)
        {
            if (region.regionInjuryStatus == InjuryStatus.Stabilized)
            {
                totalStabilized++;
            }
            if (region.regionInjuryStatus != InjuryStatus.Unharmed)
            {
                totalInjured++;
            }
        }
        stabilizedRegionTotal = totalStabilized + "/" + totalInjured;
    }
}
