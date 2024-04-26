using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class DisableRayOnTrigger : MonoBehaviour
{
    public XRRayInteractor correspondingHandRayInteractor;
    // Start is called before the first frame update

    private void OnTriggerEnter(Collider other)
    {
        correspondingHandRayInteractor.enabled = false;
    }

    private void OnTriggerExit(Collider other)
    {
        correspondingHandRayInteractor.enabled = true;
    }
}
