using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircularProgressBar : MonoBehaviour
{
    [Header("Progress Bar Settings")]
    [SerializeField] public float timeTillCompletion = 0f;

    [Header("Display Settings")]
    [SerializeField] private Sprite symbolSprite;
    [SerializeField] private Sprite symbolShadowSprite;
    [SerializeField] private Color fullStatusColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] private Color lowStatusColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    [SerializeField] private Color completeColor = Color.green;

    public float currentProgress = 0f;
    public bool isProgressing = false;

    private Image symbolImage;
    private Image symbolShadowImage;
    private Image outerRingImage;
    private Image outerRingShadowImage;

    void Start()
    {
        InitializeForDisplay();
    }

    void Update()
    {
        if (isProgressing) UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        if (timeTillCompletion <= 0)
        {
            Debug.LogWarning("Set time till completion to value greater than 0.");
        }

        if (currentProgress < timeTillCompletion)
        {
            currentProgress += Time.deltaTime;

            if (outerRingImage != null)
                outerRingImage.fillAmount = currentProgress / timeTillCompletion;

            if (outerRingShadowImage != null)
                outerRingShadowImage.fillAmount = currentProgress / timeTillCompletion;
        }

        if (currentProgress > timeTillCompletion) currentProgress = timeTillCompletion;

        UpdateHealthColor();
    }

    private void UpdateHealthColor()
    {
        Color healthColor;

        if (currentProgress == timeTillCompletion) healthColor = completeColor;
        else healthColor = Color.Lerp(lowStatusColor, fullStatusColor, (currentProgress / timeTillCompletion));

        symbolImage.color = healthColor;
        symbolShadowImage.color = healthColor;
        outerRingImage.color = healthColor;
        outerRingShadowImage.color = healthColor;
    }

    private void InitializeForDisplay()
    {
        // Initialize the Image fields by finding child components
        foreach (Transform child in transform)
        {
            Image childImage = child.GetComponent<Image>();
            if (childImage != null)
            {
                if (child.name.Contains("Symbol"))
                {
                    if (child.name.Contains("Shadow"))
                        symbolShadowImage = childImage;
                    else
                        symbolImage = childImage;
                }
                else if (child.name.Contains("Outer Ring"))
                {
                    if (child.name.Contains("Shadow"))
                        outerRingShadowImage = childImage;
                    else
                        outerRingImage = childImage;
                }
            }
        }

        // Verify that all required images are assigned
        if (outerRingImage == null || outerRingShadowImage == null)
        {
            Debug.LogError("Outer ring images are not assigned or found!");
        }

        symbolImage.sprite = symbolSprite;
        symbolShadowImage.sprite = symbolShadowSprite;

        UpdateHealthColor();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            InitializeForDisplay(); // Ensure references are updated in Edit mode
            UpdateProgressBar(); // Update visuals based on inspector changes
        }
    }

}
