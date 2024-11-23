using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    public int slotNum = 3;     // Number of slots for storing colonists
    public List<Colonist> slotList; // List of colonist slots full and empty
    public Image[] colonistSlots;
    public Sprite fullSlot;
    public Sprite emptySlot;

    private int selectedColonistIndex = 0; // Tracks the currently selected colonist
    private int filledSlots = 0;

    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = transform;
    }

    private void Start()
    {
        InstantiateSlotList();
        UpdateUISlots(); // Initial colonist slots UI update
    }

    //private void Update()
    //{

    //}

    public void UpdateUISlots()
    {
        for (int i = 0; i < colonistSlots.Length; i++)
        {
            if (i < slotNum)
            {
                colonistSlots[i].enabled = true;

                if (slotList[i] != null)
                {
                    colonistSlots[i].sprite = fullSlot;
                }
                else
                {
                    colonistSlots[i].sprite = emptySlot;
                }

                colonistSlots[i].color = (i == selectedColonistIndex) ? Color.green : Color.white; // Highlight the selected colonist slot
            }
            else
            {
                colonistSlots[i].enabled = false;
            }
        }

        //for (int i = 0; i < colonistSlots.Length; i++)
        //{
        //    if (i < slotNum)
        //    {
        //        colonistSlots[i].enabled = true;

        //        if (i < storedColonists.Count)
        //        {
        //            colonistSlots[i].sprite = fullSlot;
        //        }
        //        else
        //        {
        //            colonistSlots[i].sprite = emptySlot;
        //        }

        //        colonistSlots[i].color = (i == selectedColonistIndex) ? Color.green : Color.white; // Highlight the selected colonist slot
        //    }
        //    else
        //    {
        //        colonistSlots[i].enabled = false;
        //    }
        //}
    }

    private void InstantiateSlotList()
    {
        slotList = new();

        for (int x = 0; x < slotNum; x++)
        {
            slotList.Add(null);
        }
    }

    public void SelectNextColonist()
    {
        selectedColonistIndex = (selectedColonistIndex + 1) % slotNum;
        UpdateUISlots(); // Update colonist slots UI after selecting next slot
        Debug.Log($"Selected next colonist slot, slot {selectedColonistIndex}");
    }

    public void SelectPreviousColonist()
    {
        selectedColonistIndex = (selectedColonistIndex - 1 + slotNum) % slotNum;
        UpdateUISlots(); // Update colonist slots UI after selecting previous slot
        Debug.Log($"Selected previous colonist slot, slot {selectedColonistIndex}");
    }

    public void CollectColonist(Colonist colonist)
    {
        if (filledSlots >= slotNum)
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
        Debug.Log($"Colonist Collected. Slots remaining: {slotNum - filledSlots}/{slotNum}");

        //if (storedColonists.Count >= slotNum)
        //{
        //    Debug.Log("No room for Colonist"); // Notify player that there is no room for more colonists
        //    return;
        //}

        //storedColonists.Add(colonist); // Add colonist data to stored colonist list
        //colonist.gameObject.SetActive(false); // Deactivate colonist in the scene
        //UpdateUISlots(); // Update colonist slots UI to account for colonist collected
        //Debug.Log($"Colonist Collected. Slots remaining: {slotNum - storedColonists.Count}/{slotNum}");
    }

    public void EjectColonist()
    {
        if (slotList[selectedColonistIndex] != null)
        {
            --filledSlots;
            Colonist colonist = slotList[selectedColonistIndex];
            slotList[selectedColonistIndex] = null;
            colonist.transform.position = playerTransform.position + playerTransform.forward; // Eject colonist near player (1 unit in front of)
            colonist.gameObject.SetActive(true); // Reactivate colonist in the scene
            UpdateUISlots(); // Update colonist slots UI to account for colonist ejected
            colonist.gameObject.GetComponent<Rigidbody>().velocity = playerTransform.forward; // Have ejected colonist moving away from player
            Debug.Log($"Colonist Ejected. Slots remaining: {slotNum - filledSlots}/{slotNum}");
        }
        else
        {
            Debug.Log($"Slot {selectedColonistIndex} is empty, can't eject");
        }
    }

    //public void EjectColonist(Colonist colonist)
    //{
    //    if (storedColonists.Contains(colonist))
    //    {
    //        storedColonists.Remove(colonist);
    //        colonist.transform.position = playerTransform.position + playerTransform.forward; // Eject colonist near player (1 unit in front of)
    //        colonist.gameObject.SetActive(true); // Reactivate colonist in the scene
    //        UpdateUISlots(); // Update colonist slots UI to account for colonist ejected
    //        colonist.gameObject.GetComponent<Rigidbody>().velocity = playerTransform.forward; // Have ejected colonist moving away from player
    //        Debug.Log($"Colonist Ejected. Slots remaining: {slotNum - storedColonists.Count}/{slotNum}");
    //    }
    //}
}
