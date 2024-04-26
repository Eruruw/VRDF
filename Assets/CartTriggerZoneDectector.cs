using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CartTriggerZoneDectector : MonoBehaviour
{
    //dictionary to hold objects and their parents
    public Dictionary<GameObject, Transform> objectsInCart { get; private set; } = new Dictionary<GameObject, Transform>();

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
                objectsInCart.Add(other.gameObject, other.transform.parent);

                Rigidbody rb = other.GetComponent<Rigidbody>(); // make it kinematic to avoid physics forces acting on it while in the cart
                StartCoroutine(MakeKinematicAfterDelay(other));

                if (other.gameObject.name == "Bag" || other.gameObject.name == "Bag(Clone)")
                { 

                    ScoreTracker scoreTacker = GameObject.Find("Player").GetComponent<ScoreTracker>();
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


    private IEnumerator MakeKinematicAfterDelay(Collider obj)
    {
        yield return new WaitForSeconds(0.3f);

        if (obj != null)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        bool hasDirectInteractor = false;
        bool hasRayInteractor = false;

        XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            List<IXRSelectInteractor> interactors = grab.interactorsSelecting;
            foreach (IXRSelectInteractor inter in interactors)
            {
                if (inter is XRDirectInteractor)
                {
                    hasDirectInteractor = true;
                }
                if (inter is XRRayInteractor)
                {
                    hasRayInteractor = true;
                }
            }
        }


        if (objectsInCart.ContainsKey(other.gameObject))
        {
            if (hasRayInteractor || hasDirectInteractor)
            {
                Debug.Log("left trigger " + other.gameObject);
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
    }

    private void Released(SelectExitEventArgs args)
    {
        if (currentGrabInteractable.gameObject.name == "Bag(Clone)" || currentGrabInteractable.gameObject.name == "Bag")
        {
            currentObjectOriginalParent = null;
            //Debug.Log("It is a bag");
        }
        currentGrabInteractable.GetComponent<Rigidbody>().isKinematic = false;     // Make it affected by physics again
        currentGrabInteractable.transform.SetParent(currentObjectOriginalParent);  // Unparent the object
        currentGrabInteractable.selectExited.RemoveListener(Released);             // Remove listener
    }
 
}
