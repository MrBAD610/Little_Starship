using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColonistDiagramUIHandler : MonoBehaviour
{
    public List<InjuryCollection> injuryCollection = new List<InjuryCollection>();

    [Header("Body Region Images")]
    public Image[] allRegionImages = new Image[System.Enum.GetNames(typeof(BodyRegion.RegionType)).Length];

    private Sprite[] allRegionSprites = new Sprite[System.Enum.GetNames(typeof(BodyRegion.RegionType)).Length];

    [Header("Body Region Colors")]
    [SerializeField] private Color fullStatusColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] private Color lowStatusColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    [SerializeField] private Color completeColor = Color.green;
    [SerializeField] private Color nullColor = new Color(1.0f, 1.0f, 1.0f, 0.2f);

    void Start()
    {
        InitializeRegionSprites();
        SetDisplay();
    }

    private void InitializeRegionSprites()
    {
        for (int i = 0; i < allRegionSprites.Length; i++)
        {
            if (allRegionImages[i] != null)
            {
                allRegionSprites[i] = allRegionImages[i].sprite;
            }
        }
    }

    private void SetDisplay()
    {
        if (injuryCollection == null || injuryCollection.Count == 0)
        {
            Debug.LogError("Injury collection is null or empty.");
            return;
        }

        var currentInjuryCollection = injuryCollection[0];

        if (currentInjuryCollection == null || currentInjuryCollection.injuredBodyCollection == null)
        {
            Debug.LogError("Full body collection is null.");
            return;
        }

        foreach (var region in currentInjuryCollection.injuredBodyCollection)
        {
            if (region == null)
            {
                Debug.LogError("Body region is null.");
                continue;
            }

            if (allRegionImages == null || allRegionImages.Length <= (int)region.bodyRegionType)
            {
                Debug.LogError("All region images array is not properly initialized or index is out of range.");
                continue;
            }

            switch (region.regionInjuryStatus)
            {
                case InjuryStatus.Unharmed:
                    allRegionImages[(int)region.bodyRegionType].color = nullColor;
                    break;
                case InjuryStatus.Injured:
                    allRegionImages[(int)region.bodyRegionType].color = Color.Lerp(lowStatusColor, fullStatusColor, region.stabilizationProgress / region.stabilizationTime);
                    break;
                case InjuryStatus.Stabilized:
                    allRegionImages[(int)region.bodyRegionType].color = completeColor;
                    break;
            }
        }
    }
}
