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
    void Update()
    {
        if (Gustin && StrongGust)
        {
            Vector3 boostVel = TurbineForce * -turbineTrans.up;

            playerRB.velocity += boostVel;
            Debug.Log("Big Gust");
        }
        else if (Gustin)
        {
            Vector3 boostVel =(TurbineForce*Time.deltaTime)*-turbineTrans.up ;
            playerRB.velocity += boostVel;
          //  Debug.Log("norml Gust");

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
    private void OnTriggerExit(Collider other)
    {
        if(other.name == "XR Origin")
        {
            Gustin = false;
        }
    }

}
