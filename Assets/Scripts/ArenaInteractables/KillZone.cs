using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.XR.CoreUtils;

public class KillZone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        //PlayerController.photonView.ViewID;
        if (other.name == "XR Origin")
        {
            // hitPlayer = other.GetComponent<PlayerController>();
            // Debug.Log(netPlayer.name);
            PlayerController.instance.IsHit(100);
            //PlayerController.photonView.RPC("RPC_Hit", RpcTarget.All, 100, PlayerController.photonView.ViewID, Vector3.zero, (int)DeathCause.TRAP);
        }
    }
}
