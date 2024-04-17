using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CartController : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private float initialHandYRotation;
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
        initialHandYRotation = interactor.transform.rotation.eulerAngles.y;
        initalCartYRotation = transform.eulerAngles.y;

        CartTriggerZoneDectector cartTrigger = GetComponentInChildren<CartTriggerZoneDectector>();

        if (cartTrigger.objectsInCart.Count != 0)
        {
            foreach (GameObject key in cartTrigger.objectsInCart.Keys)
            {
                Collider[] objectColliders = key.GetComponentsInChildren<Collider>();
                foreach (Collider col in objectColliders)
                {
                    col.enabled = false; // Disable each collider
                }
            }
        }
    }

    void OnRelease(SelectExitEventArgs args)
    {
        // Optional: Reset or adjust rotation on release if needed
        interactor = null;

        CartTriggerZoneDectector cartTrigger = GetComponentInChildren<CartTriggerZoneDectector>();

        if (cartTrigger.objectsInCart.Count != 0)
        {
            foreach (GameObject key in cartTrigger.objectsInCart.Keys)
            {
                Collider[] objectColliders = key.GetComponentsInChildren<Collider>();
                foreach (Collider col in objectColliders)
                {
                    if (key.name == "REALcamera" && col is CapsuleCollider)
                    {
                        //do nothing
                    }
                    else 
                    {
                        col.enabled = true; // Enable each collider
                    }
                    
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (grabInteractable.isSelected)
        {

            float newHandYRotation = interactor.transform.rotation.eulerAngles.y;
            float YRotation = (initalCartYRotation - (initialHandYRotation - newHandYRotation));
    
            Quaternion newRotation = Quaternion.Euler(0, YRotation, 0);

            //transform.position = newPosition;
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.fixedDeltaTime * rotationDampening);
        }
    }
    private void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }
}
