using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

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
    [SerializeField, Tooltip("Allow use of some of the worse words in our vocabulary")] private bool useFunnyWords;

    private Room mostRecentRoom;

    internal List<int> takenColors = new List<int>();

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
        ConnectAndGiveDavidYourIPAddress(); //Immediately start trying to connect to master server
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If we are loaded into the Network Locker scene, and we are the master client
        if (scene.name == roomScene)
        {
            // The master client is only spawning 1 ReadyUpManager.
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Instantiate(readyUpManagerName, Vector3.zero, Quaternion.identity);
            }
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
            customRoomSettings.Add("RoundLength", 300);
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
            lobbyUI.ShowLaunchButton(false);
        }

        PhotonNetwork.LeaveRoom();
    }

    //NETWORK CALLBACKS:
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Server.");
        base.OnConnectedToMaster();

        // Setting up the lobby
        if (joinRoomOnLoad && !PhotonNetwork.InRoom)
        {
            JoinLobby();
        }
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
            lobbyUI.OpenMenu("title");
        }

        Debug.Log("Joined a lobby.");
        base.OnJoinedLobby();

        GenerateRandomNickname();

        // Setting up the room options
        if (joinRoomOnLoad && !PhotonNetwork.InRoom)
        {
            if(FindObjectOfType<AutoJoinRoom>() != null)
                OnCreateRoom(FindObjectOfType<AutoJoinRoom>().GetRoomName());
            else
                OnCreateRoom("Dev. Test Room");
        }
    }

    //List of random adjectives and nouns to name random players
    private readonly string[] wormAdjectives = { "Unfortunate", "Sad", "Despairing", "Grotesque", "Despicable", "Abhorrent", "Regrettable", "Incorrigible", "Greasy", "Platonic", "Sinister", "Hideous", "Glum", "Blasphemous", "Malignant", "Undulating", "Treacherous", "Hostile", "Slimy", "Squirming", "Blubbering", "Twisted", "Manic", "Slippery", "Wet", "Moist", "Lugubrious", "Tubular", "Little", "Erratic", "Pathetic" };
    private readonly string[] wormNouns = { "Invertebrate", "Wormlet", "Creature", "Critter", "Fool", "Goon", "Specimen", "Homonculus", "Grubling", "Snotling", "Wormling", "Nightcrawler", "Stinker", "Rapscallion", "Scalliwag", "Beastling", "Crawler", "Larva", "Dingus", "Freak", "Blighter", "Cretin", "Dink", "Unit", "Denizen", "Parasite", "Organism" };
    private readonly string[] wormAdjectivesBad = { "Guzzling", "Fleshy", "Sopping", "Throbbing", "Promiscuous", "Flaccid", "Erect", "Gaping" };
    private readonly string[] wormNounsBad = { "Guzzler", "Pervert", "Fucko", "Pissbaby" };

    /// <summary>
    /// Generates a random nickname for the player.
    /// </summary>
    private void GenerateRandomNickname()
    {
        Random.InitState(System.DateTime.Now.Millisecond);  //Seeds the randomizer
        List<string> realWormAdjectives = new List<string>(wormAdjectives);
        List<string> realWormNouns = new List<string>(wormNouns);
        if (useFunnyWords)
        {
            realWormAdjectives.AddRange(wormAdjectivesBad);
            realWormNouns.AddRange(wormNounsBad);
        }

        string currentWormName = realWormAdjectives[Random.Range(0, realWormAdjectives.Count)] + " " + realWormNouns[Random.Range(0, realWormNouns.Count)];
        SetPlayerNickname(currentWormName);
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
            lobbyUI.OpenMenu("room");
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
            lobbyUI.OpenMenu("error");
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
            lobbyUI.OpenMenu("room");
            lobbyUI.ShowLaunchButton(true);
        }

        //Cleanup:
        Debug.Log("Joined " + PhotonNetwork.CurrentRoom.Name + " room."); //Indicate that room has been joined
        SpawnNetworkPlayer();                                             //Always spawn a network player instance when joining a room
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join Room Failed. Reason: " + message);

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, display an error message
        if (lobbyUI != null)
        {
            lobbyUI.UpdateErrorMessage("Join Room Failed. Reason: " + message);
            lobbyUI.OpenMenu("error");
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

    public override void OnLeftRoom()
    {
        //Update lobby script:
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
        if (lobbyUI != null)
        {
            lobbyUI.OpenMenu("title");
        }

        //Cleanup:
        DeSpawnNetworkPlayer(); //De-spawn local network player whenever player leaves a room
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
        //Remove the player's color from the list of colors
        RemoveColor((int)PlayerSettingsController.ColorToColorOptions(PlayerSettingsController.Instance.charData.playerColor));
        localNetworkPlayer.SyncColors();

        if (localNetworkPlayer != null) PhotonNetwork.Destroy(localNetworkPlayer.gameObject); //Destroy local network player if possible
        localNetworkPlayer = null;                                                            //Remove reference to destroyed reference player
    }

    public void SetPlayerNickname(string name)
    {
        string currentName = name;
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
        PlayerSettingsController.Instance.charData.playerName = PhotonNetwork.NickName;
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
        FadeScreen playerScreenFader = PlayerController.instance.GetComponentInChildren<FadeScreen>();
        playerScreenFader.FadeOut();

        yield return new WaitForSeconds(playerScreenFader.GetFadeDuration());
        yield return null;

        PhotonNetwork.LoadLevel(sceneName);
    }

    public bool TryToTakeColor(ColorOptions currentColor)
    {
        if (!ColorTaken((int)currentColor))
        {
            TakeColor((int)currentColor);
            return true;
        }

        return false;
    }

    public void UpdateTakenColorList(ColorOptions currentColor, ColorOptions newTakenColor)
    {
        if (ColorTaken((int)currentColor))
            RemoveColor((int)currentColor);

        TakeColor((int)newTakenColor);
    }

    public void TakeColor(int colorOption) => takenColors.Add(colorOption);
    public void RemoveColor(int colorOption) => takenColors.Remove(colorOption);
    public bool ColorTaken(int colorOption) => takenColors.Contains(colorOption);

    public Room GetMostRecentRoom() => mostRecentRoom;
    public string GetCurrentRoom() => PhotonNetwork.CurrentRoom.Name;
    public Player[] GetPlayerList() => PhotonNetwork.PlayerList;
    public string GetLocalPlayerName() => PhotonNetwork.LocalPlayer.NickName;
    public bool IsLocalPlayerInRoom() => PhotonNetwork.InRoom;
}