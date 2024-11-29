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
    [SerializeField] private float regionSpacing = 10f;  // Spacing for regions when expanded

    private List<GameObject> listItems = new List<GameObject>();
    private List<int> placeholderIndexs = new List<int>();
    private List<MedicalEmergency> allEmergencies = new List<MedicalEmergency>();
    private List<List<BodyRegion>> allRegions = new List<List<BodyRegion>>();
    public int selectedEmergencyIndex = 0;
    private int selectedRegionIndex = -1; // -1 means no region is selected
    private int currentPlaceholder = 0;

    public void DisplayEmergenciesWithRegions(List<MedicalEmergency> emergencies, List<List<BodyRegion>> Regions)
    {
        ClearList(); // Ensure the list is empty before repopulating.

        selectedEmergencyIndex = 0;
        selectedRegionIndex = -1;
        allEmergencies = emergencies;
        allRegions = Regions;

        for (int i = 0; i < allEmergencies.Count; i++)
        {
            // Create Emergency Entry
            var emergencyItem = Instantiate(emergencyPrefab, listContainer);
            var emergencyText = emergencyItem.GetComponentInChildren<TextMeshProUGUI>();
            emergencyText.text = allEmergencies[i].emergencyName;
            listItems.Add(emergencyItem);

            // Create Placeholder for Regions
            //GameObject regionPlaceholder = new GameObject($"RegionsPlaceholder_{i}", typeof(RectTransform));
            //regionPlaceholder.transform.SetParent(listContainer);
            // Add VerticalLayoutGroup to Placeholder
            //regionPlaceholder.AddComponent<VerticalLayoutGroup>();

            GameObject regionPlaceholder = Instantiate(regionGroupPrefab, listContainer);
            RectTransform rectTrans = regionPlaceholder.GetComponent<RectTransform>();
            rectTrans.sizeDelta = new Vector2(100, 100 * allRegions[i].Count);

            for (int j = 0; j < allRegions[i].Count; j++)
            {
                // Create region and parent it under the placeholder's parent.
                GameObject regionItem = Instantiate(regionPrefab, regionPlaceholder.transform);
                var regionText = regionItem.GetComponentInChildren<TextMeshProUGUI>();
                regionText.text = allRegions[i][j].ToString();
                //regionItem.SetActive(false);
            }
            
            listItems.Add(regionPlaceholder);
            placeholderIndexs.Add(listItems.Count - 1);
            regionPlaceholder.SetActive(false);
        }

        HighlightEmergency(selectedEmergencyIndex);
        DebugListStructure(); // Log the structure after initialization.
    }


    public void ExpandRegions()
    {
        if (currentPlaceholder > 0)
        {
            listItems[currentPlaceholder].SetActive(false);
        }

        listItems[placeholderIndexs[selectedEmergencyIndex]].SetActive(true);
        currentPlaceholder = placeholderIndexs[selectedEmergencyIndex];

        //CollapseRegions(); // Clear previously expanded regions.

        //int emergencyIndex = selectedEmergencyIndex;
        //int placeholderIndex = emergencyIndex + 1; // Calculate placeholder position.
        //if (placeholderIndex < 0 || placeholderIndex >= listItems.Count)
        //{
        //    Debug.LogError($"ExpandRegions failed: placeholderIndex {placeholderIndex} out of bounds.");
        //    return;
        //}

        //var placeholder = listItems[placeholderIndex];
        //if (placeholder == null)
        //{
        //    Debug.LogError($"ExpandRegions failed: Placeholder at index {placeholderIndex} is null.");
        //    return;
        //}

        //Debug.Log($"Expanding regions for emergency at index {emergencyIndex}, placeholderIndex {placeholderIndex}.");

        //for (int i = 0; i < allRegions[emergencyIndex].Count; i++)
        //{
        //    // Create region and parent it under the placeholder's parent.
        //    var regionItem = Instantiate(regionPrefab, placeholder.transform.parent);
        //    var regionText = regionItem.GetComponentInChildren<TextMeshProUGUI>();
        //    regionText.text = allRegions[emergencyIndex][i].ToString();

        //    // Insert region directly after the placeholder in `listItems`.
        //    listItems.Insert(placeholderIndex + 1 + i, regionItem);
        //}

        //AdjustSpacing(placeholderIndex, allRegions[emergencyIndex].Count); // Adjust spacing for regions.
        //HighlightRegion(0); // Highlight the first region.
        //DebugListStructure(); // Log the updated list structure.
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
        if (selectedRegionIndex == -1) // Scrolling through emergencies
        {
            if (allEmergencies.Count == 0)
            {
                return;
            }


            int newEmergencyIndex = (selectedEmergencyIndex + direction + allEmergencies.Count) % allEmergencies.Count;

            if (newEmergencyIndex < 0 || newEmergencyIndex * 2 >= listItems.Count)
            {
                Debug.LogWarning("Scroll failed: No more emergencies to scroll to.");
                return;
            }

            HighlightEmergency(newEmergencyIndex);
        }
        else // Scrolling through regions
        {
            int newRegionIndex = selectedRegionIndex + direction;

            // Locate the placeholder for the currently selected emergency.
            int placeholderIndex = selectedEmergencyIndex * 2 + 1;

            if (placeholderIndex >= listItems.Count || listItems[placeholderIndex] == null)
            {
                Debug.LogWarning("Scroll failed: Placeholder is invalid or missing.");
                return;
            }

            // Count regions under the placeholder.
            int regionsCount = 0;
            for (int i = placeholderIndex + 1; i < listItems.Count; i++)
            {
                if (listItems[i].name.Contains("Region"))
                    regionsCount++;
                else
                    break; // Stop counting when the next emergency is encountered.
            }

            if (newRegionIndex < 0 || newRegionIndex >= regionsCount)
            {
                HighlightEmergency(selectedEmergencyIndex + direction); // Wrap back to emergency scrolling.
            }
            else
            {
                HighlightRegion(newRegionIndex);
            }
        }

        DebugListStructure(); // Log the list structure after scrolling.

        //if (selectedRegionIndex == -1) // Scrolling through emergencies
        //{
        //    if (allEmergencies.Count == 0)
        //    {
        //        return;
        //    }


        //    int newEmergencyIndex = (selectedEmergencyIndex + direction + allEmergencies.Count) % allEmergencies.Count;

        //    if (newEmergencyIndex < 0 || newEmergencyIndex * 2 >= listItems.Count)
        //    {
        //        Debug.LogWarning("Scroll failed: No more emergencies to scroll to.");
        //        return;
        //    }

        //    HighlightEmergency(newEmergencyIndex);
        //}
        //else // Scrolling through regions
        //{
        //    int newRegionIndex = selectedRegionIndex + direction;

        //    // Locate the placeholder for the currently selected emergency.
        //    int placeholderIndex = selectedEmergencyIndex * 2 + 1;

        //    if (placeholderIndex >= listItems.Count || listItems[placeholderIndex] == null)
        //    {
        //        Debug.LogWarning("Scroll failed: Placeholder is invalid or missing.");
        //        return;
        //    }

        //    // Count regions under the placeholder.
        //    int regionsCount = 0;
        //    for (int i = placeholderIndex + 1; i < listItems.Count; i++)
        //    {
        //        if (listItems[i].name.Contains("Region"))
        //            regionsCount++;
        //        else
        //            break; // Stop counting when the next emergency is encountered.
        //    }

        //    if (newRegionIndex < 0 || newRegionIndex >= regionsCount)
        //    {
        //        HighlightEmergency(selectedEmergencyIndex + direction); // Wrap back to emergency scrolling.
        //    }
        //    else
        //    {
        //        HighlightRegion(newRegionIndex);
        //    }
        //}

        //DebugListStructure(); // Log the list structure after scrolling.
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

    //private void AdjustSpacing(int placeholderIndex, int regionCount)
    //{
    //    if (placeholderIndex < 0 || placeholderIndex >= listItems.Count)
    //    {
    //        Debug.LogError($"AdjustSpacing failed: placeholderIndex {placeholderIndex} out of bounds.");
    //        return;
    //    }

    //    var placeholder = listItems[placeholderIndex];
    //    if (placeholder == null)
    //    {
    //        Debug.LogError($"AdjustSpacing failed: Placeholder at index {placeholderIndex} is null.");
    //        return;
    //    }

    //    var layoutElement = placeholder.GetComponent<LayoutElement>();
    //    if (layoutElement == null)
    //    {
    //        Debug.LogError($"AdjustSpacing failed: No LayoutElement found on placeholder at index {placeholderIndex}. Adding one dynamically.");
    //        layoutElement = placeholder.AddComponent<LayoutElement>();
    //    }

    //    layoutElement.preferredHeight = regionSpacing * regionCount; // Adjust height for regions.
    //    Debug.Log($"Adjusted spacing for placeholder at index {placeholderIndex} to {layoutElement.preferredHeight}.");
    //}


    //private void ResetSpacing()
    //{
    //    foreach (var item in listItems)
    //    {
    //        if (item.name.Contains("RegionsPlaceholder"))
    //        {
    //            var layout = item.GetComponent<LayoutElement>();
    //            if (layout != null)
    //            {
    //                layout.preferredHeight = 0;
    //            }
    //        }
    //    }
    //}

    private void ClearList()
    {
        foreach (var item in listItems)
        {
            Destroy(item);
        }
        listItems.Clear();
        placeholderIndexs.Clear();
        currentPlaceholder = 0;
    }

    private void DebugListStructure()
    {
        Debug.Log("Current List Structure:");
        for (int i = 0; i < listItems.Count; i++)
        {
            string itemType = listItems[i]?.name ?? "Null";
            Debug.Log($"Index {i}: {itemType}");
        }
    }

}
