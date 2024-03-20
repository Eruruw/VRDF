using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class BagSocket : MonoBehaviour
{
    //public GameObject bags;
    private ScoreTracker tracker;
    private XRSocketInteractor socket;
    private GameObject item = null;
    private WarrantManager manager;
    private string warrantDesk;
    private List<string> tagsList;
    private bool found = false;
    private string itemName;
    UserManager user;

    void Start()
    {
        GameObject userManager = GameObject.Find("UserManager");
        if (userManager != null)
        {
            user = userManager.GetComponent<UserManager>();
        }
        
        GameObject warrant = GameObject.FindWithTag("manager");
        GameObject bags = GameObject.Find("Bags");
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
        itemName = item.name;
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

        SetText();
    }

    public void SetText()
    { 
        Transform parentTransform = transform.parent;
        Transform caseNumberTransform = parentTransform.Find("Plastic_Evidence_Bag/EvidenceText/CaseNumber");
        Transform userTransform = parentTransform.Find("Plastic_Evidence_Bag/EvidenceText/User");
        Transform suspectTransform = parentTransform.Find("Plastic_Evidence_Bag/EvidenceText/Suspect");
        Transform offenceTransform = parentTransform.Find("Plastic_Evidence_Bag/EvidenceText/Offence");
        Transform itemTransform = parentTransform.Find("Plastic_Evidence_Bag/EvidenceText/Item");
        Transform dateTimeTransform = parentTransform.Find("Plastic_Evidence_Bag/EvidenceText/DateTime");
        if (caseNumberTransform != null && userTransform != null && suspectTransform != null && offenceTransform != null)
        {
            GameObject caseNumber = caseNumberTransform.gameObject;
            GameObject evidenceBagUser = userTransform.gameObject;
            GameObject suspect = suspectTransform.gameObject;
            GameObject offence = offenceTransform.gameObject;
            GameObject item = itemTransform.gameObject;
            GameObject dateTime = dateTimeTransform.gameObject;
            TextMeshProUGUI caseNumberText = caseNumber.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI evidenceBagUserText = evidenceBagUser.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI suspectText = suspect.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI offenceText = offence.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemText = item.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateTimeText = dateTime.GetComponent<TextMeshProUGUI>();

            if (caseNumberText != null && evidenceBagUserText != null && suspectText != null && offenceText != null)
            {
                caseNumberText.text = user.caseNumber.ToString();
                evidenceBagUserText.text = user.currentUser;
                suspectText.text = manager.exactDesk;
                offenceText.text = manager.typeOfCrime;
                itemText.text = itemName;
                dateTimeText.text = (DateTime.Now).ToString("M/dd/yyyy HH:mm:ss");
            }    
        }
        else 
        {
            Debug.Log("Children Objects not found");
        }
    }
}

