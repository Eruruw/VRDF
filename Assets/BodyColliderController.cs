using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyColliderController : MonoBehaviour
{
    public bool isColliding = false;
    public Transform cameraTransform; 
    private List<Collider> cols = new List<Collider>();
    private void FixedUpdate()
    {
        transform.position = new Vector3(cameraTransform.position.x, transform.position.y, cameraTransform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name != "XR Origin")
        {
            cols.Add(other);
            isColliding = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (cols.Contains(other))
        {
            cols.Remove(other);
        }
        if (cols.Count == 0)
        {
            isColliding = false;
        }

    }
}
