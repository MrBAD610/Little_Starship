using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;

public class EmergencyUIHandler : MonoBehaviour // Script for handling emergency UI
{
    [Header("Prefabs and Containers")]
    [SerializeField] private Transform InjuryCollectionReadoutContainer;    // Parent container for injury collections
    [SerializeField] private GameObject InjuryCollectionReadoutPrefab; // Prefab for injury collections
    [SerializeField] private GameObject TransmitButtonPrefab;      // Button for transmitting stabilized colonist

    [Header("Script Reference")]
    [SerializeField] private ColonistDiagramUIHandler ColonistDiagramUIHandler; // Reference to ColonistDiagramUIHandler

    private List<InjuryCollection> currentInjuryCollections; // List of injury collections for the current colonist

    private List<GameObject> injuryCollectionReadoutItems = new List<GameObject>(); // List of injury collection readout items
    private List<TextMeshProUGUI> regionTotals = new List<TextMeshProUGUI>(); // List of region totals text components for each injury collection
    private List<float> injuryCollectionProgressTimes = new List<float>(); // List of injury collection progress times
    private List<Button> injuryCollectionButtons = new List<Button>(); // List of injury collection buttons

    private Button TransmitButton; // Button for transmitting stabilized colonist

    private Colonist currentColonist; // Reference to the current colonist

    private void Awake()
    {
        if (TransmitButtonPrefab == null) // Check if the transmit button prefab is assigned
        {
            Debug.LogError("TransmitButtonPrefab not found on EmergencyUIHandler.");
        }
        TransmitButton = TransmitButtonPrefab.GetComponent<Button>(); // Get the button component from the transmit button prefab
    }

    private void Start()
    {
        TransmitButton.interactable = false; // Disable the transmit button by default

        if (InjuryCollectionReadoutPrefab == null || InjuryCollectionReadoutContainer == null)
        {
            Debug.LogError("EmergencyUIHandler is missing a prefab or container reference.");
        }
        if (TransmitButton == null)
        {
            Debug.LogError("TransmitButton not found on TransmitButtonPrefab.");
            return;
        }
        if (ColonistDiagramUIHandler == null)
        {
            Debug.LogError("ColonistDiagramUIHandler not found on EmergencyUIHandler.");
            return;
        }
    }

    public void DisplayNewColonistCollections(Colonist colonistInput)
    {
        currentColonist = colonistInput;

        currentInjuryCollections = currentColonist.colonistInjuryCollections;
        var colonistInjuryCollectionStabilizationTimes = currentColonist.neededTimeForEachInjuryCollection;
        var colonistInjuryCollectionProgressTimes = currentColonist.progressOfInjuryCollections;

        // Debug logs to check the values
        Debug.Log($"New Injury Collections Count: {currentInjuryCollections.Count}");
        Debug.Log($"Stabilization Times Count: {colonistInjuryCollectionStabilizationTimes.Count}");
        Debug.Log($"Progress Times Count: {colonistInjuryCollectionProgressTimes.Count}");

        if (currentInjuryCollections.Count != colonistInjuryCollectionStabilizationTimes.Count || currentInjuryCollections.Count != colonistInjuryCollectionProgressTimes.Count)
        {
            Debug.LogError($"Mismatch: currentInjuryCollections.Count ({currentInjuryCollections.Count}) != colonistInjuryCollectionStabilizationTimes.Count ({colonistInjuryCollectionStabilizationTimes.Count}) or != colonistInjuryCollectionProgressTimes.Count ({colonistInjuryCollectionProgressTimes.Count})");
            return; // Exit early to prevent runtime error
        }

        if (colonistInjuryCollectionStabilizationTimes.Count == 0 || colonistInjuryCollectionProgressTimes.Count == 0)
        {
            Debug.LogWarning($"colonistInjuryCollectionStabilizationTimes ({colonistInjuryCollectionStabilizationTimes.Count}) or colonistInjuryCollectionProgressTimes ({colonistInjuryCollectionProgressTimes.Count}) is empty.");
            return; // Avoid processing further
        }

        ClearEmergencyUI(); // Ensure the list is empty before repopulating.
        TransmitButton.interactable = false; // Disable the transmit button for the new colonist

        injuryCollectionProgressTimes = new List<float>(colonistInjuryCollectionProgressTimes); // Update injuryCollectionProgressTimes

        for (int i = 0; i < currentInjuryCollections.Count; i++)
        {
            var injuryCollectionItem = Instantiate(InjuryCollectionReadoutPrefab, InjuryCollectionReadoutContainer); // Instantiate the injury collection prefab
            var injuryCollectionTexts = injuryCollectionItem.GetComponentsInChildren<TextMeshProUGUI>(); // Get all TextMeshProUGUI components in the injury collection item

            if (injuryCollectionTexts.Length > 0) // Check if TextMeshProUGUI components are found
            {
                injuryCollectionTexts[0].text = currentInjuryCollections[i].displayedName; // Update the injury collection name text
                currentInjuryCollections[i].UpdateStabilizedRegionCount(); // Update the stabilized region count for the injury collection
                injuryCollectionTexts[1].text = currentInjuryCollections[i].stabilizedRegionTotal; // Update the region total text
                regionTotals.Add(injuryCollectionTexts[1]); // Add the region total text component to the regionTotals list
            }
            else // Log a warning if no TextMeshProUGUI components are found
            {
                Debug.LogWarning("No TextMeshProUGUI components found in injuryCollectionItem.");
            }

            injuryCollectionReadoutItems.Add(injuryCollectionItem);
            var button = injuryCollectionItem.GetComponent<Button>();
            injuryCollectionButtons.Add(button);

            // Add listener to the button
            int index = i; // Capture the current index
            button.onClick.AddListener(() => OnInjuryCollectionButtonClicked(index));
        }

        // Restore the previously selected injury collection index for the current colonist
        int bookmarkedIndex = currentColonist.selectedInjuryCollectionIndex;
        OnInjuryCollectionButtonClicked(bookmarkedIndex);
        UpdateRegionTotals();
    }

