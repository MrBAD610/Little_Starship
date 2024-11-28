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
    private int selectedEmergencyIndex = 0;
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

            // Add a marker for where regions will appear dynamically
            var regionPlaceholder = new GameObject($"RegionsPlaceholder_{i}", typeof(RectTransform));
            regionPlaceholder.transform.SetParent(listContainer);
            regionPlaceholder.AddComponent<LayoutElement>().preferredHeight = 0;
            listItems.Add(regionPlaceholder);
        }

        HighlightEmergency(selectedEmergencyIndex);
    }

    public void ExpandRegions(int emergencyIndex, List<BodyRegion> regions)
    {
        // Clear previous regions
        CollapseRegions();

        // Locate the placeholder position
        int placeholderIndex = emergencyIndex * 2 + 1; // RegionsPlaceholder index
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
        var placeholder = listItems[placeholderIndex];
        var layout = placeholder.GetComponent<LayoutElement>();
        layout.preferredHeight = regionSpacing * regionCount;
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
