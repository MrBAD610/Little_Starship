using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colonist : MonoBehaviour
{
    [SerializeField] private List<InjuryCollection> InitialInjuryCollections = new List<InjuryCollection>();
    public List<InjuryCollection> colonistInjuryCollections = new List<InjuryCollection>();

    public List<float> neededTimeForEachInjuryCollection = new List<float>();
    public List<float> progressOfInjuryCollections = new List<float>();
    public float neededTimeToStabilizeColonist = 0.0f;
    public float totalStabilizationProgress = 0.0f;

    public Rigidbody ColonistRigidbody { get; private set; }

    private void Awake()
    {
        ColonistRigidbody = GetComponent<Rigidbody>();
        InitializeInjuries(); // Initialize random regions on Awake
    }

    private void InitializeInjuries()
    {
        for (int i = 0; i < InitialInjuryCollections.Count; i++)
        {
            InjuryCollection currentinjuryCollection = InitialInjuryCollections[i];
            InjuryCollection newInjuryCollection = ScriptableObject.CreateInstance<InjuryCollection>();

            float currentTotalCollectionTime = 0.0f;

            if (currentinjuryCollection == null)
            {
                Debug.LogError("Injury collection is null.");
                return;
            }

            List<BodyRegion> currentPresetAffectedRegions = currentinjuryCollection.presetAffectedRegions;
            List<BodyRegion> currentRandomAffectedRegions = currentinjuryCollection.randomAffectedRegions;
            int currentDesiredRandomRegions = currentinjuryCollection.desiredRandomRegions;

            List<BodyRegion> newPresetAffectedRegions = new List<BodyRegion>();

            if (currentPresetAffectedRegions != null)
            {
                foreach (BodyRegion currentPresetRegion in currentPresetAffectedRegions)
                {
                    BodyRegion newPresetRegion = ScriptableObject.CreateInstance<BodyRegion>();
                    newPresetRegion.bodyRegionType = currentPresetRegion.bodyRegionType;
                    newPresetRegion.regionInjuryStatus = InjuryStatus.Injured; // Set the injury status for the preset region
                    newPresetRegion.stabilizationTime = currentPresetRegion.stabilizationTime; // Set the stabilization time for the preset region
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
                    randomRegion.stabilizationTime = currentRandomAffectedRegions[index].stabilizationTime; // Set the stabilization time for the random region
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
                    currentTotalCollectionTime += region.stabilizationTime;
                    newInjuryCollection.injuredBodyCollection[index] = region; // Set the region in the full body collection to the preset region
                }
                else
                {
                    Debug.LogWarning($"Invalid body region type index: {index} for region: {region.name}");
                }
            }

            neededTimeForEachInjuryCollection.Add(currentTotalCollectionTime);
            newInjuryCollection.displayedName = currentinjuryCollection.displayedName;
            colonistInjuryCollections.Add(newInjuryCollection);
            progressOfInjuryCollections.Add(0.0f);
        }
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
