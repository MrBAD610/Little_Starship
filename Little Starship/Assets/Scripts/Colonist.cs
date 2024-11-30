using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colonist : MonoBehaviour
{
    public List<MedicalEmergency> emergencies;
    public List<List<BodyRegion>> colonistRegions = new List<List<BodyRegion>>();
    public List<List<float>> colonistStabilizationTimes = new List<List<float>>();
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
            colonistStabilizationTimes.Add(new List<float>());  // Initialize new list of stabilization times

            // Skip if the random regions are already chosen
            if (emergency.presetAffectedRegions != null && emergency.presetAffectedRegions.Count > 0) 
            {
                colonistRegions[regionAndTimeListIndex] = emergency.presetAffectedRegions;

                for (int regionIndex = 0; regionIndex < colonistRegions[regionAndTimeListIndex].Count; regionIndex++)
                {
                    colonistStabilizationTimes[regionAndTimeListIndex].Add(emergency.stabilizationTime);
                }
            }

            if (emergency.randomAffectedRegions == null || emergency.randomAffectedRegions.Count == 0) continue;

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
                colonistStabilizationTimes[regionAndTimeListIndex].Add(emergency.stabilizationTime);
            }
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
