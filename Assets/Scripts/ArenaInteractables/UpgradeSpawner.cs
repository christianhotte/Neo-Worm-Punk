using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class UpgradeSpawner : MonoBehaviour
{
    //Objects & Components:
    public static UpgradeSpawner primary;
    private static List<UpgradeSpawner> spawners = new List<UpgradeSpawner>();
    public List<WormHoleTrigger> WormholeTriggers = new List<WormHoleTrigger>();
    /// <summary>
    /// Position and orientation where upgrades spawn (forward is the direction they are pushed in).
    /// </summary>
    public Transform spawnPoint;
    private Jumbotron jumboScript;
    private AudioSource thisAud;
    private int spawnedPowerups=0,totalLevelTime,powerDelay;
    public int randomIndex, randRange;
    private bool Cooldown = false, IsUpgradeActive;
    private LevelTimer Timer;
    private RoundManager roundTimer;

    //Settings:
    [Header("Settings:")]
    public PowerUpSettings settings;

    [SerializeField] private string[] upgradeResourceNames = { "PowerUpTest" };
    [Space()]
    [Header("Debug Options:")]
    public PowerUp.PowerUpType currentPowerUp;
    [SerializeField] private bool debugSpawn;
    [SerializeField] private bool debugGiveSelectedUpgrade;
    [Space(10)]

    [Header("Round Settings:")]
    [SerializeField, Tooltip("The amount of time that we wait to start spawning upgrades as soon as the round starts.")] private float startRoundBuffer;
    [SerializeField, Tooltip("The amount of time left in a round when upgrades stop spawning.")] private float endRoundBuffer;

    //Runtime Variables:
    internal float powerupsPerMin,powerupTime;
    
    private float timeUntilNextUpgrade = 0;

    //EVENTS & COROUTINES:
    public IEnumerator DoPowerUp(PowerUp.PowerUpType powerType, float waitTime)
    {
        currentPowerUp = powerType;
        switch (currentPowerUp)
        {
            case PowerUp.PowerUpType.Invulnerability:
                PlayerController.instance.MakeInvulnerable(waitTime / 2);
                break;
            case PowerUp.PowerUpType.HeatVision:
                PlayerController.photonView.RPC("StartMaterialEvent", RpcTarget.All, 1, 2, waitTime);
                foreach (NetworkPlayer otherPlayer in NetworkPlayer.instances)
                {
                    otherPlayer.StartMaterialEvent(1, 2, waitTime);
                }
                break;
        }

        PlayerController.instance.combatHUD.AddToUpgradeInfo(powerType, waitTime);   //Adds the power up to the player's HUD

        yield return new WaitForSeconds(waitTime);
        currentPowerUp = PowerUp.PowerUpType.None;
    }

    //RUNTIME METHODS:
    private void Awake()
    {
        primary = this;
        spawners.Add(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (spawnPoint == null) spawnPoint = transform;
        thisAud = this.GetComponent<AudioSource>();
        jumboScript = FindObjectOfType<Jumbotron>();
        Timer = FindObjectOfType<Jumbotron>().GetComponentInChildren<LevelTimer>();
        PhotonView masterPV = PhotonView.Find(17);
        roundTimer = masterPV.GetComponent<RoundManager>(); 
       // totalLevelTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"];
        //powerDelay = totalLevelTime / (powerupsPerMin * 4);
      //  print("PowerDelay = " + (int)PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"]);
    }
    private void OnDestroy()
    {
        if (spawners.Contains(this)) spawners.Remove(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (currentPowerUp == PowerUp.PowerUpType.HeatVision)
        {
            foreach (var player in NetworkPlayer.instances)
            {
                if (player == NetworkManagerScript.localNetworkPlayer) continue;
            }
        }
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        WormholeTriggers.AddRange(FindObjectsOfType<WormHoleTrigger>());
        randRange = WormholeTriggers.Count;
        int randomIndex = Random.Range(0, randRange);
        if (PhotonNetwork.InRoom)
        {
            IsUpgradeActive = (bool)PhotonNetwork.CurrentRoom.CustomProperties["UpgradesActive"];
            powerupsPerMin = (float)PhotonNetwork.CurrentRoom.CustomProperties["UpgradeFrequency"];
            timeUntilNextUpgrade = 0f;
        }
    }
       
        private void Update()
    {
        if (debugSpawn)
        {
            StartCoroutine(SpawnAlert());
            debugSpawn = false;
            SpawnUpgrade();
        }
        if (debugGiveSelectedUpgrade)
        {
            debugGiveSelectedUpgrade = false;
            StartCoroutine(DoPowerUp(currentPowerUp, 15));
        }
        //if (!Cooldown)
        //{
        //    Cooldown = true;
        //    StartCoroutine(SpawnAlert());

        //    StartCoroutine(SpawnPause(SpawnDelay));
        //    if (primary == this && PhotonNetwork.IsMasterClient)
        //    {
        //        SpawnRandomUpgrade();
        //    }
        //}

        if (IsUpgradeActive)
        {
            //If the amount of seconds in the room has not reached the start buffer and the remaining time left is greater than the end round buffer, increment the spawn timer
            if (roundTimer.GetCurrentRoundTime() > startRoundBuffer && roundTimer.GetTotalSecondsLeft() > endRoundBuffer)
            {
                if (primary == this)
                {

                    if (timeUntilNextUpgrade <= 0)
                    {
                        StartCoroutine(SpawnAlert());
                        if (PhotonNetwork.IsMasterClient)
                        {
                            SpawnRandomUpgrade();
                            timeUntilNextUpgrade = (powerupsPerMin * 60f);
/*                            if ((powerupsPerMin * 60f) >= roundTimer.GetTotalSecondsLeft())
                            {
                                timeUntilNextUpgrade = (powerupsPerMin * 60f);
                            }
                            else
                            {
                                timeUntilNextUpgrade = -1;
                            }*/
                        }
                        else
                        {
                            timeUntilNextUpgrade = (powerupsPerMin * 60f);
                        }
                    }
                    else
                        timeUntilNextUpgrade -= Time.deltaTime;

                }
            }
        }
    }

    //FUNCTIONALITY METHODS:
    public void SpawnUpgrade()
    {
        int randomIndex = Random.Range(0, randRange);
        spawnPoint = WormholeTriggers[randomIndex].transform;
        if (!PhotonNetwork.IsMasterClient) return; 
        string resourceName = "PowerUps/" + upgradeResourceNames[Random.Range(0, upgradeResourceNames.Length)];
        PowerUp newUpgrade = PhotonNetwork.Instantiate(resourceName, spawnPoint.position, spawnPoint.rotation).GetComponent<PowerUp>();
        newUpgrade.rb.AddForce(spawnPoint.up * settings.LaunchForce, ForceMode.Impulse);
    }
    public IEnumerator SpawnPause(float time)
    {
        yield return new WaitForSeconds(time);
        Cooldown = false;
    }
    public IEnumerator PPMSpawn(int delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        StartCoroutine(SpawnAlert());

        if (primary == this && PhotonNetwork.IsMasterClient)
        {
            SpawnRandomUpgrade();
        }
        if (Timer.GetTotalSecondsLeft() <= (float)delayTime)
        {
            StopCoroutine(PPMSpawn(delayTime));
        }
        else
        {
            StartCoroutine(PPMSpawn(delayTime));
        }
    }
    public static void SpawnRandomUpgrade()
    {
        if (spawners.Count == 0) return;
        UpgradeSpawner chosenSpawner = spawners[Random.Range(0, spawners.Count)];
        chosenSpawner.SpawnUpgrade();
    }
    public IEnumerator SpawnAlert()
    {
        for (int x = 0; x < 3; x++)
        {
            yield return new WaitForSeconds(0.7f);
            if (primary == this) thisAud.PlayOneShot(settings.AlertSound, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));
        }
    }
}
