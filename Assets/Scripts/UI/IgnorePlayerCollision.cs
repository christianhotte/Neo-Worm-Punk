using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnorePlayerCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("PlayerHand"))
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider, true);
            Debug.Log(gameObject.name + " Ignoring Collision Of " + collision.collider + "...");
        }
        else
        {
            Debug.Log(gameObject.name + "Colliding With " + collision.collider + "...");
        }
    }
}
