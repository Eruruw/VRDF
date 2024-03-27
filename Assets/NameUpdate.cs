using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NameUpdate : MonoBehaviour
{
    public TextMeshProUGUI namePlateText;

    private void OnTriggerStay(Collider other)
    {
        // Check if the collider belongs to an object with the "evidence" tag
        if (other.CompareTag("evidence"))
        {
            // Access the EvidenceID script attached to the collider's GameObject
            EvidenceID evidenceID = other.GetComponent<EvidenceID>();

            // Check if the EvidenceID script is found
            if (evidenceID != null)
            {
                if (namePlateText == null)
                {
                    // Set the name to the text from the TextMeshPro object
                    evidenceID.desk = namePlateText.text;
                }
            }
        }
    }
}