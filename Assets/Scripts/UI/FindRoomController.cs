using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FindRoomController : MonoBehaviour
{
    [SerializeField] private GameObject roomSelector;
    [SerializeField] private RectTransform arrowObject;
    [SerializeField] private RectTransform scrollArea;
    [SerializeField] private TextMeshProUGUI noRoomsText;
    [SerializeField] private GameObject connectButton;

    [SerializeField] private bool debugAddDummyRoom;

    private float menuItemGlobalHeight;
    private RoomListItem selectedRoom;
    private RoomListItem[] listedRooms;

    private int roomCount;

    private void OnEnable()
    {
        RefreshRoomListItems();
        UpdateMenu();
    }

    private void SelectFirstRoomByDefault()
    {
        if (listedRooms.Length > 0)
        {
            selectedRoom = listedRooms[0];
            selectedRoom.OnSelect();
        }
    }

    /// <summary>
    /// Changes the scroll area's position.
    /// </summary>
    /// <param name="sliderPos">The position of the slider.</param>
    public void IncrementRoomList(int increment)
    {
        float maximumYPos = Mathf.Abs(GetArrowYPos()) * (listedRooms.Length - 1);

        Vector3 scrollLocalPos = scrollArea.anchoredPosition;
        scrollLocalPos.y += (GetArrowYPos() * increment);
        scrollLocalPos.y = Mathf.Clamp(scrollLocalPos.y, 0, maximumYPos);
        scrollArea.anchoredPosition = scrollLocalPos;

        UpdateMenu();
    }

    /// <summary>
    /// Updates the menu and selects an active room.
    /// </summary>
    private void UpdateMenu()
    {
        if (roomCount > 0)
            ShowRoomList(true);
        else
        {
            ShowRoomList(false);
            return;
        }

        if(roomCount < 2)
            SelectFirstRoomByDefault();

        int roomIndex = (int)(scrollArea.anchoredPosition.y / Mathf.Abs(GetArrowYPos()));

        if (selectedRoom != null)
            selectedRoom.OnDeselect();

        selectedRoom = listedRooms[roomIndex];
        selectedRoom.OnSelect();
    }

    private void ShowRoomList(bool showRoomList)
    {
        scrollArea.gameObject.SetActive(showRoomList);
        arrowObject.gameObject.SetActive(showRoomList);
        connectButton.SetActive(showRoomList);
        noRoomsText.gameObject.SetActive(!showRoomList);
        roomSelector.SetActive(showRoomList);
    }

    private void Update()
    {
        if (debugAddDummyRoom)
        {
            debugAddDummyRoom = false;
            FindObjectOfType<LobbyUIScript>().AddDummyRoom();
        }
    }

    /// <summary>
    /// Connects to the selected room.
    /// </summary>
    public void ConnectToRoom()
    {
        NetworkManagerScript.instance.JoinRoom(FindObjectOfType<LobbyUIScript>().GetRoomToConnectTo());
    }

    public void SetRoomToConnectTo()
    {
        if (selectedRoom != null)
            FindObjectOfType<LobbyUIScript>().SetRoomToConnectTo(selectedRoom.GetRoomListInfo().Name);
    }

    /// <summary>
    /// Refreshes the list of rooms.
    /// </summary>
    public void RefreshRoomListItems(int newRoomCount = -1)
    {
        if (newRoomCount != -1)
            roomCount = newRoomCount;

        listedRooms = scrollArea.GetComponentsInChildren<RoomListItem>();
        scrollArea.anchoredPosition = Vector3.zero;
        UpdateMenu();
    }


    public float GetArrowYPos() => arrowObject.anchoredPosition.y;
}
