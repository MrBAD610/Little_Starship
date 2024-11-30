using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    private List<List<CircularProgressBar>> regionProgressBars = new List<List<CircularProgressBar>>();
    private List<CircularProgressBar> emergencyProgressBars = new List<CircularProgressBar>();
    private int selectedIndex = 0;
    private int selectedRegionIndex = 0;
    private int expansionIndex = 0;
    private bool hasExpanded = false;

    public void DisplayEmergenciesWithRegions(Colonist colonistInput)
    {
        currentColonist = colonistInput;

        List<MedicalEmergency> emergencies = currentColonist.emergencies;
        List<List<BodyRegion>> regions = currentColonist.colonistRegions;

        ClearList(); // Ensure the list is empty before repopulating.

        for (int i = 0; i < emergencies.Count; i++)
        {
            // Create Emergency Entry
            var emergencyItem = Instantiate(emergencyPrefab, listContainer);
            var emergencyText = emergencyItem.GetComponentInChildren<TextMeshProUGUI>();
            emergencyText.text = emergencies[i].emergencyName;
            emergencyItems.Add(emergencyItem);

            GameObject regionPlaceholder = Instantiate(regionGroupPrefab, listContainer);
            //RectTransform rectTrans = regionPlaceholder.GetComponent<RectTransform>();
            //rectTrans.sizeDelta = new Vector2(100, 100 * Regions[i].Count);

            regionItems.Add(new List<GameObject>());
            for (int j = 0; j < regions[i].Count; j++)
            {
                // Create region and parent it under the placeholder's parent.
                GameObject regionItem = Instantiate(regionPrefab, regionPlaceholder.transform);
                var regionText = regionItem.GetComponentInChildren<TextMeshProUGUI>();
                regionText.text = regions[i][j].ToString();

                regionItems[i].Add(regionItem);
            }

            placeholderItems.Add(regionPlaceholder);
            regionPlaceholder.SetActive(false);
        }

        HighlightEmergency(0);
    }


    public void ExpandRegions()
    {
        if (selectedIndex < 0 || selectedIndex >= placeholderItems.Count)
        {
            Debug.LogWarning($"Can't expand, selectedIndex {selectedIndex} out of range (0) - ({placeholderItems.Count - 1})");
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

        expansionIndex = selectedIndex;

        if (!hasExpanded) hasExpanded = true;
        placeholderItems[expansionIndex].SetActive(true);
        ResizeRegionGroup(placeholderItems[expansionIndex], regionItems[expansionIndex].Count);

        HighlightRegion(0);
    }

    public void HighlightEmergency(int index)
    {
        if (index < 0 || index >= emergencyItems.Count)
        {
            Debug.LogWarning($"Can't highlight Emergency at index {index} since it is out of range (0) - ({emergencyItems.Count - 1})");
            return;
        }

        ResetHighlights();
        selectedIndex = index;

        var emergencyItem = emergencyItems[index];
        emergencyItem.GetComponent<Image>().color = Color.yellow;
        Debug.Log($"Highlighted Emergency at index {index} between range (0) - ({emergencyItems.Count - 1})");
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
        Debug.Log($"Highlighted Region in index {selectedIndex} region group at index {regionIndex} between range (0) - ({regionItems[selectedIndex].Count - 1})");
    }

    public void Scroll(int direction)
    {
        if (emergencyItems.Count == 0) // Check so no divide by zero
        {
            Debug.LogWarning($"Cannot divide to find new index since there are {emergencyItems.Count} emergencies");
            return;
        }
        int newIndex = (selectedIndex + direction + emergencyItems.Count) % emergencyItems.Count;

        if (selectedIndex != expansionIndex) // Scrolling through emergencies
        {
            if (selectedIndex < 0 || selectedIndex >= emergencyItems.Count)
            {
                Debug.LogWarning($"Scroll failed: {newIndex} is an invalid emergency index to scroll to.");
                return;
            }

            if (selectedIndex == expansionIndex && hasExpanded)
            {
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
        else // Scrolling through regions
        {
            if (selectedIndex >= placeholderItems.Count || placeholderItems[selectedIndex] == null)
            {
                Debug.LogWarning($"Scroll failed: Placeholder at index {selectedIndex} is invalid or missing.");
                return;
            }

            int newRegionIndex = selectedRegionIndex + direction;
            //if (selectedIndex > expansionIndex) newRegionIndex = 0;

            if (newRegionIndex < 0 || newRegionIndex >= regionItems[selectedIndex].Count)
            {
                ResizeRegionGroup(placeholderItems[expansionIndex], 1);

                // Wrap back to emergency scrolling.
                if (direction > 0)
                {
                    Debug.Log($"Selected Emeregency at index {newIndex} since it is below the region group");
                    HighlightEmergency(newIndex);        // Next index is below region group
                    return;
                }
                else if (direction < 0)
                {
                    Debug.Log($"Selected Emeregency at index {expansionIndex} since it is above the region group");
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
        expansionIndex = 0; // reset expansionIndex
        hasExpanded = false;
    }

}
