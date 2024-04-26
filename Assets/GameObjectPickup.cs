using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(XRGrabInteractable))]
[DisallowMultipleComponent]
public class GameObjectPickup : MonoBehaviour
{
    private XRGrabInteractable XRGrab;
    private string selectionLayer = "Selection";
    private int originalLayer;

    private void Start()
    {
        XRGrab = GetComponent<XRGrabInteractable>();

        XRGrab.selectEntered.AddListener(Grab);
        XRGrab.selectExited.AddListener(Drop);
        
    }

    public void Grab(SelectEnterEventArgs args)
    {
        if (args.interactorObject is not XRSocketInteractor)
        {

            int newLayer = LayerMask.NameToLayer(selectionLayer);

            originalLayer = this.gameObject.layer;
            this.gameObject.layer = newLayer;
        }
    }

    public void Drop(SelectExitEventArgs args)
    {
        this.gameObject.layer = originalLayer;
    }
}
