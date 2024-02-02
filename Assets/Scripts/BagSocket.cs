using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BagSocket : MonoBehaviour
{
    public GameObject bags;
    private ScoreTracker tracker;
    private XRSocketInteractor socket;
    private GameObject item = null;
    private WarrantManager manager;
    private string warrantDesk;
    private List<string> tagsList;
    private bool found = false;

    void Start()
    {
        GameObject warrant = GameObject.FindWithTag("manager");
        tracker = bags.GetComponent<ScoreTracker>();
        socket = GetComponent<XRSocketInteractor>();
        manager = warrant.GetComponent<WarrantManager>();
        warrantDesk = manager.exactDesk;
        tagsList = manager.validEvidenceList;
    }

    public void SocketCheck()
    {
        IXRSelectInteractable selItem = socket.GetOldestInteractableSelected();
        item = selItem.transform.gameObject;
        EvidenceID ID = item.GetComponent<EvidenceID>();
        if (ID.desk == warrantDesk)
        {
            if (tagsList.Contains(ID.type))
            {
                tracker.score++;
                found = true;
            }
        }
        if (found)
            found = false;
        else
            tracker.score--;
        item.SetActive(false);
        socket.enabled = false;
    }
}
