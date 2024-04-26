using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomRayLine : MonoBehaviour
{
    public XRRayInteractor ray;
    public float minimumDistance = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        //ray = GetComponent<XRRayInteractor>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ray != null)
        {
            if (ray.TryGetHitInfo(out Vector3 hitPosition, out Vector3 hitNormal, out int hitColliderIndex, out bool isValidTarget))
            {
                // Check if the hit information is valid
                if (isValidTarget)
                {
                    // Calculate the distance from the ray origin to the hit position
                    float distance = Vector3.Distance(ray.transform.position, hitPosition);

                    if (distance < minimumDistance && !ray.hasSelection)
                    {
                        ray.enabled = false;
                        ray.allowSelect = false;
                    }
                    else 
                    {
                        ray.allowSelect = true;
                    }
                }
                else
                {
                    ray.enabled = true;
                }
            }
        }
    }
}
