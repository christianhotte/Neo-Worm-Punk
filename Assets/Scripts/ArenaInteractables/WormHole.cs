using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement;
public class WormHole : NetworkedArenaElement
{
    public Transform holePos1, holePos2,wormZone,playerHead,wormZoneShifted;
    public GameObject wormZoneParticles, playerCam;
    public static GameObject wormZoneInstance;
    public float waitTime,exitSpeed=30,wormZoneSpeed=5;
    internal bool locked = false, inZone = false, luckyProc = false;
    public bool randomExit = false;
    private NewShotgunController NSC;
    public PlayerController PC;
    public GameObject playerOrigin;
    public static List<WormHole> ActiveWormholes = new List<WormHole>();
    public List<WormHoleTrigger> OpenExits = new List<WormHoleTrigger>();
    public AudioSource wormHoleAud;
    public AudioClip enterSound,suctionSound;
    private WormHoleTrigger triggerScript,EntryTrigger,lastEntry;
    public WormZone wormZoneScript;
    internal NetworkPlayer netPlayer;
    public int luckyChance = 10,randRange;
    private bool IsUpgradeActive;
    void Start()
    {
        wormHoleAud = this.GetComponent<AudioSource>();
        wormZoneScript = wormZoneParticles.GetComponent<WormZone>();
    }
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameSettings.arenaScene)
        {
            OpenExits.AddRange(FindObjectsOfType<WormHoleTrigger>());
            randRange = OpenExits.Count;
            int randomIndex = Random.Range(0, randRange);
            if (PhotonNetwork.IsConnected) IsUpgradeActive = (bool)PhotonNetwork.CurrentRoom.CustomProperties["UpgradesActive"];
        }
    }
    void Update()
    {
    }
    public IEnumerator StartWormhole(WormHoleTrigger startHole,GameObject playerOBJ)
    {
        inZone = true;
        PC = PlayerController.instance;
        if (GetComponentInChildren<NewGrapplerController>() != null) PC.GetComponentInChildren<NewGrapplerController>().locked = false;
        PlayerController.instance.UnHolsterAll();
        if (PhotonNetwork.IsConnected)
        {
            netPlayer = PlayerController.photonView.GetComponent<NetworkPlayer>();
            if (netPlayer.networkPlayerStats.deathStreak >= 1)
            {
                int adjustedLuckChance = luckyChance * netPlayer.networkPlayerStats.deathStreak;
                if (adjustedLuckChance > 100)
                {
                    adjustedLuckChance = 100;
                }
                int randTarget = 100 - adjustedLuckChance; //the number need random range to be greater than.
                Random.seed = (int)Time.realtimeSinceStartup;
                int randRoll = Random.Range(0, 101);
                Debug.Log("Roll is " + randRoll + " Target is " + randTarget);
                if (randRoll > randTarget && IsUpgradeActive)
                {
                    luckyProc = true;
                    int randPower = Random.Range(1, 4);

                    switch (randPower)
                    {
                        case 1:
                            wormZoneScript.heatVision.SetActive(true);
                            break;
                        case 2:
                            wormZoneScript.multiShot.SetActive(true);
                            break;
                        default:
                            wormZoneScript.invincibility.SetActive(true);
                            break;
                    }
                }
            }
            else
            {
                int randRoll = Random.Range(0, 101);
                Debug.Log("Roll is " + randRoll + " Target is " + luckyChance);
                if (randRoll < luckyChance&& IsUpgradeActive)
                {
                    luckyProc = true;
                    int randPower = Random.Range(1, 4);
                    switch (randPower)
                    {
                        case 1:
                            wormZoneScript.heatVision.SetActive(true);
                            break;
                        case 2:
                            wormZoneScript.multiShot.SetActive(true);
                            break;
                        default:
                            wormZoneScript.invincibility.SetActive(true);
                            break;
                    }
                }
            }
        }      
        RearView heatScanner = PlayerController.instance.GetComponentInChildren<RearView>();
        if (heatScanner != null)
        {
            heatScanner.StartCoroutine(heatScanner.DisableScanner(waitTime));
        }
        locked = true; // Locks the worm whole circut      
        Transform exitPos;                                                           //define Exit Point
        Rigidbody playerRB;

        if (!randomExit)
        {
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
        }
        else
        {
            EntryTrigger = startHole;
            OpenExits.Remove(startHole);
            randRange = OpenExits.Count;
            int randomIndex = Random.Range(0, randRange);
            triggerScript = OpenExits[randomIndex];
            exitPos = triggerScript.transform;
        }
        triggerScript.exitCam.gameObject.SetActive(true);
        startHole.holeAnim.SetBool("Locked", true);
        startHole.particle.SetActive(false);
        startHole.locked = true;
        triggerScript.locked = true;
        triggerScript.exiting = true;//Tells the trigger script it will be the exit
        PC = PlayerController.instance; // Gets the controller of the player instance
        playerRB = PC.bodyRb;      //sets rigidbody reference
        playerCam = PC.cam.gameObject;      //sets camera reference
        ActiveWormholes.Add(this);//Adds this to the static wormhole list
        wormZoneShifted = wormZone; //gives the shifted zone its starting point
      //  wormZoneShifted.transform.position = new Vector3(wormZone.position.x + 100 * ActiveWormholes.Count,wormZone.position.y,wormZone.position.z);//moves the wormhole instance so each player has their own      
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
       foreach(PlayerEquipment pe in PC.attachedEquipment)
        {
            pe.Shutdown(waitTime);
        }

        if (PhotonNetwork.IsConnected)
        {
            PlayerController.photonView.RPC("RPC_MakeInvisible", RpcTarget.Others);
        }
        playerOBJ.transform.position = wormZoneShifted.position; //Player enters worm zone here
        wormHoleAud.clip = null;
        wormHoleAud.loop = false;
        //wormHoleAud.Stop();
        if (enterSound != null) wormHoleAud.PlayOneShot(enterSound, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));
        float entryDiff = playerCam.transform.eulerAngles.y - wormZoneShifted.eulerAngles.y; //difference for player to face down wormhole
        playerOBJ.transform.rotation = Quaternion.Euler(playerOBJ.transform.eulerAngles.x, playerOBJ.transform.eulerAngles.y - entryDiff, playerOBJ.transform.eulerAngles.z);
        float startRot = playerCam.transform.eulerAngles.y;//reference the starting rotation of the players camera
        playerRB.isKinematic = true;
        if (wormZoneInstance == null)
        {
            if (FindObjectOfType<WormZone>() != null)
            {
                wormZoneInstance = FindObjectOfType<WormZone>().gameObject;
            }
            else
            {
                wormZoneInstance = Instantiate(wormZoneParticles);//spawns the wormhole instance
            }
            
        }

        wormZoneInstance.transform.position = new Vector3(PC.cam.transform.position.x , PC.cam.transform.position.y, PC.cam.transform.position.z);//moves the wormhole into position
        wormZoneInstance.transform.eulerAngles = new Vector3(0, startRot, 0); // sets the wormhole to be aligned with your face
        wormZoneSpeed = 120;// The speed you fly through the wormholes at

        playerRB.isKinematic = false;
        playerRB.velocity = Vector3.zero;
        playerRB.velocity = wormZoneInstance.transform.forward * wormZoneSpeed;//giving the speed to the player
        yield return new WaitForSeconds(waitTime);//time to wait while traveling down worm hole
        float diff = playerCam.transform.eulerAngles.y - exitPos.transform.eulerAngles.y; // gets the difference in angle between the player and the exit
        float exitDiff = playerCam.transform.eulerAngles.y - startRot;//adjusts the players rotation by the difference in the wormhole and as they turn in the wormhole
        diff = diff - exitDiff;
        triggerScript.locked = true;
        playerOBJ.transform.rotation = Quaternion.Euler(playerOBJ.transform.eulerAngles.x, playerOBJ.transform.eulerAngles.y - diff, playerOBJ.transform.eulerAngles.z);//turns the player to face out of the worhole
        playerOBJ.transform.position = exitPos.position; //takes the player out of the wormhole
        if (PhotonNetwork.IsConnected)
            PlayerController.photonView.RPC("RPC_MakeVisible", RpcTarget.Others);
        playerRB.useGravity = true; //Bring back Gravity
        playerRB.velocity = exitPos.forward * exitSpeed;    //launch out of wormhole
        triggerScript.exiting = false;

        if (luckyProc)
        {
            wormZoneScript.heatVision.SetActive(false);
            wormZoneScript.multiShot.SetActive(false);
            wormZoneScript.invincibility.SetActive(false);
        }
        inZone = false;
        yield return new WaitForSeconds(0.2f);  //Wait for the player to get clear of the wormhole
        lastEntry = startHole;
        triggerScript.exitCam.gameObject.SetActive(false);
        triggerScript.holeAnim.SetBool("Locked", true);
        triggerScript.particle.SetActive(false);
        ActiveWormholes.Remove(this);
        //Destroy(wormZoneInstance);
        // locked = false;   //Unlock the Womrhole circut
        StartCoroutine(TimedLock());
        triggerScript.StartCoroutine(triggerScript.TimedUnlock(6.0f));
        startHole.StartCoroutine(startHole.TimedUnlock(6.0f));
    }

    public IEnumerator TimedLock()
    {
        yield return new WaitForSeconds(6.0f);
        if (lastEntry != null)
        {
            OpenExits.Add(lastEntry);
        }
    }
}
