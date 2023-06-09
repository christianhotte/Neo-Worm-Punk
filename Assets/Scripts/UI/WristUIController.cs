using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class WristUIController : MonoBehaviour
{
    [SerializeField, Tooltip("The player controller.")] private PlayerController playerController;
    [SerializeField, Tooltip("The player input actions asset.")] private InputActionAsset inputActions;
    [SerializeField, Tooltip("The menu ray interactors.")] private GameObject[] rayInteractors;
    [SerializeField, Tooltip("The interactable HUD menu.")] private PlayerHUDController playerHUDController;

    [SerializeField, Tooltip("The button that allows the player to go back to the main menu.")] private GameObject quitToMainButton;
    [SerializeField, Tooltip("The button that allows the masterclient to leave the round early.")] private GameObject endRoundEarlyButton;
    [SerializeField, Tooltip("The button that allows the player to leave their room.")] private GameObject leaveRoomButton;

    private Canvas wristCanvas; //The canvas that shows the wrist menu
    private InputAction menu;   //The action that activates the menu

    // Start is called before the first frame update
    void Start()
    {
        wristCanvas = GetComponent<Canvas>();   //Get the canvas component
    }

    private void OnEnable()
    {
        menu = inputActions.FindActionMap("XRI LeftHand").FindAction("Menu");   //Find the menu action from the left hand action map
        menu.Enable();
        menu.performed += ToggleMenu;
    }

    private void OnDisable()
    {
        menu.Disable();
        menu.performed -= ToggleMenu;
    }

    /// <summary>
    /// Toggles the wrist menu when pressing a button
    /// </summary>
    /// <param name="ctx">The information from the action.</param>
    public void ToggleMenu(InputAction.CallbackContext ctx)
    {
        ShowMenu(!wristCanvas.enabled); //Toggle the canvas component
    }

    /// <summary>
    /// Shows or hides the wrist menu.
    /// </summary>
    /// <param name="showMenu">If true, the menu is shown. If false, the menu is hidden.</param>
    public void ShowMenu(bool showMenu)
    {
        wristCanvas.enabled = showMenu;
        playerHUDController.GetComponent<Canvas>().enabled = showMenu;
        playerController.SetCombat(!showMenu);
        
        if(SceneManager.GetActiveScene().name != GameSettings.tutorialScene)
            playerController.inverteboy.ForceOpenInverteboy(showMenu);

        foreach (var interactor in rayInteractors)
            interactor.SetActive(showMenu);
    }

    private void Update()
    {
        UpdateButtonDisplay();
    }

    /// <summary>
    /// Updates the visibility of the leave room button.
    /// </summary>
    private void UpdateButtonDisplay()
    {
        //If the player is in a room, not in the main menu, and the leave button is not showing, activate the leave room button.
        if (PhotonNetwork.InRoom)
        {
            if (!leaveRoomButton.activeInHierarchy)
                leaveRoomButton.SetActive(true);

            //Hide the quit to main button when in a room
            if(quitToMainButton.activeInHierarchy)
                quitToMainButton.SetActive(false);

            endRoundEarlyButton.SetActive(PhotonNetwork.IsMasterClient && SceneManager.GetActiveScene().name == GameSettings.arenaScene);
        }
        //If they are not in a room, set the button to false.
        else
        {
            leaveRoomButton.SetActive(false);
            endRoundEarlyButton.SetActive(false);
            quitToMainButton.SetActive(SceneManager.GetActiveScene().name != GameSettings.titleScreenScene);
        }
    }

    /// <summary>
    /// Removes the player from the room and sends them back to the main menu.
    /// </summary>
    public void LeaveRoomToMain()
    {
        PhotonNetwork.LeaveRoom();  //Leave the room
        PhotonNetwork.LeaveLobby(); //Leave the lobby
        PhotonNetwork.Disconnect(); //Disconnects from the server
    }

    public void QuitToMain()
    {
        NetworkManagerScript.instance.LoadSceneWithFade(GameSettings.titleScreenScene);
    }

    public void EndRoundEarly()
    {
        if (FindObjectOfType<RoundManager>() != null)
            FindObjectOfType<RoundManager>().ForceEndRound();
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
