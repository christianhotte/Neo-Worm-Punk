using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

// Code was used from https://youtu.be/zPZK7C5_BQo?t=781

public enum LobbyMenuState { LOADING, NICKNAME, START, TUTORIALS, ONLINE, HOST, PUBLIC, PRIVATE, ROOM, ERROR}

public class LobbyUIScript : MonoBehaviour
{

    [SerializeField] private ConveyerController playerConveyorController;
    [SerializeField, Tooltip("The list of menus in the lobby.")] private GameObject[] menus;
    private GameObject currentMenu;

    [SerializeField, Tooltip("The loading screen message.")] private TextMeshProUGUI loadingScreenMessage;

    [SerializeField, Tooltip("The text object that shows the player's nickname.")] private TextMeshProUGUI playerNameText;
    [SerializeField, Tooltip("The text object that shows whether explicit words are used or not.")] private TextMeshProUGUI explicitWordsText;

    [SerializeField, Tooltip("The text object that displays the reason for an error.")] TextMeshProUGUI errorText;

    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject playerListItemPrefab;

    [Header("Individual Menu Managers")]
    [SerializeField, Tooltip("The Create Room controller.")] private CreateRoomController createRoom;
    [SerializeField, Tooltip("The Find Room controller.")] private FindRoomController findRoom;

    //Runtime Variables
    private List<string> playerList = new List<string>();
    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();

    private int currentAdjective, currentNoun;

    private string roomToConnectTo;

    private void Start()
    {
        currentMenu = menus[(int)LobbyMenuState.START];     //Sets the first menu as the starting menu

        //Ensures that the first menu is always the start menu
        if (menus[(int)LobbyMenuState.START])
            SwitchMenu(LobbyMenuState.START);

        UpdateFunnyText();
    }

    #region MenuStates
    /// <summary>
    /// Switches the menu state, hiding the current menu state and showing the new menu state.
    /// </summary>
    /// <param name="menu">The new menu state.</param>
    public void SwitchMenu(LobbyMenuState menu)
    {
        OpenMenu(menu);
    }

    /// <summary>
    /// Switches the menu state, hiding the current menu state and showing the new menu state.
    /// </summary>
    /// <param name="menu">The new menu state.</param>
    public void SwitchMenu(int menu)
    {
        OpenMenu((LobbyMenuState)menu);
    }

    /// <summary>
    /// Shows or hides a menu state without altering the current menu state open.
    /// </summary>
    /// <param name="menu">The menu state to open.</param>
    /// <param name="showState">If true, the menu state is showing. If false, the menu state is not showing.</param>
    public void ShowMenuState(LobbyMenuState menu, bool showState)
    {
        //menus[(int)menu].SetActive(showState);
    }

    /// <summary>
    /// Makes a menu state active.
    /// </summary>
    /// <param name="menu">The new menu state.</param>
    /// <param name="switchMenu">If true, hide the current menu state and show the new menu state.</param>
    private void OpenMenu(LobbyMenuState menu)
    {
        GameObject newMenu = menus[(int)menu];
        GameObject prevMenu = currentMenu;

        currentMenu = newMenu;
        //prevMenu.SetActive(false);
        //currentMenu.SetActive(true);
    }
    #endregion

    #region LoadingScreen
    /// <summary>
    /// Opens the loading screen.
    /// </summary>
    /// <param name="loadingMessage">The initial message for the loading screen.</param>
    public void OpenLoadingScreen(string loadingMessage = "Loading...")
    {
        loadingScreenMessage.text = loadingMessage;
        SwitchMenu(LobbyMenuState.LOADING);
    }

    /// <summary>
    /// Changes the loading menu screen message.
    /// </summary>
    /// <param name="newMessage">The new loading menu message.</param>
    public void UpdateLoadingScreenMessage(string newMessage)
    {
        loadingScreenMessage.text = newMessage;
    }
    #endregion

    #region ServerConnection
    public void StartOnlinePlay()
    {
        OpenLoadingScreen("Connecting To Server...");
        NetworkManagerScript.instance.ConnectAndGiveDavidYourIPAddress();
    }

