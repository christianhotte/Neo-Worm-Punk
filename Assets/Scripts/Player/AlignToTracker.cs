using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignToTracker : MonoBehaviour
{
    // In the Inspector window, add the XR Origin/CenterEyeAnchor
    public Transform hmdOrientation;

    private Vector3 tempRot;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        tempRot.x = -hmdOrientation.localEulerAngles.y;
        tempRot.y = hmdOrientation.localEulerAngles.z;
        tempRot.z = -hmdOrientation.localEulerAngles.x;

        // Update head rotation
        transform.localEulerAngles = tempRot;
    }
}
