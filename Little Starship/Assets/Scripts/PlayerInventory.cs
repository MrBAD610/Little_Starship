using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int numberOfSlots = 3;     // Number of slots for storing colonists
    [SerializeField] private List<Colonist> slotList;   // List of colonist slots full and empty

    [Header("UI Settings")]
    [SerializeField] private Image[] colonistSlotImages;
    [SerializeField] private Sprite fullSlotSprite;
    [SerializeField] private Sprite emptySlotSprite;

    private int selectedColonistIndex = 0; // Tracks the currently selected colonist
    private int filledSlots = 0;

    private EmergencyUIHandler emergencyUIHandler;
    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = transform;
        emergencyUIHandler = GetComponent<EmergencyUIHandler>();

        if (colonistSlotImages == null || emergencyUIHandler == null)
        {
            Debug.LogError("UI components or EmergencyUIHandler are not assigned.");
        }
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
            emergencyUIHandler.ClearEmergencyUI(); // Clear the Emergency UI if no colonist is selected
            return;
        }
        Colonist selectedColonist = slotList[selectedColonistIndex];
        emergencyUIHandler.DisplayNewColonistCollections(selectedColonist); // Format emergencies for display
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
        updatedSlotImage.sprite = (slotList[index] != null) ? fullSlotSprite : emptySlotSprite;
        updatedSlotImage.color = (index == selectedColonistIndex) ? Color.green : Color.white;

        if (index == selectedColonistIndex)
        {
            UpdateEmergencyUI(); // Update the Emergency UI for the selected colonist
        }
    }

    public void SelectNextColonist()
    {
        ChangeSelectedColonist((selectedColonistIndex + 1) % numberOfSlots);
    }

    public void SelectPreviousColonist()
    {
        ChangeSelectedColonist((selectedColonistIndex - 1 + numberOfSlots) % numberOfSlots);
    }

    private void ChangeSelectedColonist(int newIndex)
    {
        colonistSlotImages[selectedColonistIndex].color = Color.white; // Reset old slot color
        selectedColonistIndex = newIndex;
        colonistSlotImages[selectedColonistIndex].color = Color.green; // Highlight newly selected slot
        Debug.Log($"Selected colonist slot {selectedColonistIndex + 1}");

        UpdateEmergencyUI(); // Update the UI for the new selection
    }

    public void CollectColonist(Colonist colonist)
    {
        if (filledSlots >= numberOfSlots)
        {
            Debug.Log("No room for Colonist"); // Notify player that there is no room for more colonists
            return;
        }
        if (slotList[selectedColonistIndex] == null) // Check if the selected slot is empty
        {
            if (colonist.firstPickup) // Check if this is the first time the colonist is being picked up
            {
                colonist.firstPickup = false; // Set firstPickup to false after the first pickup
            }
            UpdateUISlot(selectedColonistIndex, colonist); // Update colonist slots UI to account for colonist collected
        }
        else // Find the first empty slot and update it with the new colonist
        {
            int firstEmptySlotIndex = slotList.FindIndex(slot => slot == null); // Find the first empty slot
            if (firstEmptySlotIndex == -1)
            {
                Debug.LogError("No empty slots available.");
                return;
            }
            if (colonist.firstPickup) // Check if this is the first time the colonist is being picked up
            {
                colonist.firstPickup = false; // Set firstPickup to false after the first pickup
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
        colonist = emergencyUIHandler.ApplyProgressionToColonist(colonist);

        EjectColonistFromSlot(colonist);

        UpdateUISlot(selectedColonistIndex, null); // Update colonist slots UI to account for colonist ejected
        --filledSlots;
        Debug.Log($"Colonist Ejected. Slots remaining: {numberOfSlots - filledSlots}/{numberOfSlots}");

        UpdateEmergencyUI(); // Update the UI for the new selection
    }

    private void EjectColonistFromSlot(Colonist colonist)
    {
        colonist.transform.position = playerTransform.position + playerTransform.forward; // Eject colonist near player (1 unit in front of)
        colonist.gameObject.SetActive(true); // Reactivate colonist in the scene

        Rigidbody colonistRb = colonist.ColonistRigidbody;
        if (colonistRb != null)
        {
            colonistRb.velocity = playerTransform.forward; // Apply velocity to have ejected colonist moving away from player
        }
    }

    public void TransmitColonist()
    {
        if (slotList[selectedColonistIndex] == null)
        {
            Debug.Log($"Slot {selectedColonistIndex + 1} is empty, can't transmit");
            return;
        }
        Colonist colonist = slotList[selectedColonistIndex];

        Destroy(colonist.gameObject); // Destroy the colonist object

        UpdateUISlot(selectedColonistIndex, null); // Update colonist slots UI to account for colonist transmitted
        UpdateEmergencyUI(); // Update the UI for the transmitted colonist
        --filledSlots;
    }

    public int GetSelectedColonistIndex()
    {
        return selectedColonistIndex;
    }
}
