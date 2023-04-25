using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public int rotSpeed;
    public bool grinder = false;
    internal Grinder grindScript;
    internal TurbineZone TurbineScript;
    // Start is called before the first frame update
    void Start()
    {
        if(grinder) grindScript = GetComponentInParent<Grinder>();
        else
        {
            TurbineScript = GetComponentInParent<TurbineZone>();
        }

        rotSpeed = 200;
    }

    // Update is called once per frame
    void Update()
    {
        if (grinder&&grindScript.Activated)
        {
            transform.Rotate(0, rotSpeed * Time.deltaTime, 0);
        }
        else if (!grinder&&TurbineScript.Enabled)
        {
            transform.Rotate(0, rotSpeed * Time.deltaTime, 0);
        }
    }
}
