using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnManager5 : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<GameObject> availableSpawnPoints = new List<GameObject>();
    private Dictionary<int, GameObject> spawnPointOwners = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    private void Awake()
    {
        // Find all Spawn Points in the scene with PhotonView components and add them to availableSpawnPoints list
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        foreach (PhotonView photonView in photonViews)
        {
            if (photonView.gameObject.GetComponent<SpawnPointManager>())
            {
                availableSpawnPoints.Add(photonView.gameObject);
                spawnPointOwners[photonView.ViewID] = null;
            }
        }
    }

    public void SpawnPlayer()
    {
        // Find an available Spawn Point
        GameObject spawnPoint = null;
        foreach (GameObject point in availableSpawnPoints)
        {
            if (spawnPointOwners[point.GetPhotonView().ViewID] == null)
            {
                spawnPoint = point;
                break;
            }
        }

        if (spawnPoint != null)
        {
            // Assign ownership of Spawn Point to player who spawned at it
            PhotonView spawnPointPhotonView = spawnPoint.GetPhotonView();
            spawnPointPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            spawnPointOwners[spawnPointPhotonView.ViewID] = PhotonNetwork.LocalPlayer;

            // Spawn player at selected Spawn Point
            Transform spawnTransform = spawnPoint.GetComponent<Transform>();
            GameObject playerObject = PhotonNetwork.Instantiate("Player", spawnTransform.position, spawnTransform.rotation);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Release the Spawn Point owned by the player who left the room
        foreach (GameObject point in availableSpawnPoints)
        {
            PhotonView photonView = point.GetPhotonView();
            if (photonView.Owner == otherPlayer)
            {
                spawnPointOwners[photonView.ViewID] = null;
                break;
            }
        }
    }
}