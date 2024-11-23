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
    [SerializeField] public Image[] colonistSlotSprites;
    [SerializeField] public Sprite fullSlotSprite;
    [SerializeField] public Sprite emptySlotSprite;

    private int selectedColonistIndex = 0; // Tracks the currently selected colonist
    private int filledSlots = 0;

    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = transform;
        InstantiateSlotList();
    }

    private void Start()
    {
        UpdateUISlots(); // Initial colonist slots UI update
    }

    private void InstantiateSlotList()
    {
        //Initializes a new List<Colonist> with a predefined capacity of numberOfSlots and fills it with null values
        slotList = new List<Colonist>(new Colonist[numberOfSlots]);

        //slotList = new();
        //for (int x = 0; x < numberOfSlots; x++)
        //{
        //    slotList.Add(null);
        //}
    }

    public void UpdateUISlots()
    {
        for (int i = 0; i < colonistSlotSprites.Length; i++)
        {
            if (i < numberOfSlots)
            {
                colonistSlotSprites[i].enabled = true;

                if (slotList[i] != null)
                {
                    colonistSlotSprites[i].sprite = fullSlotSprite;
                }
                else
                {
                    colonistSlotSprites[i].sprite = emptySlotSprite;
                }

                colonistSlotSprites[i].color = (i == selectedColonistIndex) ? Color.green : Color.white; // Highlight the selected colonist slot
            }
            else
            {
                colonistSlotSprites[i].enabled = false;
            }
        }
    }

    public void SelectNextColonist()
    {
        selectedColonistIndex = (selectedColonistIndex + 1) % numberOfSlots;
        UpdateUISlots(); // Update colonist slots UI after selecting next slot
        Debug.Log($"Selected next colonist slot, slot {selectedColonistIndex}");
    }

    public void SelectPreviousColonist()
    {
        selectedColonistIndex = (selectedColonistIndex - 1 + numberOfSlots) % numberOfSlots;
        UpdateUISlots(); // Update colonist slots UI after selecting previous slot
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
        }
        else
        {
            int firstEmptySlotIndex = 0;
            while (slotList[firstEmptySlotIndex] != null)
            {
                ++firstEmptySlotIndex;
            }
            slotList[firstEmptySlotIndex] = colonist; // Add colonist data to stored colonist list first empty slot
        }

        colonist.gameObject.SetActive(false); // Deactivate colonist in the scene
        UpdateUISlots(); // Update colonist slots UI to account for colonist collected
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
        Rigidbody colonistRb = colonist.gameObject.GetComponent<Rigidbody>();
        if (colonistRb != null)
        {
            colonistRb.velocity = playerTransform.forward; // Apply velocity to have ejected colonist moving away from player
        }

        UpdateUISlots(); // Update colonist slots UI to account for colonist ejected
        Debug.Log($"Colonist Ejected. Slots remaining: {numberOfSlots - filledSlots}/{numberOfSlots}");
    }
}
