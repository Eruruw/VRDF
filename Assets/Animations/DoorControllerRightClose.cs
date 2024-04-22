using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControllerRightClose : MonoBehaviour
{
    Animator _rdoorAnim;
    AudioSource _audioSource;

    private void OnTriggerEnter(Collider other)
    {
        _rdoorAnim.SetBool("isOpening", false);
        if (_audioSource != null) // Check if AudioSource component exists
        {
            _audioSource.Play(); // Play the audio clip attached to the AudioSource component
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        _rdoorAnim = this.transform.parent.GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {

    }
}
