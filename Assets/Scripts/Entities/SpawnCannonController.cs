using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

public class SpawnCannonController : MonoBehaviour
{
    //Static stuff:
    public static List<SpawnCannonController> sceneSpawns = new List<SpawnCannonController>();
    public const byte UpdateCannonStatusEventCode = 100;

    //Objects & Components:
    private int ID; //The ID number of this spawn cannon (assigned sequentially at runtime)

    //Settings:
    [Header("Settings:")]
    [SerializeField, Tooltip("")] private float spawnWaitTime = 3;
    [SerializeField, Tooltip("Force with which player is launched from cannon when spawning.")]       private float launchForce;
    [SerializeField, Range(0, 90), Tooltip("Maximum angle which player can turn launch capsule to.")] private float maxAngle;
    [SerializeField, Tooltip("Speed at which capsule rotates toward launch target.")]                 private float gimbalLerpRate;
    [SerializeField, Tooltip("Speed at which pedestal rotates toward launch target.")]                private float pedestalLerpRate;
    [Space()]
    [SerializeField, Tooltip("Position player XR origin is locked to while inside spawn cannon.")]                    private Transform playerLockPoint;
    [SerializeField, Tooltip("Rotating pod assembly which points in the direction player's head is facing.")]         private Transform gimbal;
    [SerializeField, Tooltip("Rotating lever assembly which contains levers used by the player to enter the match.")] private Transform pedestal;

    //Runtime Variables:
    [SerializeField] internal NetworkPlayer occupyingPlayer; //Player currently occupying this cannon (null if unoccupied)
    internal float timeUntilReady = 0;      //Time until spawn cannon is ready to fire
    private Quaternion baseGimbalRot;       //Base rotation of gimbal assembly

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialization:
        ID = sceneSpawns.Count; //Get index of this spawn cannon in scene
        sceneSpawns.Add(this);  //Add every spawn cannon in scene to master list

