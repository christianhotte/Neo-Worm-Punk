using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using RootMotion.FinalIK;

// This script was used from https://youtu.be/KHWuTBmT1oI?t=1511\
// ^ Now heavily modified by Invertebrates

/// <summary>
/// Maintains consistency of position and appearance for players over the network. Each player gains one when joining a room and keeps that same one until the leave the room, at which point it is destroyed.
/// </summary>
public class NetworkPlayer : MonoBehaviour
{
    //Objects & Components:
    /// <summary>
    /// List of all instantiated network players in the room.
    /// </summary>
    public static List<NetworkPlayer> instances = new List<NetworkPlayer>();

    internal PhotonView photonView;                              //PhotonView network component used by this NetworkPlayer to synchronize movement
    private SkinnedMeshRenderer bodyRenderer;                    //Renderer component for main player body/skin
    private TrailRenderer trail;                                 //Renderer for trail that makes players more visible to each other
    internal PlayerStats networkPlayerStats = new PlayerStats(); //The stats for the network player

    private Transform headTarget;      //True local position of player head
    private Transform leftHandTarget;  //True local position of player left hand
    private Transform rightHandTarget; //True local position of player right hand
    private Transform modelTarget;     //True local position of player model base
    private Transform headRig;         //Networked transform which follows position of player head
    private Transform leftHandRig;     //Networked transform which follows position of player left hand
    private Transform rightHandRig;    //Networked transform which follows position of player right hand
    private Transform modelRig;        //Networked transform which follows position of player model

    //Runtime Variables:
    private bool visible = true; //Whether or not this network player is currently visible
    internal Color currentColor; //Current player color this networkPlayer instance is set to
    private int lastTubeNumber;  //Number of the tube this player was latest spawned at

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        instances.Add(this); //Add network player to list of instances in scene

        //Get objects & components:
        photonView = GetComponent<PhotonView>();                      //Get photonView component from local object
        bodyRenderer = GetComponentInChildren<SkinnedMeshRenderer>(); //Get body renderer component from model in children
        trail = GetComponentInChildren<TrailRenderer>();              //Get trail renderer component from children (there should only be one)

        //Set up rig:
        foreach (PhotonTransformView view in GetComponentsInChildren<PhotonTransformView>()) //Iterate through each network-tracked component
        {
            //Check for rig labels:
            if (view.name.Contains("Head")) { headRig = view.transform; continue; }           //Get head rig
            if (view.name.Contains("Left")) { leftHandRig = view.transform; continue; }       //Get left hand rig
            if (view.name.Contains("Right")) { rightHandRig = view.transform; continue; }     //Get right hand rig
            if (view.TryGetComponent(out VRIK vrik)) { modelRig = view.transform; continue; } //Get model rig
        }
        if (headRig == null || leftHandRig == null || rightHandRig == null) { Debug.LogError("Network Player " + name + " was not able to successfully get its rigged components. Have the names of its children been changed?"); }

        //Alternate setup modes:
        if (photonView.IsMine) //This script is the master instance of this particular NetworkPlayer
        {
            //Object & component setup:
            PlayerController.photonView = photonView; //Give playerController a reference to local client photon view component

            //Local initialization:
            PlayerController.instance.playerSetup.ApplyAllSettings();                                //Apply default settings to player
            SyncData();                                                                              //Sync settings between every version of this network player
            foreach (Renderer r in transform.GetComponentsInChildren<Renderer>()) r.enabled = false; //Local player should never be able to see their own NetworkPlayer
        }

