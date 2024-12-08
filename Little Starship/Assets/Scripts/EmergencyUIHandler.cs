using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EmergencyUIHandler : MonoBehaviour
{
    [Header("Prefabs and Containers")]
    [SerializeField] private Transform emergencyContainer;    // Parent container for emergencies and regions
    [SerializeField] private GameObject emergencyPrefab; // Prefab for emergencies
    [SerializeField] private GameObject regionPrefab;    // Prefab for regions
    [SerializeField] private GameObject regionGroupPrefab; // Prefab for region vertical groups
    
    [SerializeField] private GameObject TransmitButtonPrefab;      // Button for transmitting stabilized colonist

    private List<GameObject> emergencyItems = new List<GameObject>();
    private List<GameObject> placeholderItems = new List<GameObject>();
    private List<List<GameObject>> regionItems = new List<List<GameObject>>();
    
    private List<float> emergencyProgressionTimes = new List<float>();
    private List<List<float>> regionProgressionTimes = new List<List<float>>();

    private Button TransmitButton;

    private CircularProgressBar totalProgressBar;
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

    private void Start()
    {
        if (TransmitButtonPrefab == null)
        {
            Debug.LogError("TransmitButtonPrefab not found on EmergencyUIHandler.");
        }

        if (emergencyPrefab == null || regionPrefab == null || regionGroupPrefab == null || emergencyContainer == null)
        {
            Debug.LogError("EmergencyUIHandler is missing a prefab or container reference.");
        }

        TransmitButton = TransmitButtonPrefab.GetComponent<Button>();
        if (TransmitButton == null)
        {
            Debug.LogError("TransmitButton not found on TransmitButtonPrefab.");
            return;
        }

        totalProgressBar = TransmitButtonPrefab.GetComponentInChildren<CircularProgressBar>();
        if (totalProgressBar == null)
        {
            Debug.LogError("Total progress bar not found on TransmitButtonPrefab.");
            return;
        }
    }

    private void Update()
    {
        if (totalProgressBar.isComplete)
        {
            TransmitButton.interactable = true;
        }
        else
        {
            TransmitButton.interactable = false;
        }
    }

    public void DisplayEmergenciesWithRegions(Colonist colonistInput)
    {
        Colonist currentColonist = colonistInput;

        List<MedicalEmergency> emergencies = currentColonist.emergencies;
        List<List<BodyRegion>> regions = currentColonist.colonistRegions;

        var colonistEmergencyStabilizationTimes = currentColonist.neededTimeForEachEmergency;
        var colonistRegionStabilizationTimes = currentColonist.neededTimeForEachRegion;

        var colonistEmergencyProgressTimes = currentColonist.progressOfEmergencies;
        var colonistRegionProgressTimes = currentColonist.progressOfRegions;

        totalProgressBar.isProgressing = false;
        totalProgressBar.timeTillCompletion = currentColonist.neededTimeToStabilizeColonist;
        totalProgressBar.currentProgress = currentColonist.totalStabilizationProgress;

        ClearList(); // Ensure the list is empty before repopulating.

        for (int i = 0; i < emergencies.Count; i++)
        {
            // Create Emergency Entry
            var emergencyItem = Instantiate(emergencyPrefab, emergencyContainer);

            var emergencyText = emergencyItem.GetComponentInChildren<TextMeshProUGUI>();
            emergencyText.text = emergencies[i].emergencyName;

            var emergencyProgressBar = emergencyItem.GetComponentInChildren<CircularProgressBar>();
           
            if (emergencies.Count != colonistEmergencyStabilizationTimes.Count || emergencies.Count != colonistEmergencyProgressTimes.Count)
            {
                Debug.LogError($"Mismatch: emergencies.Count ({emergencies.Count}) != colonistEmergencyStabilizationTimes.Count ({colonistEmergencyStabilizationTimes.Count}) or != colonistEmergencyProgressTimes.Count ({colonistEmergencyProgressTimes.Count})");
                return; // Exit early to prevent runtime error
            }
            if (colonistEmergencyStabilizationTimes.Count == 0 || colonistEmergencyProgressTimes.Count == 0)
            {
                Debug.LogWarning($"colonistEmergencyStabilizationTimes ({colonistEmergencyStabilizationTimes.Count}) or colonistEmergencyProgressTimes ({colonistEmergencyProgressTimes.Count}) is empty.");
                return; // Avoid processing further
            }
            if (i < colonistEmergencyStabilizationTimes.Count && i < colonistEmergencyProgressTimes.Count)
            {
                emergencyProgressBar.timeTillCompletion = colonistEmergencyStabilizationTimes[i];
                emergencyProgressionTimes.Add(colonistEmergencyProgressTimes[i]);
                emergencyProgressBar.currentProgress = colonistEmergencyProgressTimes[i];
            }
            else
            {
                Debug.LogError($"Index {i} out of range for colonistEmergencyProgressTimes (Count: {colonistEmergencyStabilizationTimes.Count})");
            }

            emergencyProgressBars.Add(emergencyProgressBar);
            emergencyItems.Add(emergencyItem);

            GameObject regionPlaceholder = Instantiate(regionGroupPrefab, emergencyContainer);

            regionItems.Add(new List<GameObject>());
            regionProgressionTimes.Add(new List<float>());
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
                regionText.text = regions[i][j].regionInjuryStatus.ToString();

                var regionProgressBar = regionItem.GetComponentInChildren<CircularProgressBar>();
                if (regionProgressBar != null && i < colonistRegionStabilizationTimes.Count && j < colonistRegionStabilizationTimes[i].Count && i < colonistRegionProgressTimes.Count && j < colonistRegionProgressTimes[i].Count)
                {
                    regionProgressBar.timeTillCompletion = colonistRegionStabilizationTimes[i][j];
                    regionProgressBar.currentProgress = colonistRegionProgressTimes[i][j];
                    regionProgressionTimes[i].Add(colonistRegionProgressTimes[i][j]);
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
            totalProgressBar.isProgressing = false;
        }

        progressingEmergencyIndex = expansionIndex;
        progressingRegionIndex = selectedRegionIndex;

        progressingEmergency = emergencyProgressBars[progressingEmergencyIndex];
        progressingRegion = regionProgressBars[progressingEmergencyIndex][progressingRegionIndex];

        totalProgressBar.isProgressing = true;
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



    public Colonist ApplyProgressionToColonist(Colonist newColonist)
    {
        Colonist updatedColonist = newColonist;
        if (updatedColonist == null)
        {
            Debug.LogWarning("Cannot apply progression to null colonist.");
            return null;
        }
        if (updatedColonist.emergencies.Count != emergencyProgressionTimes.Count)
        {
            Debug.LogWarning($"Cannot apply progression to colonist with {updatedColonist.emergencies.Count} emergencies and {emergencyProgressionTimes.Count} progression times.");
            return updatedColonist;
        }
        if (updatedColonist.colonistRegions.Count != regionProgressionTimes.Count)
        {
            Debug.LogWarning($"Cannot apply progression to colonist with {updatedColonist.colonistRegions.Count} region groups and {regionProgressionTimes.Count} progression times.");
            return updatedColonist;
        }
        if (progressingRegion == null || progressingEmergency == null)
        {
            Debug.LogWarning("Cannot apply progression without a progressing region or emergency.");
            return updatedColonist;
        }

        if (progressingRegion.isProgressing == true || progressingEmergency.isProgressing == true || totalProgressBar.isProgressing == true)
        {
            progressingRegion.isProgressing = false;
            progressingEmergency.isProgressing = false;
            totalProgressBar.isProgressing = false;
            regionProgressionTimes[progressingEmergencyIndex][progressingRegionIndex] = progressingRegion.currentProgress;
            emergencyProgressionTimes[progressingEmergencyIndex] += progressingRegion.currentProgress;
        }

        updatedColonist.totalStabilizationProgress = totalProgressBar.currentProgress;
        updatedColonist.progressOfEmergencies = emergencyProgressionTimes;
        updatedColonist.progressOfRegions = regionProgressionTimes;
        return updatedColonist;
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

        //foreach (var emergencyBar in emergencyProgressBars)
        //{
        //    Destroy(emergencyBar);
        //}
        emergencyProgressBars.Clear();

        //foreach (List<CircularProgressBar> regionBarGroup in regionProgressBars) foreach (var regionBar in regionBarGroup)
        //    {
        //        Destroy(regionBar);
        //    }
        regionProgressBars.Clear();

        emergencyProgressionTimes = new List<float>();
        regionProgressionTimes = new List<List<float>>();

        if (totalProgressBar != null)
        {
            totalProgressBar.isProgressing = false;
            totalProgressBar.currentProgress = 0f;
        }

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
                totalProgressBar.isProgressing = false;

                regionProgressionTimes[progressingEmergencyIndex][progressingRegionIndex] = progressingRegion.currentProgress;
                emergencyProgressionTimes[progressingEmergencyIndex] += progressingRegion.currentProgress;
            }
            else
            {
                progressingEmergency.isProgressing = true;
                totalProgressBar.isProgressing = true;
                yield return null;
            }
        }
    }

}
