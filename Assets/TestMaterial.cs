using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMaterial : MonoBehaviour
{
    private Collider[] cols;
    // Start is called before the first frame update
    public PhysicMaterial highFrictionMaterial;
    void Start()
    {
        cols = GetComponents<Collider>();
        foreach (Collider collider in cols)
        {
            collider.material = highFrictionMaterial;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
