using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PlayerController))]
public class PlayerSetup : MonoBehaviour
{
    //FUNCTIONALITY METHODS:

    /// <summary>
    /// Applies all settings to local player instance (using local PlayerSettings instance).
    /// </summary>
    public void ApplyAllSettings()
    {
        SetColor(PlayerSettingsController.Instance.charData.playerColor); //DEMO: Set player color
    }
    /// <summary>
    /// DEMO FUNCTION: Set a color on the player.
    /// </summary>
    /// <param name="newColor">The color given to the player.</param>
    public void SetColor(Color newColor)
    {
        if(GameSettings.debugMode)
            Debug.Log("Setting Player Color To " + newColor.ToString() + " ...");

        //Change color of player body:
        foreach(Material mat in PlayerController.instance.bodyRenderer.materials) mat.SetColor("_Color", newColor); //Set every material in player body to new color

        //Change color of player hands (TEMP):
        foreach (var controller in FindObjectsOfType<ActionBasedController>()) //Iterate through each hand in player
        {
            if (controller.GetComponentInChildren<MeshRenderer>() != null) controller.GetComponentInChildren<MeshRenderer>().material.SetColor("_Color", newColor); //Change hand color (if possible, may be deprecated)
        }

        //Change color of player helmet
        PlayerController.instance.combatHUD.ChangeHelmetColor(newColor);
    }
}