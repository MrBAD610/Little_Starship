using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Colors")]
    [SerializeField] private Color FullHealthColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] private Color LowHealthColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);

    private PlayerHealth playerHealth;
    private Image barOutlineImage;
    private Image regularBarImage;
    private Image restorationBarImage;

    private void Awake() // Awake is called before Start and is used to initialize variables
    {
        barOutlineImage = transform.Find("Bar Outline").GetComponent<Image>(); // Find the bar outline image in the scene
        if (barOutlineImage == null) // Check if the bar outline image is found in the scene
        {
            Debug.LogError("Bar Outline image not found in the scene.");
        }

        regularBarImage = transform.Find("Regular Bar").GetComponent<Image>(); // Find the regular bar image in the scene
        if (regularBarImage == null) // Check if the regular bar image is found in the scene
        {
            Debug.LogError("Regular Bar image not found in the scene.");
        }

        restorationBarImage = transform.Find("Restoration Bar").GetComponent<Image>(); // Find the restoration bar image in the scene
        if (restorationBarImage == null) // Check if the restoration bar image is found in the scene
        {
            Debug.LogError("Restoration Bar image not found in the scene.");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // Find the player object in the scene
        if (player == null)
        {
            Debug.LogError("Player object not found in the scene.");
        }
        playerHealth = player.GetComponent<PlayerHealth>(); // Get the PlayerHealth component from the player object
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth component not found in the scene.");
        }
        SetHealth(playerHealth.GetHealthNormalized());
        playerHealth.OnInitializeHealth += PlayerHealth_OnInitializeHealth; // Subscribe to the OnInitializeHealth event
        playerHealth.OnDamaged += PlayerHealth_OnDamaged; // Subscribe to the OnDamaged event
        playerHealth.OnSetRecovery += PlayerHealth_OnSetRecovery; // Subscribe to the OnSetRecovery event
        playerHealth.OnRegenerate += PlayerHealth_OnRegenerate; // Subscribe to the OnRegenerate event
    }

    private void PlayerHealth_OnInitializeHealth(object sender, System.EventArgs e)
    {
        SetHealth(playerHealth.GetHealthNormalized()); // Update the health bar when the player initializes health
        SetRestoration(playerHealth.GetRecoverableHealthNormalized()); // Update the restoration bar when the player initializes health
    }
    private void PlayerHealth_OnDamaged(object sender, System.EventArgs e)
    {
        SetHealth(playerHealth.GetHealthNormalized()); // Update the health bar when the player takes damage
        SetRestoration(playerHealth.GetRecoverableHealthNormalized()); // Update the restoration bar when the player takes damage
    }

    private void PlayerHealth_OnSetRecovery(object sender, System.EventArgs e)
    {
        SetRestoration(playerHealth.GetRecoverableHealthNormalized()); // Update the restoration bar when the player sets recovery
    }

    private void PlayerHealth_OnRegenerate(object sender, System.EventArgs e)
    {
        SetHealth(playerHealth.GetHealthNormalized()); // Update the health bar when the player regenerates health
    }

    public void SetHealth(float healthNormalized) // Set the health bar to a normalized value between 0 and 1
    {
        if (regularBarImage != null && barOutlineImage != null)
        {
            regularBarImage.fillAmount = healthNormalized;
            regularBarImage.color = Color.Lerp(LowHealthColor, FullHealthColor, healthNormalized);
            barOutlineImage.color = Color.Lerp(LowHealthColor, FullHealthColor, healthNormalized);
        }
        else
        {
            Debug.LogError("Can't set health, Regular Bar image not found in the scene.");
        }
    }

    public void SetRestoration(float restorationNormalized) // Set the restoration bar to a normalized value between 0 and 1
    {
        if (restorationBarImage != null)
        {
            restorationBarImage.fillAmount = restorationNormalized;
        }
        else
        {
            Debug.LogError("Can't set restoration, Restoration Bar image not found in the scene.");
        }
    }
}
