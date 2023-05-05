using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyLightController : MonoBehaviour
{
    [SerializeField, Tooltip("The color of the light when the player is ready.")] private Color readyColor = Color.green;
    [SerializeField] private float notReadyEmissionIntensity = 1.015028f;
    [SerializeField] private float readyEmissionIntensity = 1.5f;
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
        {
            readyLightMaterial.SetColor("_BaseColor", readyColor);

            readyLightMaterial.EnableKeyword("_EMISSION");
            readyLightMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            // Update the emission color and intensity of the material.
            readyLightMaterial.SetColor("_EmissionColor", readyColor * readyEmissionIntensity);

            // Makes the renderer update the emission and albedo maps of our material.
            RendererExtensions.UpdateGIMaterials(GetComponentInChildren<MeshRenderer>());

            // Inform Unity's GI system to recalculate GI based on the new emission map.
            DynamicGI.SetEmissive(GetComponentInChildren<MeshRenderer>(), readyColor * readyEmissionIntensity);
            DynamicGI.UpdateEnvironment();
        }
        else
        {
            readyLightMaterial.SetColor("_BaseColor", notReadyColor);

            readyLightMaterial.EnableKeyword("_EMISSION");
            readyLightMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            // Update the emission color and intensity of the material.
            readyLightMaterial.SetColor("_EmissionColor", notReadyColor * notReadyEmissionIntensity);

            // Makes the renderer update the emission and albedo maps of our material.
            RendererExtensions.UpdateGIMaterials(GetComponentInChildren<MeshRenderer>());

            // Inform Unity's GI system to recalculate GI based on the new emission map.
            DynamicGI.SetEmissive(GetComponentInChildren<MeshRenderer>(), notReadyColor * notReadyEmissionIntensity);
            DynamicGI.UpdateEnvironment();
        }
    }
}
