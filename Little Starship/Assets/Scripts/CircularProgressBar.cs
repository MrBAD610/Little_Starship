using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircularProgressBar : MonoBehaviour
{
    [Header("Progress Bar Settings")]
    [SerializeField] public float timeTillCompletion = 10f;

    [Header("Display Settings")]
    [SerializeField] private Sprite symbolSprite;
    [SerializeField] private Sprite symbolShadowSprite;
    [SerializeField] private Color fullStatusColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] private Color lowStatusColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    [SerializeField] private Color completeColor = Color.green;

    public float currentProgress = 0f;
    public bool isProgressing = false;
    public bool isComplete { get; private set;}

    private Image symbolImage;
    private Image symbolShadowImage;
    private Image outerRingImage;
    private Image outerRingShadowImage;

    void Start()
    {
        InitializeForDisplay();

        if (currentProgress == timeTillCompletion)
        {
            isComplete = true;
        }
        else
        {
            isComplete = false;
        }   
    }

    void Update()
    {
        if (isProgressing) UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        if (Application.isPlaying)
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

            if (currentProgress >= timeTillCompletion)
            {
                currentProgress = timeTillCompletion;
                isProgressing = false;
                isComplete = true;
            }
        }

        // Always update health color (edit or runtime)
        UpdateHealthColor();
    }

    private void UpdateHealthColor()
    {
        // Check if references are properly assigned
        if (symbolImage == null || symbolShadowImage == null || outerRingImage == null || outerRingShadowImage == null)
        {
            Debug.LogWarning("One or more required images are not assigned. Ensure InitializeForDisplay has been executed.");
            return; // Exit early if any references are null
        }

        // Update health color logic
        Color healthColor;

        if (currentProgress == timeTillCompletion)
            healthColor = completeColor;
        else
            healthColor = Color.Lerp(lowStatusColor, fullStatusColor, currentProgress / timeTillCompletion);

        // Apply colors to the relevant images
        symbolImage.color = healthColor;
        symbolShadowImage.color = healthColor;
        outerRingImage.color = healthColor;
        outerRingShadowImage.color = healthColor;
    }


    private void InitializeForDisplay()
    {
        // Initialize the Image fields by finding child components
        foreach (Transform child in transform) // Iterate over all children of the parent transform
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
            InitializeForDisplay(); // Ensure all references are set
            currentProgress = Mathf.Clamp(currentProgress, 0f, timeTillCompletion);

            // Update visuals based on currentProgress
            if (outerRingImage != null)
                outerRingImage.fillAmount = currentProgress / timeTillCompletion;

            if (outerRingShadowImage != null)
                outerRingShadowImage.fillAmount = currentProgress / timeTillCompletion;

            UpdateHealthColor();
        }
    }
}
