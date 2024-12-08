using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColonistDiagramUIHandler : MonoBehaviour
{
    public InjuryCollection displayedInjuryCollection;

    [Header("Body Region Images")]
    public Image[] allRegionImages = new Image[System.Enum.GetNames(typeof(BodyRegion.RegionType)).Length];

    private Sprite[] allRegionSprites = new Sprite[System.Enum.GetNames(typeof(BodyRegion.RegionType)).Length];

    [Header("Body Region Colors")]
    [SerializeField] private Color fullStatusColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] private Color lowStatusColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    [SerializeField] private Color completeColor = Color.green;
    [SerializeField] private Color nullColor = new Color(1.0f, 1.0f, 1.0f, 0.2f);

    private void Awake()
    {
        displayedInjuryCollection = ScriptableObject.CreateInstance<InjuryCollection>();
    }

    void Start()
    {
        InitializeRegionImagesAndSprites();
    }

    private void Update()
    {
        SetDisplay();
    }

    private void InitializeRegionImagesAndSprites()
    {
        for (int i = 0; i < allRegionSprites.Length; i++)
        {
            if (allRegionImages[i] != null)
            {
                allRegionImages[i].color = nullColor;
                allRegionSprites[i] = allRegionImages[i].sprite;
            }
        }
    }

    public void SetDisplay()
    {
        if (displayedInjuryCollection == null || displayedInjuryCollection.injuredBodyCollection == null)
        {
            Debug.LogError("Injury collection or full body collection is null.");
            return;
        }

        foreach (var region in displayedInjuryCollection.injuredBodyCollection)
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
                    if (region.stabilizationTime > 0)
                    {
                        allRegionImages[(int)region.bodyRegionType].color = Color.Lerp(lowStatusColor, fullStatusColor, region.stabilizationProgress / region.stabilizationTime);
                    }
                    else
                    {
                        Debug.LogError("Stabilization time is zero or negative.");
                        allRegionImages[(int)region.bodyRegionType].color = lowStatusColor;
                    }
                    break;
                case InjuryStatus.Stabilized:
                    allRegionImages[(int)region.bodyRegionType].color = completeColor;
                    break;
            }
        }
    }
}
