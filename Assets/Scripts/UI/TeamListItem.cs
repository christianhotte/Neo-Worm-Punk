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

    public void RefreshPlayerList(string [] playerNames)
    {
        ClearPlayerNames();

        foreach (var player in playerNames)
        {
            TextMeshProUGUI newPlayer = Instantiate(playerNamePrefab, playerNameContainer);
            ChangePlayerName(newPlayer, player);
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

        if (numberOfTeamMembers == 0)
            gameObject.SetActive(false);
    }

    private void ClearPlayerNames()
    {
        foreach (Transform trans in playerNameContainer)
            Destroy(trans.gameObject);

        numberOfTeamMembers = 0;
    }

    public void ChangeBackgroundColor(ColorOptions newColor)
    {
        backgroundImage.color = PlayerSettingsController.ColorOptionsToColor(newColor);
        currentColor = newColor;
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
