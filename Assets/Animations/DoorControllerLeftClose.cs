using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControllerLeftClose : MonoBehaviour
{
    Animator _ldoorAnim;
    AudioSource _audioSource; // Add an AudioSource variable

    // Start is called before the first frame update
    void Start()
    {
        _ldoorAnim = transform.parent.GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>(); // Get the AudioSource component attached to this GameObject
    }

    private void OnTriggerEnter(Collider other)
    {
        _ldoorAnim.SetBool("IsOpeningLeft", false);
        if (_audioSource != null) // Check if AudioSource component exists
        {
            _audioSource.Play(); // Play the audio clip attached to the AudioSource component
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}