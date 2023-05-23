using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using Photon.Voice.Unity;
using System.Linq;

/* Code was referenced from https://www.youtube.com/watch?v=KHWuTBmT1oI
 * https://www.youtube.com/watch?v=zPZK7C5_BQo&list=PLhsVv9Uw1WzjI8fEBjBQpTyXNZ6Yp1ZLw */

/* MonoBehaviourPunCallbacks allows us to override some of the initial functions that are
being called when we are connected to the server, or someone joins the server/room/etc. */

public class NetworkManagerScript : MonoBehaviourPunCallbacks
{
    //Objects & Components:
    public static NetworkManagerScript instance;    //Singleton-ized instance of this script in scene
    public static NetworkPlayer localNetworkPlayer; //Instance of local client's network player in scene

    //Settings:
    [Tooltip("Turn this on to force player to join room as soon as game is loaded.")]   public bool joinRoomOnLoad = false;
    [Tooltip("Name of primary menu scene.")]                                            public string mainMenuScene;
    [Tooltip("Name of primary multiplayer room scene.")]                                public string roomScene;
    [SerializeField, Tooltip("Name of network player prefab in Resources folder.")]     private string networkPlayerName;
    [SerializeField]                                                                    private string readyUpManagerName = "ReadyUpManager";

    [SerializeField] private WordStructure[] wormAdjectives = { new WordStructure("Unfortunate", new int[4]), new WordStructure("Sad", new int[4]), new WordStructure("Despairing", new int[4]), new WordStructure("Grotesque", new int[4]), new WordStructure("Despicable", new int[4]), new WordStructure("Abhorrent", new int[4]), new WordStructure("Regrettable", new int[4]), new WordStructure("Incorrigible", new int[4]), new WordStructure("Greasy", new int[4]), new WordStructure("Platonic", new int[4]), new WordStructure("Sinister", new int[4]), new WordStructure("Hideous", new int[4]), new WordStructure("Glum", new int[4]), new WordStructure("Blasphemous", new int[4]), new WordStructure("Malignant", new int[4]), new WordStructure("Undulating", new int[4]), new WordStructure("Treacherous", new int[4]), new WordStructure("Hostile", new int[4]), new WordStructure("Slimy", new int[4]), new WordStructure("Squirming", new int[4]), new WordStructure("Blubbering", new int[4]), new WordStructure("Twisted", new int[4]), new WordStructure("Manic", new int[4]), new WordStructure("Slippery", new int[4]), new WordStructure("Wet", new int[4]), new WordStructure("Moist", new int[4]), new WordStructure("Lugubrious", new int[4]), new WordStructure("Tubular", new int[4]), new WordStructure("Little", new int[4]), new WordStructure("Erratic", new int[4]), new WordStructure("Pathetic", new int[4]) };
    [SerializeField] private WordStructure[] wormNouns = { new WordStructure("Invertebrate", new int[4]), new WordStructure("Wormlet", new int[4]), new WordStructure("Creature", new int[4]), new WordStructure("Critter", new int[4]), new WordStructure("Fool", new int[4]), new WordStructure("Goon", new int[4]), new WordStructure("Specimen", new int[4]), new WordStructure("Homonculus", new int[4]), new WordStructure("Grubling", new int[4]), new WordStructure("Snotling", new int[4]), new WordStructure("Wormling", new int[4]), new WordStructure("Nightcrawler", new int[4]), new WordStructure("Stinker", new int[4]), new WordStructure("Rapscallion", new int[4]), new WordStructure("Scalliwag", new int[4]), new WordStructure("Beastling", new int[4]), new WordStructure("Crawler", new int[4]), new WordStructure("Larva", new int[4]), new WordStructure("Dingus", new int[4]), new WordStructure("Freak", new int[4]), new WordStructure("Blighter", new int[4]), new WordStructure("Cretin", new int[4]), new WordStructure("Dink", new int[4]), new WordStructure("Unit", new int[4]), new WordStructure("Denizen", new int[4]), new WordStructure("Parasite", new int[4]), new WordStructure("Organism", new int[4]), new WordStructure("Worm", new int[4]), new WordStructure("Oonge", new int[4]), new WordStructure("Bwarp", new int[4]) };
    [SerializeField] private WordStructure[] wormAdjectivesGood;
    [SerializeField] private WordStructure[] wormNounsGood;
    [SerializeField] private WordStructure[] wormAdjectivesBad = { new WordStructure("Guzzling", new int[4]), new WordStructure("Fleshy", new int[4]), new WordStructure("Sopping", new int[4]), new WordStructure("Throbbing", new int[4]), new WordStructure("Promiscuous", new int[4]), new WordStructure("Flaccid", new int[4]), new WordStructure("Erect", new int[4]), new WordStructure("Gaping", new int[4]) };
    [SerializeField] private WordStructure[] wormNounsBad = { new WordStructure("Guzzler", new int[4]), new WordStructure("Pervert", new int[4]), new WordStructure("Fucko", new int[4]), new WordStructure("Pissbaby", new int[4]) };

