using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class teehee : MonoBehaviour
{

    private void Start()
    {
        //StartCoroutine(SUMFUNII());
    }

    IEnumerator SUMFUNII()
    {
        yield return new WaitForSeconds(6);

        SumFunni();

    }

    public void SumFunni()
    {
        Material funni = Resources.Load("TeeHee", typeof(Material)) as Material;
        Texture2D funnni = Resources.Load("HAHA", typeof(Texture2D)) as Texture2D;

        //Transform[] allTransforms = GameObject.FindObjectsOfType<Transform>();
        MeshRenderer[] allMeshRenderers = GameObject.FindObjectsOfType<MeshRenderer>();
        foreach (MeshRenderer rend in allMeshRenderers)
        {
            rend.material = funni;
            //rend.material.mainTexture = funnni;
        }
    }

}
