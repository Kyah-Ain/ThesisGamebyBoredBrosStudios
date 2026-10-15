using UnityEngine;

public class Instantiator : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [SerializeField] AudioClip[] Instantiables;

    // -------------------------- METHODS -------------------------

    void InstantiateAudio()
    {
        foreach (AudioClip clip in Instantiables)
        {
            PlayClipAndDestroy(clip);
        }
    }

    void PlayClipAndDestroy(AudioClip clip)
    {
        // Create an empty GameObject to host the AudioSource
        GameObject audioObj = new GameObject("Audio_" + clip.name);
        audioObj.transform.position = transform.position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.Play();

        // Destroy the GameObject after the clip finishes
        Destroy(audioObj, clip.length);
    }
}