    [SerializeField, Tooltip("The names for the teams.")] private string[] teamNames;

    List<WordStructure> availableWormAdjectives = new List<WordStructure>();
    List<WordStructure> availableWormNouns = new List<WordStructure>();

    List<WordStructure> totalWormAdjectives = new List<WordStructure>();
    List<WordStructure> totalWormNouns = new List<WordStructure>();

    private bool useFunnyWords;

    private Room mostRecentRoom;

    private bool sceneLoadFailed = false;
    private string currentErrorMessage;

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        if (instance == null) { instance = this; } else Destroy(gameObject); //Singleton-ize this script instance

        //Get objects & components:
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void Start()
    {
        SetNameOnStart();

        // Subscribes event handlers
        PhotonNetwork.AddCallbackTarget(this);

        if (FindObjectOfType<AutoJoinRoom>() != null)
            ConnectAndGiveDavidYourIPAddress(); //Immediately start trying to connect to master server
    }

    private void SetNameOnStart()
    {
        useFunnyWords = PlayerPrefs.GetInt("FunnyWords") == 1;
        SetGlobalWormNameList();        //Generates the total list of potential worm names
        RefreshWormNames();             //Generates the list of available worm names
        //Generates a random nickname on start if the player does not have a saved name
        if (PlayerPrefs.GetInt("WormAdjective", -1) == -1 || PlayerPrefs.GetInt("WormNoun", -1) == -1)
            GenerateRandomNickname(true);
        else
        {
            SetPlayerNickname(totalWormAdjectives[PlayerPrefs.GetInt("WormAdjective")], totalWormNouns[PlayerPrefs.GetInt("WormNoun")]);

            LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

            //If there is a lobby in the scene, update the player name text
            if (lobbyUI != null)
                lobbyUI.UpdateNameText(PlayerPrefs.GetInt("WormAdjective"), PlayerPrefs.GetInt("WormNoun"));
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If we are loaded into the Network Locker scene, and we are the master client
        if (scene.name == GameSettings.roomScene)
        {
            // The master client is only spawning 1 ReadyUpManager.
            if (ReadyUpManager.instance == null && PhotonNetwork.IsMasterClient)
            {
                //PhotonNetwork.Instantiate(readyUpManagerName, Vector3.zero, Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject(readyUpManagerName, Vector3.zero, Quaternion.identity);
            }
        }

        if(scene.name == GameSettings.titleScreenScene)
        {
            if (PlayerPrefs.GetInt("FirstRun") == 0 && !GameSettings.debugMode)
            {
                MovePlayerToCredits();
            }

            SetNameOnStart();
            if (sceneLoadFailed)
            {
                MovePlayerToOnlineErrorMessage();
                sceneLoadFailed = false;
            }

            LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
            //If there is a lobby in the scene, update the room list
            if (lobbyUI != null)
                lobbyUI.UpdateLobbyList(roomDictionary.Values.ToList());
        }
    }

    //NETWORK FUNCTIONS:
    public void ConnectAndGiveDavidYourIPAddress()
    {
        if (!PhotonNetwork.IsConnected) { ConnectToServer(); }
    }
    void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Trying To Connect To Server...");
    }

