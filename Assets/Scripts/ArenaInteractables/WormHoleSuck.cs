using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
public class WormHoleSuck : MonoBehaviour
{
    public bool suckin = false;
    public float pullForce = 5;
    public Transform womrHoleCenter;
    internal GameObject player;
    internal Rigidbody playerRB;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (suckin)
        {
            Debug.Log("TrynaSuck");
            Vector3 newVel;
            womrHoleCenter.LookAt(player.transform);
            newVel = ((pullForce *Time.deltaTime) *-womrHoleCenter.forward);
            playerRB.velocity += newVel;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "XR Origin")
        {
            playerRB = other.GetComponent<Rigidbody>();
            player = other.gameObject;
            suckin = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "XR Origin")
        {
            suckin = false;
        }
    }
}
