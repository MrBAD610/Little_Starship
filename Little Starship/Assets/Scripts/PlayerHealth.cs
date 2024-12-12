using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public event EventHandler OnInitializeHealth;
    public event EventHandler OnDamaged;
    public event EventHandler OnSetRecovery;
    public event EventHandler OnRegenerate;

    [Header("Health Settings")]
    public float currentHealth = 75.0f;
    public float maxHealth = 100.0f;

    [Header("Recovery Settings")]
    public float recoveryPerInterval = 1.0f;
    public float recoveryIntervalDuration = 1.0f;


    private float recoverableHealth = 100.0f;
    private Coroutine regenerationCoroutine = null;

    //public float knockBack = 10.0f;

    //private Transform playerTransform;
    //private Rigidbody playerRigidB;

    public bool isAlive = true;
    public float recoveryAmount = 25.0f;

    // Start is called before the first frame update
    void Start()
    {
        InitializeHealth(); // Initialize the player's health
        //playerTransform = transform;
        //playerRigidB = GetComponent<Rigidbody>();
    }

    public void InitializeHealth() // Method to initialize the player's health
    {
        currentHealth = maxHealth;  
        recoverableHealth = currentHealth; // Set the recoverable health to the current health
        if (OnInitializeHealth != null) OnInitializeHealth(this, EventArgs.Empty); // Invoke the OnInitializeHealth event when the player initializes health
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount; // Decrease the current health by the damage amount
        recoverableHealth -= damageAmount; // Decrease the recoverable health by the damage amount

        if (currentHealth <= 0) // Check if the current health is less than or equal to 0
        {
            currentHealth = 0; // Set the current health to 0
            isAlive = false; // Set the isAlive flag to false
        }
        if (regenerationCoroutine == null) // Check if the regeneration coroutine is null
        {
            regenerationCoroutine = StartCoroutine(RegenerateHealth()); // Start the regeneration coroutine
        }
        if (OnDamaged != null) OnDamaged(this, EventArgs.Empty); // Invoke the OnDamaged event when the player takes damage
    }

    public void SetRecovery(float recoveryAmount)
    {
        recoverableHealth = Mathf.Clamp(recoverableHealth + recoveryAmount, 0, maxHealth); // proposedHealth is the recoverable health plus the recovery amount, clamped between 0 and the max health
        if (OnSetRecovery != null) OnSetRecovery(this, EventArgs.Empty); // Invoke the OnSetRecovery event when the player sets recovery

        if (regenerationCoroutine == null) // Check if the regeneration coroutine is null
        {
            regenerationCoroutine = StartCoroutine(RegenerateHealth()); // Start the regeneration coroutine
        }
    }

    public float GetHealthNormalized()
    {
        return currentHealth / maxHealth;
    }

    public float GetRecoverableHealthNormalized()
    {
        return recoverableHealth / maxHealth;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) // Check if the player collides with an enemy
        {
            TakeDamage(15);
            //playerRigidB.AddForce(collision.transform.position - playerTransform.position * knockBack, ForceMode.Impulse);
        }

        if (collision.gameObject.CompareTag("Healing Ore")) // Check if the player collides with a healing ore
        {
            SetRecovery(recoveryAmount); // Set the recovery amount to the heal amount
            Destroy(collision.gameObject); // Destroy the healing ore
        }
    }

    private IEnumerator RegenerateHealth() // Coroutine to regenerate health over time
    {
        while (currentHealth < recoverableHealth)  // Loop while the current health is less than the recoverable health
        {
            yield return new WaitForSeconds(recoveryIntervalDuration); // Wait for the recovery interval duration
            currentHealth = Mathf.Clamp(currentHealth + recoveryPerInterval, 0, recoverableHealth); // Increase the current health by the recovery per interval
            if (OnRegenerate != null) OnRegenerate(this, EventArgs.Empty); // Invoke the OnRegenerate event when the player regenerates health
        }
        regenerationCoroutine = null; // Set the regeneration coroutine to null when the coroutine is finished
    }
}
