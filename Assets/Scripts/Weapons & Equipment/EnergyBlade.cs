using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
public class EnergyBlade : MonoBehaviour
{
    private AudioSource sawAud;
    public AudioClip laserSwordCut;
    // Start is called before the first frame update
    void Awake()
    {
        sawAud = GetComponentInParent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("BladeTouched");
        NetworkPlayer targetPlayer = other.GetComponentInParent<NetworkPlayer>();     //Try to get network player from hit collider
        if (targetPlayer == null) targetPlayer = other.GetComponent<NetworkPlayer>(); //Try again for network player if it was not initially gotten
        if (targetPlayer != null)
        {
            if (targetPlayer.photonView.IsMine) { print("Hit myself"); return; }
            targetPlayer.photonView.RPC("RPC_Hit", RpcTarget.All, 5);
            sawAud.PlayOneShot(laserSwordCut, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f));
        }
    }
}
