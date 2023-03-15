using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

// Script was used from https://youtu.be/KHWuTBmT1oI?t=1186

/// <summary>
/// DEPRECATED. All functionality should now be implemented in NetworkManagerScript.
/// </summary>
public class NetworkPlayerSpawn : MonoBehaviourPunCallbacks
{
    //Objects & Components:
    public static NetworkPlayerSpawn instance; //Singleton instance of this script in scene
    
    private NetworkPlayer clientNetworkPlayer; //Instance of local client's network player in scene
    //[SerializeField] private string networkSceneName = "NetworkLockerRoom";

    //Settings:
    [Header("Resource References:")]
    [SerializeField, Tooltip("Exact name of network player prefab in Resources folder.")] private string networkPlayerName;

    private void Awake()
    {
        //Initialization:
        if (instance == null) { instance = this; } else { Debug.LogError("Tried to load two NetworkPlayerSpawn scripts in the same scene!"); Destroy(this); } //Singleton-ize this script

        //init = FindObjectOfType<GameManager>().gameObject;
        
        // If it's the main menu scene, then we are throwing the DemoPlayer into the DontDestroyOnLoad
        SceneManager.sceneLoaded += OnSceneLoaded; // Subscribes to event manager
    }

    // Unsubscribes to scene event manager
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Spawns the network player once the scene has loaded in.
    public void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
    {
        // Checks for debugging if you have debugging checked off. Throw a AutoJoinRoom script on something.
        //StartCoroutine(CheckForDebugging());

        // Spawns the network player in the tube scene.
        /*if (loadedScene.name == networkSceneName)
        {
            SpawnNetworkPlayer();
        }*/
    }

    // When someone joins a room, we spawn the player.
    public override void OnJoinedRoom()
    {
        //Initialization:
        base.OnJoinedRoom();

        // The network players should never spawn in the main menu
        Scene scene = SceneManager.GetActiveScene();

        SpawnNetworkPlayer();

        /*if (scene.name == mainMenuScene)
        {
            return;
        }

        // Spawns network players when you join a room on any other scene besides the main menu.
        else
        {
            SpawnNetworkPlayer();
        }*/
    }

    // When someone leaves a room, we want to remove the player from the game.
    public override void OnLeftRoom()
    {
        Debug.Log("A player has left the room.");
        base.OnLeftRoom();
        DeSpawnNetworkPlayer();
    }

    // Spawns the Network Player.
    public void SpawnNetworkPlayer()
    {
        //Spawn network player:
        clientNetworkPlayer = PhotonNetwork.Instantiate(networkPlayerName, Vector3.zero, Quaternion.identity).GetComponent<NetworkPlayer>(); //Spawn instance of network player and get reference to its script
        if (clientNetworkPlayer == null) Debug.LogError("Tried to spawn network player prefab that doesn't have NetworkPlayer component!");  //Indicate problem if relevant
        /*else
            clientNetworkPlayer.transform.SetParent(init.transform);*/
    }
    public void DeSpawnNetworkPlayer()
    {
        if (clientNetworkPlayer != null) PhotonNetwork.Destroy(clientNetworkPlayer.gameObject);
    }

    // If we want to play without having to start from the Main Menu scene...
    /*IEnumerator CheckForDebugging()
    {
        // Waits for the Network Manager Script to check for joinRoomOnLoad
        while (!PhotonNetwork.InRoom) yield return null; //Wait until system is connected to network

        // If we are debugging, then we can just test without having to start from the main menu.
        if (NetworkManagerScript.instance.joinRoomOnLoad == true)
        {
            // We don't spawn an extra Network Player in the tube scene
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name == networkSceneName)
            {
                // Do nothing
            }

            // We never spawn a Network player in the Main Menu
            else if (scene.name == mainMenuScene)
            {
                // Do nothing
            }

            // Spawns a Network Player in other people's scenes.
            else
            {
                SpawnNetworkPlayer();
            }
        }
    }*/
}