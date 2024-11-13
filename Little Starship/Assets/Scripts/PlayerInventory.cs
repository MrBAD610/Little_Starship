using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    //public int NumberOfCollonists { get; private set; }
    public int slotNum = 3;
    private int colonistNum = 0;

    public Image[] colonistSlots;
    public Sprite fullSlot;
    public Sprite emptySlot;

    private void Update()
    {
        for (int i = 0; i < colonistSlots.Length; i++)
        {
            if (i < colonistNum)
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

    public void ColonistCollected()
    {
        //NumberOfCollonists++;
        colonistNum++;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Colonist")
        {
            Debug.Log("A collider has made contact with the Colonist Collider");

            if (colonistNum < slotNum)
            {
                ColonistCollected();
                other.gameObject.SetActive(false);
                Debug.Log("Colonist Collected");
            }
            else
            {
                Debug.Log("No room for Colonist");
            }

        }
        else
        {
            Debug.Log("Collision Detected");
        }
    }
}
