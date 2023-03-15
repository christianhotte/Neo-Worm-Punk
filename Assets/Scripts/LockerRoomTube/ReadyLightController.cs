using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyLightController : MonoBehaviour
{
    [SerializeField, Tooltip("The color of the light when the player is ready.")] private Color readyColor = Color.green;
    private Color notReadyColor;
    private Material readyLightMaterial;

    // Start is called before the first frame update
    void Start()
    {
        readyLightMaterial = GetComponentInChildren<MeshRenderer>().material;
        notReadyColor = readyLightMaterial.color;
    }

    public void ActivateLight(bool isActivated)
    {
        if (isActivated)
            readyLightMaterial.color = readyColor;
        else
            readyLightMaterial.color = notReadyColor;
    }
}
