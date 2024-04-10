using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RevolverSound : MonoBehaviour
{
    public AudioSource fire_sound;
    public AudioSource ringing_sound;
    private XRGrabInteractable grabbable;
    public bool gun_grabbed = false;
    // Start is called before the first frame update
    void Start()
    {
        grabbable = GetComponent<XRGrabInteractable>();

        

    }

    public void play_sound()
    {
        if (fire_sound != null)
        {
            fire_sound.Play();
        }
        if (ringing_sound != null)
        {
            ringing_sound.Play();
        }
        grabbable.enabled = false;
        gun_grabbed = true;

    }
}
