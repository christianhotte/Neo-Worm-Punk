using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnManager4 : MonoBehaviourPunCallbacks
{
    // The index of the next available spawn point in the list
    private int nextSpawnPointIndex = 0;

    // Start is called before the first frame update
    private void Start()
    {
        // Get the GameManager instance and retrieve the list of spawn points from it
        GameManager gameManager = FindObjectOfType<GameManager>();
        List<Transform> spawnPoints = gameManager.spawnPoints;
        List<LockerTubeController> tubes = gameManager.tubes;

        // Spawn the local player at the next available spawn point
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            Transform spawnPoint = spawnPoints[nextSpawnPointIndex];
            GameObject player = PhotonView.Find(actorNumber).gameObject;
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;

            // Loops through the list of players and searches for the specific player ID.
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                int playerId = PhotonNetwork.PlayerList[i].ActorNumber;
                int spawnNumber = i % spawnPoints.Count;

                // Assigns the spawn points to the corresponding tube and calling ReadyUpManager functions
                LockerTubeController spawnTube = tubes[spawnNumber];
                if (spawnTube != null)
                {
                    //spawnTube.occupied = true;
                    //PlayerController.instance.bodyRb.transform.position = spawnTube.spawnPoint.position;
                    //PlayerController.instance.bodyRb.transform.rotation = spawnTube.spawnPoint.rotation;

                    if (ReadyUpManager.instance != null)
                    {
                        ReadyUpManager.instance.localPlayerTube = spawnTube;
                        ReadyUpManager.instance.UpdateStatus(spawnNumber + 1);
                        ReadyUpManager.instance.localPlayerTube.SpawnPlayerName(NetworkManagerScript.instance.GetLocalPlayerName());
                        NetworkManagerScript.localNetworkPlayer.UpdateTakenColorsOnJoin();
                        ReadyUpManager.instance.localPlayerTube.GetComponentInChildren<PlayerColorChanger>().RefreshButtons();
                        if (PhotonNetwork.IsMasterClient)
                            ReadyUpManager.instance.localPlayerTube.ShowHostSettings(true); //Show the settings if the player being moved is the master client
                    }
                }

                else
                {
                    Debug.LogError("Spawn problem with spawn number " + spawnNumber);
                }
            }

            // Increase the index of the next available spawn point
            nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Count;
        }

        else
        {
            // Move the existing network player object to the selected spawn point
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            GameObject player = PhotonView.Find(actorNumber).gameObject;
            Transform spawnPoint = spawnPoints[nextSpawnPointIndex];
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;



            // Loops through the list of players and searches for the specific player ID.
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                int playerId = PhotonNetwork.PlayerList[i].ActorNumber;
                int spawnNumber = i % spawnPoints.Count;

                // Assigns the spawn points to the corresponding tube and calling ReadyUpManager functions
                LockerTubeController spawnTube = tubes[spawnNumber];
                if (spawnTube != null)
                {
                    //spawnTube.occupied = true;
                    //PlayerController.instance.bodyRb.transform.position = spawnTube.spawnPoint.position;
                    //PlayerController.instance.bodyRb.transform.rotation = spawnTube.spawnPoint.rotation;

                    if (ReadyUpManager.instance != null)
                    {
                        ReadyUpManager.instance.localPlayerTube = spawnTube;
                        ReadyUpManager.instance.UpdateStatus(spawnNumber + 1);
                        ReadyUpManager.instance.localPlayerTube.SpawnPlayerName(NetworkManagerScript.instance.GetLocalPlayerName());
                        NetworkManagerScript.localNetworkPlayer.UpdateTakenColorsOnJoin();
                        ReadyUpManager.instance.localPlayerTube.GetComponentInChildren<PlayerColorChanger>().RefreshButtons();
                        if (PhotonNetwork.IsMasterClient)
                            ReadyUpManager.instance.localPlayerTube.ShowHostSettings(true); //Show the settings if the player being moved is the master client
                    }
                }

                else
                {
                    Debug.LogError("Spawn problem with spawn number " + spawnNumber);
                }
            }

            // Increase the index of the next available spawn point
            nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Count;
        }
    }
}