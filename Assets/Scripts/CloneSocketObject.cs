using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CloneSocketObject : MonoBehaviour
{
    public GameObject clonePrefab;
    public int numObjs = 0;
    private Collider col;
    private Rigidbody rb;
    private GameObject gameObjectClone;
    private bool capped, disable;

    void FixedUpdate()
    {
        if (numObjs == 4 && capped)
        {
            capped = false;
            col.enabled = true;
            rb.isKinematic = false;
        }
    }

    public void CloneInteractable(SelectExitEventArgs args)
    {
        StartCoroutine(WaitThenSpawn(args));
    }

    public void CloneInteractable()
    {
        Instantiate(clonePrefab, gameObject.transform.position, gameObject.transform.rotation);
    }

    public IEnumerator WaitThenSpawn(SelectExitEventArgs args)
    {
        yield return new WaitForSeconds(0.1f);
        IXRInteractor socket = args.interactorObject;
        GameObject gameObjectClone = Instantiate(clonePrefab, socket.transform.position, socket.transform.rotation);
        col = gameObjectClone.GetComponent<Collider>();
        rb = gameObjectClone.GetComponent<Rigidbody>();
        col.enabled = true;
        rb.useGravity = true;
        GameObject item = socket.transform.gameObject;
        item.GetComponent<XRSocketInteractor>().StartManualInteraction(gameObjectClone.GetComponent<IXRSelectInteractable>());
    }
}