    public void DisconnectFromServer()
    {
        PhotonNetwork.Disconnect();
        Debug.Log("Disconnecting From Server...");
    }

    public void OnCreateRoom(string roomName, RoomOptions roomOptions = null, Hashtable customRoomSettings = null)
    {

        if (roomOptions == null)
        {
            roomOptions = new RoomOptions();
            roomOptions.IsVisible = true; // The player is able to see the room
        }

        if (customRoomSettings == null)
        {
            customRoomSettings = new Hashtable();
        }

        AddCustomRoomSetting("RoundActive", false, ref customRoomSettings);
        AddCustomRoomSetting("RoundLength", GameSettings.defaultMatchLength, ref customRoomSettings);
        AddCustomRoomSetting("PlayerHP", GameSettings.HPDefault, ref customRoomSettings);
        AddCustomRoomSetting("HazardsActive", GameSettings.hazardsActiveDefault, ref customRoomSettings);
        AddCustomRoomSetting("UpgradesActive", GameSettings.upgradesActiveDefault, ref customRoomSettings);
        AddCustomRoomSetting("UpgradeFrequency", GameSettings.defaultUpgradeFrequency, ref customRoomSettings);
        AddCustomRoomSetting("UpgradeLength", GameSettings.defaultUpgradeLength, ref customRoomSettings);
        AddCustomRoomSetting("TeamMode", GameSettings.teamModeDefault, ref customRoomSettings);
        AddCustomRoomSetting("TubeOccupants", new bool[6] { false, false, false, false, false, false }, ref customRoomSettings);
        AddCustomRoomSetting("TeamNames", GenerateTeamNameList().ToArray(), ref customRoomSettings);

        // Debug.Log("Tube Occupants On Create Room: " + customRoomSettings["TubeOccupants"]);

        roomOptions.IsOpen = true; // The room is open.
        roomOptions.EmptyRoomTtl = 0; // Leave the room open for 0 milliseconds after the room is empty
        roomOptions.MaxPlayers = 6;
        roomOptions.CustomRoomProperties = customRoomSettings;
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    /// <summary>
    /// Generates a list of random team names.
    /// </summary>
    /// <returns>The list of random team names that has a length that equals the number of player colors.</returns>
    public List<string> GenerateTeamNameList()
    {
        //Generates random team names for the room
        List<string> currentTeamNames = new List<string>();
        for (int i = 0; i < PlayerSettingsController.NumberOfPlayerColors(); i++)
        {
            bool validTeamName = false;
            while (!validTeamName)
            {
                Random.InitState(System.DateTime.Now.Millisecond);  //Seeds the randomizer
                string newTeamName = teamNames[Random.Range(0, teamNames.Length)];

                if (!currentTeamNames.Contains(newTeamName))
                {
                    validTeamName = true;
                    currentTeamNames.Add(newTeamName);
                }
            }
        }

        return currentTeamNames;
    }

    public void AddCustomRoomSetting(string name, object value, ref Hashtable roomSettings)
    {
        if (!roomSettings.ContainsKey(name))
            roomSettings.Add(name, value);
        else
            roomSettings[name] = value;
    }

    public void SetMatchActive(bool isMatchActive)
    {
        PhotonNetwork.CurrentRoom.IsOpen = !isMatchActive;
        UpdateRoomSettings("RoundActive", isMatchActive);
    }

    public void JoinRoom(string roomName)
    {
        // Joins the room on the network
        if (!PhotonNetwork.JoinRoom(roomName))
        {
            ResetTitleScene("Join Room Failed. Reason: Room Does Not Exist.");
        }

        mostRecentRoom = PhotonNetwork.CurrentRoom;

        if (PhotonNetwork.InRoom) Debug.Log("Successfully Connected To " + roomName);
    }

    public void LeaveRoom()
    {
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
        //Update room information
        if (lobbyUI != null)
        {
            lobbyUI.UpdateRoomList();
            lobbyUI.ShowMenuState(LobbyMenuState.NICKNAME, true);
        }

        PhotonNetwork.LeaveRoom();
    }

    //NETWORK CALLBACKS:
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Server.");
        base.OnConnectedToMaster();

        // Setting up the lobby
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
        //Update room information
        if (lobbyUI != null)
        {
            lobbyUI.UpdateLoadingScreenMessage("Joining The Lobby...");
        }

        JoinLobby();
    }