    public void ExitOnlinePlay()
    {
        OpenLoadingScreen("Disconnecting...");
        NetworkManagerScript.instance.DisconnectFromServer();
    }
    #endregion

    /// <summary>
    /// Updates the player name text.
    /// </summary>
    /// <param name="adjective">The index of the new adjective.</param>
    /// <param name="noun">The index of the new noun.</param>
    public void UpdateNameText(int adjective, int noun)
    {
        playerNameText.text = NetworkManagerScript.instance.GetTotalWormAdjectives()[adjective].word + " " + NetworkManagerScript.instance.GetTotalWormNouns()[noun].word;

        //Saving the objects locally so that the player can choose to change this nickname
        currentAdjective = adjective;
        currentNoun = noun;
    }

    public void IncrementAdjective(int increment)
    {
        currentAdjective += increment;

        if (currentAdjective >= NetworkManagerScript.instance.GetAvailableWormAdjectives().Count)
            currentAdjective = 0;
        if (currentAdjective < 0)
            currentAdjective = NetworkManagerScript.instance.GetAvailableWormAdjectives().Count - 1;

        PlayerPrefs.SetInt("WormAdjective", currentAdjective);
        NetworkManagerScript.instance.SetPlayerNickname(NetworkManagerScript.instance.GetTotalWormAdjectives()[currentAdjective], NetworkManagerScript.instance.GetTotalWormNouns()[PlayerPrefs.GetInt("WormNoun")]);
        UpdateNameText(currentAdjective, currentNoun);
    }

    public void IncrementNoun(int increment)
    {
        currentNoun += increment;

        if (currentNoun >= NetworkManagerScript.instance.GetAvailableWormNouns().Count)
            currentNoun = 0;
        if (currentNoun < 0)
            currentNoun = NetworkManagerScript.instance.GetAvailableWormNouns().Count - 1;

        PlayerPrefs.SetInt("WormNoun", currentNoun);

        Debug.Log("Worm Adjective: " + PlayerPrefs.GetInt("WormAdjective"));
        Debug.Log("Worm Noun: " + PlayerPrefs.GetInt("WormNoun"));

        NetworkManagerScript.instance.SetPlayerNickname(NetworkManagerScript.instance.GetTotalWormAdjectives()[PlayerPrefs.GetInt("WormAdjective")], NetworkManagerScript.instance.GetTotalWormNouns()[currentNoun]);
        UpdateNameText(currentAdjective, currentNoun);
    }

    /// <summary>
    /// Generates a random nickname for the player.
    /// </summary>
    public void GenerateRandomNickname()
    {
        NetworkManagerScript.instance.GenerateRandomNickname(true);
    }

    /// <summary>
    /// Toggles the use of funny words.
    /// </summary>
    public void ToggleFunnyWords()
    {
        NetworkManagerScript.instance.UpdateFunnyWords(!NetworkManagerScript.instance.IsUsingFunnyWords());
        UpdateFunnyText();

        if (NetworkManagerScript.instance.IsUsingFunnyWords())
        {
            if (!AchievementListener.Instance.IsAchievementUnlocked(5))
                AchievementListener.Instance.UnlockAchievement(5);
        }
    }

    private void UpdateFunnyText()
    {
        explicitWordsText.text = "Explicit Words: " + (PlayerPrefs.GetInt("FunnyWords") == 1 ? "On" : "Off");
    }

    // Displays the error message to the player.
    public void UpdateErrorMessage(string errorMessage)
    {
        errorText.text = errorMessage;
    }

    public void CreateRoom()
    {
        //OpenMenu("loading"); // Opens the loading screen
        // Creates a room with the name of what the player has typed in.
        OpenLoadingScreen("Creating Room...");
        NetworkManagerScript.instance.OnCreateRoom(createRoom.GenerateRoomCode(), createRoom.GetRoomOptions(), createRoom.GetCustomRoomSettings());
    }
    public void JoinRoom()
    {
        //OpenMenu("loading");
        OpenLoadingScreen("Joining Room...");
        NetworkManagerScript.instance.JoinRoom(roomToConnectTo);
    }

    public void SetRoomToConnectTo(string roomName)
    {
        roomToConnectTo = roomName;
    }

    public string GetRoomToConnectTo() => roomToConnectTo;

