using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderHandleController : GrabbableUI
{
    [SerializeField, Tooltip("The bounds that keeps the handle within the slider.")] private Transform handleSnapPointLeft, handleSnapPointRight;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void OnGrab()
    {
        base.OnGrab();
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }
}