    public void JoinLobby()
    {
        // Joins the lobby
        PhotonNetwork.JoinLobby();
    }

    public void LeaveLobby()
    {
        //Leaves the lobby
        PhotonNetwork.LeaveLobby();
    }

    public override void OnJoinedLobby()
    {
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, show the title screen
        if (lobbyUI != null)
        {
            lobbyUI.SwitchMenu(LobbyMenuState.ONLINE);
        }

        Debug.Log("Joined a lobby.");
        base.OnJoinedLobby();

        // Setting up the room options
        if (joinRoomOnLoad && !PhotonNetwork.InRoom)
        {
            if(FindObjectOfType<AutoJoinRoom>() != null)
                OnCreateRoom(FindObjectOfType<AutoJoinRoom>().GetRoomName());
            else
                OnCreateRoom("Dev. Test Room");
        }
    }

    /// <summary>
    /// Refreshes the list of potential worm adjectives and nouns.
    /// </summary>
    public void RefreshWormNames()
    {
        availableWormAdjectives = new List<WordStructure>();
        availableWormNouns = new List<WordStructure>();

        availableWormAdjectives.AddRange(wormAdjectives);
        availableWormNouns.AddRange(wormNouns);

        if (useFunnyWords)
        {
            availableWormAdjectives.AddRange(wormAdjectivesBad);
            availableWormNouns.AddRange(wormNounsBad);
        }
    }

    private void SetGlobalWormNameList()
    {
        totalWormAdjectives = new List<WordStructure>();
        totalWormNouns = new List<WordStructure>();

        totalWormAdjectives.AddRange(wormAdjectives);
        totalWormNouns.AddRange(wormNouns);
        totalWormAdjectives.AddRange(wormAdjectivesBad);
        totalWormNouns.AddRange(wormNounsBad);
    }

    /// <summary>
    /// Generates a random nickname for the player.
    /// </summary>
    /// <param name="setNickname">If true, this sets the name for the player.</param>
    public void GenerateRandomNickname(bool setNickname = false)
    {
        Debug.Log("Generating Random Nickname...");

        RefreshWormNames();
        int adjectiveIndex = GenerateRandomAdjective();
        int nounIndex = GenerateRandomNoun();
        WordStructure currentAdjective = availableWormAdjectives[adjectiveIndex];
        WordStructure currentNoun = availableWormNouns[nounIndex];

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, update the player name text
        if (lobbyUI != null)
            lobbyUI.UpdateNameText(adjectiveIndex, nounIndex);

        //If set nickname is true, set the player nickname in the settings
        if (setNickname)
        {
            PlayerPrefs.SetInt("WormAdjective", adjectiveIndex);
            PlayerPrefs.SetInt("WormNoun", nounIndex);
            SetPlayerNickname(currentAdjective, currentNoun);
        }
    }

    /// <summary>
    /// Generates a random adjective for the player nickname.
    /// </summary>
    /// <returns>A random adjective index.</returns>
    private int GenerateRandomAdjective()
    {
        Random.InitState(System.DateTime.Now.Millisecond);  //Seeds the randomizer
        return Random.Range(0, availableWormAdjectives.Count);
    }

    /// <summary>
    /// Generates a random noun for the player nickname.
    /// </summary>
    /// <returns>A random noun index.</returns>
    private int GenerateRandomNoun()
    {
        Random.InitState(System.DateTime.Now.Millisecond);  //Seeds the randomizer
        return Random.Range(0, availableWormNouns.Count);
    }