    public void LeaveRoom()
    {
        NetworkManagerScript.instance.LeaveRoom();
    }

    public void UpdateLobbyList(List<RoomInfo> roomListInfo)
    {
        if (findRoom != null)
        {
            // We clear the list every time we update.
            foreach (Transform trans in roomListContent)
                Destroy(trans.gameObject);

            int roomCount = 0;

            /*if (GameSettings.debugMode)
            {
                // Loops through the list of dummy rooms.
                for (int i = 0; i < 10; i++)
                {
                    Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().UpdateText("Dummy Room " + (i + 1));
                }
            }*/

            // Loops through the list of rooms.
            for (int i = 0; i < roomListInfo.Count; i++)
            {
                Debug.Log("Room Found: " + roomListInfo[i].Name);
                Debug.Log("Room Details: \nPlayers: " + roomListInfo[i].PlayerCount + " / " + roomListInfo[i].MaxPlayers + "\nIs Open: " + roomListInfo[i].IsOpen + "\nIs Visible: " + roomListInfo[i].IsVisible);

                if (IsValidRoom(roomListInfo[i]))
                {
                    // Adds the rooms to the list of rooms.
                    GameObject newRoom = Instantiate(roomListItemPrefab, roomListContent);
                    newRoom.GetComponent<RoomListItem>().SetUp(roomListInfo[i]);
                    newRoom.name = newRoom.GetComponent<RoomListItem>().GetRoomListInfo().Name;
                    newRoom.GetComponent<RectTransform>().anchoredPosition = new Vector3(newRoom.GetComponent<RectTransform>().anchoredPosition.x, findRoom.GetArrowYPos() + (findRoom.GetArrowYPos() * roomCount));
                    roomCount++;
                }
            }

            findRoom.RefreshRoomListItems(roomCount);
        }
        else
        {
            Debug.Log("No Find Room System Found.");
        }
    }

    public void AddDummyRoom()
    {
        //Refresh the find room menu's list of rooms
        FindRoomController findRoomController = FindObjectOfType<FindRoomController>();

        if (findRoomController != null)
        {
            // Adds the rooms to the list of rooms.
            GameObject newRoom = Instantiate(roomListItemPrefab, roomListContent);
            newRoom.name = "Dummy Room";
            newRoom.GetComponent<RectTransform>().anchoredPosition = new Vector3(newRoom.GetComponent<RectTransform>().anchoredPosition.x, findRoomController.GetArrowYPos() + (findRoomController.GetArrowYPos() * (roomListContent.childCount - 1)));

            findRoomController.RefreshRoomListItems();
        }
    }

    private bool IsValidRoom(RoomInfo room)
    {
        return (room.PlayerCount > 0 && room.PlayerCount < room.MaxPlayers && room.IsOpen);
    }

    public void OpenRoomList()
    {
        UpdateRoomList();
        //OpenMenu("room");
    }

    public void UpdateRoomList()
    {
        // Opens the room menu UI
        playerList = NetworkManagerScript.instance.GetPlayerNameList();
        print("Players in room list: " + playerList.Count);

        //Destroy the list before updating
        foreach (Transform trans in playerListContent)
        {
            Destroy(trans.gameObject);
        }

        playerListItems.Clear();

        // Loops through the list of players and adds to the list of players in the room.
        for (int i = 0; i < playerList.Count; i++)
        {
            //If the player does not exist in the list, add them to the list
            if (!DoesPlayerExist(playerList[i]))
            {
                PlayerListItem newItem = Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>();
                newItem.SetUp(playerList[i]);
                playerListItems.Add(newItem);
            }
        }
    }

    // Returns a bool if the player with the name exists
    private bool DoesPlayerExist(string name)
    {
        foreach (var player in playerListItems)
            if (name == player.GetName())
                return true;

        return false;
    }

    // Loads into the shotgun tutorial scene.
    public void ShotGunTutorial()
    {
        PhotonNetwork.LoadLevel(3);
    }

    // Loads into the chainsaw tutorial scene.
    public void ChainsawTutorial()
    {
        PhotonNetwork.LoadLevel(4);
    }

    public ConveyerController GetPlayerConveyorBelt() => playerConveyorController;
}