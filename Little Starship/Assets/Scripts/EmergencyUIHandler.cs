using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmergencyUIHandler : MonoBehaviour
{
    [Header("Prefabs and Containers")]
    [SerializeField] private GameObject emergencyPrefab; // Prefab for emergencies
    [SerializeField] private GameObject regionPrefab;    // Prefab for regions
    [SerializeField] private Transform listContainer;    // Parent container for emergencies and regions
    [SerializeField] private float regionSpacing = 10f;  // Spacing for regions when expanded

    private List<GameObject> listItems = new List<GameObject>();
    public int selectedEmergencyIndex = 0;
    private int selectedRegionIndex = -1; // -1 means no region is selected

    public void DisplayEmergenciesWithRegions(List<MedicalEmergency> emergencies)
    {
        ClearList();

        for (int i = 0; i < emergencies.Count; i++)
        {
            // Create Emergency Entry
            var emergencyItem = Instantiate(emergencyPrefab, listContainer);
            var emergencyText = emergencyItem.GetComponentInChildren<TextMeshProUGUI>();
            emergencyText.text = emergencies[i].emergencyName;
            listItems.Add(emergencyItem);

            // Create Placeholder for Regions
            var regionPlaceholder = new GameObject($"RegionsPlaceholder_{i}", typeof(RectTransform));
            regionPlaceholder.transform.SetParent(listContainer);

            // Add LayoutElement to Placeholder
            var layoutElement = regionPlaceholder.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 0; // Ensure default height is set to 0

            listItems.Add(regionPlaceholder);

            Debug.Log($"Created placeholder at index {listItems.Count - 1} with LayoutElement.");
        }

        HighlightEmergency(selectedEmergencyIndex);
    }

    public void ExpandRegions(int emergencyIndex, List<BodyRegion> regions)
    {
        // Clear previous regions
        CollapseRegions();

        // Locate the placeholder position
        int placeholderIndex = emergencyIndex * 2 + 1; // Each emergency is followed by a placeholder

        if (placeholderIndex < 0 || placeholderIndex >= listItems.Count)
        {
            Debug.LogError($"ExpandRegions failed: placeholderIndex {placeholderIndex} out of bounds.");
            return;
        }

        Debug.Log($"Expanding regions at placeholderIndex {placeholderIndex} for emergencyIndex {emergencyIndex}.");

        var placeholder = listItems[placeholderIndex];

        for (int i = 0; i < regions.Count; i++)
        {
            var regionItem = Instantiate(regionPrefab, placeholder.transform.parent);
            var regionText = regionItem.GetComponentInChildren<TextMeshProUGUI>();
            regionText.text = regions[i].ToString();
            listItems.Insert(placeholderIndex + i + 1, regionItem);
        }

        AdjustSpacing(placeholderIndex, regions.Count);
        HighlightRegion(0); // Automatically highlight the first region
    }

    public void CollapseRegions()
    {
        for (int i = listItems.Count - 1; i >= 0; i--)
        {
            if (listItems[i].name.Contains("Region"))
            {
                Destroy(listItems[i]);
                listItems.RemoveAt(i);
            }
        }

        ResetSpacing();
    }

    public void HighlightEmergency(int index)
    {
        if (index < 0 || index * 2 >= listItems.Count) return;

        ResetHighlights();
        selectedEmergencyIndex = index;
        selectedRegionIndex = -1;

        var emergencyItem = listItems[index * 2];
        emergencyItem.GetComponent<Image>().color = Color.yellow;
    }

    public void HighlightRegion(int regionIndex)
    {
        ResetHighlights();

        selectedRegionIndex = regionIndex;
        int regionListStartIndex = selectedEmergencyIndex * 2 + 2;

        if (regionListStartIndex + regionIndex >= listItems.Count) return;

        var regionItem = listItems[regionListStartIndex + regionIndex];
        regionItem.GetComponent<Image>().color = Color.green;
    }

    public void Scroll(int direction)
    {
        if (selectedRegionIndex == -1)
        {
            // Scrolling emergencies
            int newEmergencyIndex = selectedEmergencyIndex + direction;

            if (newEmergencyIndex < 0 || newEmergencyIndex * 2 >= listItems.Count)
            {
                return; // No more emergencies to scroll
            }

            HighlightEmergency(newEmergencyIndex);
        }
        else
        {
            // Scrolling regions
            int newRegionIndex = selectedRegionIndex + direction;
            var emergency = listItems[selectedEmergencyIndex * 2];
            var regionsCount = emergency.GetComponent<MedicalEmergency>().presetAffectedRegions.Count;

            if (newRegionIndex < 0 || newRegionIndex >= regionsCount)
            {
                HighlightEmergency(selectedEmergencyIndex + direction);
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

    private void ResetHighlights()
    {
        foreach (var item in listItems)
        {
            var image = item.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white; // Reset to default
            }
        }
    }

    private void AdjustSpacing(int placeholderIndex, int regionCount)
    {
        if (placeholderIndex < 0 || placeholderIndex >= listItems.Count)
        {
            Debug.LogError($"AdjustSpacing failed: placeholderIndex {placeholderIndex} out of bounds.");
            return;
        }

        var placeholder = listItems[placeholderIndex];
        if (placeholder == null)
        {
            Debug.LogError($"AdjustSpacing failed: Placeholder at index {placeholderIndex} is null.");
            return;
        }

        var layoutElement = placeholder.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            Debug.LogError($"AdjustSpacing failed: No LayoutElement found on placeholder at index {placeholderIndex}. Adding one dynamically.");
            layoutElement = placeholder.AddComponent<LayoutElement>(); // Dynamically add if missing
        }

        layoutElement.preferredHeight = regionSpacing * regionCount;
        Debug.Log($"Adjusted spacing for placeholderIndex {placeholderIndex}: preferredHeight set to {layoutElement.preferredHeight}.");
    }

    private void ResetSpacing()
    {
        foreach (var item in listItems)
        {
            if (item.name.Contains("RegionsPlaceholder"))
            {
                var layout = item.GetComponent<LayoutElement>();
                if (layout != null)
                {
                    layout.preferredHeight = 0;
                }
            }
        }
    }

    private void ClearList()
    {
        foreach (var item in listItems)
        {
            Destroy(item);
        }
        listItems.Clear();
    }
}