    public void UpdateFunnyWords(bool funnyWords)
    {
        useFunnyWords = funnyWords;
        PlayerPrefs.SetInt("FunnyWords", useFunnyWords? 1: 0);
        RefreshWormNames();
    }

    /// <summary>
    /// Adds death information to the jumbotron.
    /// </summary>
    /// <param name="killerName">The killer's user name.</param>
    /// <param name="victimName">The victim's user name.</param>
    public void AddDeathToJumbotron(string killerName, string victimName, DeathCause deathCause)
    {
        foreach(var jumbotron in FindObjectsOfType<Jumbotron>())
            jumbotron.AddToDeathInfoBoard(killerName, victimName, deathCause);

        foreach (var killFeed in FindObjectsOfType<GlobalKillFeedScreen>())
            killFeed.AddToKillFeed(killerName, victimName, deathCause);
    }

    public override void OnCreatedRoom()
    {
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, display room information
        if (lobbyUI != null)
        {
            lobbyUI.SwitchMenu(LobbyMenuState.ROOM);
        }
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Create Room Failed: " + returnCode);

        //base.OnCreateRoomFailed(returnCode, message);
        string errorMessage = "Room Creation Failed: " + message;

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
        //If there is a lobby in the scene, display an error message
        if (lobbyUI != null)
            lobbyUI.UpdateErrorMessage(errorMessage);
    }
    public override void OnJoinedRoom()
    {
        //Automatically load the player into the locker room if the auto join script calls for it
        AutoJoinRoom autoJoin = FindObjectOfType<AutoJoinRoom>();
        if (autoJoin != null && autoJoin.GoToLockerRoom())
            autoJoin.AutoLoadScene(GameSettings.roomScene);

        //Sets the player's values
        SpawnNetworkPlayer();                                             //Always spawn a network player instance when joining a room
        Hashtable photonPlayerSettings = new Hashtable();
        photonPlayerSettings.Add("Color", PlayerPrefs.GetInt("PreferredColorOption"));
        photonPlayerSettings.Add("IsReady", false);
        photonPlayerSettings.Add("TubeID", -1);
        PlayerController.photonView.Owner.SetCustomProperties(photonPlayerSettings);

        //Assigns the player a tube ID
        OccupyNextAvailableTube();

        //Loads the locker room scene when the player joins a room from the title screen. This is so epic can we hit 10 likes
        if (SceneManager.GetActiveScene().name == GameSettings.titleScreenScene)
            GameManager.Instance.LoadGame(GameSettings.roomScene);

        //Update lobby UI:
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
        if (lobbyUI != null) //If there is a lobby in the scene, display room information
        {
            lobbyUI.UpdateRoomList();
            lobbyUI.SwitchMenu(LobbyMenuState.ROOM);
            lobbyUI.ShowMenuState(LobbyMenuState.NICKNAME, false);
        }

        //Cleanup:
        Debug.Log("Joined " + PhotonNetwork.CurrentRoom.Name + " room."); //Indicate that room has been joined
        localNetworkPlayer.SetNetworkPlayerProperties("IsReady", false);;
        AdjustVoiceVolume();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

        Debug.LogError("Join Room Failed. Reason: " + message + ".");
        ResetTitleScene("Join Room Failed. Reason: " + message + ".");
    }

    private void ResetTitleScene(string errorMessage)
    {
        //Reload into the title screen scene for now if failed
        sceneLoadFailed = true;
        Debug.Log("Returning To Main Menu...");
        currentErrorMessage = errorMessage;
        GameManager.Instance.LoadGame(GameSettings.titleScreenScene);
    }

    private void MovePlayerToOnlineErrorMessage()
    {
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
        //Update error information
        if (lobbyUI != null)
        {
            lobbyUI.UpdateErrorMessage(currentErrorMessage);
            lobbyUI.GetPlayerConveyorBelt().TeleportConveyer(2);
        }
    }

