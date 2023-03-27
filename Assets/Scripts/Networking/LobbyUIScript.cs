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

    [SerializeField, Tooltip("The list of menus in the lobby.")] private GameObject[] menus;
    private GameObject currentMenu;

    [SerializeField, Tooltip("The loading screen message.")] private TextMeshProUGUI loadingScreenMessage;

    [SerializeField, Tooltip("The text object that shows the player's nickname.")] private TextMeshProUGUI playerNameText;
    [SerializeField, Tooltip("The text object that shows when the player successfully sets their nickname.")] private TextMeshProUGUI nameSetSuccessText;
    [SerializeField, Tooltip("The number of seconds for the name success animation to play for.")] private float nameSetSuccessAnimationPlayDuration;
    [SerializeField, Tooltip("The number of seconds for the name success animation to pause for when shown.")] private float nameSetSuccessAnimationPauseDuration;
    [SerializeField, Tooltip("The ease type for the success animation.")] private LeanTweenType nameSetSuccessEaseType;

    [SerializeField, Tooltip("The text object that displays the reason for an error.")] TextMeshProUGUI errorText;

    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject playerListItemPrefab;

    [Header("Individual Menu Managers")]
    [SerializeField, Tooltip("The Create Room controller.")] private CreateRoomController createRoom;

    //Runtime Variables
    private List<string> playerList = new List<string>();
    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();

    private WordStructure currentDisplayedAdjective, currentDisplayedNoun;

    private void Start()
    {
        currentMenu = menus[(int)LobbyMenuState.START];     //Sets the first menu as the starting menu

        //Ensures that the first menu is always the start menu
        if (menus[(int)LobbyMenuState.START])
            SwitchMenu(LobbyMenuState.START);
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
        menus[(int)menu].SetActive(showState);
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
        prevMenu.SetActive(false);
        currentMenu.SetActive(true);
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
    /// <param name="nameText">The new name for the player.</param>
    public void UpdateNameText(WordStructure currentAdjective, WordStructure currentNoun)
    {
        playerNameText.text = currentAdjective.word + " " + currentNoun.word;

        //Saving the objects locally so that the player can choose to set this nickname
        currentDisplayedAdjective = currentAdjective;
        currentDisplayedNoun = currentNoun;
    }

    /// <summary>
    /// Generates a random nickname for the player.
    /// </summary>
    public void GenerateRandomNickname()
    {
        NetworkManagerScript.instance.GenerateRandomNickname();
    }

    /// <summary>
    /// Sets the nickname of the player.
    /// </summary>
    public void SetNickname()
    {
        NetworkManagerScript.instance.SetPlayerNickname(currentDisplayedAdjective, currentDisplayedNoun);
        PlayNameSetSuccessAnimation();
    }

    /// <summary>
    /// Plays an animation with the name success text.
    /// </summary>
    private void PlayNameSetSuccessAnimation()
    {
        LeanTween.alphaCanvas(nameSetSuccessText.GetComponent<CanvasGroup>(), 1f, nameSetSuccessAnimationPlayDuration).setEase(nameSetSuccessEaseType).setOnComplete(() => LeanTween.delayedCall(nameSetSuccessAnimationPauseDuration, PlayNameSetSuccessExitAnimation));
    }

    /// <summary>
    /// Plays an exit animation for the name success text.
    /// </summary>
    private void PlayNameSetSuccessExitAnimation()
    {
        LeanTween.alphaCanvas(nameSetSuccessText.GetComponent<CanvasGroup>(), 0f, nameSetSuccessAnimationPlayDuration).setEase(nameSetSuccessEaseType);
    }

    /// <summary>
    /// Adds letters to the room name box.
    /// </summary>
    /// <param name="segment">The segment of letter(s) to add.</param>
    public void AddLetterToNameBox(string segment)
    {
/*        if (roomNameInPutField.text.Length < roomNameLength)
            roomNameInPutField.text += segment;
        else
            Debug.Log("Room Name Too Long.");*/
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
        NetworkManagerScript.instance.OnCreateRoom(createRoom.GenerateRoomCode(), createRoom.GetRoomOptions(), createRoom.GetCustomRoomSettings());
    }
    public void JoinRoom(string roomName)
    {
        //OpenMenu("loading");
        NetworkManagerScript.instance.JoinRoom(roomName);
    }

    public void LeaveRoom()
    {
        NetworkManagerScript.instance.LeaveRoom();
    }

    public void UpdateLobbyList(List<RoomInfo> roomListInfo)
    {
        // We clear the list every time we update.
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        if (GameSettings.debugMode)
        {
            // Loops through the list of dummy rooms.
            for (int i = 0; i < 10; i++)
            {
                Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().UpdateText("Dummy Room " + (i + 1));
            }
        }

        // Loops through the list of rooms.
        for (int i = 0; i < roomListInfo.Count; i++)
        {
            if (IsValidRoom(roomListInfo[i]))
            {
                // Adds the rooms to the list of rooms.
                Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomListInfo[i]);
            }
        }

        //Refresh the find room menu's list of rooms
        FindRoomController findRoomController = FindObjectOfType<FindRoomController>();

        if (findRoomController != null)
            findRoomController.RefreshRoomListItems();
    }

    private bool IsValidRoom(RoomInfo room)
    {
        return (room.PlayerCount > 0 && room.PlayerCount < room.MaxPlayers);
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
}