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

    [SerializeField] private ColonistDiagramUIHandler ColonistDiagramUIHandler;

    private List<InjuryCollection> injuryCollections;

    private List<GameObject> injuryCollectionReadoutItems = new List<GameObject>();
    private List<float> injuryCollectionProgressTimes = new List<float>();
    private List<Button> injuryCollectionButtons = new List<Button>();

    private Button TransmitButton;

    private int selectedInjuryCollectionIndex = 0;

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
    }

    public void DisplayInjuryCollections(Colonist colonistInput)
    {
        currentColonist = colonistInput;

        injuryCollections = currentColonist.colonistInjuryCollections;
        var colonistInjuryCollectionStabilizationTimes = currentColonist.neededTimeForEachInjuryCollection;
        var colonistInjuryCollectionProgressTimes = currentColonist.progressOfInjuryCollections;

        // Debug logs to check the values
        Debug.Log($"Injury Collections Count: {injuryCollections.Count}");
        Debug.Log($"Stabilization Times Count: {colonistInjuryCollectionStabilizationTimes.Count}");
        Debug.Log($"Progress Times Count: {colonistInjuryCollectionProgressTimes.Count}");

        if (injuryCollections.Count != colonistInjuryCollectionStabilizationTimes.Count || injuryCollections.Count != colonistInjuryCollectionProgressTimes.Count)
        {
            Debug.LogError($"Mismatch: injuryCollections.Count ({injuryCollections.Count}) != colonistInjuryCollectionStabilizationTimes.Count ({colonistInjuryCollectionStabilizationTimes.Count}) or != colonistInjuryCollectionProgressTimes.Count ({colonistInjuryCollectionProgressTimes.Count})");
            return; // Exit early to prevent runtime error
        }

        if (colonistInjuryCollectionStabilizationTimes.Count == 0 || colonistInjuryCollectionProgressTimes.Count == 0)
        {
            Debug.LogWarning($"colonistInjuryCollectionStabilizationTimes ({colonistInjuryCollectionStabilizationTimes.Count}) or colonistInjuryCollectionProgressTimes ({colonistInjuryCollectionProgressTimes.Count}) is empty.");
            return; // Avoid processing further
        }

        ClearList(); // Ensure the list is empty before repopulating.

        injuryCollectionProgressTimes = new List<float>(colonistInjuryCollectionProgressTimes); // Update injuryCollectionProgressTimes

        for (int i = 0; i < injuryCollections.Count; i++)
        {
            // Create Injury Collection Entry
            var injuryCollectionItem = Instantiate(InjuryCollectionReadoutPrefab, InjuryCollectionReadoutContainer);

            var injuryCollectionTexts = injuryCollectionItem.GetComponentsInChildren<TextMeshProUGUI>();
            if (injuryCollectionTexts.Length > 0)
            {
                // Assuming you want to modify the first TextMeshProUGUI component
                injuryCollectionTexts[0].text = injuryCollections[i].displayedName;
                injuryCollections[i].UpdateStabilizedRegionTotal();
                injuryCollectionTexts[1].text = injuryCollections[i].stabilizedRegionTotal;
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
        OnInjuryCollectionButtonClicked(currentColonist.selectedInjuryCollectionIndex);
    }

    private void OnInjuryCollectionButtonClicked(int index)
    {
        Debug.Log($"Button {index} clicked.");

        ColonistDiagramUIHandler.SetDisplayedInjuryCollection(injuryCollections[index]);

        selectedInjuryCollectionIndex = index;

        // Save the selected injury collection index for the current colonist
        if (currentColonist != null)
        {
            currentColonist.selectedInjuryCollectionIndex = index;
        }
    }

    public void ClearDisplay()
    {
        ClearList();
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

    private void ClearList()
    {
        foreach (var injuryCollection in injuryCollectionReadoutItems)
        {
            Destroy(injuryCollection);
        }
        injuryCollectionReadoutItems.Clear();

        injuryCollectionProgressTimes = new List<float>();

        InjuryCollection emptyInjuryCollection = ScriptableObject.CreateInstance<InjuryCollection>();
        ColonistDiagramUIHandler.SetDisplayedInjuryCollection(emptyInjuryCollection);
    }
}
