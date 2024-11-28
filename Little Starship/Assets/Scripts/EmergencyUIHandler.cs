using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmergencyUIHandler : MonoBehaviour
{
    [Header("Prefabs and Containers")]
    [SerializeField] private GameObject emergencyPrefab;
    [SerializeField] private GameObject regionPrefab;
    [SerializeField] private Transform listContainer;

    private List<GameObject> listItems = new List<GameObject>();
    private int selectedEmergencyIndex = 0;
    private int selectedRegionIndex = -1;

    public void DisplayEmergenciesWithRegions(List<MedicalEmergency> emergencies)
    {
        ClearList();

        for (int i = 0; i < emergencies.Count; i++)
        {
            var emergencyItem = Instantiate(emergencyPrefab, listContainer);
            var emergencyText = emergencyItem.GetComponentInChildren<TextMeshProUGUI>();
            emergencyText.text = emergencies[i].emergencyName;
            listItems.Add(emergencyItem);

            var regionPlaceholder = new GameObject($"RegionsPlaceholder_{i}", typeof(RectTransform));
            regionPlaceholder.transform.SetParent(listContainer);
            listItems.Add(regionPlaceholder);
        }

        HighlightEmergency(0);
    }

    public void ExpandRegions(int emergencyIndex, List<BodyRegion> regions)
    {
        CollapseRegions();

        int placeholderIndex = emergencyIndex * 2 + 1;
        var placeholder = listItems[placeholderIndex];

        foreach (var region in regions)
        {
            var regionItem = Instantiate(regionPrefab, placeholder.transform.parent);
            var regionText = regionItem.GetComponentInChildren<TextMeshProUGUI>();
            regionText.text = region.ToString();
            listItems.Insert(placeholderIndex + 1, regionItem);
        }
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
    }

    public void HighlightEmergency(int index)
    {
        selectedEmergencyIndex = index;
        selectedRegionIndex = -1;
    }

    public void HighlightRegion(int index)
    {
        selectedRegionIndex = index;
    }

    public void Scroll(int direction)
    {
        if (selectedRegionIndex == -1)
        {
            HighlightEmergency(selectedEmergencyIndex + direction);
        }
        else
        {
            HighlightRegion(selectedRegionIndex + direction);
        }
    }

    public int GetSelectedEmergencyIndex()
    {
        return selectedEmergencyIndex;
    }

    public int GetSelectedRegionIndex()
    {
        return selectedRegionIndex;
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
