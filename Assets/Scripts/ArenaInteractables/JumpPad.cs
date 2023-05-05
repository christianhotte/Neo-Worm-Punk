using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class JumpPad : MonoBehaviour
{
    public float jumpForce=10;
    public Transform jumpDirection;
    private float startingYPos;
    private float changeYPos = 0.03f;
    internal AudioSource jumpPadAudio;
    public AudioClip jumpPadSound;
    private float maxDist = 0.5f;
    //[SerializeField, Tooltip("The arrow halo effect that needs to scroll")] public MeshRenderer haloMesh;
    private float offset;

    // Start is called before the first frame update
    void Start()
    {
        startingYPos = jumpDirection.localPosition.y;
        jumpPadAudio = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
        jumpDirection.Rotate(new Vector3(0, -0.25f, 0));    //rotates the middle jump section
        if(Mathf.Abs(jumpDirection.localPosition.y - startingYPos) >= maxDist)  //checks if the transform is outside of a its maxDistance
        {
            changeYPos *= -1;   //changes the directions of movement
        }
        jumpDirection.Translate(new Vector3(0, changeYPos, 0)); //moves the middle jump section

        //offset += (Time.deltaTime * 0.5f) / 10.0f;
        //haloMesh.material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
        //haloMesh.material.SetTextureOffset
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out XROrigin playerOrigin))
        {
            foreach (PlayerEquipment equipment in PlayerController.instance.attachedEquipment)
            {
                NewGrapplerController grapple = equipment.GetComponent<NewGrapplerController>(); // Searches for the grappling hook atached to the hit player
                if (grapple == null) continue;
                if (grapple.hook != null && grapple.hook.state != HookProjectile.HookState.Stowed)
                {
                    grapple.hook.Release();//forces the grapplehook to release when hitting jump pad
                    grapple.hook.Stow();//forces the hookshot to release
                }
            }
            Bounce();
            return;
        }
    }

    public void Bounce()
    {
        if(jumpPadSound!=null)jumpPadAudio.PlayOneShot(jumpPadSound, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));
        Rigidbody playerRb = PlayerController.instance.bodyRb;
        playerRb.transform.position = jumpDirection.position; // moves the player to the center of the jump pad
        playerRb.velocity = jumpDirection.up * jumpForce;// Launches the player off of the pad
    }
}