    private void MovePlayerToCredits()
    {
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
        if (lobbyUI != null)
            lobbyUI.GetPlayerConveyorBelt().TeleportConveyer(8);

        PlayerPrefs.SetInt("FirstRun", 1);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log(newPlayer.NickName + " has joined.");

        PlayerController.instance.inverteboy.AddToRoomLog(newPlayer.NickName + " Joined The Game.");

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
        //Update room information
        if (lobbyUI != null)
        {
            lobbyUI.UpdateRoomList();
        }

        AdjustVoiceVolume();

        foreach (var host in FindObjectsOfType<PlayerManagementController>())
            host.UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log(otherPlayer.NickName + " has left or disconnected.");

        PlayerController.instance.inverteboy.AddToRoomLog(otherPlayer.NickName + " Left The Game.");

        // Raises an event on player left room.
        PhotonNetwork.RaiseEvent(1, otherPlayer.ActorNumber, RaiseEventOptions.Default, SendOptions.SendReliable);
        localNetworkPlayer.SyncColors();
        localNetworkPlayer.SyncTeams();

        SetTubeOccupantStatus((int)otherPlayer.CustomProperties["TubeID"], false);

        if (ReadyUpManager.instance != null)
            ReadyUpManager.instance.UpdateReadyText();

        foreach (var host in FindObjectsOfType<PlayerManagementController>())
            host.UpdatePlayerList();
    }

    // This method is called when a custom event is received
    public void OnEvent(byte eventCode, object content, int senderId)
    {
        if (eventCode == 1)
        {
            int actorNumber = (int)content;
            // Do something with the actorNumber of the player who left

            // Updates the ReadyUpManager
            if (ReadyUpManager.instance != null)
            {
                ReadyUpManager.instance.UpdateStatus(ReadyUpManager.instance.localPlayerTube.GetTubeNumber());
            }
        }
    }

    public override void OnLeftRoom()
    {
        //Update lobby script:
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
        if (lobbyUI != null)
        {
            lobbyUI.SwitchMenu(LobbyMenuState.ONLINE);
        }

        //Cleanup:
        DeSpawnNetworkPlayer(); //De-spawn local network player whenever player leaves a room
        if (SceneManager.GetActiveScene().name != GameSettings.titleScreenScene)
            PhotonNetwork.LoadLevel(GameSettings.titleScreenScene);
    }

    private Dictionary<string, RoomInfo> roomDictionary = new Dictionary<string, RoomInfo>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //base.OnRoomListUpdate(roomList);

        Debug.Log("Updating Lobby List...");

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, update the room list
        if (lobbyUI != null)
        {
            foreach(var room in roomList)
            {
                if (room.RemovedFromList)
                {
                    if (roomDictionary.ContainsKey(room.Name))
                    {
                        Debug.Log("Removing " + room.Name + " From Lobby List...");
                        roomDictionary.Remove(room.Name);
                    }
                }
                else
                {
                    if (roomDictionary.ContainsKey(room.Name))
                    {
                        Debug.Log("Updating " + room.Name + "In Lobby List...");
                        roomDictionary[room.Name] = room;
                    }
                    else
                    {
                        Debug.Log("Adding " + room.Name + " To Lobby List...");
                        roomDictionary.Add(room.Name, room);
                    }
                }
            }

            lobbyUI.UpdateLobbyList(roomDictionary.Values.ToList());
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from server for reason " + cause.ToString());

        switch (cause)
        {
            case DisconnectCause.DisconnectByServerLogic:
                Debug.Log("You have been kicked from the server.");
                break;
        }

/*        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, go back to the starting menu
        if (lobbyUI != null)
        {
            lobbyUI.SwitchMenu(LobbyMenuState.START);
        }*/
    }

    // When the master client leaves the room, we transfer object ownership to new master client.
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("New Master Client: " + newMasterClient.NickName);

