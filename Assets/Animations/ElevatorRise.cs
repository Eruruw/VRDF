using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorRise : MonoBehaviour
{
    Animator _elevatorRise;

    private void OnTriggerEnter(Collider other)
    {
        _elevatorRise.SetBool("isElevatorRise", true);
    }
    // Start is called before the first frame update
    void Start()
    {
        _elevatorRise = this.transform.parent.GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {

    }
}
