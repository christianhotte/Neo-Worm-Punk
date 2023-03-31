using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;

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

    List<WordStructure> availableWormAdjectives = new List<WordStructure>();
    List<WordStructure> availableWormNouns = new List<WordStructure>();

    List<WordStructure> totalWormAdjectives = new List<WordStructure>();
    List<WordStructure> totalWormNouns = new List<WordStructure>();

    private bool useFunnyWords;

    private Room mostRecentRoom;

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        if (instance == null) { instance = this; } else Destroy(gameObject); //Singleton-ize this script instance

        SetNameOnStart();

        //Get objects & components:
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void Start()
    {
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
            SetNameOnStart();
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
        if(roomOptions == null)
        {
            roomOptions = new RoomOptions();
            roomOptions.IsVisible = true; // The player is able to see the room
        }

        if (customRoomSettings == null)
        {
            customRoomSettings = new Hashtable();
            customRoomSettings.Add("RoundLength", GameSettings.testMatchLength);
            customRoomSettings.Add("PlayerHP", GameSettings.HPDefault);
        }

        roomOptions.IsOpen = true; // The room is open.
        roomOptions.EmptyRoomTtl = 0; // Leave the room open for 0 milliseconds after the room is empty
        roomOptions.MaxPlayers = 6;
        roomOptions.CustomRoomProperties = customRoomSettings;
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }
    public void JoinRoom(string roomName)
    {
        // Joins the room on the network
        PhotonNetwork.JoinRoom(roomName);

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
    public void AddDeathToJumbotron(string killerName, string victimName)
    {
        foreach(var jumbotron in FindObjectsOfType<Jumbotron>())
            jumbotron.AddToDeathInfoBoard(killerName, victimName);
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
        {
            lobbyUI.UpdateErrorMessage(errorMessage);
            lobbyUI.SwitchMenu(LobbyMenuState.ERROR);
        }
    }
    public override void OnJoinedRoom()
    {
        //Automatically load the player into the locker room if the auto join script calls for it
        AutoJoinRoom autoJoin = FindObjectOfType<AutoJoinRoom>();
        if (autoJoin != null && autoJoin.GoToLockerRoom())
            autoJoin.AutoLoadScene(GameSettings.roomScene);

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
        SpawnNetworkPlayer();                                             //Always spawn a network player instance when joining a room
        localNetworkPlayer.SetNetworkPlayerProperties("IsReady", false);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join Room Failed. Reason: " + message);

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, display an error message
        if (lobbyUI != null)
        {
            lobbyUI.UpdateErrorMessage("Join Room Failed. Reason: " + message);
            lobbyUI.SwitchMenu(LobbyMenuState.ERROR);
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log(newPlayer.NickName + " has joined.");

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //Update room information
        if (lobbyUI != null)
        {
            lobbyUI.UpdateRoomList();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log(otherPlayer.NickName + " has left or disconnected.");

        // Raises an event on player left room.
        PhotonNetwork.RaiseEvent(1, otherPlayer.ActorNumber, RaiseEventOptions.Default, SendOptions.SendReliable);
        localNetworkPlayer.SyncColors();
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

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //base.OnRoomListUpdate(roomList);

        Debug.Log("Updating Lobby List...");

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, update the room list
        if (lobbyUI != null)
        {
            lobbyUI.UpdateLobbyList(roomList);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from server for reason " + cause.ToString());

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, go back to the starting menu
        if (lobbyUI != null)
        {
            lobbyUI.SwitchMenu(LobbyMenuState.START);
        }
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

        Debug.Log("Current Name: " + currentName);
        Debug.Log("Photon Name: " + PhotonNetwork.NickName);

        if (playWormSound)
        {
            //Plays the sound of the worm's nickname when setting it
        }
    }

    //UTILITY METHODS:
    public List<string> GetPlayerNameList()
    {
        List<string> playerNameList = new List<string>();

        foreach (var player in GetPlayerList())
        {
            playerNameList.Add(player.NickName);
        }

        return playerNameList;
    }

    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeLevelRoutine(sceneName));
    }

    private IEnumerator FadeLevelRoutine(string sceneName)
    {
        GameManager.Instance.levelTransitionActive = true;

        FadeScreen playerScreenFader = PlayerController.instance.GetComponentInChildren<FadeScreen>();
        playerScreenFader.FadeOut();

        yield return new WaitForSeconds(playerScreenFader.GetFadeDuration());
        yield return null;

        PhotonNetwork.LoadLevel(sceneName);

        // Unready
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
    public string GetLocalPlayerName() => PhotonNetwork.LocalPlayer.NickName;
    public bool IsLocalPlayerInRoom() => PhotonNetwork.InRoom;
}