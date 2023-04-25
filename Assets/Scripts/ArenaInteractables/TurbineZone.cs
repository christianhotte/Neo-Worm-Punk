using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurbineZone : MonoBehaviour
{
    public float TurbineForce = 10f;
    private PlayerController PC;
    private Rigidbody playerRB;
    private Transform turbineTrans;
    public bool StrongGust = false;
    internal bool Gustin;
    // Start is called before the first frame update
    void Start()
    {
        turbineTrans = this.transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Gustin && StrongGust)
        {
            Vector3 boostVel = TurbineForce * -turbineTrans.up;

            playerRB.velocity += boostVel;
            Debug.Log("Big Gust");
        }
        else if (Gustin)
        {
            playerRB.AddForce(new Vector3(0, -1f, 0), ForceMode.VelocityChange);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "XR Origin")
        {
            PC = PlayerController.instance;
            playerRB = PC.bodyRb;

            Gustin = true;
        }
    }
    /*
    private void OnTriggerStay(Collider other)
    {
        if (other.name == "XR Origin")
        {
            float addAcceleration = 3f;

            PC = PlayerController.instance;
            playerRB = PC.bodyRb;
            if (playerRB.velocity.y < -0.4f)
            {
                playerRB.velocity -= new Vector3(0, addAcceleration, 0);
            }
            Gustin = true;
        }
    }
    */
    private void OnTriggerExit(Collider other)
    {
        if(other.name == "XR Origin")
        {
            Gustin = false;
        }
    }

}
