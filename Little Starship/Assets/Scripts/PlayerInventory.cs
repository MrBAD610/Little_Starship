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
            if (colonistNum > slotNum)
            {
                colonistNum = slotNum;
            }

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
}
