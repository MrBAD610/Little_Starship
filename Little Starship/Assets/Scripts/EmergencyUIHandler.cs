using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EmergencyUIHandler : MonoBehaviour
{
    [Header("Prefabs and Containers")]
    [SerializeField] private GameObject emergencyPrefab; // Prefab for emergencies
    [SerializeField] private GameObject regionPrefab;    // Prefab for regions
    [SerializeField] private GameObject regionGroupPrefab; // Prefab for region vertical groups
    [SerializeField] private Transform listContainer;    // Parent container for emergencies and regions

    private Colonist currentColonist;

    private List<GameObject> emergencyItems = new List<GameObject>();
    private List<GameObject> placeholderItems = new List<GameObject>();
    private List<List<GameObject>> regionItems = new List<List<GameObject>>();
    private List<float> emergencyProgresses = new List<float>();
    private List<List<float>> regionProgresses = new List<List<float>>();
    private List<CircularProgressBar> emergencyProgressBars = new List<CircularProgressBar>();
    private List<List<CircularProgressBar>> regionProgressBars = new List<List<CircularProgressBar>>();

    private int selectedEmergencyIndex = 0;
    private int selectedRegionIndex = 0;
    private int expansionIndex = 0;
    private int progressingEmergencyIndex = -1;
    private int progressingRegionIndex = -1;
    private CircularProgressBar progressingEmergency;
    private CircularProgressBar progressingRegion;
    private bool hasExpanded = false;

    private enum NavigationState { Emergency, Region }
    private NavigationState currentState = NavigationState.Emergency;

    public void DisplayEmergenciesWithRegions(Colonist colonistInput)
    {
        currentColonist = colonistInput;

        List<MedicalEmergency> emergencies = currentColonist.emergencies;
        List<List<BodyRegion>> regions = currentColonist.colonistRegions;
        List<float> emergencyMaxTimes = currentColonist.neededTimeForEachEmergency;
        List<List<float>> regionMaxTimes = currentColonist.neededTimeForEachRegion;

        emergencyProgresses = currentColonist.progressOfEmergencies;
        regionProgresses = currentColonist.progressOfRegions;

        ClearList(); // Ensure the list is empty before repopulating.

        for (int i = 0; i < emergencies.Count; i++)
        {
            if (!ValidateEmergencyData(i, emergencies, emergencyMaxTimes, emergencyProgresses)) return;

            var emergencyItem = CreateEmergencyItem(emergencies[i], emergencyMaxTimes[i], emergencyProgresses[i]);
            emergencyItems.Add(emergencyItem);

            var regionPlaceholder = Instantiate(regionGroupPrefab, listContainer);
            placeholderItems.Add(regionPlaceholder);

            regionItems.Add(new List<GameObject>());
            regionProgressBars.Add(new List<CircularProgressBar>());

            if (i >= regions.Count || regions[i] == null)
            {
                Debug.LogError($"Invalid index or null region at index {i}");
                continue;
            }

            CreateRegionItems(i, regions[i], regionMaxTimes[i], regionProgresses[i], regionPlaceholder);
            regionPlaceholder.SetActive(false);
        }

        HighlightEmergency(0);
    }

    private bool ValidateEmergencyData(int index, List<MedicalEmergency> emergencies, List<float> emergencyMaxTimes, List<float> emergencyProgresses)
    {
        if (emergencies.Count != emergencyMaxTimes.Count || emergencies.Count != emergencyProgresses.Count)
        {
            Debug.LogError($"Mismatch: emergencies.Count ({emergencies.Count}) != emergencyMaxTimes.Count ({emergencyMaxTimes.Count}) or != emergencyProgresses.Count ({emergencyProgresses.Count})");
            return false;
        }
        if (emergencyMaxTimes.Count == 0 || emergencyProgresses.Count == 0)
        {
            Debug.LogWarning($"emergencyMaxTimes ({emergencyMaxTimes.Count}) or emergencyProgresses ({emergencyProgresses.Count}) is empty.");
            return false;
        }
        if (index >= emergencyMaxTimes.Count || index >= emergencyProgresses.Count)
        {
            Debug.LogError($"Index {index} out of range for emergencyProgresses (Count: {emergencyMaxTimes.Count})");
            return false;
        }
        return true;
    }

    private GameObject CreateEmergencyItem(MedicalEmergency emergency, float maxTime, float progress)
    {
        var emergencyItem = Instantiate(emergencyPrefab, listContainer);
        var emergencyText = emergencyItem.GetComponentInChildren<TextMeshProUGUI>();
        emergencyText.text = emergency.emergencyName;

        var emergencyProgressBar = emergencyItem.GetComponentInChildren<CircularProgressBar>();
        emergencyProgressBar.timeTillCompletion = maxTime;
        emergencyProgressBar.currentProgress = progress;

        emergencyProgressBars.Add(emergencyProgressBar);
        return emergencyItem;
    }

    private void CreateRegionItems(int emergencyIndex, List<BodyRegion> regions, List<float> regionMaxTimes, List<float> regionProgresses, GameObject regionPlaceholder)
    {
        for (int j = 0; j < regions.Count; j++)
        {
            var regionItem = Instantiate(regionPrefab, regionPlaceholder.transform);
            var regionText = regionItem.GetComponentInChildren<TextMeshProUGUI>();
            regionText.text = regions[j].ToString();

            var regionProgressBar = regionItem.GetComponentInChildren<CircularProgressBar>();
            if (regionProgressBar != null && j < regionMaxTimes.Count && j < regionProgresses.Count)
            {
                regionProgressBar.timeTillCompletion = regionMaxTimes[j];
                regionProgressBar.currentProgress = regionProgresses[j];
                regionProgressBars[emergencyIndex].Add(regionProgressBar);
            }
            else
            {
                Debug.LogWarning($"Invalid progress bar data at region {emergencyIndex}, index {j}");
            }

            regionItems[emergencyIndex].Add(regionItem);
        }
    }

    public void PerformSelection()
    {
        if (currentState == NavigationState.Emergency)
        {
            ExpandRegions();
        }
        else if (currentState == NavigationState.Region)
        {
            MakeProgress();
        }
    }

    private void ExpandRegions()
    {
        if (!IsValidIndex(selectedEmergencyIndex, placeholderItems.Count, "selectedEmergencyIndex")) return;
        if (!IsValidIndex(expansionIndex, placeholderItems.Count, "expansionIndex")) return;

        if (placeholderItems[expansionIndex].activeSelf)
        {
            placeholderItems[expansionIndex].SetActive(false);
            ResizeRegionGroup(placeholderItems[expansionIndex], 1);
        }

        expansionIndex = selectedEmergencyIndex;
        hasExpanded = true;
        placeholderItems[expansionIndex].SetActive(true);
        ResizeRegionGroup(placeholderItems[expansionIndex], regionItems[expansionIndex].Count);
        currentState = NavigationState.Region;

        HighlightRegion(0);
    }

    private bool IsValidIndex(int index, int count, string indexName)
    {
        if (index < 0 || index >= count)
        {
            Debug.LogWarning($"Index {indexName} {index} out of range (0) - ({count - 1})");
            return false;
        }
        return true;
    }

    private void MakeProgress()
    {
        if (!IsValidIndex(expansionIndex, regionItems.Count, "expansionIndex")) return;
        if (!IsValidIndex(selectedRegionIndex, regionItems[expansionIndex].Count, "selectedRegionIndex")) return;

        if (progressingRegionIndex != -1 && progressingEmergencyIndex != -1)
        {
            progressingRegion.isProgressing = false;
            progressingEmergency.isProgressing = false;
        }

        progressingEmergencyIndex = expansionIndex;
        progressingRegionIndex = selectedRegionIndex;

        progressingEmergency = emergencyProgressBars[progressingEmergencyIndex];
        progressingRegion = regionProgressBars[progressingEmergencyIndex][progressingRegionIndex];

        progressingEmergency.isProgressing = true;
        progressingRegion.isProgressing = true;

        StartCoroutine(TrackingEmergencyProgress());
    }

    public void HighlightEmergency(int emergencyIndex)
    {
        if (!IsValidIndex(emergencyIndex, emergencyItems.Count, "emergencyIndex")) return;

        ResetHighlights();
        selectedEmergencyIndex = emergencyIndex;

        var emergencyItem = emergencyItems[emergencyIndex];
        emergencyItem.GetComponent<Image>().color = Color.yellow;
        Debug.Log($"Highlighted Emergency at index {emergencyIndex} between range (0) - ({emergencyItems.Count - 1})");
    }

    public void HighlightRegion(int regionIndex)
    {
        if (!IsValidIndex(regionIndex, regionItems[expansionIndex].Count, "regionIndex")) return;

        ResetHighlights();
        selectedRegionIndex = regionIndex;

        var regionItem = regionItems[expansionIndex][regionIndex];
        regionItem.GetComponent<Image>().color = Color.green;
        Debug.Log($"Highlighted Region in index {selectedEmergencyIndex} region group at index {regionIndex} between range (0) - ({regionItems[selectedEmergencyIndex].Count - 1})");
    }

    public void Scroll(int direction)
    {
        if (emergencyItems.Count == 0)
        {
            Debug.LogWarning($"Cannot divide to find new index since there are {emergencyItems.Count} emergencies");
            return;
        }
        int newIndex = (selectedEmergencyIndex + direction + emergencyItems.Count) % emergencyItems.Count;

        if (currentState == NavigationState.Emergency)
        {
            if (!IsValidIndex(selectedEmergencyIndex, emergencyItems.Count, "selectedEmergencyIndex")) return;

            if (selectedEmergencyIndex == expansionIndex && hasExpanded)
            {
                currentState = NavigationState.Region;
                ResizeRegionGroup(placeholderItems[expansionIndex], regionItems[expansionIndex].Count);

                if (direction > 0)
                {
                    HighlightRegion(0);
                    return;
                }
                else if (direction < 0)
                {
                    HighlightRegion(regionItems[expansionIndex].Count - 1);
                    return;
                }
            }
            else
            {
                HighlightEmergency(newIndex);
            }
        }
        else if (currentState == NavigationState.Region)
        {
            if (!IsValidIndex(selectedEmergencyIndex, placeholderItems.Count, "selectedEmergencyIndex")) return;

            int newRegionIndex = selectedRegionIndex + direction;

            if (newRegionIndex < 0 || newRegionIndex >= regionItems[selectedEmergencyIndex].Count)
            {
                currentState = NavigationState.Emergency;
                ResizeRegionGroup(placeholderItems[expansionIndex], 1);

                if (direction > 0)
                {
                    HighlightEmergency((expansionIndex + 1 + emergencyItems.Count) % emergencyItems.Count);
                    return;
                }
                else if (direction < 0)
                {
                    HighlightEmergency((expansionIndex - 1 + emergencyItems.Count) % emergencyItems.Count);
                    return;
                }
            }
            else
            {
                HighlightRegion(newRegionIndex);
            }
        }
    }

    public void ClearDisplay()
    {
        ClearList();
    }

    private void ResizeRegionGroup(GameObject rectangleObject, int numRegions)
    {
        RectTransform rectTrans = rectangleObject.GetComponent<RectTransform>();
        float width = rectTrans.sizeDelta.x;
        float height = (100 * numRegions);
        rectTrans.sizeDelta = new Vector2(width, height);

        // Update parent layout
        RectTransform parentRect = rectTrans.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);

        // Update TextMeshPro objects
        foreach (Transform child in parentRect)
        {
            TextMeshProUGUI textMeshPro = child.GetComponentInChildren<TextMeshProUGUI>();
            if (textMeshPro != null)
            {
                textMeshPro.ForceMeshUpdate();
            }
        }
    }

    private void ResetHighlights()
    {
        foreach (var emergency in emergencyItems)
        {
            var image = emergency.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white; // Reset to default
            }
        }

        foreach (List<GameObject> regionGroup in regionItems)
        {
            foreach (var region in regionGroup)
            {
                var image = region.GetComponent<Image>();
                if (image != null)
                {
                    image.color = Color.white; // Reset to default
                }
            }
        }
    }

    private void ClearList()
    {
        ClearGameObjectList(emergencyItems);
        ClearGameObjectList(placeholderItems);
        ClearRegionItems();

        expansionIndex = 0;
        progressingEmergencyIndex = -1;
        progressingRegionIndex = -1;
        hasExpanded = false;
        currentState = NavigationState.Emergency;
    }

    private void ClearGameObjectList(List<GameObject> list)
    {
        foreach (var item in list)
        {
            Destroy(item);
        }
        list.Clear();
    }

    private void ClearRegionItems()
    {
        foreach (List<GameObject> regionGroup in regionItems)
        {
            foreach (var region in regionGroup)
            {
                Destroy(region);
            }
        }
        regionItems.Clear();
    }

    private IEnumerator TrackingEmergencyProgress()
    {
        while (progressingEmergency.isProgressing)
        {
            if (!progressingRegion.isProgressing)
            {
                progressingEmergency.isProgressing = false;
            }
            else
            {
                yield return null;
            }
        }
    }
}
