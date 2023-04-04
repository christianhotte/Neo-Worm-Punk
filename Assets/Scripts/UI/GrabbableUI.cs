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
        inputActions.XRILeftHandInteraction.Grip.started += _ => OnGrab();
        inputActions.XRIRightHandInteraction.Grip.started += _ => OnGrab();
        inputActions.XRILeftHandInteraction.Grip.canceled += _ => OnRelease();
        inputActions.XRIRightHandInteraction.Grip.canceled += _ => OnRelease();

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand") && !isGrabbable)
        {
            isGrabbable = true;
            followObject = other.transform;
            SetAllMaterials(inRangeMat);
            SendHapticsFeedback(0.3f, 0.2f);
        }
    }

   private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlayerHand") && followObject == null)
        {
            followObject = other.transform;
            SetAllMaterials(inRangeMat);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand") && isGrabbable && !isGrabbed)
        {
            isGrabbable = false;
            SetDefaultMaterials();
        }
    }

    public virtual void OnGrab()
    {
        if (isGrabbable && !isGrabbed)
        {
            //Debug.Log("Grabbing Object...");
            isGrabbed = true;
            SetAllMaterials(grabbedMat);
            SendHapticsFeedback(0.4f, 0.15f);
        }
    }

    public virtual void OnRelease()
    {
        if (isGrabbed)
        {
            //Debug.Log("Releasing Object...");
            SendHapticsFeedback(0.1f, 0.2f);
            isGrabbed = false;
            followObject = null;
            SetDefaultMaterials();
        }
    }

    private void SetAllMaterials(Material newMat)
    {
        for (int i = 0; i < handleRenderers.Length; i++)
        {
            handleRenderers[i].material = newMat;
        }
    }

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
            InputDeviceRole playerHand = followObject.name == "LeftHand" ? InputDeviceRole.LeftHanded : InputDeviceRole.RightHanded;
            PlayerController.instance.SendHapticImpulse(playerHand, new Vector2(amplitude, duration));
        }
    }

    public bool IsGrabbed() => isGrabbed;
    public Transform GetFollowObject() => followObject;
}
