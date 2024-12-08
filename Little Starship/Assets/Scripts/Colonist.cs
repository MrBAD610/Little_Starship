using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colonist : MonoBehaviour
{
    public List<MedicalEmergency> emergencies;
    public List<List<BodyRegion>> colonistRegions = new List<List<BodyRegion>>();
    public List<List<float>> neededTimeForEachRegion = new List<List<float>>();
    public List<float> neededTimeForEachEmergency = new List<float>();
    public float neededTimeToStabilizeColonist = 0f;
    public float totalStabilizationProgress = 0f;
    public List<List<float>> progressOfRegions = new List<List<float>>();
    public List<float> progressOfEmergencies = new List<float>();

    public List<InjuryCollection> injuryCollectionsInput = new List<InjuryCollection>();
    public List<InjuryCollection> colonistInjuryCollections = new List<InjuryCollection>();
    public Rigidbody ColonistRigidbody { get; private set; }

    private void Awake()
    {
        ColonistRigidbody = GetComponent<Rigidbody>();
        InitializeInjuries(); // Initialize random regions on Awake
    }

    private void InitializeInjuries()
    {
        for (int i = 0; i < injuryCollectionsInput.Count; i++)
        {
            InjuryCollection currentinjuryCollection = injuryCollectionsInput[i];
            InjuryCollection newInjuryCollection = ScriptableObject.CreateInstance<InjuryCollection>();

            if (currentinjuryCollection == null)
            {
                Debug.LogError("Injury collection is null.");
                return;
            }

            List<BodyRegion> currentPresetAffectedRegions = currentinjuryCollection.presetAffectedRegions;
            List<BodyRegion> currentRandomAffectedRegions = currentinjuryCollection.randomAffectedRegions;
            int currentDesiredRandomRegions = currentinjuryCollection.desiredRandomRegions;
            float currentStabilizationTime = currentinjuryCollection.stabilizationTime;

            List<BodyRegion> newPresetAffectedRegions = new List<BodyRegion>();

            if (currentPresetAffectedRegions != null)
            {
                foreach (BodyRegion currentPresetRegion in currentPresetAffectedRegions)
                {
                    BodyRegion newPresetRegion = ScriptableObject.CreateInstance<BodyRegion>();
                    newPresetRegion.bodyRegionType = currentPresetRegion.bodyRegionType;
                    newPresetRegion.regionInjuryStatus = InjuryStatus.Injured; // Set the injury status for the preset region
                    newPresetRegion.stabilizationTime = currentStabilizationTime; // Set the stabilization time for the preset region
                    newPresetAffectedRegions.Add(newPresetRegion);
                }
            }

            if (currentRandomAffectedRegions != null && currentRandomAffectedRegions.Count > 0)
            {
                HashSet<int> selectedIndices = new HashSet<int>(); // Ensure unique indices
                HashSet<BodyRegion.RegionType> uniqueRandomRegionTypes = new HashSet<BodyRegion.RegionType>();

                while (selectedIndices.Count < currentDesiredRandomRegions && selectedIndices.Count < currentRandomAffectedRegions.Count) // Choose random regions
                {
                    int randomIndex = Random.Range(0, currentRandomAffectedRegions.Count); // Choose a random index
                    BodyRegion randomRegion = currentRandomAffectedRegions[randomIndex];
                    if (uniqueRandomRegionTypes.Add(randomRegion.bodyRegionType))
                    {
                        selectedIndices.Add(randomIndex);
                    }
                }

                foreach (int index in selectedIndices)
                {
                    BodyRegion randomRegion = ScriptableObject.CreateInstance<BodyRegion>();
                    randomRegion.bodyRegionType = currentRandomAffectedRegions[index].bodyRegionType;
                    randomRegion.regionInjuryStatus = InjuryStatus.Injured; // Set the injury status for the random region
                    randomRegion.stabilizationTime = currentStabilizationTime; // Set the stabilization time for the random region
                    if (!newPresetAffectedRegions.Contains(randomRegion))
                    {
                        newPresetAffectedRegions.Add(randomRegion); // Add the chosen region to the preset list
                    }
                }
            }

            HashSet<BodyRegion.RegionType> uniqueRegionTypes = new HashSet<BodyRegion.RegionType>();

            for (int j = newPresetAffectedRegions.Count - 1; j >= 0; j--)
            {
                BodyRegion region = newPresetAffectedRegions[j];
                if (!uniqueRegionTypes.Add(region.bodyRegionType))
                {
                    newPresetAffectedRegions.RemoveAt(j); // Remove duplicate region
                }
            }

            foreach (BodyRegion region in newPresetAffectedRegions)
            {
                int index = (int)region.bodyRegionType;
                if (index >= 0 && index < newInjuryCollection.injuredBodyCollection.Length)
                {
                    newInjuryCollection.injuredBodyCollection[index] = region; // Set the region in the full body collection to the preset region
                }
                else
                {
                    Debug.LogWarning($"Invalid body region type index: {index} for region: {region.name}");
                }
            }

            colonistInjuryCollections.Add(newInjuryCollection);
        }
    }

    //private void InitializeInjuries()
    //{
    //    totalStabilizationProgress = 0f;
    //    int regionAndTimeListIndex = 0;

    //    foreach (var emergency in emergencies)
    //    {
    //        colonistRegions.Add(new List<BodyRegion>());        // Initialize new list of body regions
    //        neededTimeForEachRegion.Add(new List<float>());     // Initialize new list of stabilization times
    //        progressOfRegions.Add(new List<float>());           // Initialize new list storing region progresses
    //        float emergencyTotalTime = 0f;
    //        string regionTimeLog = $"{gameObject.name}: required region time at index {regionAndTimeListIndex} is ";
    //        string regionProgressLog = $"{gameObject.name}: region progress at index {regionAndTimeListIndex} is ";

    //        // Skip if the random regions are already chosen
    //        if (emergency.presetAffectedRegions != null && emergency.presetAffectedRegions.Count > 0) 
    //        {
    //            colonistRegions[regionAndTimeListIndex] = emergency.presetAffectedRegions;

    //            for (int regionIndex = 0; regionIndex < colonistRegions[regionAndTimeListIndex].Count; regionIndex++)
    //            {
    //                neededTimeForEachRegion[regionAndTimeListIndex].Add(emergency.currentStabilizationTime);
    //                progressOfRegions[regionAndTimeListIndex].Add(0f);
    //                regionTimeLog += $"({emergency.currentStabilizationTime})";
    //                regionProgressLog += $"({0f})";
    //                emergencyTotalTime += emergency.currentStabilizationTime;
    //            }
    //        }

    //        if (emergency.randomAffectedRegions == null || emergency.randomAffectedRegions.Count == 0)
    //        {
    //            Debug.Log(regionTimeLog);
    //            Debug.Log($"{gameObject.name}: required emergency time at index {regionAndTimeListIndex} is {emergencyTotalTime}");
    //            Debug.Log($"{gameObject.name}: emergency progress at index {regionAndTimeListIndex} is {0f}");
    //            neededTimeForEachEmergency.Add(emergencyTotalTime);
    //            progressOfEmergencies.Add(0f);
    //            continue;
    //        }

    //        HashSet<int> selectedIndices = new HashSet<int>();

    //        // Ensure unique indices are chosen
    //        while (selectedIndices.Count < emergency.desiredRandomRegions && selectedIndices.Count < emergency.randomAffectedRegions.Count)
    //        {
    //            int randomIndex = Random.Range(0, emergency.randomAffectedRegions.Count);
    //            selectedIndices.Add(randomIndex);
    //        }

    //        // Add the chosen regions to the emergency's preset list
    //        foreach (int index in selectedIndices)
    //        {
    //            colonistRegions[regionAndTimeListIndex].Add(emergency.randomAffectedRegions[index]);
    //            neededTimeForEachRegion[regionAndTimeListIndex].Add(emergency.currentStabilizationTime);
    //            progressOfRegions[regionAndTimeListIndex].Add(0f);
    //            regionTimeLog += $"({emergency.currentStabilizationTime})";
    //            regionProgressLog += $"({0f})";
    //            emergencyTotalTime += emergency.currentStabilizationTime;
    //        }

    //        Debug.Log(regionTimeLog);
    //        Debug.Log(regionProgressLog);
    //        Debug.Log($"{gameObject.name}: required emergency time at index {regionAndTimeListIndex} is {0f}");
    //        Debug.Log($"{gameObject.name}: emergency progress at index {regionAndTimeListIndex} is {emergencyTotalTime}");
    //        neededTimeForEachEmergency.Add(emergencyTotalTime);
    //        neededTimeToStabilizeColonist += emergencyTotalTime;
    //        progressOfEmergencies.Add(0f);
    //        ++regionAndTimeListIndex;
    //    }

    //    //Debug.Log($"Random regions initialized for Colonist: {gameObject.name}");
    //}

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerInventory playerInventory = other.gameObject.GetComponent<PlayerInventory>();
            if (playerInventory != null)
            {
                playerInventory.CollectColonist(this);
            }
        }
    }
}
