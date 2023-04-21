using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using RootMotion.FinalIK;

// This script was used from https://youtu.be/KHWuTBmT1oI?t=1511\
// ^ Now heavily modified by Invertebrates

/// <summary>
/// Maintains consistency of position and appearance for players over the network. Each player gains one when joining a room and keeps that same one until the leave the room, at which point it is destroyed.
/// </summary>
public class NetworkPlayer : MonoBehaviour
{
    //Classes, Enums & Structs:
    [System.Serializable]
    public class MatChangeEvent
    {
        //Settings:
        /// <summary>
        /// The material this event will cause the player to look like.
        /// </summary>
        public Material targetMat;
        /// <summary>
        /// Higher values mean higher priority.
        /// </summary>
        [Min(0)] public int priority;

        //Runtime Variables:
        public float timeRemaining;

        public MatChangeEvent(Material _targetMat, int _priority, float _duration)
        {
            targetMat = _targetMat;
            priority = _priority;
            timeRemaining = _duration;
        }
    }
    [System.Serializable]
    public class MatChangeCombo
    {
        //Settings:
        public Material inputMatA;
        public Material inputMatB;
        [Space()]
        public Material outputMat;
    }

    //Objects & Components:
    /// <summary>
    /// List of all instantiated network players in the room.
    /// </summary>
    public static List<NetworkPlayer> instances = new List<NetworkPlayer>();

    internal PhotonView photonView;                              //PhotonView network component used by this NetworkPlayer to synchronize movement
    private SkinnedMeshRenderer bodyRenderer;                    //Renderer component for main player body/skin
    private TrailRenderer trail;                                 //Renderer for trail that makes players more visible to each other
    internal PlayerStats networkPlayerStats = new PlayerStats(); //The stats for the network player
    internal Hashtable photonPlayerSettings;
    
    [Header("Material System:")]
    public Material[] altMaterials;
    public MatChangeCombo[] matCombos;

    private Transform headTarget;      //True local position of player head
    private Transform leftHandTarget;  //True local position of player left hand
    private Transform rightHandTarget; //True local position of player right hand
    private Transform modelTarget;     //True local position of player model base
    private Transform headRig;         //Networked transform which follows position of player head
    private Transform leftHandRig;     //Networked transform which follows position of player left hand
    private Transform rightHandRig;    //Networked transform which follows position of player right hand
    private Transform modelRig;        //Networked transform which follows position of player model

    //Runtime Variables:
    /// <summary>
    /// List of all events affecting the material/color of this version of the network player.
    /// </summary>
    public List<MatChangeEvent> matChangeEvents = new List<MatChangeEvent>();

    private bool visible = true; //Whether or not this network player is currently visible
    internal Color currentColor; //Current player color this networkPlayer instance is set to
    private int lastTubeNumber;  //Number of the tube this player was latest spawned at
    internal bool inTube = false;

    private TextMeshProUGUI wormName;
    private Material origTrailMat;

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        instances.Add(this); //Add network player to list of instances in scene

        //Get objects & components:
        photonView = GetComponent<PhotonView>();                      //Get photonView component from local object
        bodyRenderer = GetComponentInChildren<SkinnedMeshRenderer>(); //Get body renderer component from model in children
        trail = GetComponentInChildren<TrailRenderer>();              //Get trail renderer component from children (there should only be one)
        wormName = GetComponentInChildren<TextMeshProUGUI>();

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

            photonPlayerSettings = new Hashtable();                       //Create new custom player settings
            InitializePhotonPlayerSettings();
            PlayerController.instance.playerSetup.ApplyAllSettings();                                //Apply default settings to player
            SyncData();                                                                              //Sync settings between every version of this network player
            foreach (Renderer r in transform.GetComponentsInChildren<Renderer>()) r.enabled = false; //Local player should never be able to see their own NetworkPlayer

            NetworkManagerScript.instance.AdjustVoiceVolume();  //Adjusts the volume of all players

