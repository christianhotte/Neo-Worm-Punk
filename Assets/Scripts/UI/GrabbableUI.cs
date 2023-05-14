using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class GrabbableUI : MonoBehaviour
{
    protected bool isGrabbable = false;
    protected bool isGrabbed = false;

    protected Transform followObject;

    private MeshRenderer[] handleRenderers;
    private List<Material> defaultMats = new List<Material>();
    [SerializeField] protected Material inRangeMat, grabbedMat;

    private HotteInputActions inputActions;
    protected virtual void Awake()
    {
        inputActions = new HotteInputActions();
        inputActions.XRILeftHandInteraction.GripHold.started += _ => OnGrab();
        inputActions.XRIRightHandInteraction.GripHold.started += _ => OnGrab();
        inputActions.XRILeftHandInteraction.GripHold.canceled += _ => OnRelease();
        inputActions.XRIRightHandInteraction.GripHold.canceled += _ => OnRelease();

        handleRenderers = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < handleRenderers.Length; i++)
            defaultMats.Add(handleRenderers[i].material);

        SetDefaultMaterials();

        isGrabbable = false;
        isGrabbed = false;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    /// <summary>
    /// Let the player know that this object is grabbable when entering the trigger.
    /// </summary>
    /// <param name="other">The object colliding with the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand") && other.transform.name.Contains("Controller") && !isGrabbable)
        {
            isGrabbable = true;
            followObject = other.transform;
            SetAllMaterials(inRangeMat);
            SendHapticsFeedback(0.3f, 0.2f);
            //Debug.Log("Enter UI Object Trigger With " + followObject.name);
        }
    }

    /// <summary>
    /// While inside of the trigger, ensure that the collider knows that there is something that can grab it.
    /// </summary>
    /// <param name="other">The object colliding with the trigger.</param>
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlayerHand") && other.transform.name.Contains("Controller") && followObject == null)
        {
            followObject = other.transform;
            SetAllMaterials(inRangeMat);
            //Debug.Log("Staying In UI Object Trigger With " + followObject.name);
        }
    }

    /// <summary>
    /// If exiting the trigger and they are not grabbing the object, set as default and let the object know that it cannot be grabbed.
    /// </summary>
    /// <param name="other">The object colliding with the trigger.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand") && other.transform.name.Contains("Controller") && isGrabbable && !isGrabbed)
        {
            isGrabbable = false;
            SetDefaultMaterials();
            //Debug.Log("Exiting UI Object Trigger With " + other.transform.name);
        }
    }

    /// <summary>
    /// The logic for when the object is grabbed. It is only called if the object has not been grabbed but can be.
    /// </summary>
    public virtual void OnGrab()
    {
        if (isGrabbable && !isGrabbed && followObject != null)
        {
            //Debug.Log("Grabbing UI Object With " + followObject.name);
            isGrabbed = true;
            SetAllMaterials(grabbedMat);
            SendHapticsFeedback(0.4f, 0.15f);
        }
    }

    /// <summary>
    /// The logic for when the object is released. It is only called when the object is already grabbed beforehand.
    /// </summary>
    public virtual void OnRelease()
    {
        if (isGrabbed)
        {
            //Debug.Log("Releasing UI Object With " + followObject.name);
            SendHapticsFeedback(0.1f, 0.2f);
            isGrabbed = false;
            followObject = null;
            SetDefaultMaterials();
        }
    }

    /// <summary>
    /// Sets all of the materials of the object to a new material.
    /// </summary>
    /// <param name="newMat">The new material.</param>
    private void SetAllMaterials(Material newMat)
    {
        for (int i = 0; i < handleRenderers.Length; i++)
        {
            handleRenderers[i].material = newMat;
        }
    }

    /// <summary>
    /// Sets the default materials of the item.
    /// </summary>
    private void SetDefaultMaterials()
    {
        for (int i = 0; i < handleRenderers.Length; i++)
        {
            handleRenderers[i].material = defaultMats[i];
        }
    }

    /// <summary>
    /// Sends a haptic impulse to the hand.
    /// </summary>
    /// <param name="amplitude">The strength of the impulse.</param>
    /// <param name="duration">The duration of the impulse.</param>
    public void SendHapticsFeedback(float amplitude, float duration)
    {
        if(followObject != null)
        {
            InputDeviceRole playerHand = followObject.name.Contains("LeftHand") ? InputDeviceRole.LeftHanded : InputDeviceRole.RightHanded;
            PlayerController.instance.SendHapticImpulse(playerHand, new Vector2(amplitude, duration));
        }
    }

    public bool IsGrabbed() => isGrabbed;
    public Transform GetFollowObject() => followObject;
}
