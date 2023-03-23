using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverHandleController : GrabbableUI
{
    private LeverController leverController;
    private Vector3 startingVector;

    protected override void Awake()
    {
        base.Awake();
        leverController = GetComponentInParent<LeverController>();
        startingVector = leverController.transform.up;
        Debug.DrawRay(leverController.transform.position, leverController.transform.up * 10, Color.red, 20);
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
            Quaternion lookMover = Quaternion.FromToRotation(transform.up, Vector3.ProjectOnPlane((followObject.position - leverController.transform.position).normalized, leverController.transform.right)) * transform.rotation;
            Quaternion lookAngle = Quaternion.Euler(Mathf.Clamp(lookMover.eulerAngles.x, leverController.GetMinimumAngle(), leverController.GetMaximumAngle()), lookMover.eulerAngles.y, lookMover.eulerAngles.z);
            transform.rotation = lookAngle;
        }
    }

    public void MoveToAngle(LeverController lever, float newAngle)
    {
        transform.localRotation = Quaternion.Euler(Mathf.Clamp(newAngle, lever.GetMinimumAngle(), lever.GetMaximumAngle()), 0, 0);
    }

    public float GetAngle() => (transform.localEulerAngles.x > 180) ? transform.localEulerAngles.x - 360 : transform.localEulerAngles.x;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
    }
}
