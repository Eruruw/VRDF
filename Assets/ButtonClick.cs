using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonClick : MonoBehaviour
{
    public GameObject button;
    public UnityEvent onPress;
    public UnityEvent onRelease;
    public List<Light> lightSources;
    GameObject presser;
    bool isPressed;
    bool isPlayerNearby; // New variable to track player proximity

    void Start()
    {
        isPressed = false;
        isPlayerNearby = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed && !isPlayerNearby)
        {
            button.transform.localPosition = new Vector3(0, -0.003f, 0); // Adjust local position
            presser = other.gameObject;
            onPress.Invoke();
            isPressed = true;
            TurnOnOffLights();
            isPlayerNearby = true; // Set to true when player is near
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == presser)
        {
            button.transform.localPosition = new Vector3(0, -0.015f, 0); // Adjust local position
            onRelease.Invoke();
            isPressed = false;
            TurnOnOffLights();
            isPlayerNearby = false; // Set to false when player moves away
        }
    }

    public void TurnOnOffLights()
    {
        foreach (Light lightSource in lightSources)
        {
            if (lightSource != null)
            {
                lightSource.enabled = !lightSource.enabled; // Toggle the light's state
            }
        }
    }
}