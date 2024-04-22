using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Office_Noises : MonoBehaviour
{
    public AudioClip[] audioClips;
    private AudioSource audioSource;

    public float minDelay = 20f;
    public float maxDelay = 60f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayRandomAudioWithDelay());
    }

    IEnumerator PlayRandomAudioWithDelay()
    {
        while (true)
        {
            if (audioClips.Length > 0)
            {
                int randomIndex = Random.Range(0, audioClips.Length);
                audioSource.clip = audioClips[randomIndex];
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("No audio clips assigned to play.");
            }

            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);
        }
    }
}