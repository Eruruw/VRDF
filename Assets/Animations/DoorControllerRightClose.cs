using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControllerRightClose : MonoBehaviour
{
    Animator _rdoorAnim;

    private void OnTriggerEnter(Collider other)
    {
        _rdoorAnim.SetBool("isOpening", false);
    }
    // Start is called before the first frame update
    void Start()
    {
        _rdoorAnim = this.transform.parent.GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {

    }
}