            //Bone evaporator:
            foreach (Rigidbody tailRb in GetComponentsInChildren<Rigidbody>())
            {
                tailRb.isKinematic = true;
                if (tailRb.TryGetComponent(out Collider coll)) Destroy(coll);
            }
        }

        //Runtime variable setup:
        origTrailMat = trail.sharedMaterial; //Get base material for worm trail

        //Event subscriptions:
        SceneManager.sceneLoaded += OnSceneLoaded;     //Subscribe to scene load event (every NetworkPlayer should do this)
        SceneManager.sceneUnloaded += OnSceneUnloaded; //Subscribe to scene unload event (every NetworkPlayer should do this)
        DontDestroyOnLoad(gameObject);                 //Make sure network players are not destroyed when a new scene is loaded
    }

    /// <summary>
    /// Initializes the player's photon player settings.
    /// </summary>
    private void InitializePhotonPlayerSettings()
    {
        photonPlayerSettings.Add("Color", PlayerPrefs.GetInt("PreferredColorOption"));
        photonPlayerSettings.Add("IsReady", false);
        photonPlayerSettings.Add("TubeID", -1);
        PlayerController.photonView.Owner.SetCustomProperties(photonPlayerSettings);
    }

    void Start()
    {
        if (photonView.IsMine) //Initialization for local network player
        {
            RigToActivePlayer();                                                                                                                                  //Rig to active player immediately
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;                                                                        //Client NetworkPlayer is always invisible to them
            GetComponentInChildren<Canvas>().enabled = false;
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
            //Sync player puppet:
            MapPosition(headRig, headTarget);           //Update position of head rig
            MapPosition(leftHandRig, leftHandTarget);   //Update position of left hand rig
            MapPosition(rightHandRig, rightHandTarget); //Update position of right hand rig
            MapPosition(modelRig, modelTarget);         //Update position of base model
        }

        //Sync colors:
        for (int i = 0; i < matChangeEvents.Count;)
        {
            //Update events:
            MatChangeEvent matEvent = matChangeEvents[i];
            matEvent.timeRemaining -= Time.deltaTime;
            if (matEvent.timeRemaining <= 0)
            {
                matChangeEvents.Remove(matEvent); //Kill event
                ReCalculateMaterialEvents();
            }
            else i++; //Move to next event
        }

        //Crungy trail solution:
        if (!photonView.IsMine)
        {
            if (GameManager.Instance.InMenu()) trail.enabled = false;
            else trail.enabled = true;
        }
    }
    private void OnDestroy()
    {
        //photonView.RPC("RPC_TubeVacated", RpcTarget.All, lastTubeNumber);

        //Reference cleanup:
        instances.Remove(this);                                                                                 //Remove from instance list
        if (photonView.IsMine && PlayerController.photonView == photonView) PlayerController.photonView = null; //Clear client photonView reference
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PhotonNetwork.IsConnectedAndReady)  //If the NetworkPlayer is on the network and ready for calls
        {
            if (photonView.IsMine) //Local player has loaded into a new scene
            {
                //Local scene setup:
                RigToActivePlayer(); //Re-apply rig to new scene's PlayerController

                //Local visibility:
                if (scene.name == NetworkManagerScript.instance.mainMenuScene)
                {
                    photonView.RPC("RPC_MakeInvisible", RpcTarget.OthersBuffered);  //Hide all remote players when entering main menu
                }
                else if (scene.name == NetworkManagerScript.instance.roomScene)
                {
                    //PhotonNetwork.AutomaticallySyncScene = true;                    // Start syncing scene with other players
                    photonView.RPC("RPC_MakeVisible", RpcTarget.OthersBuffered);    //Show all remote players when entering locker room
                    UpdateAllRoomSettingsDisplays();
                }
            }
            else
            {
                trail.enabled = !GameManager.Instance.InMenu();               //Disable trail while in menus
                if (scene.name == GameSettings.roomScene) trail.enabled = false; //Super disable trail if in the locker room
            }

            //Generic scene load checks:
            foreach (Collider c in transform.GetComponentsInChildren<Collider>()) c.enabled = !GameManager.Instance.InMenu(); //Always disable colliders if networkPlayer is in a menu scene
        }
        inTube = false;
    }
    public void OnSceneUnloaded(Scene scene)
    {
        matChangeEvents.Clear();
        ReCalculateMaterialEvents();
    }

    //FUNCTIONALITY METHODS:
    public void SetWormNicknameText(string nickName)
    {
        wormName.text = nickName;
    }

    private void ChangeVisibility(bool makeEnabled)
    {
        //Enable/Disable components:
        foreach (Renderer r in transform.GetComponentsInChildren<Renderer>()) r.enabled = makeEnabled; //Update status of each renderer on player avatar
        foreach (Collider c in transform.GetComponentsInChildren<Collider>()) c.enabled = makeEnabled; //Update status of each collider on player avatar
        trail.enabled = makeEnabled;                                                                   //Specifically update status of trailRenderer
        trail.Clear();                                                                                 //Clear trail status

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
    private void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Cleaning up after player " + player);

        photonView.RPC("RPC_TubeVacated", RpcTarget.All, lastTubeNumber);
        //RemoveRPCs(player);
        //DestroyPlayerObjects(player);
    }
    public void SyncStats()
    {
        Debug.Log("Syncing Player Stats...");
        string statsData = PlayerSettingsController.PlayerStatsToString(networkPlayerStats);
        photonView.RPC("LoadPlayerStats", RpcTarget.AllBuffered, statsData);
    }

    public void AddToKillBoard(string killerName, string victimName, DeathCause deathCause)
    {
        Debug.Log("Adding To Kill Board...");
        photonView.RPC("RPC_DeathLog", RpcTarget.AllBuffered, killerName, victimName, (int)deathCause);
    }

    [PunRPC]
    public void RPC_UpdateLeaderboard(string playerName, int deaths, int kills, int streak)
    {
        foreach (var leaderboard in FindObjectsOfType<LeaderboardDisplay>())
            leaderboard.UpdatePlayerStats(playerName, deaths, kills, streak);
    }

    [PunRPC]
    public void RPC_DeathLog(string killerName, string victimName, int deathCause)
    {
        NetworkManagerScript.instance.AddDeathToJumbotron(killerName, victimName, (DeathCause)deathCause);
    }
    public void UpdateRoomSettingsDisplay()
    {
        Debug.Log("Updating Room Settings...");
        photonView.RPC("RPC_UpdateRoomSettings", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_UpdateRoomSettings()
    {
        //Update any instance of the room settings display
        UpdateAllRoomSettingsDisplays();
    }

    public void UpdateAllRoomSettingsDisplays()
    {
        foreach (var display in FindObjectsOfType<RoomSettingsDisplay>())
            display.UpdateRoomSettingsDisplay();
    }

    /// <summary>
    /// Syncs and applies settings data (such as color) between all versions of this network player (only call this on the network player local to the client who's settings you want to use).
    /// </summary>
    public void SyncData()
    {
        Debug.Log("Syncing Player Data...");                                        //Indicate that data is being synced
        string characterData = PlayerSettingsController.Instance.CharDataToString();          //Encode data to a string so that it can be sent over the network
        photonView.RPC("LoadPlayerSettings", RpcTarget.AllBuffered, characterData); //Send data to every player on the network (including this one)
        SyncColors();
    }

    //COLOR MANAGEMENT:
    /// <summary>
    /// Syncs the list of taken colors in the room.
    /// </summary>
    public void SyncColors()
    {
        photonView.RPC("UpdateTakenColors", RpcTarget.All); //Send data to every player on the network (including this one)
    }
    /// <summary>
    /// Manages material event priority for this particular network player.
    /// </summary>
    public void ReCalculateMaterialEvents()
    {
        //Validitiy checks:
        print("Material event recalculation");
        if (photonView.IsMine && PlayerController.instance == null) return;

        //Get highest priority material:
        MatChangeEvent primaryEvent = null;
        foreach (MatChangeEvent matEvent in matChangeEvents)
        {
            if (primaryEvent == null || matEvent.priority > primaryEvent.priority) primaryEvent = matEvent;
        }

        //Check for combos:
        Material primaryMat = altMaterials[0];
        if (primaryEvent != null)
        {
            print("Primary event identified");
            primaryMat = primaryEvent.targetMat;

            //Look for material combos:
            while (true)
            {
                MatChangeCombo activeCombo = null;
                bool materialsCombined = false;
                foreach (MatChangeCombo combo in matCombos)
                {
                    if (combo.inputMatA == primaryMat || combo.inputMatB == primaryMat) { activeCombo = combo; break; }
                }
                if (activeCombo != null)
                {
                    Material otherComboMat = activeCombo.inputMatA == primaryMat ? activeCombo.inputMatB : activeCombo.inputMatA;
                    foreach (MatChangeEvent matEvent in matChangeEvents)
                    {
                        if (matEvent == primaryEvent) continue; //Skip self
                        if (otherComboMat == matEvent.targetMat) //Material is actually being combined
                        {
                            primaryMat = activeCombo.outputMat; //Get combined output material
                            materialsCombined = true;
                        }
                    }
                }
                if (!materialsCombined) break;
            }
        }

        //Set materials:
        SkinnedMeshRenderer targetRenderer = photonView.IsMine ? PlayerController.instance.bodyRenderer : bodyRenderer;
        targetRenderer.material = primaryMat;
        if (primaryMat == altMaterials[0])
        {
            currentColor = PlayerSettingsController.playerColors[(int)photonView.Owner.CustomProperties["Color"]];
            targetRenderer.material.SetColor("_Color", currentColor);
            if (!photonView.IsMine)
            {
                trail.material = origTrailMat;
                trail.colorGradient.colorKeys[0].color = currentColor;
                trail.colorGradient.colorKeys[1].color = currentColor;
            }
        }
        else //System is using unique material
        {
            if (!photonView.IsMine)
            {
                trail.colorGradient.colorKeys[0].color = Color.white;
                trail.colorGradient.colorKeys[1].color = Color.white;
                trail.material = primaryMat;
            }
        }
    }
        /// <summary>
        /// Creates a material event on this network player which is able to change its material properties for a certain amount of time.
        /// </summary>
        /// <param name="targetMatIndex">Index (in altMaterials) of material this event will cause player to have.</param>
        /// <param name="priority">Controls which events this event is able to override.</param>
        /// <param name="duration">How long this event will last for.</param>
        [PunRPC]
    public void StartMaterialEvent(int targetMatIndex, int priority, float duration)
    {
        MatChangeEvent newEvent = new MatChangeEvent(altMaterials[targetMatIndex], priority, duration);
        matChangeEvents.Add(newEvent);
        ReCalculateMaterialEvents();
    }

    /// <summary>
    /// When a user joins, try to take either their color or the next available color.
    /// </summary>
    public IEnumerator UpdateTakenColorsOnJoin()
    {
        yield return new WaitUntil(() => photonView.Owner.CustomProperties["Color"] != null);

        if (ReadyUpManager.instance != null)
        {
            List<int> takenColors = new List<int>();

            bool mustReplaceColor = false;

            for (int i = 0; i < NetworkManagerScript.instance.GetPlayerList().Length; i++)
            {
                Player currentPlayer = NetworkManagerScript.instance.GetPlayerList()[i];

                //If the current player is the owner of this network player, skip them
                if (currentPlayer == photonView.Owner)
                    continue;

                Debug.Log("Checking " + currentPlayer.NickName + "'s Color: " + (ColorOptions)currentPlayer.CustomProperties["Color"]);
                takenColors.Add((int)currentPlayer.CustomProperties["Color"]);

                if ((int)currentPlayer.CustomProperties["Color"] == (int)photonView.Owner.CustomProperties["Color"])
                {
                    Debug.Log((ColorOptions)currentPlayer.CustomProperties["Color"] + " is taken.");
                    mustReplaceColor = true;
                }
            }

            //If the player must replace their color, change their color
            if (mustReplaceColor)
            {
                for (int i = 0; i < PlayerSettingsController.NumberOfPlayerColors(); i++)
                {
                    //If the taken color list does not contain the current color list, take it
                    if (!takenColors.Contains(i))
                    {
                        ReadyUpManager.instance.localPlayerTube.GetComponentInChildren<PlayerColorChanger>().ChangePlayerColor(i);
                        SetNetworkPlayerProperties("Color", i);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sets the custom properties of a PUN player object.
    /// </summary>
    /// <param name="key">The property to change.</param>
    /// <param name="value">The value of the property.</param>
    public void SetNetworkPlayerProperties(string key, object value)
    {
        Hashtable playerProperties = photonView.Owner.CustomProperties;
        playerProperties[key] = value;
        photonView.Owner.SetCustomProperties(playerProperties);
    }

    //REMOTE METHODS:
    [PunRPC]
    public void UpdateTakenColors()
    {
        Debug.Log("Updating Taken Color List...");

        if (ReadyUpManager.instance != null)
        {
            //Refreshes the tubes
            if (FindObjectOfType<LockerTubeSpawner>() != null)
                foreach (var tube in FindObjectOfType<LockerTubeSpawner>().GetTubeList())
                    tube.GetComponentInChildren<PlayerColorChanger>().RefreshButtons();
        }
    }

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
        currentColor = settings.playerColor;                                  //Store color currently being used for player

        //Apply settings:
        SetWormNicknameText(photonView.Owner.NickName);
        foreach (Material mat in bodyRenderer.materials) mat.color = currentColor; //Apply color to entire player body
        trail.colorGradient.colorKeys[0].color = currentColor;
        trail.colorGradient.colorKeys[1].color = currentColor;

        /*for (int x = 0; x < trail.colorGradient.colorKeys.Length; x++) //Iterate through color keys in trail gradient
        {
            if (currentColor == Color.black) trail.colorGradient.colorKeys[x].color = Color.white;
            else trail.colorGradient.colorKeys[x].color = currentColor; //Apply color setting to trail key
        }
        if (currentColor == Color.black) { trail.startColor = Color.white; trail.endColor = Color.white; }
        else { trail.startColor = currentColor; trail.endColor = currentColor; } //Set actual trail colors (just in case)*/
    }

    /// <summary>
    /// Changes the NetworkPlayer's materials.
    /// </summary>
    /// <param name="newMaterial">The new NetworkPlayer materials.</param>
    public void ChangeNetworkPlayerMaterial(Material newMaterial)
    {
        bodyRenderer.material = newMaterial;
        trail.material = newMaterial;
        if (newMaterial == altMaterials[0])
        {
            bodyRenderer.material.SetColor("_Color", PlayerSettingsController.playerColors[(int)photonView.Owner.CustomProperties["Color"]]);
            trail.material.SetColor("_Color", PlayerSettingsController.playerColors[(int)photonView.Owner.CustomProperties["Color"]]);
        }
    }
    public void ChangeNetworkPlayerMaterial(int matIndex)
    {
        ChangeNetworkPlayerMaterial(altMaterials[matIndex]);
    }

    /// <summary>
    /// Indicates that this player has been hit by a networked projectile.
    /// </summary>
    /// <param name="damage">How much damage the projectile dealt.</param>
    /// <param name="enemyID">Identify of player who shot this projectile.</param>
    /// <param name="projectileVel">Speed and direction projectile was moving at/in when it struck this player.</param>
    [PunRPC]
    public void RPC_Hit(int damage, int enemyID, Vector3 projectileVel, int deathCause)
    {
        if (photonView.IsMine)
        {
            //Checks:
            if (PlayerController.instance.isDead) return; //Prevent dead players from being killed
            foreach (PlayerEquipment equipment in PlayerController.instance.attachedEquipment)
            {
                if (equipment.TryGetComponent(out NewChainsawController chainsaw)) //Equipment is a chainsaw
                {
                    if (chainsaw.TryDeflect(projectileVel, "Projectiles/HotteProjectile1")) return; //Do not deal damage if projectile could be deflected
                }
            }

            //Damage & death:
            bool killedPlayer = PlayerController.instance.IsHit(damage); //Inflict damage upon local player
            if (killedPlayer)
            {
                networkPlayerStats.numOfDeaths++;                                               //Increment death counter
                networkPlayerStats.killStreak = 0;                                               //Reset kill streak counter
                networkPlayerStats.deathStreak++;                                               //Increment death streak counter
                PlayerPrefs.SetInt("LifetimeDeaths", PlayerPrefs.GetInt("LifetimeDeaths") + 1); //Add to the lifetime deaths counter 
                if (PlayerPrefs.GetInt("HighestDeathStreak") < networkPlayerStats.deathStreak)
                    PlayerPrefs.SetInt("HighestDeathStreak", networkPlayerStats.deathStreak); //Add to the highest death streak counter if applicable
                PlayerController.instance.combatHUD.UpdatePlayerStats(networkPlayerStats);
                SyncStats();
                AddToKillBoard(PhotonNetwork.GetPhotonView(enemyID).Owner.NickName, PhotonNetwork.LocalPlayer.NickName, (DeathCause)deathCause);
                photonView.RPC("RPC_UpdateLeaderboard", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, networkPlayerStats.numOfDeaths, networkPlayerStats.numOfKills, networkPlayerStats.killStreak);
                if (enemyID != photonView.ViewID) PhotonNetwork.GetPhotonView(enemyID).RPC("RPC_KilledEnemy", RpcTarget.AllBuffered, photonView.ViewID, deathCause);

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
    public void RPC_KilledEnemy(int enemyID, int deathCause)
    {
        if (photonView.IsMine)
        {
            networkPlayerStats.numOfKills++;
            networkPlayerStats.killStreak++;
            networkPlayerStats.deathStreak = 0;
            PlayerPrefs.SetInt("LifetimeKills", PlayerPrefs.GetInt("LifetimeKills") + 1); //Add to the lifetime kills counter 
            if (PlayerPrefs.GetInt("BestStreak") < networkPlayerStats.killStreak)
                PlayerPrefs.SetInt("BestStreak", networkPlayerStats.killStreak); //Add to the best kill streak counter if applicable
            print(PhotonNetwork.LocalPlayer.NickName + " killed enemy with index " + enemyID);
            PlayerController.instance.combatHUD.UpdatePlayerStats(networkPlayerStats);
            SyncStats();
            PlayerController.instance.combatHUD.AddToDeathInfoBoard(PhotonNetwork.LocalPlayer.NickName, PhotonNetwork.GetPhotonView(enemyID).Owner.NickName, (DeathCause)deathCause);
            photonView.RPC("RPC_UpdateLeaderboard", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, networkPlayerStats.numOfDeaths, networkPlayerStats.numOfKills, networkPlayerStats.killStreak);
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
    [PunRPC]
    public void RPC_ChangeMaterial(int materialIndex)
    {
        ChangeNetworkPlayerMaterial(altMaterials[materialIndex]);
    }
    /// <summary>
    /// Makes this player visible to the whole network and enables remote collisions (can also be used to clear their trail).
    /// </summary>
    [PunRPC]
    public void RPC_MakeVisible()
    {
        ChangeVisibility(true); //Show renderers and enable colliders
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
            SpawnManager2.instance.MoveDemoPlayerToSpawnPoint(tubeNumber);
        }
    }
    [PunRPC]
    public void RPC_TubeVacated(int tubeNumber)
    {
        if (SpawnManager2.instance != null)
        {
            LockerTubeController tube = FindObjectOfType<LockerTubeSpawner>().GetTubeByIndex(tubeNumber);
            tube.UpdateLights(false);
            Debug.Log("TestTube" + tubeNumber + " Is Being Vacated...");
        }
        if (ReadyUpManager.instance != null)
        {
            ReadyUpManager.instance.OnLeftRoom();
            ReadyUpManager.instance.UpdateReadyText();
        }
        
    }

    //BELOW METHODS ONLY GET CALLED ON MASTER CLIENT
    [PunRPC]
    public void RPC_GiveMeSpawnpoint(int myViewID)
    {
        if (SpawnManager2.instance != null && !inTube)
        {
            LockerTubeController spawnTube = SpawnManager2.instance.GetEmptyTube();
            if (spawnTube != null)
            {
                Player targetPlayer = PhotonNetwork.GetPhotonView(myViewID).Owner;
                photonView.RPC("RPC_RemoteSpawnPlayer", targetPlayer, spawnTube.GetTubeNumber());
                inTube = true;
            }
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