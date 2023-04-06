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
            //playerRB.MovePosition(new Vector3(playerRB.position.x, playerRB.position.y - 0.5f, playerRB.position.z));

            playerRB.AddForce(new Vector3(0, -1f, 0), ForceMode.VelocityChange);

            //playerRB.velocity = new Vector3(playerRB.velocity.x, -0.4f, playerRB.velocity.z);
            //Vector3 boostVel =(TurbineForce*Time.deltaTime)*-turbineTrans.up ;
            //playerRB.velocity += boostVel;
            //  Debug.Log("norml Gust");

            //A = player velocity
            //B = turbine down force velocity
            //apply A to
            //playerRB.velocity = new Vector3(playerRB.velocity.x, playerRB.velocity.y - 0.4f, playerRB.velocity.z);




            //player motion per frame:
            //position = changing
            //dx = changing
            //velocity = changing
            //acceleration = -30

            //turbine motion per frame:
            //position = changing
            //dx = 0.4f
            //velocity = 0.4f
            //accleration = 0
            


            //applied to player: (player)










            //everyFrame(playerVel){
            //playerVel = playerVel + turbineVel
            //apply things from playerVel in game
            //everyFrame(playerVel)
            //}
            //this just increases gravity


            //everyFrame(playerVel){
            //TEMP = playerVel + turbineVel
            //apply things from TEMP in game
            //everyFrame(playerVel)
            //}
            //this adds a constant movement downward


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
