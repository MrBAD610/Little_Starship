using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100.0f;
    public float currentHealth = 75.0f;
    //public float knockBack = 10.0f;

    public Slider healthBar;
    //private Transform playerTransform;
    //private Rigidbody playerRigidB;

    public bool isAlive = true;

    public float healAmount = 25.0f;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;

        //playerTransform = transform;
        //playerRigidB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        healthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            isAlive = false;
        }
    }

    public void Heal(float healAmount)
    {
        float newHealth = currentHealth + healAmount;
        if (newHealth > maxHealth)
        {
            //return;
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = newHealth;
        }
        healthBar.value = currentHealth;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(15);
            //playerRigidB.AddForce(collision.transform.position - playerTransform.position * knockBack, ForceMode.Impulse);
        }

        if (collision.gameObject.CompareTag("Healing Ore"))
        {
            Heal(healAmount);
            Destroy(collision.gameObject);
        }
    }
}
