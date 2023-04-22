using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class LockerTubeSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField, Tooltip("The list of tubes for players to spawn into.")] private LockerTubeController[] tubes;

    private void Awake()
    {
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
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(MoveToSpawnPoint());
    }

    /// <summary>
    /// Moves the player to a spawn point in the scene.
    /// </summary>
    public IEnumerator MoveToSpawnPoint()
    {
        yield return new WaitUntil(() => ReadyUpManager.instance != null);

        int tubeID = (int)NetworkManagerScript.localNetworkPlayer.photonView.Owner.CustomProperties["TubeID"];  //Get the tube ID from the current player
        LockerTubeController spawnTube = tubes[tubeID]; //Gets the tube associated with the tube ID

        if (spawnTube != null)
        {
            // Moves the player to the spawn point
            PlayerController.instance.bodyRb.transform.position = spawnTube.spawnPoint.position;
            PlayerController.instance.bodyRb.transform.rotation = spawnTube.spawnPoint.rotation;

            ReadyUpManager.instance.localPlayerTube = spawnTube;
            ReadyUpManager.instance.UpdateStatus(tubeID + 1);
            ReadyUpManager.instance.localPlayerTube.SpawnPlayerName(NetworkManagerScript.instance.GetLocalPlayerName());
            StartCoroutine(NetworkManagerScript.localNetworkPlayer.UpdateTakenColorsOnJoin());
            if (PhotonNetwork.IsMasterClient)
                ReadyUpManager.instance.localPlayerTube.ShowHostSettings(true); //Show the settings if the player being moved is the master client
        }
    }

    public void OnLeverStateChanged()
    {
        if (ReadyUpManager.instance != null)
            ReadyUpManager.instance.LeverStateChanged();
    }

    public LockerTubeController[] GetTubeList() => tubes;
    public LockerTubeController GetTubeByIndex(int index)
    {
        if (GetTubeList().Length > 0)
            return GetTubeList()[index - 1];

        Debug.LogError("Failed to get tube number " + index + " | Tube count = " + GetTubeList().Length);
        return null;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}