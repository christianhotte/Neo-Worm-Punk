using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class RoomCode : MonoBehaviour
{
    private TextMeshProUGUI roomText;   //The text for the room code
    private bool hideRoomCode;  //If true, the room code is hidden
    private bool displayingCode = false;    //If true, the code is being displayed

    // Start is called before the first frame update
    void Start()
    {
        roomText = GetComponent<TextMeshProUGUI>();
        hideRoomCode = PlayerPrefs.GetInt("HideRoomCode", 0) == 1 ? true : false;
    }

    private void Update()
    {
        if (!displayingCode)
            UpdateRoomCode();
    }

    /// <summary>
    /// Refreshes the room code text.
    /// </summary>
    public void UpdateRoomCode()
    {
        if (PhotonNetwork.InRoom)
        {
            if (hideRoomCode)
                roomText.text = "XXXXX";
            else
                roomText.text = PhotonNetwork.CurrentRoom.Name;

            displayingCode = true;
        }
    }

    /// <summary>
    /// Determines whether the room code is hidden for the client.
    /// </summary>
    /// <param name="hideCode">If true, the room code will be hidden.</param>
    public void SetHideRoomCode(bool hideCode)
    {
        hideRoomCode = hideCode;
        UpdateRoomCode();
    }
}
