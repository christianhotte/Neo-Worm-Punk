using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerHUDController : MonoBehaviour
{
    public enum HUD_MENU_STATE { Main, Audio, Gameplay, Controls }

    [SerializeField, Tooltip("Main settings menu game object.")] private GameObject mainMenu;
    [SerializeField, Tooltip("Audio settings menu game object.")] private GameObject audioMenu;
    [SerializeField, Tooltip("Gameplay settings menu game object.")] private GameObject gameplayMenu;
    [SerializeField, Tooltip("Controls settings menu game object.")] private GameObject controlsMenu;

    [Header("Menu First Selected Items")]
    [SerializeField, Tooltip("The parent for all of the buttons on the main settings menu page.")] private GameObject mainMenuButtonContainer;
    [SerializeField, Tooltip("The selectable for the audio menu page.")] private Selectable audioMenuSelected;
    [SerializeField, Tooltip("The selectable for the gameplay menu page.")] private Selectable gameplayMenuSelected;
    [SerializeField, Tooltip("The selectable for the controls menu page.")] private Selectable controlsMenuSelected;

    private Selectable[] mainMenuButtons;   //The list of selectable buttons in the main menu
    private GameObject currentMenu; //The GameObject for the current active menu
    private Selectable currentSelectable;   //The current selectable for the menu

    private void OnEnable()
    {
        mainMenuButtons = mainMenuButtonContainer.GetComponentsInChildren<Selectable>();
        DeselectButtons();

        if (currentMenu != null)
            currentMenu.SetActive(false);

        currentMenu = mainMenu;
        currentSelectable = mainMenuButtons[0];
        SwitchMenu(HUD_MENU_STATE.Main);
    }

    public void GoToAudio()
    {
        SwitchMenu(HUD_MENU_STATE.Audio);
    }

    public void GoToGameplay()
    {
        SwitchMenu(HUD_MENU_STATE.Gameplay);
    }

    public void GoToControls()
    {
        SwitchMenu(HUD_MENU_STATE.Controls);
    }

    public void BackToMain()
    {
        SwitchMenu(HUD_MENU_STATE.Main);
    }

    /// <summary>
    /// Switches the active menu based on the menu state given.
    /// </summary>
    /// <param name="newMenuState">The new menu state.</param>
    private void SwitchMenu(HUD_MENU_STATE newMenuState)
    {
        GameObject newMenu;

        switch (newMenuState)
        {
            case HUD_MENU_STATE.Audio:
                newMenu = audioMenu;
                currentSelectable = audioMenuSelected;
                break;
            case HUD_MENU_STATE.Gameplay:
                newMenu = gameplayMenu;
                currentSelectable = gameplayMenuSelected;
                break;
            case HUD_MENU_STATE.Controls:
                newMenu = controlsMenu;
                currentSelectable = controlsMenuSelected;
                break;
            default:
                newMenu = mainMenu;
                currentSelectable = GetMainMenuSelectable();
                break;
        }

        currentMenu.SetActive(false);
        newMenu.SetActive(true);
        currentMenu = newMenu;

        SelectNewButton();
    }

    /// <summary>
    /// Gets the current menu active. Based on the current menu, select a new button on the main menu.
    /// </summary>
    /// <returns></returns>
    private Selectable GetMainMenuSelectable()
    {
        if (currentSelectable == gameplayMenuSelected)
        {
            return mainMenuButtons[1];
        }
        else if (currentSelectable == controlsMenuSelected)
        {
            return mainMenuButtons[2];
        }

        return mainMenuButtons[0];
    }

    /// <summary>
    /// Selects a new button when switch to a new menu.
    /// </summary>
    private void SelectNewButton()
    {
        DeselectButtons();
        EventSystem.current.SetSelectedGameObject(currentSelectable.gameObject);
    }

    private void DeselectButtons()
    {
        //Deselect anything that is currently selected and select the new object
        if (EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
