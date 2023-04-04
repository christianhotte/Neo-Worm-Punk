using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyLightController : MonoBehaviour
{
    [SerializeField, Tooltip("The color of the light when the player is ready.")] private Color readyColor = Color.green;
    private Color notReadyColor;
    private Material readyLightMaterial;

    private void Awake()
    {
        readyLightMaterial = GetComponentInChildren<MeshRenderer>().material;
        notReadyColor = readyLightMaterial.GetColor("_BaseColor");
    }

    public void ActivateLight(bool isActivated)
    {
        if (isActivated)
            readyLightMaterial.SetColor("_BaseColor", readyColor);
        else
            readyLightMaterial.SetColor("_BaseColor", notReadyColor);
    }
}
