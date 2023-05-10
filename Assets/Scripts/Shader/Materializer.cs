using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Materializer : MonoBehaviour
{
    public Material targetMat;

    private void Start()
    {
        
        
    }
    private void Update()
    {
        Renderer[] renderers = transform.root.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material = targetMat;
            for (int x = 0; x < r.materials.Length; x++)
            {
                r.materials[x] = targetMat;
                print("Changed material");
            }
        }
    }
}
