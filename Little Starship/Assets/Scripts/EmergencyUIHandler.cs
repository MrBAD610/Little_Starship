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
            // Create Emergency Entry
            var emergencyItem = Instantiate(emergencyPrefab, listContainer);

            var emergencyText = emergencyItem.GetComponentInChildren<TextMeshProUGUI>();
            emergencyText.text = emergencies[i].emergencyName;

            var emergencyProgressBar = emergencyItem.GetComponentInChildren<CircularProgressBar>();
            Debug.Log($"Colonist Data: emergencies.Count = {currentColonist.emergencies.Count}, emergencyProgresses.Count = {currentColonist.neededTimeForEachEmergency.Count}");
            if (emergencies.Count != emergencyMaxTimes.Count || emergencies.Count != emergencyProgresses.Count)
            {
                Debug.LogError($"Mismatch: emergencies.Count ({emergencies.Count}) != emergencyMaxTimes.Count ({emergencyMaxTimes.Count}) or != emergencyProgresses.Count ({emergencyProgresses.Count})");
                return; // Exit early to prevent runtime error
            }
            if (emergencyMaxTimes.Count == 0 || emergencyProgresses.Count == 0)
            {
                Debug.LogWarning($"emergencyMaxTimes ({emergencyMaxTimes.Count}) or emergencyProgresses ({emergencyProgresses.Count}) is empty.");
                return; // Avoid processing further
            }
            if (i < emergencyMaxTimes.Count && i < emergencyProgresses.Count)
            {
                emergencyProgressBar.timeTillCompletion = emergencyMaxTimes[i];
                emergencyProgressBar.currentProgress = emergencyProgresses[i];
            }
            else
            {
                Debug.LogError($"Index {i} out of range for emergencyProgresses (Count: {emergencyMaxTimes.Count})");
            }

            emergencyProgressBars.Add(emergencyProgressBar);
            emergencyItems.Add(emergencyItem);

            GameObject regionPlaceholder = Instantiate(regionGroupPrefab, listContainer);

            regionItems.Add(new List<GameObject>());
            regionProgressBars.Add(new List<CircularProgressBar>());

            if (i >= regions.Count || regions[i] == null)
            {
                Debug.LogError($"Invalid index or null region at index {i}");
                continue;
            }

            for (int j = 0; j < regions[i].Count; j++)
            {
                // Create region and parent it under the placeholder's parent.
                GameObject regionItem = Instantiate(regionPrefab, regionPlaceholder.transform);

                var regionText = regionItem.GetComponentInChildren<TextMeshProUGUI>();
                regionText.text = regions[i][j].ToString();

                var regionProgressBar = regionItem.GetComponentInChildren<CircularProgressBar>();
                if (regionProgressBar != null && i < regionMaxTimes.Count && j < regionMaxTimes[i].Count && i < regionProgresses.Count && j < regionProgresses[i].Count)
                {
                    regionProgressBar.timeTillCompletion = regionMaxTimes[i][j];
                    regionProgressBar.currentProgress = regionProgresses[i][j];
                    regionProgressBars[i].Add(regionProgressBar);
                }
                else
                {
                    Debug.LogWarning($"Invalid progress bar data at region {i}, index {j}");
                }

                regionItems[i].Add(regionItem);
            }

            placeholderItems.Add(regionPlaceholder);
            regionPlaceholder.SetActive(false);
        }

        HighlightEmergency(0);
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
        if (selectedEmergencyIndex < 0 || selectedEmergencyIndex >= placeholderItems.Count)
        {
            Debug.LogWarning($"Can't expand, selectedEmergencyIndex {selectedEmergencyIndex} out of range (0) - ({placeholderItems.Count - 1})");
            return;
        }

        if (expansionIndex < 0 || expansionIndex >= placeholderItems.Count)
        {
            Debug.LogWarning($"ExpansionIndex {expansionIndex} out of range (0) - ({placeholderItems.Count - 1})");
        }
        else
        {
            if (placeholderItems[expansionIndex].activeSelf == true)
            {
                placeholderItems[expansionIndex].SetActive(false);
                ResizeRegionGroup(placeholderItems[expansionIndex], 1);
            }
        }

        expansionIndex = selectedEmergencyIndex;

        if (!hasExpanded) hasExpanded = true;
        placeholderItems[expansionIndex].SetActive(true);
        ResizeRegionGroup(placeholderItems[expansionIndex], regionItems[expansionIndex].Count);
        currentState = NavigationState.Region;

        HighlightRegion(0);
    }

    private void MakeProgress()
    {
        if (expansionIndex < 0 || expansionIndex >= regionItems.Count)
        {
            Debug.LogWarning($"Can't progress, expansionIndex {expansionIndex} out of range (0) - ({regionItems.Count - 1})");
            return;
        }
        if (selectedRegionIndex < 0 || selectedRegionIndex >= regionItems[expansionIndex].Count)
        {
            Debug.LogWarning($"Can't progress, selectedRegionIndex {selectedRegionIndex} out of range (0) - ({regionItems[expansionIndex].Count - 1})");
            return;
        }

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
        if (emergencyIndex < 0 || emergencyIndex >= emergencyItems.Count)
        {
            Debug.LogWarning($"Can't highlight Emergency at index {emergencyIndex} since it is out of range (0) - ({emergencyItems.Count - 1})");
            return;
        }

        ResetHighlights();
        selectedEmergencyIndex = emergencyIndex;

        var emergencyItem = emergencyItems[emergencyIndex];
        emergencyItem.GetComponent<Image>().color = Color.yellow;
        Debug.Log($"Highlighted Emergency at index {emergencyIndex} between range (0) - ({emergencyItems.Count - 1})");
    }

    public void HighlightRegion(int regionIndex)
    {
        if (regionIndex < 0 || regionIndex >= regionItems[expansionIndex].Count)
        {
            Debug.LogWarning($"Can't highlight Region in index {expansionIndex} region group at index {regionIndex} since it is out of range (0) - ({regionItems[expansionIndex].Count - 1})");
            return;
        }

        ResetHighlights();
        selectedRegionIndex = regionIndex;

        var regionItem = regionItems[expansionIndex][regionIndex];
        regionItem.GetComponent<Image>().color = Color.green;
        Debug.Log($"Highlighted Region in index {selectedEmergencyIndex} region group at index {regionIndex} between range (0) - ({regionItems[selectedEmergencyIndex].Count - 1})");
    }

    public void Scroll(int direction)
    {
        if (emergencyItems.Count == 0) // Check so no divide by zero
        {
            Debug.LogWarning($"Cannot divide to find new index since there are {emergencyItems.Count} emergencies");
            return;
        }
        int newIndex = (selectedEmergencyIndex + direction + emergencyItems.Count) % emergencyItems.Count;

        if (currentState == NavigationState.Emergency) // Scrolling through emergencies
        {
            if (selectedEmergencyIndex < 0 || selectedEmergencyIndex >= emergencyItems.Count)
            {
                Debug.LogWarning($"Scroll failed: {newIndex} is an invalid emergency index to scroll to.");
                return;
            }

            if (selectedEmergencyIndex == expansionIndex && hasExpanded)
            {
                currentState = NavigationState.Region;
                ResizeRegionGroup(placeholderItems[expansionIndex], regionItems[expansionIndex].Count);

                if (direction > 0)
                {
                    Debug.Log($"Selected first region at index 0 benneath the current index");
                    HighlightRegion(0);                                     // Next index is below current index
                    return;
                }
                else if (direction < 0)
                {
                    Debug.Log($"Selected last region at index {regionItems[expansionIndex].Count - 1} above the current index");
                    HighlightRegion(regionItems[expansionIndex].Count - 1); // Next index is above current index
                    return;
                }
            }
            else
            {
                Debug.Log($"Selected Emeregency at index {newIndex}");
                HighlightEmergency(newIndex);
            }
        }
        else if (currentState == NavigationState.Region) // Scrolling through regions
        {
            if (selectedEmergencyIndex >= placeholderItems.Count || placeholderItems[selectedEmergencyIndex] == null)
            {
                Debug.LogWarning($"Scroll failed: Placeholder at selectedEmergencyIndex {selectedEmergencyIndex} is invalid or missing.");
                return;
            }

            int newRegionIndex = selectedRegionIndex + direction;
            //if (selectedIndex > expansionIndex) newRegionIndex = 0;

            if (newRegionIndex < 0 || newRegionIndex >= regionItems[selectedEmergencyIndex].Count)
            {
                currentState = NavigationState.Emergency;
                ResizeRegionGroup(placeholderItems[expansionIndex], 1);

                // Wrap back to emergency scrolling.
                if (direction > 0)
                {
                    Debug.Log($"Selected Emeregency at index {(expansionIndex + 1 + emergencyItems.Count) % emergencyItems.Count} since it is below the region group");
                    HighlightEmergency((expansionIndex + 1 + emergencyItems.Count) % emergencyItems.Count);        // Next index is below region group
                    return;
                }
                else if (direction < 0)
                {
                    Debug.Log($"Selected Emeregency at index {newIndex} since it is above the region group");
                    HighlightEmergency(expansionIndex);  // Next index is above region group
                    return;
                }
            }
            else
            {
                Debug.Log($"Selected Region at index {newRegionIndex}");
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

        foreach (List<GameObject> regionGroup in regionItems) foreach (var region in regionGroup)
            {
                var image = region.GetComponent<Image>();
                if (image != null)
                {
                    image.color = Color.white; // Reset to default
                }
            }
    }

    private void ClearList()
    {
        foreach (var emergency in emergencyItems)
        {
            Destroy(emergency);
        }
        emergencyItems.Clear();

        foreach (var placeholder in placeholderItems)
        {
            Destroy(placeholder);
        }
        placeholderItems.Clear();

        foreach (List<GameObject> regionGroup in regionItems) foreach (var region in regionGroup)
            {
                Destroy(region);
            }
        regionItems.Clear();
        expansionIndex = 0;              // reset expansionIndex
        progressingEmergencyIndex = -1;  // reset progressingEmergencyIndex
        progressingRegionIndex = -1;     // reset expansionIndex
        hasExpanded = false;
        currentState = NavigationState.Emergency;
    }

    private IEnumerator TrackingEmergencyProgress()
    {
        while (progressingEmergency.isProgressing == true)
        {
            if (progressingRegion.isProgressing == false)
            {
                progressingEmergency.isProgressing = false;
                
            }
            else
            {
                progressingEmergency.isProgressing = true;
                yield return null;
            }
        }
    }

}
