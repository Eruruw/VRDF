using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class testRay : MonoBehaviour
{
    // Start is called before the first frame update
    XRRayInteractor ray;
    void Start()
    {
        ray = GetComponent<XRRayInteractor>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ray.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // Now we're sure "hit" is valid
            Debug.Log("Currently hitting: " + hit.collider.gameObject.name);
        }
        else
        {
            Debug.Log("Not hitting anything.");
        }

        RaycastHit hits;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;  // Slightly above the player's feet
        Vector3 rayDirection = Vector3.down;

        if (Physics.Raycast(rayStart, rayDirection, out hits, 1.0f))
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);
        }
        else
        {
            Debug.Log("No hit");
        }
    }
}