        //Get runtime variables:
        baseGimbalRot = gimbal.rotation; //Get initial rotation of gimbal
    }
    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }
    private void OnDestroy()
    {
        //Cleanup:
        sceneSpawns.Clear(); //Clear scene cannons list (cannons will be destroyed when loading into a new scene)
    }
    private void Update()
    {
        //Point in player direction:
        if (occupyingPlayer != null)
        {
            //Update player position:
            PlayerController.instance.bodyRb.MovePosition(playerLockPoint.position); //Move player to lockpoint position

            //Rotate gimbal:
            Quaternion targetRot = Quaternion.LookRotation(occupyingPlayer.headRig.forward, Vector3.up);     //Get raw head target direction
            targetRot = Quaternion.RotateTowards(baseGimbalRot, targetRot, maxAngle);                        //Cap angle target
            Quaternion newRot = Quaternion.Lerp(gimbal.rotation, targetRot, gimbalLerpRate * Time.deltaTime); //Lerp toward target for smooth motion
            gimbal.rotation = newRot;                                                                        //Move gimbal to new rotation

            //Rotate pedestal:
            targetRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(occupyingPlayer.headRig.forward, Vector3.up), Vector3.up); //Get target head rotation projected onto horizontal plane
            targetRot = Quaternion.RotateTowards(baseGimbalRot, targetRot, maxAngle);                                             //Cap angle target
            newRot = Quaternion.Lerp(pedestal.rotation, targetRot, pedestalLerpRate * Time.deltaTime);                            //Lerp toward angle target
            pedestal.rotation = newRot;                                                                                           //Rotate pedestal to new rotation

            //Launch player:
            if (occupyingPlayer.photonView.IsMine || !PhotonNetwork.IsConnected)
            {
                if (timeUntilReady > 0) timeUntilReady = Mathf.Max(timeUntilReady - Time.deltaTime, 0); //Decrease time counter until it hits zero
                else DeployPlayer();
            }
        }

        /*if (NetworkPlayer.instances.Count > 0)
        {
            if (!PlayerController.instance.bodyRb.isKinematic) PutPlayerInCannon();
            occupyingPlayer = NetworkPlayer.instances[0];
        }*/
    }

    //NETWORKING:
    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code; //Get event code
        if (eventCode == UpdateCannonStatusEventCode) //UPDATE CANNON OCCUPATION STATUS
        {
            object[] data = (object[])photonEvent.CustomData; //Read event data as object array
            if ((int)data[0] == ID) //This tube has been sent the event
            {
                int playerID = (int)data[1]; //Get photonView ID of player occupying this tube

                //BEGIN TUBE OCCUPATION:
                if (playerID != 0) 
                {
                    print("Putting player " + playerID + " in cannon " + ID);
                    occupyingPlayer = PhotonNetwork.GetPhotonView(playerID).GetComponent<NetworkPlayer>(); //Get occupying player
                }
                //END TUBE OCCUPATION:
                else 
                {
                    print("Ejecting player " + occupyingPlayer.photonView.ViewID + " from cannon " + ID);
                    occupyingPlayer = null; //Clear occupying player
                }
            }
        }
    }
    /// <summary>
    /// Sends an event to all versions of this tube which updates its occupation status.
    /// </summary>
    /// <param name="playerID">If set, indicates that given player is occupying this spawnPoint. Otherwise, indicates that this spawnpoint is being vacated.</param>
    private void UpdateCannonStatusEvent(int playerID = 0)
    {
        object[] content = new object[] { ID, playerID };                                                            //Get object array containing identifyer for this spawnpoint and the player occupying it (or vacating it)
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };               //Indicate that this event is getting sent to every connected user
        PhotonNetwork.RaiseEvent(UpdateCannonStatusEventCode, content, raiseEventOptions, SendOptions.SendReliable); //Raise event across network
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Begins spawn sequence for local player using random spawn cannon.
    /// </summary>
    public static void Respawn()
    {
        GetRandomCannon().PutPlayerInCannon(); //Spawn player in a random unoccupied cannon.
    }
    /// <summary>
    /// Retrieves a random spawn cannon (which does not currently contain another player).
    /// </summary>
    public static SpawnCannonController GetRandomCannon()
    {
        //Eliminate occupied points:
        List<SpawnCannonController> validSpawns = sceneSpawns; //Duplicate spawn cannon list so that it can be modified
        for (int x = 0; x < validSpawns.Count;) //Iterate through valid spawnpoint list manually
        {
            if (validSpawns[x].occupyingPlayer != null) validSpawns.RemoveAt(x); //Remove taken spawn points
            else x++;                                                            //Iterate past valid spawn points
        }

        //Return valid spawn:
        if (validSpawns.Count == 0) { Debug.LogError("Spawn system could not find any valid cannons."); return null; } //Throw error if there are no useable spawnpoints
        return validSpawns[Random.Range(0, validSpawns.Count)];                                                        //Return a random valid spawnpoint
    }
    /// <summary>
    /// Loads local player into this spawn cannon.
    /// </summary>
    public void PutPlayerInCannon()
    {
        //if (PhotonNetwork.IsConnected) UpdateCannonStatusEvent(PlayerController.photonView.ViewID); //Update all versions of this spawn cannon to indicate that this player has been loaded into it
        occupyingPlayer = PlayerController.photonView.GetComponent<NetworkPlayer>();
        timeUntilReady = spawnWaitTime;

        //Move player:
        PlayerController.instance.bodyRb.isKinematic = true;                     //Make it so that player cannot move
        PlayerController.instance.bodyRb.MovePosition(playerLockPoint.position); //Move player to lockpoint position
        PlayerController.instance.bodyRb.MoveRotation(playerLockPoint.rotation); //Move player to lockpoint rotation

        //Disable equipment:
        foreach (PlayerEquipment equipment in PlayerController.instance.attachedEquipment) //Iterate through all pieces of equipment attached to the player
        {
            equipment.inputEnabled = false;                //Disable equipment input
            if (!equipment.holstered) equipment.Holster(); //Holster equipment
        }
    }
    /// <summary>
    /// Launches player from this spawn cannon.
    /// </summary>
    public void DeployPlayer()
    {
        //Initialization:
        //if (PhotonNetwork.IsConnected) UpdateCannonStatusEvent(); //Indicate that this cannon is now empty
        occupyingPlayer = null;

        //Launch:
        PlayerController.instance.bodyRb.isKinematic = false;                                       //Enable player movement
        PlayerController.instance.bodyRb.AddForce(gimbal.forward * launchForce, ForceMode.Impulse); //Apply launch force to player

        //Enable equipment:
        foreach (PlayerEquipment equipment in PlayerController.instance.attachedEquipment) //Iterate through all pieces of equipment attached to the player
        {
            equipment.inputEnabled = true;                     //Enable equipment input
            if (equipment.holstered) equipment.Holster(false); //Un-holster equipment
        }

        //Cleanup:
        PlayerController.instance.isDead = false; //Indicate that player is no longer dead
        PlayerController.instance.MakeInvulnerable(PlayerController.instance.healthSettings.spawnInvincibilityTime);
    }
}