using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
public class MovementController : MonoBehaviour
{
    private CharacterController controller;
    private XROrigin origin;
    private BodyColliderController bodyCollider;
    private Vector3 velocity;
    private float magnitude;
    private Vector3 center;
    // Start is called before the first frame update
    void Start()
    {
        origin = GetComponent<XROrigin>();
        controller = GetComponent<CharacterController>();
        bodyCollider = GetComponentInChildren<BodyColliderController>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        center = origin.CameraInOriginSpacePos;
        velocity = controller.velocity;
        magnitude = velocity.magnitude;

        /*
        Vector3 colliderPos = controller.center; 
        float distance = Mathf.Sqrt(Mathf.Pow(center.x - colliderPos.x, 2f) + Mathf.Pow(center.z - colliderPos.z, 2f));
        //Debug.Log("Distance between character controller and head collider: " + distance);*/

        if (magnitude > .35f && !bodyCollider.isColliding)
        {
            //Debug.Log("Distance between character controller and head collider: " + distance);
            //Debug.Log("moved");
            //controller.transform.position = Vector3.Lerp(targetPosition, controller.transform.position, .1f * Time.deltaTime);
            //controller.center = new Vector3(center.x, controller.center.y, center.z);
            Center();

        }
    }

    public void Center()
    {
        controller.center = new Vector3(center.x, controller.center.y, center.z);
    }
}
