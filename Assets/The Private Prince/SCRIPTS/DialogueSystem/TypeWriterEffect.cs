using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypeWriterEffect : MonoBehaviour
{
    [SerializeField] private float typeWriterSpeed = 50f; // Controls how fast characters appear (higher = faster)

    public bool isRunning { get; private set; }

    private readonly Dictionary<HashSet<char>, float> punctuations = new Dictionary<HashSet<char>, float>()
    {
        {new HashSet<char>(){'.', '!', '?'}, 0.6f},
        {new HashSet<char>(){',', ';', ':'}, 0.3f},
    };

    private Coroutine typingCoroutine;

    // Public method to start the typewriter effect
    public void Run(string textToType, TMP_Text textLabel)
    {
        // Starts the TypeText coroutine and returns the coroutine reference
        typingCoroutine = StartCoroutine(TypeText(textToType, textLabel));
    }

    public void Stop()
    {
        StopCoroutine(typingCoroutine);
        isRunning = false;
    }

    // Coroutine that handles the typewriter animation
    private IEnumerator TypeText(string textToType, TMP_Text textLabel)
    {
        isRunning = true;

        textLabel.text = string.Empty; // Clears the text label before starting

        float t = 0; // Timer accumulator for character progression
        int charIndex = 0; // Tracks how many characters have been revealed

        // Loop until all characters are revealed
        while (charIndex < textToType.Length)
        {
            int lastCharIndex = charIndex;

            t += Time.deltaTime * typeWriterSpeed; // Increment timer based on speed and frame time
            charIndex = Mathf.FloorToInt(t); // Convert timer to integer character index
            charIndex = Mathf.Clamp(charIndex, 0, textToType.Length); // Ensure index stays within bounds

            for (int i = lastCharIndex; i < charIndex; i++)
            {
                bool isLast = i >= textToType.Length - 1;

                textLabel.text = textToType.Substring(0, i + 1); // Display portion of text up to current index

                if (IsPunctuation(textToType[i], out float waitTime) && !isLast && !IsPunctuation(textToType[i + 1], out _))
                {
                    yield return new WaitForSeconds(waitTime);
                }
            }

            yield return null; // Wait until next frame before continuing
        }

        isRunning = false;
    }

    private bool  IsPunctuation(char character, out float waitTime)
    {
        foreach(KeyValuePair<HashSet<char>, float> punctationCategory in punctuations)
        {
            if(punctationCategory.Key.Contains(character))
            {
                waitTime = punctationCategory.Value;
                return true;
            }
        }

        waitTime = default;
        return false;
    }
}