using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopBoost : NetworkedArenaElement
{
    public Transform hoopCenter, hoopOuter, hoopInner;
    public TrapTrigger triggerScript;
    private PlayerController PC;
    private Rigidbody playerRB;
    public float boostAmount;
    public bool maintainsOtherVelocity;
    internal bool launchin = false,slimed=false;
    internal AudioSource HoopAud;
    public AudioClip HoopSound;
    public GameObject SlimeModel;
    // Start is called before the first frame update
    void Start()
    {
        HoopAud = this.GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        RotateHoop(hoopInner, new Vector3(0.75f, 0, 0));
        RotateHoop(hoopOuter, new Vector3(0f, 0, 0));

        if (slimed)
        {
            SlimeModel.SetActive(true);
        }
        else
        {
            SlimeModel.SetActive(false);
        }

    }
    private void RotateHoop(Transform hoopPart, Vector3 hoopRotateAmount)
    {
        hoopPart.Rotate(hoopRotateAmount);
    }

    public IEnumerator HoopLaunch(Collider hitPlayer)
    {
        launchin = true;
        PC = PlayerController.instance;
        playerRB = PC.bodyRb;
        if (slimed)
        {
            yield return null;
        }
        if(HoopSound!=null)HoopAud.PlayOneShot(HoopSound, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));
        if(maintainsOtherVelocity)
        {
            //just add the boostAmount to the velocity of the player
            Vector3 boostVel = Vector3.Project(playerRB.velocity, hoopCenter.forward).normalized * boostAmount;
            playerRB.velocity += boostVel;
        }
        else
        {
            //reset all velocity but in the direction of the hoop and give them a boost in that direction
            Vector3 entryVel = Vector3.Project(playerRB.velocity, hoopCenter.forward);//Gets the velocity on one axis in relation to the hoops forward
            if (entryVel.magnitude <1)
            {
                entryVel = new Vector3(5, 0, 5);
            }
            Vector3 exitVel = entryVel + (entryVel.normalized * boostAmount); // adds the projected amount to the players velocity
            playerRB.velocity = exitVel;//actually sets the velocity
        }

        
        yield return new WaitForSeconds(0.4f);//cooldown so only one boost given
        launchin = false;
    }
    public IEnumerator SlimeHoop()
    {
        yield return new WaitForSeconds(10.0f);
        slimed = false;
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.name == "XR Origin" && !launchin)
        {
            StartCoroutine(HoopLaunch(other));
        }
    }
}
