using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColonistDiagramUIHandler : MonoBehaviour
{
    private InjuryCollection displayedInjuryCollection;

    [Header("Body Regions")]
    public GameObject[] allRegionObjects = new GameObject[System.Enum.GetNames(typeof(BodyRegion.RegionType)).Length];
    
    private Image[] allRegionImages = new Image[System.Enum.GetNames(typeof(BodyRegion.RegionType)).Length];
    private Sprite[] allRegionSprites = new Sprite[System.Enum.GetNames(typeof(BodyRegion.RegionType)).Length];
    private Button[] allRegionButtons = new Button[System.Enum.GetNames(typeof(BodyRegion.RegionType)).Length];

    [Header("Body Region Colors")]
    [SerializeField] private Color fullStatusColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] private Color lowStatusColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    [SerializeField] private Color completeColor = Color.green;
    [SerializeField] private Color nullColor = new Color(1.0f, 1.0f, 1.0f, 0.2f);

    [Header("Script Reference")]
    [SerializeField] private EmergencyUIHandler emergencyUIHandler;

    private bool progressingRegion = false;

    private void Awake()
    {
        displayedInjuryCollection = ScriptableObject.CreateInstance<InjuryCollection>();
        InitializeRegionImagesAndSprites();
    }

    private void InitializeRegionImagesAndSprites()
    {
        for (int i = 0; i < allRegionObjects.Length; i++)
        {
            if (allRegionObjects[i] != null)
            {
                Image image = allRegionObjects[i].GetComponent<Image>();
                Button button = allRegionObjects[i].GetComponent<Button>();
                if (image != null)
                {
                    allRegionImages[i] = image;
                    allRegionSprites[i] = image.sprite;
                    allRegionImages[i].color = nullColor;

                    allRegionButtons[i] = button;
                    // Add listener to the button
                    int index = i; // Capture the current index
                    button.onClick.AddListener(() => OnRegionButtonClicked(index));
                }
                else
                {
                    Debug.LogError($"No Image component found on {allRegionObjects[i].name}");
                }
            }
            else
            {
                Debug.LogError($"allRegionObjects[{i}] is null");
            }
        }

        // Check if allRegionImages array is properly initialized
        for (int i = 0; i < allRegionImages.Length; i++)
        {
            if (allRegionImages[i] == null)
            {
                Debug.LogError($"Image for region {(BodyRegion.RegionType)i} is null.");
            }
        }
    }

    public void SetDisplayedInjuryCollection(InjuryCollection newInjuryCollection)
    {
        if (newInjuryCollection == null)
        {
            Debug.LogError("No collection entered");
            return;
        }

        if (currentStabilizationCoroutine != null)
        {
            StopCoroutine(currentStabilizationCoroutine);
        }

        displayedInjuryCollection = newInjuryCollection;
        UpdateDiagramDisplay();
    }

    public void UpdateDiagramDisplay()
    {
        if (displayedInjuryCollection == null)
        {
            Debug.LogError("Injury collection is null.");
            return;
        }

        if (displayedInjuryCollection.injuredBodyCollection == null)
        {
            Debug.LogError("Injured body collection is null.");
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

            if (allRegionImages[(int)region.bodyRegionType] == null)
            {
                Debug.LogError($"Image for region {region.bodyRegionType} is null.");
                continue;
            }

            switch (region.regionInjuryStatus)
            {
                case InjuryStatus.Unharmed:
                    allRegionImages[(int)region.bodyRegionType].color = nullColor;
                    allRegionButtons[(int)region.bodyRegionType].interactable = false;
                    break;
                
                case InjuryStatus.Injured:
                    if (region.stabilizationTime > 0)
                    {
                        allRegionImages[(int)region.bodyRegionType].color = Color.Lerp(lowStatusColor, fullStatusColor, region.stabilizationProgress / region.stabilizationTime);
                        allRegionButtons[(int)region.bodyRegionType].interactable = true;
                    }
                    else
                    {
                        Debug.LogError("Stabilization time is zero or negative.");
                        allRegionImages[(int)region.bodyRegionType].color = lowStatusColor;
                        allRegionButtons[(int)region.bodyRegionType].interactable = false;
                    }
                    break;
                
                case InjuryStatus.Stabilized:
                    allRegionImages[(int)region.bodyRegionType].color = completeColor;
                    allRegionButtons[(int)region.bodyRegionType].interactable = false;
                    break;
            }
        }
    }

    private Coroutine currentStabilizationCoroutine;

    private void OnRegionButtonClicked(int index)
    {
        Debug.Log($"{allRegionObjects[index].name} Button clicked.");

        if (currentStabilizationCoroutine != null)
        {
            StopCoroutine(currentStabilizationCoroutine);
        }

        currentStabilizationCoroutine = StartCoroutine(StabilizeRegion(index));
    }

    private IEnumerator StabilizeRegion(int index) // Stabilize the region over time
    {
        if (displayedInjuryCollection == null)
        {
            Debug.LogError("Injury collection is null.");
            yield break;
        }
        if (displayedInjuryCollection.injuredBodyCollection == null)
        {
            Debug.LogError("Injured body collection is null.");
            yield break;
        }
        if (displayedInjuryCollection.injuredBodyCollection.Length <= index)
        {
            Debug.LogError("Index is out of range.");
            yield break;
        }
        BodyRegion region = displayedInjuryCollection.injuredBodyCollection[index];
        if (region == null)
        {
            Debug.LogError("Body region is null.");
            yield break;
        }
        if (region.regionInjuryStatus != InjuryStatus.Injured)
        {
            Debug.LogWarning("Region is not injured.");
            yield break;
        }
        if (region.stabilizationTime <= 0)
        {
            Debug.LogError("Stabilization time is zero or negative.");
            yield break;
        }

        float progress = region.stabilizationProgress; // Get the current progress of the region
        progressingRegion = true; // Start progressing the region
        while (progressingRegion) // While the progress is less than the stabilization time
        {
            if (progress < region.stabilizationTime)
            {
                progress += Time.deltaTime;     // Increment the progress
                region.stabilizationProgress = progress; // Update the stabilization progress of the region
                allRegionImages[index].color = Color.Lerp(lowStatusColor, fullStatusColor, progress / region.stabilizationTime); // Update the color of the region image
            }
            else
            {
                progress = region.stabilizationTime; // Set the progress to the stabilization time
                region.stabilizationProgress = progress; // Update the stabilization progress of the region
                region.regionInjuryStatus = InjuryStatus.Stabilized; // Set the region status to stabilized
                progressingRegion = false; // Stop progressing the region
            }
            yield return null; // Wait for the next frame
        }

        UpdateDiagramDisplay(); // Update the display
        displayedInjuryCollection.UpdateStabilizedRegionTotal(); // Update the stabilized region total
        displayedInjuryCollection.GetProgressSum(); // Update the progress sum of the injury collection
        emergencyUIHandler.UpdateRegionTotals(); // Update the emergency UI
    }

}
