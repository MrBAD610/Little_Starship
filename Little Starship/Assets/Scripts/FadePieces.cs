using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePieces : MonoBehaviour
{
    [Header("Fade Settings")]
    public float timeBeforeFade = 1f;
    public float fadeDuration = 1f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeAndDestroyPieces()); // Start the coroutine to fade and destroy the pieces
    }

    // Update is called once per frame
    private void Update()
    {
       
    }

    private IEnumerator FadeAndDestroyPieces() // Coroutine to destroy the object after a set time
    {
        float elapsedTime = 0f; // Set the elapsed time to 0
        Renderer[] renderers = GetComponentsInChildren<Renderer>(); // Get all the renderers in the broken object

        // Create a shared material instance
        Material sharedMaterial = new Material(renderers[0].material); // Create a new material instance from the first renderer's material
        foreach (Renderer renderer in renderers) // For each renderer in the broken object
        {
            renderer.material = sharedMaterial; // Set the material to the shared material
        }

        Color originalColor = sharedMaterial.color; // Store the original color

        yield return new WaitForSeconds(timeBeforeFade); // Wait for the specified time

        while (elapsedTime < fadeDuration) // While the elapsed time is less than the fade duration
        {
            elapsedTime += Time.deltaTime; // Increment the elapsed time
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration); // Calculate the alpha value
            Color newColor = originalColor; // Get the original color
            newColor.a = alpha; // Set the alpha value
            sharedMaterial.color = newColor; // Set the color of the shared material
            yield return null; // Wait for the next frame
        }
        Destroy(gameObject); // Destroy the broken object once the fade is complete
    }
}
