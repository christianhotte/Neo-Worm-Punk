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

        //Wait until the local player tube is assigned
        StartCoroutine(WaitUntilLocalPlayerTube());
    }

    /// <summary>
    /// Waits until the local player tube is assigned to update the ReadyUpManager status.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitUntilLocalPlayerTube()
    {
        yield return new WaitUntil(() => ReadyUpManager.instance != null && ReadyUpManager.instance.localPlayerTube != null);
        // Updates the ReadyUpManager
        ReadyUpManager.instance.UpdateStatus(ReadyUpManager.instance.localPlayerTube.GetTubeNumber());
    }

    private void OnDestroy()
    {
        instance = null;
    }

    /// <summary>
    /// Moves a player to a spawn point in the tube.
    /// </summary>
    /// <param name="tubeIndex">The index for the tube that we want to put the player into.</param>
    public void MoveDemoPlayerToSpawnPoint(int tubeIndex = 0)
    {
        LockerTubeController spawnTube;
        if (tubeIndex == 0)
            spawnTube = GetEmptyTube();
        else
            spawnTube = FindObjectOfType<LockerTubeSpawner>().GetTubeByIndex(tubeIndex);

        Debug.Log("Tube Being Occupied: TestTube" + spawnTube.GetTubeNumber() + " By " + NetworkManagerScript.instance.GetLocalPlayerName());

        if (spawnTube != null)
        {
            //spawnTube.occupied = true;
            PlayerController.instance.bodyRb.transform.position = spawnTube.spawnPoint.position;
            PlayerController.instance.bodyRb.transform.rotation = spawnTube.spawnPoint.rotation;

            if (ReadyUpManager.instance != null)
            {
                ReadyUpManager.instance.localPlayerTube = spawnTube;
                ReadyUpManager.instance.UpdateStatus(spawnTube.GetTubeNumber());
                ReadyUpManager.instance.localPlayerTube.SpawnPlayerName(NetworkManagerScript.instance.GetLocalPlayerName());
                NetworkManagerScript.localNetworkPlayer.CheckExclusiveColors();
                ReadyUpManager.instance.localPlayerTube.GetComponentInChildren<PlayerColorChanger>().RefreshButtons();
                if (PhotonNetwork.IsMasterClient)
                    ReadyUpManager.instance.localPlayerTube.ShowHostSettings(true); //Show the settings if the player being moved is the master client
            }
        }
    }

    public LockerTubeController GetEmptyTube()
    {
/*        TubeManager tubeManager = FindObjectOfType<TubeManager>();

        for (int x = 1; x <= tubeManager.roomTubes.Count; x++)
        {
            foreach (LockerTubeController tube in tubeManager.roomTubes)
            {
                if (tube.GetTubeNumber() == x)
                {
                    if (!tube.occupied)
                    {
                        tube.occupied = true;
                        return tube;
                    }
                }
            }
        }*/

        Debug.LogError("Error: Could Not Find An Empty Tube.");
        return null;
    }
}