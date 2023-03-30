using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeZone : MonoBehaviour
{
    // Start is called before the first frame update
    internal bool playerinSlime = false;
    internal PlayerController PlayerScript;
    internal HoopBoost Hoopscript;
    public MeshRenderer SlimeModel;
    internal Rigidbody playerRb;
    void Start()
    {
        Hoopscript = GetComponentInParent<HoopBoost>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "XR Origin")
        {
            playerinSlime = true;
            playerRb = other.GetComponent<Rigidbody>();
            PlayerScript = other.GetComponent<PlayerController>();
            
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "XR Origin")
        {
            if (Hoopscript.slimed)
            {
                PlayerScript.ResetDrag();
                Hoopscript.slimed = false;
            }
            playerinSlime = false;
            playerRb = null;
            PlayerScript = null;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Hoopscript.slimed)
        {
            SlimeModel.enabled = true;
        }
        else
        {
            SlimeModel.enabled = false;
        }
        if (playerinSlime && Hoopscript.slimed)
        {
            playerRb.drag = 4f;
        }
    }

}
