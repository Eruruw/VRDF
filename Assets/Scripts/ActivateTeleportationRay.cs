using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
public class ActivateTeleportationRay : MonoBehaviour
{
    public XRBaseInteractor lefthandInteractor;
    public XRBaseInteractor righthandInteractor;

    public GameObject leftTeleportationRay;
    public GameObject rightTeleportationRay;

    public GameObject leftUIRay;
    public GameObject rightUIRay;

    public InputActionProperty leftActivate;
    public InputActionProperty rightActivate;

    public InputActionProperty leftCancel;
    public InputActionProperty rightCancel;

    public XRRayInteractor rightRay;
    public XRRayInteractor leftRay;

    public bool isEnable;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SwitchLocomotion(int locomotionValue)
    {
        if (locomotionValue == 0)
        {
            isEnable = false;
        }
        else if (locomotionValue == 1)
        {
            isEnable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool isLeftRayHovering = leftRay.TryGetHitInfo(out Vector3 leftPos, out Vector3 leftNormal, out int leftNumber, out bool leftVaild);

        bool isRightRayHovering = rightRay.TryGetHitInfo(out Vector3 rightPos, out Vector3 rightNormal, out int rightNumber, out bool rightVaild);

        leftTeleportationRay.SetActive(isEnable && !isLeftRayHovering && leftCancel.action.ReadValue<float>() == 0 && leftActivate.action.ReadValue<float>() > 0.1f);
        rightTeleportationRay.SetActive(isEnable && !isRightRayHovering && rightCancel.action.ReadValue<float>() == 0 && rightActivate.action.ReadValue<float>() > 0.1f);

        bool isLeftHandActive = lefthandInteractor.hasSelection;
        bool isRightHandActive = righthandInteractor.hasSelection;
        
        leftUIRay.SetActive(!isLeftHandActive);
        rightUIRay.SetActive(!isRightHandActive);
    }
}
