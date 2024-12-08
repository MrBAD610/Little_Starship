using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EmergencyUIHandler : MonoBehaviour
{
    [Header("Prefabs and Containers")]
    [SerializeField] private Transform EmergencyContainer;    // Parent container for injury collections
    [SerializeField] private GameObject EmergencyPrefab; // Prefab for injury collections
    [SerializeField] private GameObject TransmitButtonPrefab;      // Button for transmitting stabilized colonist

    [SerializeField] private ColonistDiagramUIHandler ColonistDiagramUIHandler;

    private List<InjuryCollection> injuryCollections;

    private List<GameObject> emergencyItems = new List<GameObject>();
    private List<float> emergencyProgressionTimes = new List<float>();

    private Button TransmitButton;

    private CircularProgressBar totalProgressBar;
    private List<CircularProgressBar> emergencyProgressBars = new List<CircularProgressBar>();

    private int selectedEmergencyIndex = 0;

    private int progressingEmergencyIndex = -1;

    private CircularProgressBar progressingEmergency;

    private void Start()
    {
        if (TransmitButtonPrefab == null)
        {
            Debug.LogError("TransmitButtonPrefab not found on EmergencyUIHandler.");
        }

        if (EmergencyPrefab == null || EmergencyContainer == null)
        {
            Debug.LogError("EmergencyUIHandler is missing a prefab or container reference.");
        }

        TransmitButton = TransmitButtonPrefab.GetComponent<Button>();
        if (TransmitButton == null)
        {
            Debug.LogError("TransmitButton not found on TransmitButtonPrefab.");
            return;
        }

        totalProgressBar = TransmitButtonPrefab.GetComponentInChildren<CircularProgressBar>();
        if (totalProgressBar == null)
        {
            Debug.LogError("Total progress bar not found on TransmitButtonPrefab.");
            return;
        }
    }

    private void Update()
    {
        if (totalProgressBar.isComplete)
        {
            TransmitButton.interactable = true;
        }
        else
        {
            TransmitButton.interactable = false;
        }
    }

    public void DisplayInjuryCollections(Colonist colonistInput)
    {
        Colonist currentColonist = colonistInput;

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

        totalProgressBar.isProgressing = false;
        totalProgressBar.timeTillCompletion = currentColonist.neededTimeToStabilizeColonist;
        totalProgressBar.currentProgress = currentColonist.totalStabilizationProgress;

        ClearList(); // Ensure the list is empty before repopulating.

        for (int i = 0; i < injuryCollections.Count; i++)
        {
            // Create Injury Collection Entry
            var emergencyItem = Instantiate(EmergencyPrefab, EmergencyContainer);

            var emergencyText = emergencyItem.GetComponentInChildren<TextMeshProUGUI>();
            emergencyText.text = injuryCollections[i].displayedName;

            var emergencyProgressBar = emergencyItem.GetComponentInChildren<CircularProgressBar>();

            emergencyProgressBar.timeTillCompletion = colonistInjuryCollectionStabilizationTimes[i];
            emergencyProgressionTimes.Add(colonistInjuryCollectionProgressTimes[i]);
            emergencyProgressBar.currentProgress = colonistInjuryCollectionProgressTimes[i];

            emergencyProgressBars.Add(emergencyProgressBar);
            emergencyItems.Add(emergencyItem);
        }

        HighlightSelectedInjuryCollection(0);
    }

    public void PerformSelection()
    {
        MakeProgress();
    }

    private void MakeProgress()
    {
        if (selectedEmergencyIndex < 0 || selectedEmergencyIndex >= emergencyItems.Count)
        {
            Debug.LogWarning($"Can't progress, selectedEmergencyIndex {selectedEmergencyIndex} out of range (0) - ({emergencyItems.Count - 1})");
            return;
        }

        if (progressingEmergencyIndex != -1)
        {
            progressingEmergency.isProgressing = false;
            totalProgressBar.isProgressing = false;
        }

        progressingEmergencyIndex = selectedEmergencyIndex;
        progressingEmergency = emergencyProgressBars[progressingEmergencyIndex];

        totalProgressBar.isProgressing = true;
        progressingEmergency.isProgressing = true;

        StartCoroutine(TrackingEmergencyProgress());
    }

    public void HighlightSelectedInjuryCollection(int emergencyIndex)
    {
        if (emergencyIndex < 0 || emergencyIndex >= emergencyItems.Count)
        {
            Debug.LogWarning($"Can't highlight Emergency at index {emergencyIndex} since it is out of range (0) - ({emergencyItems.Count - 1})");
            return;
        }

        ResetHighlights();
        selectedEmergencyIndex = emergencyIndex;
        var emergencyItem = emergencyItems[emergencyIndex];
        ColonistDiagramUIHandler.displayedInjuryCollection = injuryCollections[emergencyIndex];
        ColonistDiagramUIHandler.SetDisplay();
        emergencyItem.GetComponent<Image>().color = Color.yellow;

        Debug.Log($"Highlighted Emergency at index {emergencyIndex} between range (0) - ({emergencyItems.Count - 1})");
    }

    public void Scroll(int direction)
    {
        if (emergencyItems.Count == 0) // Check so no divide by zero
        {
            Debug.LogWarning($"Cannot divide to find new index since there are {emergencyItems.Count} emergencies");
            return;
        }
        int newIndex = (selectedEmergencyIndex + direction + emergencyItems.Count) % emergencyItems.Count;

        if (selectedEmergencyIndex < 0 || selectedEmergencyIndex >= emergencyItems.Count)
        {
            Debug.LogWarning($"Scroll failed: {newIndex} is an invalid emergency index to scroll to.");
            return;
        }

        Debug.Log($"Selected Emeregency at index {newIndex}");
        HighlightSelectedInjuryCollection(newIndex);

    }

    public void ClearDisplay()
    {
        ClearList();
    }

    public Colonist ApplyProgressionToColonist(Colonist newColonist)
    {
        Colonist updatedColonist = newColonist;
        if (updatedColonist == null)
        {
            Debug.LogWarning("Cannot apply progression to null colonist.");
            return null;
        }
        if (updatedColonist.colonistInjuryCollections.Count != emergencyProgressionTimes.Count)
        {
            Debug.LogWarning($"Cannot apply progression to colonist with {updatedColonist.colonistInjuryCollections.Count} injury collections and {emergencyProgressionTimes.Count} progression times.");
            return updatedColonist;
        }
        if (progressingEmergency == null)
        {
            Debug.LogWarning("Cannot apply progression without a progressing emergency.");
            return updatedColonist;
        }

        if (progressingEmergency.isProgressing == true || totalProgressBar.isProgressing == true)
        {
            progressingEmergency.isProgressing = false;
            totalProgressBar.isProgressing = false;
            emergencyProgressionTimes[progressingEmergencyIndex] += progressingEmergency.currentProgress;
        }

        updatedColonist.totalStabilizationProgress = totalProgressBar.currentProgress;
        updatedColonist.progressOfInjuryCollections = emergencyProgressionTimes;
        return updatedColonist;
    }

    private void ResetHighlights()
    {
        foreach (var emergency in emergencyItems)
        {
            var image = emergency.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white; // Reset to default
            }
        }
    }

    private void ClearList()
    {
        foreach (var emergency in emergencyItems)
        {
            Destroy(emergency);
        }
        emergencyItems.Clear();

        emergencyProgressBars.Clear();
        emergencyProgressionTimes = new List<float>();

        InjuryCollection emptyInjuryCollection = ScriptableObject.CreateInstance<InjuryCollection>();
        ColonistDiagramUIHandler.displayedInjuryCollection = emptyInjuryCollection;
        ColonistDiagramUIHandler.SetDisplay();

        if (totalProgressBar != null)
        {
            totalProgressBar.isProgressing = false;
            totalProgressBar.currentProgress = 0f;
        }

        progressingEmergencyIndex = -1;  // reset progressingEmergencyIndex
    }

    private IEnumerator TrackingEmergencyProgress()
    {
        while (progressingEmergency.isProgressing == true)
        {
            if (progressingEmergency.isProgressing == false)
            {
                progressingEmergency.isProgressing = false;
                totalProgressBar.isProgressing = false;

                emergencyProgressionTimes[progressingEmergencyIndex] += progressingEmergency.currentProgress;
            }
            else
            {
                progressingEmergency.isProgressing = true;
                totalProgressBar.isProgressing = true;
                yield return null;
            }
        }
    }
}
