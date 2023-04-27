using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class LockerTubeSpawner : MonoBehaviourPunCallbacks
{
    public static LockerTubeSpawner instance;
    [SerializeField, Tooltip("The list of tubes for players to spawn into.")] private LockerTubeController[] tubes = new LockerTubeController[6];
    [SerializeField, Tooltip("The list of spawn points to instantiate the spawn tubes at.")] private Transform[] spawnPoints;
    [SerializeField, Tooltip("The tube prefab.")] private GameObject tubePrefab;

    private void Awake()
    {
        instance = this;
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

        if(tubes[tubeID] == null)
        {
            LockerTubeController newTube = PhotonNetwork.Instantiate(tubePrefab.name, spawnPoints[tubeID].position, spawnPoints[tubeID].rotation).GetComponent<LockerTubeController>();
            newTube.name = "PlayerTube" + (tubeID + 1);
            tubes[tubeID] = newTube;
        }

        LockerTubeController spawnTube = tubes[tubeID]; //Gets the tube associated with the tube ID

        if (spawnTube != null)
        {
            ReadyUpManager.instance.localPlayerTube = spawnTube;

            // Moves the player to the spawn point
            PlayerController.instance.bodyRb.transform.position = spawnTube.spawnPoint.position;
            PlayerController.instance.bodyRb.transform.rotation = spawnTube.spawnPoint.rotation;

            //set own player up
            spawnTube.GiveTubeAPlayer(PlayerController.instance.xrOrigin.transform);
            //move your own player up
            spawnTube.PlayerToLobbyPosition();

            //renders everyone's tubes FOR ME if they're already spawned
            SetExistingTubePositions();

            ReadyUpManager.instance.UpdateStatus(tubeID + 1);
            ReadyUpManager.instance.localPlayerTube.SpawnPlayerName(NetworkManagerScript.instance.GetLocalPlayerName());
            StartCoroutine(NetworkManagerScript.localNetworkPlayer.CheckExclusiveColors());
            if (PhotonNetwork.IsMasterClient)
                ReadyUpManager.instance.localPlayerTube.ShowHostSettings(true); //Show the settings if the player being moved is the master client
        }
    }

    private void SetExistingTubePositions()
    {
        /*
        foreach (var networkPlayer in NetworkPlayer.instances)
        {
            if (networkPlayer != NetworkManagerScript.localNetworkPlayer)
            {
                //tubes[(int)networkPlayer.photonView.Owner.CustomProperties["TubeID"]].StartOtherPlayersTube(networkPlayer.transform.position);
                //tubes[(int)networkPlayer.photonView.Owner.CustomProperties["TubeID"]].transform.position = networkPlayer.transform.position - tubes[(int)networkPlayer.photonView.Owner.CustomProperties["TubeID"]].spawnPointBias;
                tubes[(int)networkPlayer.photonView.Owner.CustomProperties["TubeID"]].GiveTubeAPlayer(networkPlayer.originRig);
            }
        }*/
    }



    /// <summary>
    /// Stoopid.
    /// </summary>
    /// <param name="tubeID">Also stoopid</param>
    public void StartMyTubeForOthersByDavid(int tubeID, Transform networkPlayerTransform)
    {
        tubes[tubeID].GiveTubeAPlayer(networkPlayerTransform);
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