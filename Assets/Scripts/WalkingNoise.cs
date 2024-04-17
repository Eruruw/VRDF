using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WalkingNoise : MonoBehaviour
{

    private CharacterController characterController;
    public AudioClip soundClip;

    public float thresholdVelocity = 1.0f;
    private bool playing;
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponentInChildren<CharacterController>();

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (characterController != null)
        {
            //Debug.Log("first");
            // Calculate velocity
            Vector3 velocity = characterController.velocity;
            // Check if the magnitude of velocity exceeds the threshold
            if (velocity.magnitude > thresholdVelocity)
            {
                //Debug.Log("PLaying things0");
                // Play the sound clip
                PlaySound();

            }
            else
            {
                StopSound();
            }
        }
    }

    void PlaySound()
    {
        if (soundClip != null && audioSource != null && playing == false)
        {
            //audioSource.PlayOneShot(soundClip);
            audioSource.Play();
            audioSource.loop = true;
            playing = true;
        }
    }

    void StopSound()
    {
        if (soundClip != null && audioSource != null && playing == true)
        {
            //audioSource.PlayOneShot(soundClip);
            audioSource.Stop();
            audioSource.loop = false;
            playing = false;
        }
    }
}
