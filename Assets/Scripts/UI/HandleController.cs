using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandleController : MonoBehaviour
{
    [SerializeField, Tooltip("The bounds that keeps the handle within the slider.")] private Transform handleSnapPointLeft, handleSnapPointRight;

    private MeshRenderer[] handleRenderers;
    private List<Material> defaultMats = new List<Material>();
    [SerializeField] private Material inRangeMat, closestOneMat, grabbedMat;

    private bool isGrabbable = false;
    private bool isGrabbed = false;

    private Transform followObject;
    private LeverController leverController;

    private Vector3 startingVector;

    private void Awake()
    {
        leverController = GetComponentInParent<LeverController>();
        startingVector = transform.up;

        handleRenderers = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < handleRenderers.Length; i++)
            defaultMats.Add(handleRenderers[i].material);

        SetDefaultMaterials();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand") && !isGrabbable)
        {
            isGrabbable = true;
            followObject = other.transform;
            SetAllMaterials(inRangeMat);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand") && isGrabbable && !isGrabbed)
        {
            isGrabbable = false;
            StopGrabLever();
        }
    }

    public void StartGrabLever()
    {
        isGrabbed = true;
        SetAllMaterials(grabbedMat);
    }

    public void StopGrabLever()
    {
        isGrabbed = false;
        followObject = null;
        SetDefaultMaterials();
    }

    private void Update()
    {
        if (isGrabbed && followObject != null)
        {
            Quaternion lookAngle = Quaternion.Euler(Mathf.Clamp(Vector2.SignedAngle(followObject.position - transform.position, startingVector), leverController.GetMinimumAngle(), leverController.GetMaximumAngle()), 0, 0);
            transform.localRotation = lookAngle;
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

    public void MoveToAngle(float newAngle)
    {
        transform.localRotation = Quaternion.Euler(Mathf.Clamp(newAngle, leverController.GetMinimumAngle(), leverController.GetMaximumAngle()), 0, 0);
    }

    public float GetAngle() => (transform.localEulerAngles.x > 180) ? transform.localEulerAngles.x - 360 : transform.localEulerAngles.x;
    public bool IsGrabbed => IsGrabbed;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
    }
}
