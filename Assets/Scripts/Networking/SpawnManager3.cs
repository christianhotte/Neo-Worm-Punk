using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class SpawnManager3 : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private LockerTubeController[] tubes;

    private Dictionary<int, Transform> playerSpawnPoints = new Dictionary<int, Transform>();
    private int nextSpawnPointIndex = 0;

    private void Awake()
    {
        // Event subscription
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Has to be connected to the network
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("SpawnManager should only be used in a networked game.");
            return;
        }

        // Cannot have no spawn points.
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("SpawnManager is missing spawn points.");
            return;
        }

        //Wait until the local player tube is assigned
        StartCoroutine(WaitUntilLocalPlayerTube());

        // Assigns the player a spawn point when they get into the locker scene.
        AssignSpawnPointsToPlayers();
    }

    // Hard resets the dictionary of spawn points everytime you load into the scene.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerSpawnPoints.Clear();
    }

    // Cleanup
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Waits until the local player tube is assigned to update the ReadyUpManager status.
    private IEnumerator WaitUntilLocalPlayerTube()
    {
        yield return new WaitUntil(() => ReadyUpManager.instance != null && ReadyUpManager.instance.localPlayerTube != null);
        // Updates the ReadyUpManager
        ReadyUpManager.instance.UpdateStatus(ReadyUpManager.instance.localPlayerTube.GetTubeNumber());
    }

    // The players gets a spawn point from their player ID when they join a room.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        int playerId = newPlayer.ActorNumber;
        Transform spawnPoint = GetNextAvailableSpawnPoint();
        playerSpawnPoints[playerId] = spawnPoint;
    }

    // We free up the spawn point when the player leaves the room using the player ID.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int playerId = otherPlayer.ActorNumber;
        if (playerSpawnPoints.ContainsKey(playerId))
        {
            Transform spawnPoint = playerSpawnPoints[playerId];
            playerSpawnPoints.Remove(playerId);
            ReleaseSpawnPoint(spawnPoint);
        }
    }

    // Gets the next available spawn point.
    private Transform GetNextAvailableSpawnPoint()
    {
        Transform spawnPoint = spawnPoints[nextSpawnPointIndex];
        nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Length;
        return spawnPoint;
    }

    // Frees up the spawn point from the player that left.
    private void ReleaseSpawnPoint(Transform spawnPoint)
    {
        nextSpawnPointIndex = (nextSpawnPointIndex - 1 + spawnPoints.Length) % spawnPoints.Length;
    }

    // If we don't already have a network player we can spawn one later. This function is not needed.
    /*private void SpawnPlayer(Player player)
    {
        PhotonView photonView = PlayerController.photonView;
        if (photonView != null)
        {
            photonView.TransferOwnership(player.ActorNumber);
        }
    }*/

    // Assigns the spawn points to the player ONLY when they join the scene.
    private void AssignSpawnPointsToPlayers()
    {
        // Loops through the list of players and searches for the specific player ID.
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int playerId = PhotonNetwork.PlayerList[i].ActorNumber;
            int spawnNumber = i % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnNumber];
            playerSpawnPoints.Add(playerId, spawnPoint);

            // Assigns the spawn points to the corresponding tube and calling ReadyUpManager functions
            LockerTubeController spawnTube = tubes[spawnNumber];
            if (spawnTube != null)
            {
                spawnTube.occupied = true;
                PlayerController.instance.bodyRb.transform.position = spawnTube.spawnPoint.position;
                PlayerController.instance.bodyRb.transform.rotation = spawnTube.spawnPoint.rotation;

                if (ReadyUpManager.instance != null)
                {
                    ReadyUpManager.instance.localPlayerTube = spawnTube;
                    ReadyUpManager.instance.UpdateStatus(spawnNumber);
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
    }

    // Assigns the spawn points to the player ONLY when they join the scene.
    public void AssignSpawnPointToPlayer(int playerId)
    {
        if (!playerSpawnPoints.ContainsKey(playerId))
        {
            Transform spawnPoint = GetNextAvailableSpawnPoint();
            playerSpawnPoints[playerId] = spawnPoint;
        }
    }

    // Frees open the spawn point for when the player leaves.
    public void ReleaseSpawnPointForPlayer(int playerId)
    {
        if (playerSpawnPoints.ContainsKey(playerId))
        {
            Transform spawnPoint = playerSpawnPoints[playerId];
            playerSpawnPoints.Remove(playerId);
            ReleaseSpawnPoint(spawnPoint);
        }
    }

    // Looks for an empty tube.
    public LockerTubeController GetAssignedTube()
    {
        TubeManager tubeManager = FindObjectOfType<TubeManager>();

        /*for (int x = 0; x = spawnPoints.Length; x++)
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