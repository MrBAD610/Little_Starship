using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("Health Ore Collection")]
    [SerializeField] private GameObject HealthOreCollectionObject; // Health Ore Collection Object
    [SerializeField] private Color emptyOreCollectionColor = new Color(1.0f, 1.0f, 1.0f, 0.2f); // Color for empty ore collection
    [SerializeField] private Color fullOreCollectionColor = new Color(0.0f, 1.0f, 1.0f, 1.0f); // Color for full ore collection
    private Image healthOreIcon = null;
    private TextMeshProUGUI healthOreText = null;
    private int numberOfOres;

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
        InitializeOreCollection(); // Initialize the player's ore collection
        //playerTransform = transform;
        //playerRigidB = GetComponent<Rigidbody>();
    }

    public void InitializeHealth() // Method to initialize the player's health
    {
        currentHealth = maxHealth;
        recoverableHealth = currentHealth; // Set the recoverable health to the current health
        if (OnInitializeHealth != null) OnInitializeHealth(this, EventArgs.Empty); // Invoke the OnInitializeHealth event when the player initializes health
    }

    public void InitializeOreCollection() // Method to initialize the player's ore collection
    {
        numberOfOres = 0; // Set the number of ores to 0
        if (HealthOreCollectionObject != null) // Check if Health Ore Collection Object is not null
        {
            healthOreIcon = HealthOreCollectionObject.GetComponentInChildren<Image>(); // Get the Health Ore Icon component from the Health Ore Collection Object
            if (healthOreIcon == null)
            {
                Debug.LogError("Health Ore Icon is not found."); // Log an error if the Health Ore Icon is not found
                return;
            }
            healthOreText = HealthOreCollectionObject.GetComponentInChildren<TextMeshProUGUI>(); // Get the Health Ore Text component from the Health Ore Collection Object
            if (healthOreText == null) 
            {
                Debug.LogError("Health Ore Text is not found."); // Log an error if the Health Ore Text is not found
                return;
            }
            UpdateHealthOreDisplay(); // Update the health ore display
        }
        else
        {
            Debug.LogError("Health Ore Collection Object is not found."); // Log an error if the Health Ore Collection Object is not found
        }
    }

    private void AddHealthOre() // Add a health ore to the player's collection
    {
        numberOfOres++; // Increase the number of ores by 1
        UpdateHealthOreDisplay(); // Update the health ore display
    }

    public void UseHealthOre() // Remove a health ore from the player's collection
    {
        if (numberOfOres == 0) return; // Check if the number of ores is 0 and don't use the health ore if it is
        numberOfOres--; // Decrease the number of ores by 1
        SetRecovery(recoveryAmount); // Set the recovery amount
        UpdateHealthOreDisplay(); // Update the health ore display
    }

    private void UpdateHealthOreDisplay() // Update the health ore display
    {
        healthOreText.text = ($"x {numberOfOres.ToString()}"); // Set the health ore text to the number of ores
        if (numberOfOres == 0) // Check if the number of ores is 0
        {
            healthOreIcon.color = emptyOreCollectionColor; // Set the health ore icon color to the empty ore collection color
            healthOreText.color = emptyOreCollectionColor; // Set the health ore text color to the empty ore collection color
        }
        else // If the number of ores is not 0
        {
            healthOreIcon.color = fullOreCollectionColor; // Set the health ore icon color to the full ore collection color
            healthOreText.color = fullOreCollectionColor; // Set the health ore text color to the full ore collection color
        }
    }

    public void TakeDamage(int damageAmount) // Method to take damage
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

    public void SetRecovery(float recoveryAmount) // Method to set the recovery amount
    {
        recoverableHealth = Mathf.Clamp(recoverableHealth + recoveryAmount, 0, maxHealth); // proposedHealth is the recoverable health plus the recovery amount, clamped between 0 and the max health
        if (OnSetRecovery != null) OnSetRecovery(this, EventArgs.Empty); // Invoke the OnSetRecovery event when the player sets recovery

        if (regenerationCoroutine == null) // Check if the regeneration coroutine is null
        {
            regenerationCoroutine = StartCoroutine(RegenerateHealth()); // Start the regeneration coroutine
        }
    }

    public float GetHealthNormalized() // Method to get the normalized health value
    {
        return currentHealth / maxHealth; // Return the current health divided by the max health
    }

    public float GetRecoverableHealthNormalized() // Method to get the normalized recoverable health value
    {
        return recoverableHealth / maxHealth;   // Return the recoverable health divided by the max health
    }

    private void OnCollisionEnter(Collision collision) // Method to handle collision events
    {
        if (collision.gameObject.CompareTag("Enemy")) // Check if the player collides with an enemy
        {
            TakeDamage(15);
            //playerRigidB.AddForce(collision.transform.position - playerTransform.position * knockBack, ForceMode.Impulse);
        }

        if (collision.gameObject.CompareTag("Healing Ore")) // Check if the player collides with a healing ore
        {
            AddHealthOre(); // Add a health ore to the player's collection
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
