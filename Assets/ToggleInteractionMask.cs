using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class ToggleInteractionMask : MonoBehaviour
{
    public XRRayInteractor rayInteractor; // Assign the ray interactor
    public InteractionLayerMask interactionLayer; // Layer to switch to
    public InputActionProperty leftActivate;

    public XRDirectInteractor leftDirectInteractor;
    public XRDirectInteractor rightDirectInteractor;

    private bool buttonPressed;

    private void OnEnable()
    {
        leftActivate.action.Enable();
        leftActivate.action.started += OnActivateStarted;
        leftActivate.action.canceled += OnActivateCanceled;
    }

    private void OnDisable()
    {
        leftActivate.action.Disable();
        leftActivate.action.started -= OnActivateStarted;
        leftActivate.action.canceled -= OnActivateCanceled;
    }

    private void OnActivateStarted(InputAction.CallbackContext context)
    {
        if (!buttonPressed)
        {
            buttonPressed = true;
            ToggleMask(); // Call your activation logic here
        }
    }

    private void OnActivateCanceled(InputAction.CallbackContext context)
    {
        buttonPressed = false; // Reset the press state when the button is released
    }

    void ToggleMask()
    {
        bool leftHandHasSelection = leftDirectInteractor.hasSelection;
        bool rightHandHasSelection = rightDirectInteractor.hasSelection;
        if (!leftHandHasSelection && !rightHandHasSelection)
        {
            // Check the current mask and toggle accordingly
            if (rayInteractor.interactionLayers == 0)
            {
                rayInteractor.interactionLayers = interactionLayer; // Set to the desired layer
            }
            else
            {
                rayInteractor.interactionLayers = 0; // Set back to "Nothing"
            }
        }
    }
}
