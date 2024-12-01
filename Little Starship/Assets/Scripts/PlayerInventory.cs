using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] public int numberOfSlots = 3;     // Number of slots for storing colonists
    [SerializeField] public List<Colonist> slotList;   // List of colonist slots full and empty

    [Header("UI Settings")]
    [SerializeField] public Image[] colonistSlotImages;
    [SerializeField] public Sprite fullSlotSprite;
    [SerializeField] public Sprite emptySlotSprite;

    private int selectedColonistIndex = 0; // Tracks the currently selected colonist
    private int filledSlots = 0;

    private EmergencyUIHandler emergencyUIHandler;

    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = transform;
        emergencyUIHandler = GetComponent<EmergencyUIHandler>();
    }

    private void Start()
    {
        InstantiateSlots(); // Instantiate slot list and slot UI
        UpdateEmergencyUI(); // Update UI for the initially selected slot
    }

    public void UpdateEmergencyUI()
    {
        if (slotList == null || slotList[selectedColonistIndex] == null)
        {
            emergencyUIHandler.ClearDisplay();
            return;
        }
        Colonist selectedColonist = slotList[selectedColonistIndex];
        emergencyUIHandler.DisplayEmergenciesWithRegions(selectedColonist); // Format emergencies for display
    }

    private void InstantiateSlots()
    {
        slotList = new List<Colonist>(new Colonist[numberOfSlots]); // Initialize slotList filled with preset capacity, numberOfSlots, of null values
        for (int i = 0; i < colonistSlotImages.Length; i++)
        {
            var slotImage = colonistSlotImages[i]; // Cache reference
            if (i < numberOfSlots)
            {
                slotImage.enabled = true;
                slotImage.sprite = emptySlotSprite;
                slotImage.color = (i == selectedColonistIndex) ? Color.green : Color.white; // Highlight the selected colonist slot
            }
            else
            {
                slotImage.enabled = false;
            }
        }
    }

    public void UpdateUISlot(int index, Colonist colonist)
    {
        if (index < 0 || index >= numberOfSlots || slotList == null)
        {
            Debug.LogWarning("Invalid slot index or uninitialized slot list.");
            return;
        }

        slotList[index] = colonist;  // Update the slot data

        var updatedSlotImage = colonistSlotImages[index];
        if (slotList[index] != null)
        {
            updatedSlotImage.sprite = fullSlotSprite;
        }
        else
        {
            updatedSlotImage.sprite = emptySlotSprite;
        }
        updatedSlotImage.color = (index == selectedColonistIndex) ? Color.green : Color.white; // Highlight the selected colonist slot
    }

    public void SelectNextColonist()
    {
        colonistSlotImages[selectedColonistIndex].color = Color.white; // Reset old slot color
        selectedColonistIndex = (selectedColonistIndex + 1) % numberOfSlots;
        colonistSlotImages[selectedColonistIndex].color = Color.green; // Highlight newly selected slot
        Debug.Log($"Selected next colonist slot, slot {selectedColonistIndex + 1}");

        UpdateEmergencyUI(); // Update the UI for the new selection
    }

    public void SelectPreviousColonist()
    {
        colonistSlotImages[selectedColonistIndex].color = Color.white; // Reset old slot color
        selectedColonistIndex = (selectedColonistIndex - 1 + numberOfSlots) % numberOfSlots;
        colonistSlotImages[selectedColonistIndex].color = Color.green; // Highlight newly selected slot
        Debug.Log($"Selected previous colonist slot, slot {selectedColonistIndex + 1}");

        UpdateEmergencyUI(); // Update the UI for the new selection
    }

    public void CollectColonist(Colonist colonist)
    {
        if (filledSlots >= numberOfSlots)
        {
            Debug.Log("No room for Colonist"); // Notify player that there is no room for more colonists
            return;
        }
        if (slotList[selectedColonistIndex] == null)
        {
            UpdateUISlot(selectedColonistIndex, colonist); // Update colonist slots UI to account for colonist collected
            UpdateEmergencyUI();                            // Update the Emergency UI
        }
        else
        {
            int firstEmptySlotIndex = slotList.FindIndex(slot => slot == null);
            if (firstEmptySlotIndex == -1)
            {
                Debug.LogError("No empty slots available.");
                return;
            }
            UpdateUISlot(firstEmptySlotIndex, colonist);
        }
        ++filledSlots;
        colonist.gameObject.SetActive(false); // Deactivate colonist in the scene
        Debug.Log($"Colonist Collected. Slots remaining: {numberOfSlots - filledSlots}/{numberOfSlots}");
    }

    public void EjectColonist()
    {
        if (slotList[selectedColonistIndex] == null)
        {
            Debug.Log($"Slot {selectedColonistIndex + 1} is empty, can't eject");
            return;
        }

        Colonist colonist = slotList[selectedColonistIndex];
        colonist.transform.position = playerTransform.position + playerTransform.forward; // Eject colonist near player (1 unit in front of)
        colonist.gameObject.SetActive(true); // Reactivate colonist in the scene

        Rigidbody colonistRb = colonist.ColonistRigidbody;
        if (colonistRb != null)
        {
            colonistRb.velocity = playerTransform.forward; // Apply velocity to have ejected colonist moving away from player
        }

        UpdateUISlot(selectedColonistIndex, null); // Update colonist slots UI to account for colonist ejected
        --filledSlots;
        Debug.Log($"Colonist Ejected. Slots remaining: {numberOfSlots - filledSlots}/{numberOfSlots}");

        UpdateEmergencyUI(); // Update the UI for the new selection
    }

    public int GetSelectedColonistIndex()
    {
        return selectedColonistIndex;
    }
}
