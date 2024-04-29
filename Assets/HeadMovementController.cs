using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadMovementController : MonoBehaviour
{

    public MovementController movementController;

    private void OnTriggerStay(Collider other)
    {
        movementController.Center();
    }

}
