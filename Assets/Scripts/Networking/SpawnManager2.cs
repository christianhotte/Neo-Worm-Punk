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

            if (ReadyUpManager.instance != null)
            {
                ReadyUpManager.instance.localPlayerTube = spawnTube;
                ReadyUpManager.instance.UpdateStatus(spawnTube.GetTubeNumber());
                ReadyUpManager.instance.localPlayerTube.SpawnPlayerName(NetworkManagerScript.instance.GetLocalPlayerName());
                NetworkManagerScript.localNetworkPlayer.UpdateTakenColorsOnJoin();
                ReadyUpManager.instance.localPlayerTube.GetComponent<PlayerColorChanger>().RefreshButtons();
            }
            
        }
    }
    public void MoveDemoPlayerToSpawnPoint(int tubeIndex)
    {
        LockerTubeController spawnTube = FindObjectOfType<TubeManager>().GetTubeByNumber(tubeIndex);
        if (spawnTube != null)
        {
            spawnTube.occupied = true;
            PlayerController.instance.bodyRb.transform.position = spawnTube.spawnPoint.position;
            PlayerController.instance.bodyRb.transform.rotation = spawnTube.spawnPoint.rotation;

            if (ReadyUpManager.instance != null)
            {
                ReadyUpManager.instance.localPlayerTube = spawnTube;
                ReadyUpManager.instance.UpdateStatus(spawnTube.GetTubeNumber());
                ReadyUpManager.instance.localPlayerTube.SpawnPlayerName(NetworkManagerScript.instance.GetLocalPlayerName());
                NetworkManagerScript.localNetworkPlayer.UpdateTakenColorsOnJoin();
                ReadyUpManager.instance.localPlayerTube.GetComponent<PlayerColorChanger>().RefreshButtons();
            }
        }
    }

    public LockerTubeController GetEmptyTube()
    {
        TubeManager tubeManager = FindObjectOfType<TubeManager>();

        for (int x = 0; x < tubeManager.roomTubes.Count; x++)
        {
            foreach (LockerTubeController tube in tubeManager.roomTubes)
            {
                if (tube.GetTubeNumber() == x + 1)
                {
                    if (!tube.occupied) return tube;
                }
            }
        }

        Debug.LogError("Error: Could Not Find An Empty Tube.");
        return null;
    }
}