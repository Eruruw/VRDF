using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;


public class HandClosedFilter : MonoBehaviour, IXRSelectFilter
{
    private bool inTrigger = false;
    private bool canSelect = false;
    private bool isHandClosed = false;
    private XRDirectInteractor interactor;

    // Start is called before the first frame update
    void Start()
    {
        interactor = GetComponent<XRDirectInteractor>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!interactor.isSelectActive)
        {
            isHandClosed = false;
            if (inTrigger)
            {
                canSelect = true;
                inTrigger = false;
            }
        }
        else
        {
            isHandClosed = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!interactor.isSelectActive)
        {
            canSelect = true;
        }
        else
        {
            if (interactor.hasSelection == false)
            {
                inTrigger = true;
                canSelect = false;
            }
        }
    }
    
    public bool canProcess => isActiveAndEnabled;

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        return canSelect;
    }
}
