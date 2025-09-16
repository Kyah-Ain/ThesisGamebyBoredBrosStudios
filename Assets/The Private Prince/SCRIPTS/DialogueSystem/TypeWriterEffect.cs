using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypeWriterEffect : MonoBehaviour
{
    [SerializeField] private float typeWriterSpeed = 50f; // Controls how fast characters appear (higher = faster)

    // Public method to start the typewriter effect
    public Coroutine Run(string textToType, TMP_Text textLabel)
    {
        // Starts the TypeText coroutine and returns the coroutine reference
        return StartCoroutine(TypeText(textToType, textLabel));
    }

    // Coroutine that handles the typewriter animation
    private IEnumerator TypeText(string textToType, TMP_Text textLabel)
    {
        textLabel.text = string.Empty; // Clears the text label before starting

        float t = 0; // Timer accumulator for character progression
        int charIndex = 0; // Tracks how many characters have been revealed

        // Loop until all characters are revealed
        while (charIndex < textToType.Length)
        {
            t += Time.deltaTime * typeWriterSpeed; // Increment timer based on speed and frame time
            charIndex = Mathf.FloorToInt(t); // Convert timer to integer character index
            charIndex = Mathf.Clamp(charIndex, 0, textToType.Length); // Ensure index stays within bounds

            textLabel.text = textToType.Substring(0, charIndex); // Display portion of text up to current index

            yield return null; // Wait until next frame before continuing
        }

        textLabel.text = textToType; // Ensure full text is displayed after completion
    }
}