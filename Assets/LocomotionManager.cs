using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LocomotionManager : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject leftRayTeleport;
    public GameObject rightRayTeleport;

    public TeleportationProvider teleportationProvider;
    public ContinuousMoveProviderBase continuousMoveProviderBase;

    void Start()
    {
        //teleportationProvider = GetComponent<TeleportationProvider>();
        //continuousMoveProviderBase = GetComponent<ContinuousMoveProviderBase>();
    }

    public void SwitchLocomotion(int locomotionValue)
    {
        if (locomotionValue == 1)
        {
            DisableContinuous();
            EnableTeleport();
        }
        else if (locomotionValue == 0)
        {
            DisableTeleport();
            EnableContinuous();
        }
    }

    private void DisableTeleport()
    {
        //leftRayTeleport.SetActive(false);
        //rightRayTeleport.SetActive(false);
        teleportationProvider.enabled = false;
        
    }

    private void DisableContinuous()
    {
        continuousMoveProviderBase.enabled = false;
    }

    private void EnableTeleport()
    {
        // leftRayTeleport.SetActive(true);
        //rightRayTeleport.SetActive(true);
        teleportationProvider.enabled = true;
    }

    private void EnableContinuous()
    {
        continuousMoveProviderBase.enabled = true;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
