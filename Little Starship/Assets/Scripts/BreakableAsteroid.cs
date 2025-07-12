using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableAsteroid : MonoBehaviour
{
    [Header("Fracture Settings")]
    public float fractureForce = 1000f;
    public GameObject fracturedVersion;

    public bool beingHeld = false;

    [Header("Dropable Items")]
    public List<GameObject> dropableHealthOre = new List<GameObject>(); // List of possible health ore prefabs to drop

    private Rigidbody asteroidRigidbody;

    private void Awake()
    {
        asteroidRigidbody = GetComponent<Rigidbody>(); // Get the rigidbody component
    }

    private void DropItem(List<GameObject> listOfVariations)
    {
        if (listOfVariations.Count > 0) // If there are items in the list
        {
            int randomIndex = Random.Range(0, listOfVariations.Count); // Get a random index
            GameObject itemToDrop = listOfVariations[randomIndex]; // Get the item to drop
            Instantiate(itemToDrop, transform.position, transform.rotation); // Spawn the item
        }
    }

    private void OnCollisionEnter(Collision collision) // When the object collides with something
    {
        Vector3 impactForce = collision.impulse / Time.deltaTime; // Calculate the force of the impact
        Vector3 velocityAtImpact = -collision.relativeVelocity; // Get the velocity at the point of impact
        if (impactForce.magnitude > fractureForce && beingHeld) // If the force is greater than the fracture force and the object is being held
        {
            GameObject fracturedObject = Instantiate(fracturedVersion, transform.position, transform.rotation); // Spawn in the broken version
            Transform fracturedTransform = fracturedObject.transform; // Get the transform of the broken object
            foreach (Transform child in fracturedTransform) // For each child in the broken object
            {
                Rigidbody childRigidbody = child.GetComponent<Rigidbody>(); // Get the rigidbody component
                if (childRigidbody != null) // If the child has a rigidbody
                {
                    childRigidbody.velocity = velocityAtImpact; // Set the velocity of the child to the impact velocity
                    //childRigidbody.AddExplosionForce(impactForce.normalized.magnitude, transform.position, 1f); // Add an explosion
                }
            }

            if (collision.gameObject.CompareTag("Enemy")) // If the object collided with an enemy kill the enemy
            {
                //collision.gameObject.GetComponent<OldEnemyNavScript>().Kill(); // Kill the enemy
            }

            DropItem(dropableHealthOre); // Drop a health ore item
            Destroy(gameObject); // Destroy the object to stop it getting in the way
        }
    }
}
