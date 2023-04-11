using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class UpgradeSpawner : MonoBehaviour
{
    //Objects & Components:
    public static UpgradeSpawner primary;
    private static List<UpgradeSpawner> spawners = new List<UpgradeSpawner>();
    /// <summary>
    /// Position and orientation where upgrades spawn (forward is the direction they are pushed in).
    /// </summary>
    public Transform spawnPoint;
    private Jumbotron jumboScript;
    private AudioSource thisAud;
    private int spawnedPowerups=0;
    private bool Cooldown = false;
    //Settings:
    [Header("Settings:")]
    public PowerUpSettings settings;
    [SerializeField] private string[] upgradeResourceNames = { "PowerUpTest" };
    [Space()]
    [SerializeField] private bool debugSpawn;
    [SerializeField] private bool debugGiveSelectedUpgrade;
    public float SpawnDelay;

    //Runtime Variables:
    public PowerUp.PowerUpType currentPowerUp;

    //EVENTS & COROUTINES:
    public IEnumerator DoPowerUp(PowerUp.PowerUpType powerType, float waitTime)
    {
        currentPowerUp = powerType;
        if (currentPowerUp == PowerUp.PowerUpType.Invulnerability)
        {
            PlayerController.instance.MakeInvulnerable(waitTime);
        }
        else if (currentPowerUp == PowerUp.PowerUpType.HeatVision)
        {
            PlayerController.photonView.RPC("StartMaterialEvent", RpcTarget.All, 1, 2, waitTime);
            foreach (NetworkPlayer otherPlayer in NetworkPlayer.instances)
            {
                otherPlayer.StartMaterialEvent(1, 2, waitTime);
            }
        }

        yield return new WaitForSeconds(waitTime);
        currentPowerUp = PowerUp.PowerUpType.None;
    }

    //RUNTIME METHODS:
    private void Awake()
    {
        primary = this;
        spawners.Add(this);

        if (spawnPoint == null) spawnPoint = transform;
        thisAud = this.GetComponent<AudioSource>();
        jumboScript = FindObjectOfType<Jumbotron>();
    }
    private void OnDestroy()
    {
        if (spawners.Contains(this)) spawners.Remove(this);

        if (currentPowerUp == PowerUp.PowerUpType.HeatVision)
        {
            foreach (var player in NetworkPlayer.instances)
            {
                if (player == NetworkManagerScript.localNetworkPlayer) continue;
            }
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
        if (!Cooldown)
        {
            Cooldown = true;
            StartCoroutine(SpawnAlert());

            StartCoroutine(SpawnPause(SpawnDelay));
            if (primary == this && PhotonNetwork.IsMasterClient)
            {
                SpawnRandomUpgrade();
            }
        }

        //    float LevelTimePercent = jumboScript.GetLevelTimer().LevelTimePercentage();
        //        if (LevelTimePercent > 25 && spawnedPowerups < 1)
        //        {
        //            StartCoroutine(SpawnAlert());
        //            spawnedPowerups++;
        //            if (primary == this && PhotonNetwork.IsMasterClient)
        //            {
        //                SpawnRandomUpgrade();
        //            }

        //        }
        //        else if (LevelTimePercent > 50 && spawnedPowerups < 2)
        //        {
        //            StartCoroutine(SpawnAlert());
        //            spawnedPowerups++;
        //            if (primary == this && PhotonNetwork.IsMasterClient)
        //            {
        //                SpawnRandomUpgrade();
        //            }
        //        }
        //        else if (LevelTimePercent > 75 && spawnedPowerups < 3)
        //        {
        //            StartCoroutine(SpawnAlert());
        //            spawnedPowerups++;
        //            if (primary == this && PhotonNetwork.IsMasterClient)
        //            {
        //                SpawnRandomUpgrade();
        //            }
        //        }
    }

    //FUNCTIONALITY METHODS:
    public void SpawnUpgrade()
    {
       
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
