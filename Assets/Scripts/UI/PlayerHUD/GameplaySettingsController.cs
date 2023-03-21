using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplaySettingsController : MonoBehaviour
{
    [SerializeField, Tooltip("The toggle for hide room code.")] private Toggle hideRoomCodeToggle;

    private void OnEnable()
    {
        UpdateSettings();
    }

    private void UpdateSettings()
    {
        hideRoomCodeToggle.isOn = PlayerPrefs.GetInt("HideRoomCode", 0) == 1 ? true : false;
    }

    public void ToggleHideRoomCode(bool isOn)
    {
        PlayerPrefs.SetInt("HideRoomCode", isOn ? 1 : 0);

        //Updates any visible room codes accordingly
        foreach (var roomCode in FindObjectsOfType<RoomCode>())
            roomCode.SetHideRoomCode(PlayerPrefs.GetInt("HideRoomCode", 0) == 1 ? true : false);
    }

}
