using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControllerLeftClose : MonoBehaviour
{
    Animator _ldoorAnim;

    private void OnTriggerEnter(Collider other)
    {
        _ldoorAnim.SetBool("IsOpeningLeft", false);
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
