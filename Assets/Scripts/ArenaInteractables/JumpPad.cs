using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class JumpPad : MonoBehaviour
{
    public float jumpForce=10;
    public Transform jumpDirection;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
  
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out XROrigin playerOrigin))
        {
            foreach (PlayerEquipment equipment in PlayerController.instance.attachedEquipment)
            {
                NewGrapplerController grapple = equipment.GetComponent<NewGrapplerController>(); // Searches for the grappling hook atached to the hit player
                if (grapple == null) continue;
                if (grapple.hook.state != HookProjectile.HookState.Stowed)
                {
                    grapple.hook.Release();//forces the grapplehook to release when hitting jump pad
                    grapple.hook.Stow();
                }
            }
            Bounce();
            return;
        }
    }

    public void Bounce()
    {
        Rigidbody playerRb = PlayerController.instance.bodyRb;
        playerRb.transform.position = jumpDirection.position; // moves the player to the center of the jump pad
        playerRb.velocity = jumpDirection.up * jumpForce;// Launches the player off of the pad
    }
}
