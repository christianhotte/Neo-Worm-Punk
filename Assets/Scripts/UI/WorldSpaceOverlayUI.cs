using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceOverlayUI : MonoBehaviour
{
    private const string shaderTestMode = "unity_GUIZTestMode";
    [SerializeField] UnityEngine.Rendering.CompareFunction desiredUIComprasion = UnityEngine.Rendering.CompareFunction.Always;
    [SerializeField] Graphic[] uiElementsToApplyEffectTo;

    private Dictionary<Material, Material> materialsMapping = new Dictionary<Material, Material>();

    // Start is called before the first frame update
    void Start()
    {
        foreach(var graphic in uiElementsToApplyEffectTo)
        {
            Material material = graphic.materialForRendering;
            if(material == null)
            {
                Debug.LogWarning("Target material does not have rendering component.");
                continue;
            }

            if(materialsMapping.TryGetValue(material, out Material materialCopy) == false)
            {
                materialCopy = new Material(material);
                materialsMapping.Add(material, materialCopy);
            }

            materialCopy.SetInt(shaderTestMode, (int)desiredUIComprasion);
            graphic.material = materialCopy;
        }
    }
}
