using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPointManager : MonoBehaviour
{
    // This has to be on Awake()
    private void Awake()
    {
        // Gets the photonView component of the spawn points
        PhotonView photonView = GetComponent<PhotonView>();

        // Gives the ownership of the photonView to the master client so other players can access them.
        if (photonView != null)
        {
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
        }
    }

    private void OnDestroy()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}