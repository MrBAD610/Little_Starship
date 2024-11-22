using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colonist : MonoBehaviour
{
    public List<MedicalEmergency> emergencies;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerInventory playerInventory = other.gameObject.GetComponent<PlayerInventory>();
            if (playerInventory != null)
            {
                playerInventory.CollectColonist(this);
            }
        }
    }
}
