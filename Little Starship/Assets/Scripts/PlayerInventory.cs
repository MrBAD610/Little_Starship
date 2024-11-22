using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    public int slotNum = 3;     // Number of slots for storing colonists
    private List<Colonist> storedColonists = new();
    //private int colonistNum = 0;

    public Image[] colonistSlots;
    public Sprite fullSlot;
    public Sprite emptySlot;

    private void Update()
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
        Debug.Log($"Colonist Collected. Slots remaining: {slotNum - storedColonists.Count}/{slotNum}");

        //NumberOfCollonists++;
        //colonistNum++;
    }

    //private void OnCollisionEnter(Collision other)
    //{
    //    if (other.gameObject.tag == "Colonist")
    //    {
    //        Debug.Log("A collider has made contact with the Colonist Collider");

    //        if (colonistNum < slotNum)
    //        {
    //            ColonistCollected();
    //            other.gameObject.SetActive(false);
    //            Debug.Log("Colonist Collected");
    //        }
    //        else
    //        {
    //            Debug.Log("No room for Colonist");
    //        }

    //    }
    //    else
    //    {
    //        Debug.Log("Collision Detected");
    //    }
    //}
}
