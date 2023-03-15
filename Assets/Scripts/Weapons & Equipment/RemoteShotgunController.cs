using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using CustomEnums;

/// <summary>
/// Governs networked shotgun firing.
/// </summary>
public class RemoteShotgunController : MonoBehaviourPunCallbacks
{
    //Objects & Components:
    private NetworkPlayer networkPlayer;     //The networkPlayer holding this weapon
    internal NewShotgunController clientGun; //Client weapon associated with this networked weapon (NOTE: this reference is only present on the local client version of this script)
    internal string projectileResourceName;  //Name of projectile to be fired in Resources folder
    private AudioSource audioSource;         //Audio source component on this remote shotgun
    private ParticleSystem shotParticles;    //Particle system which spawns muzzle debris upon firing

    //Runtime Variables:
    private Handedness handedness; //Which hand this shotgun is associated with

    //RUNTIME METHODS:
    private void Awake()
    {
        //Determine handedness:
        networkPlayer = GetComponentInParent<NetworkPlayer>(); if (networkPlayer == null) { Debug.LogError("Tried to spawn RemoteShotgunController on a non-NetworkPlayer."); Destroy(this); } //Get network player component in parent
        if (transform.parent.name.Contains("Left") || transform.parent.name.Contains("left")) handedness = Handedness.Left;                                                                    //Indicate left-handedness
        else if (transform.parent.name.Contains("Right") || transform.parent.name.Contains("right")) handedness = Handedness.Right;                                                            //Indicate right-handedness
        else { Debug.LogWarning("RemoteShotgunController " + name + " could not determine handedness, make sure it's parent object has the word Right or Left in its name."); }                //Post error if handedness could not be determined

        //Get objects & components:
        if (!TryGetComponent(out audioSource)) audioSource = gameObject.AddComponent<AudioSource>(); //Make sure gun has audioSource
        shotParticles = GetComponentInChildren<ParticleSystem>();                                    //Get particle system in children
    }
    private void Start()
    {
        if (networkPlayer.photonView.IsMine) //Weapon is on master version of networkPlayer
        {
            //Get matching shotgun controller:
            foreach (PlayerEquipment equipment in PlayerController.instance.attachedEquipment) //Iterate through list of equipment attached to player
            {
                if (equipment.handedness == handedness && equipment.TryGetComponent(out clientGun)) break; //Break once matching shotgun has been found
            }
            if (clientGun == null) { Debug.LogError("RemoteShotgunController could not find matching client weapon on player!"); Destroy(gameObject); } //Post warning if client weapon could not be found

            //Initialize:
        }
    }

    //FUNCTiONALITY METHODS:

}
