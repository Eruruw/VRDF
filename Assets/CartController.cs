using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CartController : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private float initialHandRotation;
    IXRInteractor interactor;
    public float rotationDampening = 5.0f;
    float initalCartYRotation;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        //initialRotation = transform.rotation;

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // Optional: Save initial grab rotation if needed for more complex rotation handling
        
        interactor = args.interactorObject;
        initialHandRotation = interactor.transform.rotation.eulerAngles.y;
        initalCartYRotation = transform.eulerAngles.y;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        // Optional: Reset or adjust rotation on release if needed
        interactor = null; 
    }

    void FixedUpdate()
    {
        if (grabInteractable.isSelected)
        {

            float newHandYRotation = interactor.transform.rotation.eulerAngles.y;
            float YRotation = (initalCartYRotation - (initialHandRotation - newHandYRotation));
    
            Quaternion newRotation = Quaternion.Euler(0, YRotation, 0);

            //transform.position = newPosition;
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.fixedDeltaTime * rotationDampening);
        }
    }
}
