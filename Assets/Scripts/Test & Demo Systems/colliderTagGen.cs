using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colliderTagGen : MonoBehaviour
{
    void Start()
    {
        GameObject p = GameObject.Find("[generated-meshes]");

        for(int i = 0; i < p.transform.childCount; i++)
        {
            if(p.transform.GetChild(i).name == "[generated-collider-mesh]")
            {
                p.transform.GetChild(i).gameObject.tag = "Ground";
                //p.transform.GetChild(i).gameObject.layer = 8;
            }
        }

        //GameObject.Find("[generated-collider-mesh]").tag = "Ground";
        //GameObject.Find("[generated-collider-mesh]").layer = 8;
        

    }
}
