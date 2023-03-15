using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

// Code was used from https://www.youtube.com/watch?v=KGzMxalSqQE&list=PLhsVv9Uw1WzjI8fEBjBQpTyXNZ6Yp1ZLw&index=2

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private RoomInfo currentRoomInfo;
    [SerializeField] private Image listImage;
    [SerializeField] private Color selectedRoomColor = new Color(1, 0, 0, 1);
    private Color defaultColor;

    private void Start()
    {
        defaultColor = Color.white;
    }

    // Sets the text to the name of the room
    public void SetUp(RoomInfo roomInfo)
    {
        currentRoomInfo = roomInfo;
        UpdateText(roomInfo.Name + " - " + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers);
    }

    // When the button is pressed
    public void OnClick()
    {
        // Joins the room that was selected
        NetworkManagerScript.instance.JoinRoom(currentRoomInfo.Name);
    }

    public void OnSelect()
    {
        Debug.Log("Selecting " + text.text + "...");
        listImage.color = selectedRoomColor;
    }

    public void OnDeselect()
    {
        Debug.Log("Deselecting " + text.text + "...");
        listImage.color = defaultColor;
    }

    public void UpdateText(string roomText)
    {
        text.text = roomText;
    }

    public RoomInfo GetRoomListInfo() => currentRoomInfo;
}