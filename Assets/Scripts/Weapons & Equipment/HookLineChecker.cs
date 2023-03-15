using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookLineChecker : MonoBehaviour
{
    public RocketBoost GrapScrip;
    public Rigidbody thisRB;
    public float checkSpeed = 200;
    // Start is called before the first frame update
    void Awake()
    {
        thisRB = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GrapScrip.checkinLine)
        {
            thisRB.velocity = (GrapScrip.rocketTip.forward * checkSpeed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 11)
        {
            GrapScrip.checkinLine = false;
            Debug.Log("CheckerHitHook");
            Destroy(this);
        }
        else
        {
            GrapScrip.HookInstance.transform.position = this.transform.position;
            Debug.Log("CheckerHitWall");
            GrapScrip.checkinLine = false;
            Destroy(this);
        }
    }
}
