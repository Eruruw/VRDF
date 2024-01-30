using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BagSocket : MonoBehaviour
{
    private XRSocketInteractor socket;
    private GameObject item = null;

    void Start()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    public void SocketCheck()
    {
        IXRSelectInteractable selItem = socket.GetOldestInteractableSelected();
        item = selItem.transform.gameObject;
        /*
        if (item.CompareTag("evidence"))
        {
            increment
        }
        else
        {
            decrement
        }
        */
        item.SetActive(false);
    }
}
