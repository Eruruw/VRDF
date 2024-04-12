using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NameUpdate : MonoBehaviour
{
    public TextMeshProUGUI namePlateText;

    private void OnTriggerStay(Collider other)
    {
       // Access the EvidenceID script attached to the collider's GameObject
       EvidenceID evidenceID = other.GetComponent<EvidenceID>();
            
        // Check if the EvidenceID script is found    
        if (evidenceID != null)
        {
            evidenceID.desk = namePlateText.text;
        }
    }
}