using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CartTriggerZoneDectector : MonoBehaviour
{
    //dictionary to hold objects and their parents
    private Dictionary<GameObject, Transform> objectsInCart = new Dictionary<GameObject, Transform>();

    // variables to hold values for the current object being grabbed out of the cart
    private XRGrabInteractable currentGrabInteractable;
    private Transform currentObjectOriginalParent;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable")) // Make sure to set the tag on your grabbable objects
        {
            if (!objectsInCart.ContainsKey(other.gameObject) && !other.GetComponent<XRGrabInteractable>().isSelected) 
            {
                other.transform.SetParent(this.transform);          // Parent to the cart
                
                //FixedJoint joint = other.gameObject.AddComponent<FixedJoint>();
                //joint.connectedBody = this.transform.parent.GetComponent<Rigidbody>();
                //joint.breakForce = 50f;
                objectsInCart.Add(other.gameObject, other.transform.parent);
                other.GetComponent<Rigidbody>().isKinematic = true; // make it kinematic to avoid physics forces acting on it while in the cart
                if (other.gameObject.name == "Bag" || other.gameObject.name == "Bag(Clone)")
                { 

                    ScoreTracker scoreTacker = GameObject.Find("Bags").GetComponent<ScoreTracker>();
                    BagSocket bagsocket = other.gameObject.GetComponentInChildren<BagSocket>();
                    if(bagsocket.itemName != null)
                    {
                        scoreTacker.numberOfBagsInCart += 1;
                    }
                }

                Debug.Log(other.gameObject + " added to the list");
            }
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectsInCart.ContainsKey(other.gameObject) && other.GetComponent<XRGrabInteractable>().isSelected)
        {
            //this.transform.parent.GetComponent<Rigidbody>().isKinematic = true;
            //FixedJoint joint = other.gameObject.GetComponent<FixedJoint>();
            //if (joint != null) Destroy(joint);
            //other.GetComponent<Rigidbody>().isKinematic = false;
            //this.transform.parent.GetComponent<Rigidbody>().isKinematic = false;
            //set variables for the currently grabbed object
            currentObjectOriginalParent = objectsInCart[other.gameObject];
            currentGrabInteractable = other.GetComponent<XRGrabInteractable>();
            if (currentGrabInteractable != null)
            {   
                //create listener to instansiate values after released
                currentGrabInteractable.selectExited.AddListener(Released);
            }
            objectsInCart.Remove(other.gameObject);
        }
    }

    private void Released(SelectExitEventArgs args)
    {
        if (currentGrabInteractable.gameObject.name == "Bag(Clone)" || currentGrabInteractable.gameObject.name == "Bag")
        {
            currentObjectOriginalParent = null;
            Debug.Log("It is a bag");
        }
        currentGrabInteractable.GetComponent<Rigidbody>().isKinematic = false;     // Make it affected by physics again
        currentGrabInteractable.transform.SetParent(currentObjectOriginalParent);  // Unparent the object
        currentGrabInteractable.selectExited.RemoveListener(Released);             // Remove listener
    }
 
}
