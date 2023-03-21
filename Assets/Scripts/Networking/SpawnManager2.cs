using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnManager2 : MonoBehaviourPunCallbacks
{
    public static SpawnManager2 instance;
    private GameObject demoPlayer;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        demoPlayer = PlayerController.instance.gameObject;
        if (demoPlayer == null)
        {
            Debug.LogError("Player not found in scene.");
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("Photon network is not connected and ready.");
            return;
        }

        if (PhotonNetwork.IsMasterClient) MoveDemoPlayerToSpawnPoint();
        else
        {
            PlayerController.photonView.RPC("RPC_GiveMeSpawnpoint", RpcTarget.MasterClient, PlayerController.photonView.ViewID);
        }
    }
    private void OnDestroy()
    {
        instance = null;
    }

    // Moves the local demo player to a spawn point.
    public void MoveDemoPlayerToSpawnPoint()
    {
        LockerTubeController spawnTube = GetEmptyTube();
        if (spawnTube != null)
        {
            spawnTube.occupied = true;
            PlayerController.instance.bodyRb.transform.position = spawnTube.spawnPoint.position;
            PlayerController.instance.bodyRb.transform.rotation = spawnTube.spawnPoint.rotation;

            LeverController lever = spawnTube.GetComponentInChildren<LeverController>();
            lever.OnStateChanged.AddListener(ReadyUpManager.instance.LeverStateChanged);

            if (ReadyUpManager.instance != null)
            {
                ReadyUpManager.instance.UpdateStatus(spawnTube.tubeNumber);
                ReadyUpManager.instance.localPlayerTube = spawnTube;
            }
            
        }
    }
    public void MoveDemoPlayerToSpawnPoint(int tubeIndex)
    {
        LockerTubeController spawnTube = LockerTubeController.GetTubeByNumber(tubeIndex);
        if (spawnTube != null)
        {
            spawnTube.occupied = true;
            PlayerController.instance.bodyRb.transform.position = spawnTube.spawnPoint.position;
            PlayerController.instance.bodyRb.transform.rotation = spawnTube.spawnPoint.rotation;

            //Add lever listener
            LeverController lever = spawnTube.GetComponentInChildren<LeverController>();
            lever.OnStateChanged.AddListener(ReadyUpManager.instance.LeverStateChanged);

            if (ReadyUpManager.instance != null)
            {
                ReadyUpManager.instance.UpdateStatus(spawnTube.tubeNumber);
                ReadyUpManager.instance.localPlayerTube = spawnTube;
            }
        }
    }

    public LockerTubeController GetEmptyTube()
    {
        for (int x = 0; x < LockerTubeController.tubes.Count; x++)
        {
            foreach (LockerTubeController tube in LockerTubeController.tubes)
            {
                if (tube.tubeNumber == x + 1)
                {
                    if (!tube.occupied) return tube;
                }
            }
        }

        return null;
    }
}