using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRRayInteractor))]
public class RayToggle : MonoBehaviour
{
    [SerializeField] private InputActionReference activateReference;    //The action that toggles the ray interactor

    private XRRayInteractor rayInteractor;  //The ray interactor component
    private bool isEnabled = false; //Checks to see if the ray interactor is enabled

    private void Awake()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
    }

    private void OnEnable()
    {
        activateReference.action.started += ToggleRay;
        activateReference.action.canceled += ToggleRay;
    }

    private void OnDisable()
    {
        activateReference.action.started -= ToggleRay;
        activateReference.action.canceled -= ToggleRay;
    }

    /// <summary>
    /// Uses input to toggle the ray interactor.
    /// </summary>
    /// <param name="ctx">The input action value.</param>
    private void ToggleRay(InputAction.CallbackContext ctx)
    {
        isEnabled = ctx.control.IsPressed();
    }

    private void LateUpdate()
    {
        ApplyStatus();
    }

    /// <summary>
    /// Enables or disables the ray interactor on runtime
    /// </summary>
    private void ApplyStatus()
    {
        if (rayInteractor.enabled != isEnabled)
            rayInteractor.enabled = isEnabled;
    }
}