        //Event subscriptions:
        SceneManager.sceneLoaded += OnSceneLoaded; //Subscribe to scene load event (every NetworkPlayer should do this)
        DontDestroyOnLoad(gameObject);             //Make sure network players are not destroyed when a new scene is loaded
    }
    void Start()
    {
        if (photonView.IsMine) //Initialization for local network player
        {
            RigToActivePlayer();                                                                                                                                  //Rig to active player immediately
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;                                                                        //Client NetworkPlayer is always invisible to them
            trail.enabled = false;                                                                                                                                //Disable local player trail
            if (SceneManager.GetActiveScene().name == NetworkManagerScript.instance.mainMenuScene) photonView.RPC("RPC_MakeInvisible", RpcTarget.OthersBuffered); //Remote instances are hidden while client is in the main menu
        }

        //Component activity check:
        foreach (Collider collider in GetComponentsInChildren<Collider>()) //Iterate through each collider in this network player
        {
            if (photonView.IsMine) //Client-exclusive collision operations
            {
                foreach (Collider otherCollider in PlayerController.instance.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(collider, otherCollider); //Make sure player can't collide with its own network player
            }
            collider.enabled = !GameManager.Instance.InMenu(); //Disable colliders altogether if network player is in any kind of menu
        }
        if (!photonView.IsMine) { if (GameManager.Instance.InMenu()) trail.enabled = false; } //Disable trail in menu scenes
    }
    void Update()
    {
        //Synchronize position:
        if (photonView.IsMine) //Client network player has references to actual targets
        {
            MapPosition(headRig, headTarget);           //Update position of head rig
            MapPosition(leftHandRig, leftHandTarget);   //Update position of left hand rig
            MapPosition(rightHandRig, rightHandTarget); //Update position of right hand rig
            MapPosition(modelRig, modelTarget);         //Update position of base model
        }
    }
    private void OnDestroy()
    {
        //Reference cleanup:
        instances.Remove(this);                                                                                 //Remove from instance list
        if (photonView.IsMine && PlayerController.photonView == photonView) PlayerController.photonView = null; //Clear client photonView reference
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (photonView.IsMine) //Local player has loaded into a new scene
        {
            //Local scene setup:
            RigToActivePlayer(); //Re-apply rig to new scene's PlayerController

            if (scene.name == NetworkManagerScript.instance.mainMenuScene) 
            { 
                photonView.RPC("RPC_MakeInvisible", RpcTarget.OthersBuffered);  //Hide all remote players when entering main menu
            }
            else if (scene.name == NetworkManagerScript.instance.roomScene) 
            {
                PhotonNetwork.AutomaticallySyncScene = true;                    // Start syncing scene with other players
                photonView.RPC("RPC_MakeVisible", RpcTarget.OthersBuffered);    //Show all remote players when entering locker room
            }
        }
        else
        {
            trail.enabled = !GameManager.Instance.InMenu();               //Disable trail while in menus
            if (scene.name == "NetworkLockerRoom") trail.enabled = false; //Super disable trail if in the locker room
        }

        //Generic scene load checks:
        foreach (Collider c in transform.GetComponentsInChildren<Collider>()) c.enabled = !GameManager.Instance.InMenu(); //Always disable colliders if networkPlayer is in a menu scene
        
    }

    //FUNCTIONALITY METHODS:
    private void ChangeVisibility(bool makeEnabled)
    {
        //Enable/Disable components:
        foreach (Renderer r in transform.GetComponentsInChildren<Renderer>()) r.enabled = makeEnabled; //Update status of each renderer on player avatar
        foreach (Collider c in transform.GetComponentsInChildren<Collider>()) c.enabled = makeEnabled; //Update status of each collider on player avatar

        //Cleanup:
        visible = makeEnabled; //Indicate whether or not networkPlayer is currently visible
    }
    /// <summary>
    /// Sets up networkPlayer avatar to mimic movements of current PlayerController (and XR Origin) instance in scene.
    /// </summary>
    private void RigToActivePlayer()
    {
        //Component setup:
        PlayerController attachedPlayer = PlayerController.instance; //Get reference to playerController script from current scene

        //Setup rig targets:
        headTarget = attachedPlayer.cam.transform;            //Get head from player using camera component reference
        leftHandTarget = attachedPlayer.leftHand.transform;   //Get left hand from player script (since it has already automatically collected the reference)
        rightHandTarget = attachedPlayer.rightHand.transform; //Get right hand from player script (since it has already automatically collected the reference)
        modelTarget = attachedPlayer.bodyRig.transform;       //Get base model transform from player script
    }

    public void SyncStats()
    {
        Debug.Log("Syncing Player Stats...");
        string statsData = PlayerSettingsController.PlayerStatsToString(networkPlayerStats);
        photonView.RPC("LoadPlayerStats", RpcTarget.AllBuffered, statsData);
    }

    public void AddToKillBoard(string killerName, string victimName)
    {
        Debug.Log("Adding To Kill Board...");
        photonView.RPC("RPC_DeathLog", RpcTarget.AllBuffered, killerName, victimName);
    }

    [PunRPC]
    public void RPC_DeathLog(string killerName, string victimName)
    {
        PlayerController.instance.combatHUD.AddToDeathInfoBoard(killerName, victimName);
    }

    /// <summary>
    /// Syncs and applies settings data (such as color) between all versions of this network player (only call this on the network player local to the client who's settings you want to use).
    /// </summary>
    public void SyncData()
    {
        Debug.Log("Syncing Player Data...");                                        //Indicate that data is being synced
        string characterData = PlayerSettingsController.Instance.CharDataToString();          //Encode data to a string so that it can be sent over the network
        photonView.RPC("LoadPlayerSettings", RpcTarget.AllBuffered, characterData); //Send data to every player on the network (including this one)
    }
    public void LeftRoom()
    {
        if (SpawnManager2.instance != null)
        {
            photonView.RPC("RPC_TubeVacated", RpcTarget.MasterClient, lastTubeNumber);
        }
    }

    //REMOTE METHODS:
    [PunRPC]
    public void LoadPlayerStats(string data)
    {
        //Initialization:
        Debug.Log("Applying Synced Stats...");                           //Indicate that message has been received
        PlayerStats stats = JsonUtility.FromJson<PlayerStats>(data);    //Decode stats into PlayerStats object
        networkPlayerStats = stats;
    }

    /// <summary>
    /// Loads given settings (as CharacterData) and applies them to this network player instance.
    /// </summary>
    [PunRPC]
    public void LoadPlayerSettings(string data)
    {
        //Initialization:
        Debug.Log("Applying Synced Settings...");                           //Indicate that message has been received
        CharacterData settings = JsonUtility.FromJson<CharacterData>(data); //Decode settings into CharacterData object
        currentColor = settings.testColor;                                  //Store color currently being used for player

        //Apply settings:
        foreach (Material mat in bodyRenderer.materials) mat.color = currentColor; //Apply color to entire player body
        for (int x = 0; x < trail.colorGradient.colorKeys.Length; x++) //Iterate through color keys in trail gradient
        {
            if (currentColor == Color.black) trail.colorGradient.colorKeys[x].color = Color.white;
            else trail.colorGradient.colorKeys[x].color = currentColor; //Apply color setting to trail key
        }
        if (currentColor == Color.black) { trail.startColor = Color.white; trail.endColor = Color.white; }
        else { trail.startColor = currentColor; trail.endColor = currentColor; } //Set actual trail colors (just in case)
    }

    /// <summary>
    /// Indicates that this player has been hit by a networked projectile.
    /// </summary>
    /// <param name="damage">How much damage the projectile dealt.</param>
    [PunRPC]
    public void RPC_Hit(int damage, int enemyID)
    {
        if (photonView.IsMine)
        {
            bool killedPlayer = PlayerController.instance.IsHit(damage); //Inflict damage upon local player
            if (killedPlayer)
            {
                networkPlayerStats.numOfDeaths++;                                                                      //Increment death counter
                PlayerController.instance.combatHUD.UpdatePlayerStats(networkPlayerStats);
                SyncStats();
                AddToKillBoard(PhotonNetwork.GetPhotonView(enemyID).Owner.NickName, PhotonNetwork.LocalPlayer.NickName);
                PhotonNetwork.GetPhotonView(enemyID).RPC("RPC_KilledEnemy", RpcTarget.AllBuffered, photonView.ViewID);
            }
        }
    }

    /// <summary>
    /// Launches this player with given amount of force.
    /// </summary>
    [PunRPC]
    public void RPC_Launch(Vector3 force)
    {
        if (photonView.IsMine) PlayerController.instance.bodyRb.AddForce(force, ForceMode.VelocityChange); //Apply launch force to client rigidbody
    }

    /// <summary>
    /// Indicates that this player has successfully hit an enemy with a projecile.
    /// </summary>
    [PunRPC]
    public void RPC_HitEnemy()
    {
        if (photonView.IsMine) PlayerController.instance.HitEnemy(); //Indicate that local player has hit an enemy
    }

    /// <summary>
    /// Indicates that this player has successfully killed an enemy.
    /// </summary>
    /// <param name="enemyID"></param>
    [PunRPC]
    public void RPC_KilledEnemy(int enemyID)
    {
        if (photonView.IsMine)
        {
            networkPlayerStats.numOfKills++;
            print(PhotonNetwork.LocalPlayer.NickName + " killed enemy with index " + enemyID);
            PlayerController.instance.combatHUD.UpdatePlayerStats(networkPlayerStats);
            SyncStats();
        }
    }

    /// <summary>
    /// Moves client player to designated position.
    /// </summary>
    /// <param name="point"></param>
    [PunRPC]
    public void RPC_MovePlayerToPoint(Vector3 point)
    {
        if (photonView.IsMine) PlayerController.instance.bodyRb.transform.position = point; //Move player rigidbody origin to given point
    }
    [PunRPC]
    public void RPC_ChangeVisibility()
    {
        print("why"); //This should never be called
    }
    /// <summary>
    /// Makes this player visible to the whole network and enables remote collisions (can also be used to clear their trail).
    /// </summary>
    [PunRPC]
    public void RPC_MakeVisible()
    {
        ChangeVisibility(true); //Show renderers and enable colliders
        trail.Clear();          //Clean up trail
    }
    /// <summary>
    /// Hides this player's renderers and disables all remote collision detection.
    /// </summary>
    [PunRPC]
    public void RPC_MakeInvisible()
    {
        ChangeVisibility(false); //Hide renderers and disable colliders
    }
    /// <summary>
    /// Connects player to given target and makes them move toward each other. Overrides previous target's tether state if valid. Pass again to same player to un-tether them.
    /// </summary>
    [PunRPC]
    public void RPC_Tether(int targetId)
    {

    }

    [PunRPC]
    public void RPC_RemoteSpawnPlayer(int tubeNumber)
    {
        lastTubeNumber = tubeNumber;
        if (SpawnManager2.instance != null)
        {
            LockerTubeController spawnTube = LockerTubeController.GetTubeByNumber(tubeNumber);
            spawnTube.occupied = true;
            PlayerController.instance.bodyRb.transform.position = spawnTube.spawnPoint.position;
            PlayerController.instance.bodyRb.transform.rotation = spawnTube.spawnPoint.rotation;
        }
    }

    //BELOW METHODS ONLY GET CALLED ON MASTER CLIENT
    [PunRPC]
    public void RPC_GiveMeSpawnpoint(int myViewID)
    {
        if (SpawnManager2.instance != null)
        {
            LockerTubeController spawnTube = SpawnManager2.instance.GetEmptyTube();
            if (spawnTube != null)
            {
                spawnTube.occupied = true;
                Player targetPlayer = PhotonNetwork.GetPhotonView(myViewID).Owner;
                photonView.RPC("RPC_RemoteSpawnPlayer", targetPlayer, spawnTube.tubeNumber);
            }
        }
    }
    [PunRPC]
    public void RPC_TubeVacated(int tubeNumber)
    {
        if (SpawnManager2.instance != null)
        {
            LockerTubeController tube = LockerTubeController.GetTubeByNumber(tubeNumber);
            tube.occupied = false;
        }
    }

    //UTILITY METHODS:
    /// <summary>
    /// Matches the target's position and orientation to those of the reference.
    /// </summary>
    private void MapPosition(Transform target, Transform reference)
    {
        target.position = reference.position; //Map position
        target.rotation = reference.rotation; //Map orientation
    }

    public PlayerStats GetNetworkPlayerStats() => networkPlayerStats;
    public string GetName() => photonView.Owner.NickName;
}