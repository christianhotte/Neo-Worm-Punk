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
            Quaternion lookAngle = Quaternion.FromToRotation(transform.up, Vector3.ProjectOnPlane((followObject.position - leverController.transform.position).normalized, leverController.transform.right)) * transform.rotation;
            Quaternion localLookAngle = Quaternion.Inverse(leverController.transform.rotation) * lookAngle;

            float localAngle = (localLookAngle.eulerAngles.x > 180) ? localLookAngle.eulerAngles.x - 360 : localLookAngle.eulerAngles.x;

            //Debug.Log("Local Lever Angle: " + localAngle);

            //If the player angle is not in the threshold area, allow free lever movement
            if (localAngle > leverController.GetMinimumAngle() + leverController.GetLeverMinThreshold() && localAngle < leverController.GetMaximumAngle() - leverController.GetLeverMaxThreshold())
            {
                transform.localRotation = localLookAngle;
                ClampLever();
            }
            //If the player angle is in the threshold area, move to one of the limits
            else
            {
                if (localAngle > 0)
                    MoveToAngle(leverController, leverController.GetMaximumAngle());
                else
                    MoveToAngle(leverController, leverController.GetMinimumAngle());
            }
        }
    }

    private void ClampLever() => transform.localRotation = Quaternion.Euler(Mathf.Clamp(GetAngle(), leverController.GetMinimumAngle(), leverController.GetMaximumAngle()), 0, 0);

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
