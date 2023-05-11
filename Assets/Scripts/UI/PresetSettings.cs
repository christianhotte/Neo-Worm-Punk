using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class PresetSettings : MonoBehaviour
{
    private GamePreset presetData;
    private TextMeshProUGUI presetLabel;

    private void Awake()
    {
        presetLabel = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        GetComponent<XRGrabInteractable>().interactionManager = PlayerController.instance.GetComponentInChildren<XRInteractionManager>();
    }

    public void UpdatePresetLabelText(string presetName) => presetLabel.text = presetName;
    public GamePreset GetPresetData() => presetData;
    public void SetPresetData(GamePreset gamePreset) => presetData = gamePreset;
}