        // Transfer ownership of all objects owned by the old master client to the new master client
        PhotonView[] views = PhotonView.FindObjectsOfType<PhotonView>();
        // photonView components have to be instantiated on the Photon network for ownership to transfer.
        foreach (PhotonView view in views)
        {
            if (view.Owner == PhotonNetwork.MasterClient)
            {
                view.TransferOwnership(newMasterClient);

                // Updates the ReadyUpManager
                if (ReadyUpManager.instance != null)
                {
                    ReadyUpManager.instance.HideTubeHostSettings();
                    ReadyUpManager.instance.UpdateStatus(ReadyUpManager.instance.localPlayerTube.GetTubeNumber());
                    ReadyUpManager.instance.localPlayerTube.ShowHostSettings(true);
                }
            }
        }
    }

    //FUNCTIONALITY METHODS:
    public void UpdateRoomSettings(string key, object value)
    {
        Hashtable currentRoomSettings = PhotonNetwork.CurrentRoom.CustomProperties;
        currentRoomSettings[key] = value;
        PhotonNetwork.CurrentRoom.SetCustomProperties(currentRoomSettings);
    }

    public void SpawnNetworkPlayer()
    {
        if (localNetworkPlayer != null) { Debug.LogError("Tried to spawn a second NetworkPlayer for local client."); return; }              //Abort if player already has a network player
        localNetworkPlayer = PhotonNetwork.Instantiate(networkPlayerName, Vector3.zero, Quaternion.identity).GetComponent<NetworkPlayer>(); //Spawn instance of network player and get reference to its script

        Debug.Log("Actor Number For " + GetLocalPlayerName() + ": " + PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void DeSpawnNetworkPlayer()
    {
        if (localNetworkPlayer != null) PhotonNetwork.Destroy(localNetworkPlayer.gameObject); //Destroy local network player if possible
        localNetworkPlayer = null;                                                            //Remove reference to destroyed reference player
    }

    public void SetPlayerNickname(WordStructure adjective, WordStructure noun, bool playWormSound = false)
    {
        string currentName = adjective.word + " " + noun.word;
        bool duplicateNameExists = true;
        int counter = 2;

        while (duplicateNameExists)
        {
            if (GetPlayerNameList().Contains(currentName))
            {
                currentName = name + " " + counter.ToString();
                counter++;
            }

            else
            {
                duplicateNameExists = false;
            }
        }

        PhotonNetwork.NickName = currentName;
        PlayerSettingsController.Instance.charData.playerAdjective = adjective;
        PlayerSettingsController.Instance.charData.playerNoun = noun;

        if (playWormSound)
        {
            //Plays the sound of the worm's nickname when setting it
        }

        NameAchievementChecker(currentName);
    }

    private void NameAchievementChecker(string currentName)
    {
        switch (currentName)
        {
            case "Moist Hole":
                if (!AchievementListener.Instance.IsAchievementUnlocked(6))
                    AchievementListener.Instance.UnlockAchievement(6);
                break;
            case "The Dink":
                if (!AchievementListener.Instance.IsAchievementUnlocked(23))
                    AchievementListener.Instance.UnlockAchievement(23);
                break;
            case "Pleading For You":
                if (!AchievementListener.Instance.IsAchievementUnlocked(24))
                    AchievementListener.Instance.UnlockAchievement(24);
                break;
            case "Goofy Fondler":
                if (!AchievementListener.Instance.IsAchievementUnlocked(25))
                    AchievementListener.Instance.UnlockAchievement(25);
                break;
            case "Wet Rat":
                if (!AchievementListener.Instance.IsAchievementUnlocked(26))
                    AchievementListener.Instance.UnlockAchievement(26);
                break;
        }
    }

    //UTILITY METHODS:

    /// <summary>
    /// Adjusts the volumes of all speaking players.
    /// </summary>
    public void AdjustVoiceVolume()
    {
        foreach (var speaker in FindObjectsOfType<Speaker>())
            speaker.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("VoiceChatVolume", GameSettings.defaultVoiceSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound);
    }

    public List<string> GetPlayerNameList()
    {
        List<string> playerNameList = new List<string>();

        foreach (var player in GetPlayerList())
        {
            playerNameList.Add(player.NickName);
        }

        return playerNameList;
    }

    public void LoadSceneWithFade(string sceneName, float duration = 0.5f)
    {
        StartCoroutine(FadeLevelRoutine(sceneName, duration));
    }

    private IEnumerator FadeLevelRoutine(string sceneName, float loadDelay)
    {
        GameManager.Instance.levelTransitionActive = true;

        FadeScreen playerScreenFader = PlayerController.instance.GetComponentInChildren<FadeScreen>();
        playerScreenFader.FadeOut();

        if(PlayerController.instance.hudScreen.activeInHierarchy)
            PlayerController.instance.HideHUD(0.5f);

        yield return new WaitForSeconds(playerScreenFader.GetFadeDuration());
        yield return new WaitForSeconds(loadDelay);

        PhotonNetwork.LoadLevel(sceneName);

        // Unready
        if(localNetworkPlayer != null)
            localNetworkPlayer.SetNetworkPlayerProperties("IsReady", false);

        GameManager.Instance.levelTransitionActive = false;
    }

    public List<WordStructure> GetTotalWormAdjectives() => totalWormAdjectives;
    public List<WordStructure> GetTotalWormNouns() => totalWormNouns;
    public List<WordStructure> GetAvailableWormAdjectives() => availableWormAdjectives;
    public List<WordStructure> GetAvailableWormNouns() => availableWormNouns;
    public bool IsUsingFunnyWords() => useFunnyWords;

    public Room GetMostRecentRoom() => mostRecentRoom;
    public string GetCurrentRoom() => PhotonNetwork.CurrentRoom.Name;
    public Player[] GetPlayerList() => PhotonNetwork.PlayerList;

    public int GetPlayerIndexFromList()
    {
        for(int i = 0; i < GetPlayerList().Length; i++)
        {
            if (localNetworkPlayer.photonView.Owner == GetPlayerList()[i])
                return i;
        }

        return -1;
    }

    public string GetLocalPlayerName() => PhotonNetwork.LocalPlayer.NickName;
    public bool IsLocalPlayerInRoom() => PhotonNetwork.InRoom;

    public bool[] GetTubeOccupancy()
    {
        if (PhotonNetwork.InRoom)
            return (bool[])(PhotonNetwork.CurrentRoom.CustomProperties["TubeOccupants"]);
        return null;
    }

    public void DebugDisplayRoomOccupancy()
    {
        if (GameSettings.debugMode)
        {
            for (int i = 0; i < GetTubeOccupancy().Length; i++)
                Debug.Log("Tube " + (i + 1) + ": " + (GetTubeOccupancy()[i] ? "Occupied" : "Vacant").ToString());
        }
    }

    public void SetTubeOccupantStatus(int tubeID, bool isOccupied)
    {
        bool[] tubeList = GetTubeOccupancy();
        tubeList[tubeID] = isOccupied;
        UpdateRoomSettings("TubeOccupants", tubeList);
    }

    public void OccupyNextAvailableTube()
    {
        for (int i = 0; i < GetTubeOccupancy().Length; i++)
        {
            if (!GetTubeOccupancy()[i])
            {
                if(GameSettings.debugMode)
                    Debug.Log("Assigning " + GetLocalPlayerName() + " to Tube #" + (i + 1).ToString());

                localNetworkPlayer.SetNetworkPlayerProperties("TubeID", i);
                SetTubeOccupantStatus(i, true);
                break;
            }
        }
    }

    // Is called when the button is pressed for TDM.
    public void TeamDeathMatch()
    {
        UpdateRoomSettings("TeamMode", true);

        // TO DO: Set teamColor player stats to the current color you have
    }

    // // Is called when the button is pressed for FFA.
    public void FreeForAll()
    {
        UpdateRoomSettings("TeamMode", false);
    }

    // We want to switch this from just 2 teams to having multiple colors for teams.
    public void SwitchTeam()
    {
        if (localNetworkPlayer.GetNetworkPlayerStats().teamColor == "")
        {
            
        }
    }

    public string[] GetTeamNameList() => teamNames;
}