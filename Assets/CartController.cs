using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CartController : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Quaternion initialRotation;
    IXRInteractor interactor;
    public float rotationDampening = 5.0f;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        initialRotation = transform.rotation;

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // Optional: Save initial grab rotation if needed for more complex rotation handling
        interactor = args.interactorObject; 
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
            // While selected, override position and rotation each frame
            //Vector3 newPosition = transform.position;
            float newYRotation = interactor.transform.rotation.eulerAngles.y;
            //newPosition.y = 0; // Maintain y position

            Quaternion newRotation = Quaternion.Euler(0, newYRotation, 0);

            //transform.position = newPosition;
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.fixedDeltaTime * rotationDampening);
        }
    }
}
