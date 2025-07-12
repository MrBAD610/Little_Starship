using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PlayerSensor : MonoBehaviour
{
    public delegate void PlayerEnterEvent(Transform player);
    public delegate void PlayerExitEvent(Vector3 lastKnownPosition);
    public event PlayerEnterEvent OnPlayerEnter;
    public event PlayerExitEvent OnPlayerExit;

    private void OnTriggerEnter(Collider other) // Triggers event when player enters sphere
    {
        if (other.TryGetComponent(out PlayerController player)) // Tries to get PlayerController component if it exists
        {
            OnPlayerEnter?.Invoke(player.transform); // Invokes OnPlayerEnter event
        }
    }

    private void OnTriggerExit(Collider other) // Triggers event when player exits sphere
    {
        if (other.TryGetComponent(out PlayerController player)) // Tries to get PlayerController component if it exists
        {
            OnPlayerExit?.Invoke(other.transform.position); // Invokes OnPlayerExit event
        }
    }
}
