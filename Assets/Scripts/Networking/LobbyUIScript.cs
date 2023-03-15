using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

// Code was used from https://youtu.be/zPZK7C5_BQo?t=781

public class LobbyUIScript : MonoBehaviour
{
    [SerializeField] TMP_InputField roomNameInPutField;
    [SerializeField] TMP_InputField wormNameInputField;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_Text errorText;

    [SerializeField] private int roomNameLength = 5;

    [SerializeField] private GameObject goToLevelButton;
    [SerializeField] private GameObject tutorialMenu;

    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject playerListItemPrefab;

    [SerializeField] Menus[] menus;

    private List<string> playerList = new List<string>();
    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();

    // Easier to call the  open menu method through script
    public void OpenMenu(string menuName)
    {
        // Loops through all the menus in the Canvas.
        for (int i = 0; i < menus.Length; i++)
        {
            // If the menu matches with the menu name we're trying to open...
            if (menus[i].menuName == menuName)
            {
                // Then we can open the menu
                OpenMenu(menus[i]);
            }

            // If it's not the menu we're trying to open, then we want to close it.
            else if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }

    /// <summary>
    /// Adds letters to the room name box.
    /// </summary>
    /// <param name="segment">The segment of letter(s) to add.</param>
    public void AddLetterToNameBox(string segment)
    {
        if (roomNameInPutField.text.Length < roomNameLength)
            roomNameInPutField.text += segment;
        else
            Debug.Log("Room Name Too Long.");
    }

    public void AddSectionToWormName(string segment)
    {
        wormNameInputField.text += segment;
    }

    /// <summary>
    /// Clears the name box text.
    /// </summary>
    public void ClearInputBox(TMP_InputField inputField)
    {
        inputField.text = "";
    }

    // Displays the error message to the player.
    public void UpdateErrorMessage(string errorMessage)
    {
        errorText.text = errorMessage;
    }

    // Easier to call the open menu method through hierarchy.
    public void OpenMenu(Menus menu)
    {
        // If the menu is open, we want to close it because we only want one menu open at a time.
        for (int i = 0; i < menus.Length; i++)
        {
            CloseMenu(menus[i]);
        }

        // Opens the menu.
        menu.Open();
    }

    // Closes the menu (easier through hierarchy).
    public void CloseMenu(Menus menu)
    {
        menu.Close();
    }

    public void CreateRoom()
    {
        // Doesn't allow an empty room name.
        if (string.IsNullOrEmpty(roomNameInPutField.text))
        {
            return;
        }

        OpenMenu("loading"); // Opens the loading screen
        // Creates a room with the name of what the player has typed in.
        NetworkManagerScript.instance.OnCreateRoom(roomNameInPutField.text);
    }
    public void CreateName()
    {
        // Doesn't allow an empty worm name.
        if (string.IsNullOrEmpty(wormNameInputField.text))
        {
            return;
        }

        // Creates a nickname with the name of what the player has typed in.
        NetworkManagerScript.instance.SetPlayerNickname(wormNameInputField.text);
        OpenMenu("title"); // Opens the title screen
    }
    public void JoinRoom(string roomName)
    {
        OpenMenu("loading");
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
        OpenMenu("room");
    }

    public void ShowLaunchButton(bool showButton)
    {
        goToLevelButton.SetActive(showButton);
    }

    public void UpdateRoomList()
    {
        // Opens the room menu UI
        roomNameText.text = NetworkManagerScript.instance.GetCurrentRoom();
        playerNameText.text = NetworkManagerScript.instance.GetLocalPlayerName();

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