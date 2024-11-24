using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colonist : MonoBehaviour
{
    public List<MedicalEmergency> emergencies;
    public Rigidbody ColonistRigidbody { get; private set; }

    private void Awake()
    {
        ColonistRigidbody = GetComponent<Rigidbody>(); 
        InitializeRandomRegions(); // Initialize random regions on Awake
    }

    private void InitializeRandomRegions()
    {
        foreach (var emergency in emergencies)
        {
            // Skip if the random regions are already chosen
            if (emergency.presetAffectedRegions != null && emergency.presetAffectedRegions.Count > 0)
                continue;

            if (emergency.randomAffectedRegions == null || emergency.randomAffectedRegions.Count == 0)
                continue;

            HashSet<int> selectedIndices = new HashSet<int>();

            // Ensure unique indices are chosen
            while (selectedIndices.Count < emergency.desiredRandomRegions && selectedIndices.Count < emergency.randomAffectedRegions.Count)
            {
                int randomIndex = Random.Range(0, emergency.randomAffectedRegions.Count);
                selectedIndices.Add(randomIndex);
            }

            // Add the chosen regions to the emergency's preset list
            emergency.presetAffectedRegions = new List<BodyRegion>();
            foreach (int index in selectedIndices)
            {
                emergency.presetAffectedRegions.Add(emergency.randomAffectedRegions[index]);
            }
        }

        Debug.Log($"Random regions initialized for Colonist: {gameObject.name}");
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
