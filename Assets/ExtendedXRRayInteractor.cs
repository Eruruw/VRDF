using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class ExtendedXRRayInteractor : XRRayInteractor
{
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        // Additional code will go here
        if (args.interactorObject is XRRayInteractor rayInteractor && rayInteractor.TryGetHitInfo(out Vector3 pos, out Vector3 norm, out int index, out bool validTarget))
        {
            Debug.Log("yes");
            // Calculate the difference between the hit position and the controller's position
            Vector3 controllerPosition = rayInteractor.transform.position;
            Vector3 toHand = controllerPosition - pos;

            // Set the attach position to the controller's position
            attachTransform.position = controllerPosition;

            // You might want to adjust this transform to better match how you want the object held
            //attachTransform.localPosition -= toHand;

            // Optionally adjust the rotation if needed
            //attachTransform.up = norm;
        }
    }
}
