using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class RoomCodeController : MonoBehaviour
{
    [SerializeField, Tooltip("The displayed room code text.")] private TextMeshProUGUI roomCodeText;

    private void OnEnable()
    {
        Clear();
    }

    /// <summary>
    /// Adds to the room code.
    /// </summary>
    /// <param name="newString">The new string to add to the room code.</param>
    public void AddToRoomCode(string newString)
    {
        //If the room code length has not been reached, add to the room code
        if(roomCodeText.text.Length < GameSettings.roomCodeLength)
            roomCodeText.text += newString;
    }

    /// <summary>
    /// Removes the last character in the room code.
    /// </summary>
    public void Backspace()
    {
        //If the room code is not empty, remove the last character in the string
        if(roomCodeText.text != string.Empty)
            roomCodeText.text = roomCodeText.text.Substring(0, roomCodeText.text.Length - 1);
    }

    /// <summary>
    /// Clears the room code.
    /// </summary>
    public void Clear()
    {
        roomCodeText.text = "";
    }

    /// <summary>
    /// Tries to join the room displayed in the room code.
    /// </summary>
    public void TryToJoinRoom()
    {
        NetworkManagerScript.instance.JoinRoom(roomCodeText.text);
    }
}
