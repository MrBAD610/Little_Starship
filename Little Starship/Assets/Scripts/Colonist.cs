using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colonist : MonoBehaviour
{
    public List<MedicalEmergency> emergencies;
    public List<List<BodyRegion>> colonistRegions = new List<List<BodyRegion>>();
    public List<List<float>> neededTimeForEachRegion = new List<List<float>>();
    public List<float> neededTimeForEachEmergency = new List<float>();
    public List<List<float>> progressOfRegions = new List<List<float>>();
    public List<float> progressOfEmergencies = new List<float>();
    public Rigidbody ColonistRigidbody { get; private set; }

    private void Awake()
    {
        ColonistRigidbody = GetComponent<Rigidbody>(); 
        InitializeRegionsAndTimes(); // Initialize random regions on Awake
    }

    private void InitializeRegionsAndTimes()
    {
        int regionAndTimeListIndex = 0;

        foreach (var emergency in emergencies)
        {
            colonistRegions.Add(new List<BodyRegion>());        // Initialize new list of body regions
            neededTimeForEachRegion.Add(new List<float>());     // Initialize new list of stabilization times
            progressOfRegions.Add(new List<float>());           // Initialize new list storing region progresses
            float emergencyTotalTime = 0f;
            string regionTimeLog = $"{gameObject.name}: required region time at index {regionAndTimeListIndex} is ";
            string regionProgressLog = $"{gameObject.name}: region progress at index {regionAndTimeListIndex} is ";

            // Skip if the random regions are already chosen
            if (emergency.presetAffectedRegions != null && emergency.presetAffectedRegions.Count > 0) 
            {
                colonistRegions[regionAndTimeListIndex] = emergency.presetAffectedRegions;

                for (int regionIndex = 0; regionIndex < colonistRegions[regionAndTimeListIndex].Count; regionIndex++)
                {
                    neededTimeForEachRegion[regionAndTimeListIndex].Add(emergency.stabilizationTime);
                    progressOfRegions[regionAndTimeListIndex].Add(0f);
                    regionTimeLog += $"({emergency.stabilizationTime})";
                    regionProgressLog += $"({0f})";
                    emergencyTotalTime += emergency.stabilizationTime;
                }
            }

            if (emergency.randomAffectedRegions == null || emergency.randomAffectedRegions.Count == 0)
            {
                Debug.Log(regionTimeLog);
                Debug.Log($"{gameObject.name}: required emergency time at index {regionAndTimeListIndex} is {emergencyTotalTime}");
                Debug.Log($"{gameObject.name}: emergency progress at index {regionAndTimeListIndex} is {0f}");
                neededTimeForEachEmergency.Add(emergencyTotalTime);
                progressOfEmergencies.Add(0f);
                continue;
            }

            HashSet<int> selectedIndices = new HashSet<int>();

            // Ensure unique indices are chosen
            while (selectedIndices.Count < emergency.desiredRandomRegions && selectedIndices.Count < emergency.randomAffectedRegions.Count)
            {
                int randomIndex = Random.Range(0, emergency.randomAffectedRegions.Count);
                selectedIndices.Add(randomIndex);
            }

            // Add the chosen regions to the emergency's preset list
            foreach (int index in selectedIndices)
            {
                colonistRegions[regionAndTimeListIndex].Add(emergency.randomAffectedRegions[index]);
                neededTimeForEachRegion[regionAndTimeListIndex].Add(emergency.stabilizationTime);
                progressOfRegions[regionAndTimeListIndex].Add(0f);
                regionTimeLog += $"({emergency.stabilizationTime})";
                regionProgressLog += $"({0f})";
                emergencyTotalTime += emergency.stabilizationTime;
            }

            Debug.Log(regionTimeLog);
            Debug.Log(regionProgressLog);
            Debug.Log($"{gameObject.name}: required emergency time at index {regionAndTimeListIndex} is {0f}");
            Debug.Log($"{gameObject.name}: emergency progress at index {regionAndTimeListIndex} is {emergencyTotalTime}");
            neededTimeForEachEmergency.Add(emergencyTotalTime);
            progressOfEmergencies.Add(0f);
            ++regionAndTimeListIndex;
        }

        //Debug.Log($"Random regions initialized for Colonist: {gameObject.name}");
    }

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
