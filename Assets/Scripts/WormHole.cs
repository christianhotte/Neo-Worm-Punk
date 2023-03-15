using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormHole : MonoBehaviour
{
    public Transform holePos1, holePos2,wormZone,playerHead,wormZoneShifted;
    public GameObject wormZoneParticles,wormZoneInstance,playerCam;
    public float waitTime,exitSpeed=30,wormZoneSpeed=5;
    internal bool locked = false,inZone=false;
    private NewShotgunController NSC;
    public PlayerController PC;
    public GameObject playerOrigin;
    public static List<WormHole> ActiveWormholes = new List<WormHole>();
    private WormHoleTrigger triggerScript,EntryTrigger;
    void Start()
    {
    }
    void Update()
    {
    }
    public IEnumerator StartWormhole(GameObject startHole,GameObject playerOBJ)
    {
        locked = true; // Locks the worm whole circut      
        Transform exitPos;                                                           //define Exit Point
        Rigidbody playerRB;
        if (holePos1.transform == startHole.transform)//Determine which wormhole is going to be the exit
        {
            exitPos = holePos2.transform; //Set the exit point
            triggerScript = holePos2.gameObject.GetComponent<WormHoleTrigger>();//Gets the script of the exit
            EntryTrigger = holePos1.gameObject.GetComponent<WormHoleTrigger>();//Gets the script on the entrance
        }
        else
        {
            exitPos = holePos1.transform;//Set the exit point
            triggerScript = holePos1.gameObject.GetComponent<WormHoleTrigger>();//Gets the script of the exit
            EntryTrigger = holePos2.gameObject.GetComponent<WormHoleTrigger>();//Gets the script on the entrance
        }
        triggerScript.exiting = true;//Tells the trigger script it will be the exit
        PC = PlayerController.instance; // Gets the controller of the player instance
        playerRB = PC.bodyRb;      //sets rigidbody reference
        playerCam = PC.cam.gameObject;      //sets camera reference
        ActiveWormholes.Add(this);//Adds this to the static wormhole list
        wormZoneShifted = wormZone; //gives the shifted zone its starting point
        wormZoneShifted.transform.position = new Vector3(wormZone.position.x + 100 * ActiveWormholes.Count,wormZone.position.y,wormZone.position.z);//moves the wormhole instance so each player has their own
      
        playerRB.useGravity = false;  //Turn off Gravity
        foreach (PlayerEquipment equipment in PlayerController.instance.attachedEquipment)
        {
            NewGrapplerController grapple = equipment.GetComponent<NewGrapplerController>();
            if (grapple == null) continue;
            if (grapple.hook.state != HookProjectile.HookState.Stowed)
            {
                grapple.hook.Release();
                grapple.hook.Stow();
            }
        }
        playerOBJ.transform.position = wormZoneShifted.position; //Player enters worm zone here
        float entryDiff = playerCam.transform.eulerAngles.y - wormZoneShifted.eulerAngles.y; //difference for player to face down wormhole
        playerOBJ.transform.rotation = Quaternion.Euler(playerOBJ.transform.eulerAngles.x, playerOBJ.transform.eulerAngles.y - entryDiff, playerOBJ.transform.eulerAngles.z);
        float startRot = playerCam.transform.eulerAngles.y;//reference the starting rotation of the players camera
        wormZoneInstance =Instantiate(wormZoneParticles);//spawns the wormhole instance
        wormZoneInstance.transform.position = new Vector3(PC.cam.transform.position.x , PC.cam.transform.position.y, PC.cam.transform.position.z);//moves the wormhole into position
        wormZoneInstance.transform.eulerAngles = new Vector3(0, startRot, 0); // sets the wormhole to be aligned with your face
        wormZoneSpeed = 120;// The speed you fly through the wormholes at
        playerRB.velocity = wormZoneInstance.transform.forward * wormZoneSpeed;//giving the speed to the player
        yield return new WaitForSeconds(waitTime);//time to wait while traveling down worm hole
        float diff = playerCam.transform.eulerAngles.y - exitPos.transform.eulerAngles.y; // gets the difference in angle between the player and the exit
        float exitDiff = playerCam.transform.eulerAngles.y - startRot;//adjusts the players rotation by the difference in the wormhole and as they turn in the wormhole
        diff = diff - exitDiff;
        playerOBJ.transform.rotation = Quaternion.Euler(playerOBJ.transform.eulerAngles.x, playerOBJ.transform.eulerAngles.y - diff, playerOBJ.transform.eulerAngles.z);//turns the player to face out of the worhole
        playerOBJ.transform.position = exitPos.position; //takes the player out of the wormhole
        playerRB.useGravity = true; //Bring back Gravity
        playerRB.velocity = exitPos.forward * exitSpeed;    //launch out of wormhole
        triggerScript.exiting = false;
        triggerScript.reset = true; //tells the exit to open back up
        EntryTrigger.reset = true;//tells the entrance to open back up
        yield return new WaitForSeconds(0.2f);  //Wait for the player to get clear of the wormhole
        ActiveWormholes.Remove(this);
        Destroy(wormZoneInstance);
        locked = false;   //Unlock the Womrhole circut
        
    }
}
