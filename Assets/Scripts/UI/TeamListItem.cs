using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class TeamListItem : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private TextMeshProUGUI playerNamePrefab;
    [Space(10)]

    [Header("Objects")]
    [SerializeField] private RectTransform playerNameContainer;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI teamLabel;

    private ColorOptions currentColor;

    private int numberOfTeamMembers;

    public void RefreshPlayerList(Dictionary<string, int> playerNames)
    {
        ClearPlayerNames();

        foreach (var player in playerNames)
        {
            Debug.Log("Checking " + player.Key + " Color: " + player.Value);
            Debug.Log("Current Color: " + (int)currentColor);
            if(player.Value == (int)currentColor)
            {
                Debug.Log("Adding " + player.Key + " To " + teamLabel.text + "...");
                TextMeshProUGUI newPlayer = Instantiate(playerNamePrefab, playerNameContainer);
                ChangePlayerName(newPlayer, player.Key);
                switch (currentColor)
                {
                    case ColorOptions.YELLOW:
                        ChangeLabelColor(Color.black);
                        ChangePlayerNameColor(newPlayer, Color.black);
                        break;
                    case ColorOptions.GREEN:
                        ChangeLabelColor(Color.black);
                        ChangePlayerNameColor(newPlayer, Color.black);
                        break;
                    case ColorOptions.CYAN:
                        ChangeLabelColor(Color.black);
                        ChangePlayerNameColor(newPlayer, Color.black);
                        break;
                    case ColorOptions.WHITE:
                        ChangeLabelColor(Color.black);
                        ChangePlayerNameColor(newPlayer, Color.black);
                        break;
                    default:
                        ChangeLabelColor(Color.white);
                        ChangePlayerNameColor(newPlayer, Color.white);
                        break;
                }
                numberOfTeamMembers++;
            }
        }
        gameObject.SetActive(numberOfTeamMembers > 0);
    }

    private void ClearPlayerNames()
    {
        foreach (Transform trans in playerNameContainer)
            Destroy(trans.gameObject);

        numberOfTeamMembers = 0;
    }

    public void ChangeBackgroundColor(int newColor)
    {
        backgroundImage.color = PlayerSettingsController.playerColors[newColor];
        currentColor = (ColorOptions)newColor;
    }

    public void ChangeLabel(int colorLabel)
    {
        teamLabel.text = ((string[])PhotonNetwork.CurrentRoom.CustomProperties["TeamNames"])[colorLabel];
    }

    public void ChangePlayerName(TextMeshProUGUI nameText, string newName)
    {
        nameText.text = newName;
    }

    public void ChangeLabelColor(Color newColor)
    {
        teamLabel.color = newColor;
    }

    public void ChangePlayerNameColor(TextMeshProUGUI nameText, Color newColor)
    {
        nameText.color = newColor;
    }

    public int GetNumberOfTeamMembers() => numberOfTeamMembers;

}
