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
            Debug.DrawLine(leverController.transform.position, followObject.position, Color.green, Time.deltaTime);
            Quaternion lookAngle = Quaternion.Euler(Mathf.Clamp(Vector3.SignedAngle(leverController.transform.position, followObject.position, new Vector3(0, 0, 1)), leverController.GetMinimumAngle(), leverController.GetMaximumAngle()), 0, 0);
            transform.localRotation = lookAngle;
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