    private void OnInjuryCollectionButtonClicked(int index)
    {
        Debug.Log($"Button {index} clicked.");

        ColonistDiagramUIHandler.SetDisplayedInjuryCollection(currentInjuryCollections[index]);

        // Save the selected injury collection index for the current colonist
        if (currentColonist != null)
        {
            currentColonist.selectedInjuryCollectionIndex = index;
        }
    }

    public void UpdateRegionTotals() // Update the region totals for each injury collection
    {
        bool allStabilized = true; // Flag to check if all regions are stabilized

        for (int i = 0; i < currentInjuryCollections.Count; i++) // Loop through all injury collections
        {
            if (regionTotals[i] != null)
            {
                regionTotals[i].text = currentInjuryCollections[i].stabilizedRegionTotal; // Update the region total text
            }

            if (currentInjuryCollections[i].isStabilized) // Check if the injury collection is stabilized
            {
                if (injuryCollectionButtons[i] != null)
                {
                    injuryCollectionButtons[i].interactable = false; // Disable the button if the injury collection is stabilized
                }
                else
                {
                    Debug.LogWarning("Button not found for injury collection.");
                }
            }
            else
            {
                if (injuryCollectionButtons[i] != null)
                {
                    injuryCollectionButtons[i].interactable = true; // Enable the button if the injury collection is not stabilized
                }
                allStabilized = false; // Set the flag to false if any injury collection is not stabilized
            }
        }

        if (allStabilized) // Check if all injury collections are stabilized
        {
            if (TransmitButton != null)
            {
                TransmitButton.interactable = true; // Enable the transmit button if all injury collections are stabilized
            }
        }
        else
        {
            if (TransmitButton != null)
            {
                TransmitButton.interactable = false; // Disable the transmit button if any injury collection is not stabilized
            }
        }
    }

    public Colonist ApplyProgressionToColonist(Colonist newColonist)
    {
        if (newColonist == null)
        {
            Debug.LogWarning("Cannot apply progression to null colonist.");
            return null;
        }

        if (newColonist.colonistInjuryCollections.Count != injuryCollectionProgressTimes.Count)
        {
            Debug.LogWarning($"Cannot apply progression to colonist with {newColonist.colonistInjuryCollections.Count} injury collections and {injuryCollectionProgressTimes.Count} progression times.");
            return newColonist;
        }

        for (int i = 0; i < newColonist.colonistInjuryCollections.Count; i++)
        {
            var injuryCollection = newColonist.colonistInjuryCollections[i];
            var progressTime = injuryCollectionProgressTimes[i];

            // Update the progress time for each injury collection
            injuryCollection.progressSum = progressTime;

            // Update the stabilized region total
            injuryCollection.UpdateStabilizedRegionCount();
        }

        // Optionally, update the total stabilization progress for the colonist
        newColonist.totalStabilizationProgress = injuryCollectionProgressTimes.Sum();

        return newColonist;
    }

    public void ClearEmergencyUI()
    {
        foreach (var injuryCollection in injuryCollectionReadoutItems)
        {
            Destroy(injuryCollection);
        }

        injuryCollectionReadoutItems.Clear();
        regionTotals.Clear();
        injuryCollectionProgressTimes.Clear();
        injuryCollectionButtons.Clear();

        InjuryCollection emptyInjuryCollection = ScriptableObject.CreateInstance<InjuryCollection>();
        ColonistDiagramUIHandler.SetDisplayedInjuryCollection(emptyInjuryCollection);
        TransmitButton.interactable = false;
    }
}
