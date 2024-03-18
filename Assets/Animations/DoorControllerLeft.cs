using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControllerLeft : MonoBehaviour
{
    Animator _ldoorAnim;

    private void OnTriggerEnter(Collider other)
    {
        _ldoorAnim.SetBool("IsOpeningLeft", true);
    }
    // Start is called before the first frame update
    void Start()
    {
        _ldoorAnim = this.transform.parent.GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {

    }
}