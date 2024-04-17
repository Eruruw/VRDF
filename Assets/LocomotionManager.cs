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

    private UserManager user;

    void Start()
    {
   
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
        teleportationProvider.enabled = false;
        
    }

    private void DisableContinuous()
    {
        continuousMoveProviderBase.moveSpeed = 0;
    }

    private void EnableTeleport()
    {
        teleportationProvider.enabled = true;
    }

    private void EnableContinuous()
    {
        continuousMoveProviderBase.moveSpeed = 5;

    }
    // Update is called once per frame
    void Update()
    {

    }
}
