using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class ComputerCaseController : MonoBehaviour
{

    private XRGrabInteractable grabInteractable;
    private Rigidbody rigidBody;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rigidBody = GetComponent<Rigidbody>();

        // Subscribe to interaction events
        grabInteractable.selectExited.AddListener(HandleReleased);
    }

    private void HandleReleased(SelectExitEventArgs args)
    {
        // Unlock the object when grabbed
        rigidBody.isKinematic = false;
        grabInteractable.selectExited.RemoveAllListeners();
    }


}
