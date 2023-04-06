using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SettingsCapsule : MonoBehaviour
{
    private GameMode currentGameMode;

    private void Start()
    {
        GetComponent<XRGrabInteractable>().interactionManager = PlayerController.instance.GetComponentInChildren<XRInteractionManager>();
    }

    public GameMode GetGameMode() => currentGameMode;
    public void SetGameMode(GameMode gameMode) => currentGameMode = gameMode;
}
