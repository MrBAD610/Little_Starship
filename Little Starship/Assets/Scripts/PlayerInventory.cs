using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    public int slotNum = 3;     // Number of slots for storing colonists
    public List<Colonist> storedColonists = new();
    public Image[] colonistSlots;
    public Sprite fullSlot;
    public Sprite emptySlot;

    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = transform;
    }

    private void Start()
    {
        UpdateUISlots(); // Initial colonist slots UI update
    }

    //private void Update()
    //{

    //}

    public void UpdateUISlots()
    {
        for (int i = 0; i < colonistSlots.Length; i++)
        {
            if (i < storedColonists.Count)
            {
                colonistSlots[i].sprite = fullSlot;
            }
            else
            {
                colonistSlots[i].sprite = emptySlot;
            }

            if (i < slotNum)
            {
                colonistSlots[i].enabled = true;
            }
            else
            {
                colonistSlots[i].enabled = false;
            }
        }
    }

    public void CollectColonist(Colonist colonist)
    {
        if (storedColonists.Count >= slotNum)
        {
            Debug.Log("No room for Colonist"); // Notify player that there is no room for more colonists
            return;
        }

        storedColonists.Add(colonist); // Add colonist data to stored colonist list
        colonist.gameObject.SetActive(false); // Deactivate colonist in the scene
        UpdateUISlots(); // Update colonist slots UI to account for colonist collected
        Debug.Log($"Colonist Collected. Slots remaining: {slotNum - storedColonists.Count}/{slotNum}");
    }

    public void EjectColonist(Colonist colonist)
    {
        if (storedColonists.Contains(colonist))
        {
            storedColonists.Remove(colonist);
            colonist.transform.position = playerTransform.position + playerTransform.forward; // Eject colonist near player (1 unit in front of)
            colonist.gameObject.SetActive(true); // Reactivate colonist in the scene
            UpdateUISlots(); // Update colonist slots UI to account for colonist ejected
            colonist.gameObject.GetComponent<Rigidbody>().velocity = playerTransform.forward; // Have ejected colonist moving away from player
            Debug.Log($"Colonist Ejected. Slots remaining: {slotNum - storedColonists.Count}/{slotNum}");
        }
    }
}
