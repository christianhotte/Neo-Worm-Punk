using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBodyManager : MonoBehaviour
{
    //Objects & Components:
    private PlayerController player;

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        player = GetComponentInParent<PlayerController>();
    }
    private void OnCollisionStay(Collision collision)
    {
        Vector3 normal = collision.GetContact(0).normal;
        float floorAngle = Vector3.Angle(normal, Vector3.up);
        if (floorAngle >= player.slipAngleRange.x && floorAngle < player.slipAngleRange.y)
        {
            float slipSpeed = player.slipSpeed * Time.deltaTime;
            Vector3 slopeLeft = Vector3.Cross(normal, Vector3.up);
            Vector3 addVel = Vector3.Cross(normal, slopeLeft).normalized * slipSpeed;
            player.bodyRb.AddForce(addVel, ForceMode.Impulse);
        }
    }
}
