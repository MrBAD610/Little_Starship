using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = transform;
    }

    private void Start()
    {
        InstantiateSlots(); // Instantiate slot list and slot UI
    }

    private void InstantiateSlots()
    {
        slotList = new List<Colonist>(new Colonist[numberOfSlots]); // Initialize slotList filled with preset capacity, numberOfSlots, of null values
        
        for (int i = 0; i < colonistSlotImages.Length; i++)
        {
            if (i < numberOfSlots)
            {
                colonistSlotImages[i].enabled = true;

                if (slotList[i] != null)
                {
                    colonistSlotImages[i].sprite = fullSlotSprite;
                }
                else
                {
                    colonistSlotImages[i].sprite = emptySlotSprite;
                }

                colonistSlotImages[i].color = (i == selectedColonistIndex) ? Color.green : Color.white; // Highlight the selected colonist slot
            }
            else
            {
                colonistSlotImages[i].enabled = false;
            }
        }
    }


    public void UpdateUISlot(int index)
    {
        if (index >= numberOfSlots) return;

        if (slotList[index] != null)
        {
            colonistSlotImages[index].sprite = fullSlotSprite;
        }
        else
        {
            colonistSlotImages[index].sprite = emptySlotSprite;
        }

        colonistSlotImages[index].color = (index == selectedColonistIndex) ? Color.green : Color.white; // Highlight the selected colonist slot

    }

    public void SelectNextColonist()
    {
        colonistSlotImages[selectedColonistIndex].color = Color.white; // Reset old slot color
        selectedColonistIndex = (selectedColonistIndex + 1) % numberOfSlots;
        colonistSlotImages[selectedColonistIndex].color = Color.green; // Highlight newly selected slot
        Debug.Log($"Selected next colonist slot, slot {selectedColonistIndex}");
    }

    public void SelectPreviousColonist()
    {
        colonistSlotImages[selectedColonistIndex].color = Color.white; // Reset old slot color
        selectedColonistIndex = (selectedColonistIndex - 1 + numberOfSlots) % numberOfSlots;
        colonistSlotImages[selectedColonistIndex].color = Color.green; // Highlight newly selected slot
        Debug.Log($"Selected previous colonist slot, slot {selectedColonistIndex}");
    }

    public void CollectColonist(Colonist colonist)
    {
        if (filledSlots >= numberOfSlots)
        {
            Debug.Log("No room for Colonist"); // Notify player that there is no room for more colonists
            return;
        }
        ++filledSlots;
        
        if (slotList[selectedColonistIndex] == null)
        {
            slotList[selectedColonistIndex] = colonist; // Add colonist data to stored colonist list at selected slot if empty
            UpdateUISlot(selectedColonistIndex); // Update colonist slots UI to account for colonist collected
        }
        else
        {
            int firstEmptySlotIndex = 0;
            while (slotList[firstEmptySlotIndex] != null)
            {
                ++firstEmptySlotIndex;
            }
            slotList[firstEmptySlotIndex] = colonist; // Add colonist data to stored colonist list first empty slot
            UpdateUISlot(firstEmptySlotIndex); // Update colonist slots UI to account for colonist collected
        }

        colonist.gameObject.SetActive(false); // Deactivate colonist in the scene
        Debug.Log($"Colonist Collected. Slots remaining: {numberOfSlots - filledSlots}/{numberOfSlots}");
    }

    public void EjectColonist()
    {
        if (slotList[selectedColonistIndex] == null)
        {
            Debug.Log($"Slot {selectedColonistIndex} is empty, can't eject");
            return;
        }

        Colonist colonist = slotList[selectedColonistIndex];
        slotList[selectedColonistIndex] = null;
        --filledSlots;

        colonist.transform.position = playerTransform.position + playerTransform.forward; // Eject colonist near player (1 unit in front of)
        colonist.gameObject.SetActive(true); // Reactivate colonist in the scene

        Rigidbody colonistRb = colonist.ColonistRigidbody;
        if (colonistRb != null)
        {
            colonistRb.velocity = playerTransform.forward; // Apply velocity to have ejected colonist moving away from player
        }

        UpdateUISlot(selectedColonistIndex); // Update colonist slots UI to account for colonist ejected
        Debug.Log($"Colonist Ejected. Slots remaining: {numberOfSlots - filledSlots}/{numberOfSlots}");
    }
}
