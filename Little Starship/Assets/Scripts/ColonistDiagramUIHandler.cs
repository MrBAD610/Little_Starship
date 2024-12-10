using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColonistDiagramUIHandler : MonoBehaviour // Class for handling the colonist diagram UI
{
    private InjuryCollection displayedInjuryCollection; // The injury collection currently displayed in the diagram

    [Header("Body Regions")]
    public GameObject[] allRegionObjects = new GameObject[System.Enum.GetNames(typeof(BodyRegion.RegionType)).Length]; // Array of all body region objects in the diagram stored in the same order as the body region enum

    private Image[] allRegionImages = new Image[System.Enum.GetNames(typeof(BodyRegion.RegionType)).Length]; // Array of all body region images belonging to the body regions at the same index as in the body region enum
    private Sprite[] allRegionSprites = new Sprite[System.Enum.GetNames(typeof(BodyRegion.RegionType)).Length]; // Array of all body region sprites belonging to the body regions at the same index as in the body region enum
    private Button[] allRegionButtons = new Button[System.Enum.GetNames(typeof(BodyRegion.RegionType)).Length]; // Array of all body region buttons belonging to the body regions at the same index as in the body region enum

    [Header("Body Region Colors")]
    [SerializeField] private Color fullStatusColor = new Color(0.0f, 1.0f, 1.0f, 1.0f); // Color for when nearing full stabilization
    [SerializeField] private Color lowStatusColor = new Color(1.0f, 0.0f, 0.0f, 1.0f); // Color for when stabilization is zero
    [SerializeField] private Color completeColor = Color.green; // Color for when region has been fully stabilized
    [SerializeField] private Color nullColor = new Color(1.0f, 1.0f, 1.0f, 0.2f); // Color for when region is unharmed (not doing anything)

    [Header("Script Reference")]
    [SerializeField] private EmergencyUIHandler emergencyUIHandler; // Reference to the emergency UI handler

    private bool progressingRegion = false; // Flag to check if a region is currently being stabilized

    private void Awake() // Called when the script instance is being loaded
    {
        displayedInjuryCollection = ScriptableObject.CreateInstance<InjuryCollection>(); // Create an empty injury collection for the displayed injury collection on awake
        InitializeRegionImagesAndSprites(); // Initialize the region images and sprites with the empty displayed injury collection initialized on awake
    }

    private void InitializeRegionImagesAndSprites() // Initialize the region images and sprites
    {
        for (int i = 0; i < allRegionObjects.Length; i++) // Loop through all region objects
        {
            if (allRegionObjects[i] != null) // Check if the region object is not null
            {
                Image image = allRegionObjects[i].GetComponent<Image>(); // Get the image component from the region object
                Button button = allRegionObjects[i].GetComponent<Button>(); // Get the button component from the region object
                if (image != null) // Check if the image component is not null
                {
                    if (image == null) // Log an error if no image component is found
                    {
                        Debug.LogError($"No Image component found on {allRegionObjects[i].name}");
                        continue;
                    }

                    allRegionImages[i] = image; // Add the image to the all region images array
                    allRegionSprites[i] = image.sprite; // Add the sprite of the image to the all region sprites array
                    allRegionImages[i].color = nullColor; // Set the color of the image to the null color
                    allRegionButtons[i] = button; // Add the button to the all region buttons array
                    int index = i; // Capture the current index
                    button.onClick.AddListener(() => OnRegionButtonClicked(index)); // Add a listener to the button
                }
                else
                {
                    Debug.LogError($"No Image component found on {allRegionObjects[i].name}"); // Log an error if no image component is found
                }
            }
            else
            {
                Debug.LogError($"allRegionObjects[{i}] is null"); // Log an error if the region object is null
            }
        }

        for (int i = 0; i < allRegionImages.Length; i++) // Check if allRegionImages array is properly initialized
        {
            if (allRegionImages[i] == null) // Log an error if the image is null
            {
                Debug.LogError($"Image for region {(BodyRegion.RegionType)i} is null.");
            }
        }
    }

    public void SetDisplayedInjuryCollection(InjuryCollection newInjuryCollection) // Set the displayed injury collection
    {
        if (newInjuryCollection == null) // Check if the new injury collection is null
        {
            Debug.LogError("No collection entered");
            return;
        }

        if (currentStabilizationCoroutine != null) // Stop the current stabilization coroutine if it is not null
        {
            StopCoroutine(currentStabilizationCoroutine);
        }

        displayedInjuryCollection = newInjuryCollection; // Set the displayed injury collection to the new injury collection
        UpdateDiagramDisplay(); // Update the diagram display with the new injury collection
    }

    public void UpdateDiagramDisplay() // Update the diagram display with the current injury collection
    {
        if (displayedInjuryCollection == null) // Check if the displayed injury collection is null
        {
            Debug.LogError("Injury collection is null.");
            return;
        }

        if (displayedInjuryCollection.injuredBodyCollection == null) // Check if the injured body collection is null
        {
            Debug.LogError("Injured body collection is null.");
            return;
        }

        foreach (var region in displayedInjuryCollection.injuredBodyCollection) // Loop through all body regions in the injured body collection
        {
            if (region == null) // Check if the region is null
            {
                Debug.LogError("Body region is null.");
                continue;
            }

            if (allRegionImages == null || allRegionImages.Length <= (int)region.bodyRegionType) // Check if the all region images array is not properly initialized or the index is out of range
            {
                Debug.LogError("All region images array is not properly initialized or index is out of range.");
                continue;
            }

            if (allRegionImages[(int)region.bodyRegionType] == null) // Check if the image for the region is null
            {
                Debug.LogError($"Image for region {region.bodyRegionType} is null.");
                continue;
            }

            switch (region.regionInjuryStatus) // Switch on the region injury status
            {
                case InjuryStatus.Unharmed: // Set the color of the region image to the null color and disable the button if the region is unharmed
                    allRegionImages[(int)region.bodyRegionType].color = nullColor; // Set the color of the region image to the null color
                    allRegionButtons[(int)region.bodyRegionType].interactable = false; // Disable the button for the region
                    break;
                
                case InjuryStatus.Injured: // Set the color of the region image to a color between the low status color and the full status color based on the stabilization progress and enable the button if the region is injured
                    if (region.stabilizationTime > 0) // Check if the stabilization time is greater than zero
                    {
                        allRegionImages[(int)region.bodyRegionType].color = Color.Lerp(lowStatusColor, fullStatusColor, region.stabilizationProgress / region.stabilizationTime); // Set the color of the region image to a color between the low status color and the full status color based on the stabilization progress
                        allRegionButtons[(int)region.bodyRegionType].interactable = true; // Enable the button for the region
                    }
                    else // Log an error if the stabilization time is zero or negative and set the color of the region image to the low status color and disable the button
                    {
                        Debug.LogError("Stabilization time is zero or negative.");
                        allRegionImages[(int)region.bodyRegionType].color = lowStatusColor; 
                        allRegionButtons[(int)region.bodyRegionType].interactable = false;
                    }
                    break;
                
                case InjuryStatus.Stabilized: // Set the color of the region image to the complete color and disable the button if the region is stabilized
                    allRegionImages[(int)region.bodyRegionType].color = completeColor; // Set the color of the region image to the complete color
                    allRegionButtons[(int)region.bodyRegionType].interactable = false;  // Disable the button for the region
                    break;
            }
        }
    }

    private Coroutine currentStabilizationCoroutine; // The current stabilization coroutine

    private void OnRegionButtonClicked(int index) // Called when a region button is clicked
    {
        Debug.Log($"{allRegionObjects[index].name} Button clicked."); // Log the region button clicked

        if (currentStabilizationCoroutine != null) // Stop the current stabilization coroutine if it is not null
        {
            StopCoroutine(currentStabilizationCoroutine);
        }

        currentStabilizationCoroutine = StartCoroutine(StabilizeRegion(index)); // Start the stabilization coroutine for the region
    }

    private IEnumerator StabilizeRegion(int index) // Stabilize the region over time
    {
        if (displayedInjuryCollection == null) // Check if the displayed injury collection is null
        {
            Debug.LogError("Injury collection is null.");
            yield break;
        }
        if (displayedInjuryCollection.injuredBodyCollection == null) // Check if the injured body collection is null
        {
            Debug.LogError("Injured body collection is null.");
            yield break;
        }
        if (displayedInjuryCollection.injuredBodyCollection.Length <= index) // Check if the index is out of range
        {
            Debug.LogError("Index is out of range.");
            yield break;
        }

        BodyRegion region = displayedInjuryCollection.injuredBodyCollection[index]; // Get the region from the displayed injury collection

        if (region == null) // Check if the region is null
        {
            Debug.LogError("Body region is null.");
            yield break;
        }
        if (region.regionInjuryStatus != InjuryStatus.Injured) // Check if the region is injured
        {
            Debug.LogWarning("Region is not injured.");
            yield break;
        }
        if (region.stabilizationTime <= 0) // Check if the stabilization time is zero or negative
        {
            Debug.LogError("Stabilization time is zero or negative.");
            yield break;
        }

        float progress = region.stabilizationProgress; // Get the current progress of the region
        progressingRegion = true; // Start progressing the region
        while (progressingRegion) // While the progress is less than the stabilization time
        {
            if (progress < region.stabilizationTime) // Check if the progress is less than the stabilization time
            {
                progress += Time.deltaTime;     // Increment the progress
                region.stabilizationProgress = progress; // Update the stabilization progress of the region
                allRegionImages[index].color = Color.Lerp(lowStatusColor, fullStatusColor, progress / region.stabilizationTime); // Update the color of the region image
            }
            else // If the progress is greater than or equal to the stabilization time
            {
                progress = region.stabilizationTime; // Set the progress to the stabilization time
                region.stabilizationProgress = progress; // Update the stabilization progress of the region
                region.regionInjuryStatus = InjuryStatus.Stabilized; // Set the region status to stabilized
                progressingRegion = false; // Stop progressing the region
            }
            yield return null; // Wait for the next frame
        }

        UpdateDiagramDisplay(); // Update the display
        displayedInjuryCollection.UpdateStabilizedRegionCount(); // Update the stabilized region total
        displayedInjuryCollection.GetProgressSum(); // Update the progress sum of the entire injury collection
        emergencyUIHandler.UpdateRegionTotals(); // Update the emergency UI region totals
    }

}
