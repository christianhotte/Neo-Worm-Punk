using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabbable
{
    void StartGrabbing(Transform handAnchor);
    void EnterRange();
    void ExitRange();
    void SetClosestOne();
    void StopGrabbing();
}
