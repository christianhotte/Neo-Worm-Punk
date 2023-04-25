using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class TurbineZone : MonoBehaviour
{
    public float TurbineForce = 10f;
    private PlayerController PC;
    private Rigidbody playerRB;
    private Transform turbineTrans;
    public bool StrongGust = false,killZone=false,Enabled=true;
    internal bool Gustin;
    private NetworkPlayer netPlayer;
    // Start is called before the first frame update
    void Start()
    {
        turbineTrans = this.transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Gustin && StrongGust&&Enabled)
        {
            Vector3 boostVel = TurbineForce * -turbineTrans.up;
            playerRB.velocity += boostVel;
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

    private void OnTriggerStay(Collider other)
    {
        if (other.name == "XR Origin"&&killZone&&Enabled)
        {
            netPlayer = PlayerController.photonView.GetComponent<NetworkPlayer>();
            netPlayer.photonView.RPC("RPC_Hit", RpcTarget.All, 100, netPlayer.photonView.ViewID, Vector3.zero, (int)DeathCause.TRAP);
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
