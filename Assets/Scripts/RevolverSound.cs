using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RevolverSound : MonoBehaviour
{
    public AudioSource fire_sound;
    public AudioSource ringing_sound;
    private XRGrabInteractable grabbable;
    private Rigidbody rb;
    private Vector3 minVelocity = new Vector3(-1f, -1f, -1f);
    private Vector3 maxVelocity = new Vector3(1f, 1f, 1f);
    private Vector3 randomVelocity;

    void Start()
    {
        grabbable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        Vector3 randomVelocity = new Vector3(
            Random.Range(minVelocity.x, maxVelocity.x),
            Random.Range(minVelocity.y, maxVelocity.y),
            Random.Range(minVelocity.z, maxVelocity.z)
        );
    }

    public void play_sound()
    {
        StartCoroutine(PlayAfterDelay());
    }

    IEnumerator PlayAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (fire_sound != null)
        {
            fire_sound.Play();
        }
        if (ringing_sound != null)
        {
            ringing_sound.Play();
        }
        grabbable.enabled = false;
        rb.velocity += randomVelocity;
        GameObject player = GameObject.Find("Player");
        ScoreTracker tracker = player.GetComponent<ScoreTracker>();
        tracker.crimeCommitted = true;
    }
}
