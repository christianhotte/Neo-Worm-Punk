using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandleController : GrabbableUI
{
    [SerializeField, Tooltip("The bounds that keeps the handle within the slider.")] private Transform handleSnapPointLeft, handleSnapPointRight;
    private LeverController leverController;
    private Vector3 startingVector;

    protected override void Awake()
    {
        base.Awake();
        leverController = GetComponentInParent<LeverController>();
        startingVector = transform.up;
    }

    public override void OnGrab()
    {
        base.OnGrab();
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }

    private void Update()
    {
        if (isGrabbed && followObject != null)
        {
            Quaternion lookAngle = Quaternion.Euler(Mathf.Clamp(Vector2.SignedAngle(followObject.position - transform.position, startingVector), leverController.GetMinimumAngle(), leverController.GetMaximumAngle()), 0, 0);
            transform.localRotation = lookAngle;
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
