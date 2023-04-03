using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnManager3 : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform[] spawnPoints;

    private Dictionary<int, Transform> playerSpawnPoints = new Dictionary<int, Transform>();
    private int nextSpawnPointIndex = 0;

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

        // Assigns the player a spawn point when they get into the locker scene.
        AssignSpawnPointsToPlayers();
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
    private void SpawnPlayer(Player player)
    {
        PhotonView photonView = PlayerController.photonView;
        if (photonView != null)
        {
            photonView.TransferOwnership(player.ActorNumber);
        }
    }

    // Assigns the spawn points to the player ONLY when they join the scene.
    private void AssignSpawnPointsToPlayers()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int playerId = PhotonNetwork.PlayerList[i].ActorNumber;
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
            playerSpawnPoints.Add(playerId, spawnPoint);
        }
    }

    public void AssignSpawnPointToPlayer(int playerId)
    {
        if (!playerSpawnPoints.ContainsKey(playerId))
        {
            Transform spawnPoint = GetNextAvailableSpawnPoint();
            playerSpawnPoints[playerId] = spawnPoint;
        }
    }

    public void ReleaseSpawnPointForPlayer(int playerId)
    {
        if (playerSpawnPoints.ContainsKey(playerId))
        {
            Transform spawnPoint = playerSpawnPoints[playerId];
            playerSpawnPoints.Remove(playerId);
            ReleaseSpawnPoint(spawnPoint);
        }
    }
}