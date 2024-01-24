using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WarrantManager : MonoBehaviour
{
    public TextMeshProUGUI WarrantText;

    // Start is called before the first frame update
    void Start()
    {
        if (WarrantText != null)
        {
            WarrantText.text = "\n \n Hello there";

        }
        else
        {
            Debug.LogError("TextMeshPro component not attached to the script");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
