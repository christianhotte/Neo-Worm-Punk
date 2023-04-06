using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class SettingsCapsule : MonoBehaviour
{
    private GameMode currentGameMode;
    private TextMeshProUGUI gameModeLabel;

    private void Awake()
    {
        gameModeLabel = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        GetComponent<XRGrabInteractable>().interactionManager = PlayerController.instance.GetComponentInChildren<XRInteractionManager>();
    }

    public GameMode GetGameMode() => currentGameMode;
    public void SetGameMode(GameMode gameMode)
    {
        currentGameMode = gameMode;
        UpdateGameModeText();
    }

    private void UpdateGameModeText() => gameModeLabel.text = GameModeDisplay.DisplayGameMode(currentGameMode);
}
