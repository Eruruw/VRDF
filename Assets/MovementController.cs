using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
public class MovementController : MonoBehaviour
{
    public CharacterController controller;
    public XROrigin origin;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /*Vector3 center = origin.CameraInOriginSpacePos; 
        Vector3 colliderPos = controller.center; 
        float distance = Mathf.Sqrt(Mathf.Pow(center.x - colliderPos.x, 2f) + Mathf.Pow(center.z - colliderPos.z, 2f));
        //Debug.Log("Distance between character controller and head collider: " + distance);

        if (distance > .35f)
        {
            Debug.Log("Distance between character controller and head collider: " + distance);
            Debug.Log("moved");
            //controller.transform.position = Vector3.Lerp(targetPosition, controller.transform.position, .1f * Time.deltaTime);
            controller.center = new Vector3(center.x, controller.center.y, center.z);


        }*/
    }
}
