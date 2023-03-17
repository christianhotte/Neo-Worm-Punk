using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.XR.CoreUtils;

public class Grinder : MonoBehaviour
{
    private PlayerController hitPlayer;
    private NetworkPlayer netPlayer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        //PlayerController.photonView.ViewID;
        if (other.name == "XR Origin")
        {
            // hitPlayer = other.GetComponent<PlayerController>();
            netPlayer = PlayerController.photonView.GetComponent<NetworkPlayer>();
            // Debug.Log(netPlayer.name);
            netPlayer.photonView.RPC("RPC_Hit", RpcTarget.All, 100, netPlayer.photonView.ViewID);
        }
    }
}
