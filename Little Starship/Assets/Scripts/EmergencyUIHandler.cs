using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;

public class EmergencyUIHandler : MonoBehaviour
{
    [Header("Prefabs and Containers")]
    [SerializeField] private Transform InjuryCollectionReadoutContainer;    // Parent container for injury collections
    [SerializeField] private GameObject InjuryCollectionReadoutPrefab; // Prefab for injury collections
    [SerializeField] private GameObject TransmitButtonPrefab;      // Button for transmitting stabilized colonist

    [Header("Script Reference")]
    [SerializeField] private ColonistDiagramUIHandler ColonistDiagramUIHandler;

    private List<InjuryCollection> currentInjuryCollections;

    private List<GameObject> injuryCollectionReadoutItems = new List<GameObject>();
    private List<TextMeshProUGUI> regionTotals = new List<TextMeshProUGUI>();
    private List<float> injuryCollectionProgressTimes = new List<float>();
    private List<Button> injuryCollectionButtons = new List<Button>();

    private Button TransmitButton;

    private Colonist currentColonist;

    private void Start()
    {
        if (TransmitButtonPrefab == null)
        {
            Debug.LogError("TransmitButtonPrefab not found on EmergencyUIHandler.");
        }

        if (InjuryCollectionReadoutPrefab == null || InjuryCollectionReadoutContainer == null)
        {
            Debug.LogError("EmergencyUIHandler is missing a prefab or container reference.");
        }

        TransmitButton = TransmitButtonPrefab.GetComponent<Button>();
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

        injuryCollectionProgressTimes = new List<float>(colonistInjuryCollectionProgressTimes); // Update injuryCollectionProgressTimes

        for (int i = 0; i < currentInjuryCollections.Count; i++)
        {
            // Create Injury Collection Entry
            var injuryCollectionItem = Instantiate(InjuryCollectionReadoutPrefab, InjuryCollectionReadoutContainer);

            var injuryCollectionTexts = injuryCollectionItem.GetComponentsInChildren<TextMeshProUGUI>();
            if (injuryCollectionTexts.Length > 0)
            {
                // Assuming you want to modify the first TextMeshProUGUI component
                injuryCollectionTexts[0].text = currentInjuryCollections[i].displayedName;
                currentInjuryCollections[i].UpdateStabilizedRegionTotal();
                injuryCollectionTexts[1].text = currentInjuryCollections[i].stabilizedRegionTotal;
                regionTotals.Add(injuryCollectionTexts[1]);
            }
            else
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

    public void UpdateRegionTotals() 
    {
        for (int i = 0; i < currentInjuryCollections.Count; i++)
        {
            regionTotals[i].text = currentInjuryCollections[i].stabilizedRegionTotal;
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
            injuryCollection.UpdateStabilizedRegionTotal();
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
        injuryCollectionProgressTimes = new List<float>();

        InjuryCollection emptyInjuryCollection = ScriptableObject.CreateInstance<InjuryCollection>();
        ColonistDiagramUIHandler.SetDisplayedInjuryCollection(emptyInjuryCollection);
    }
}
