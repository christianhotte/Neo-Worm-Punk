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
    //Settings:
    [Header("Settings:")]
    public PowerUpSettings settings;
    [SerializeField] private string[] upgradeResourceNames = { "PowerUpTest" };
    [Space()]
    [SerializeField] private bool debugSpawn;
    [SerializeField] private bool debugGiveSelectedUpgrade;

    //Runtime Variables:
    public PowerUp.PowerUpType currentPowerUp;

    //EVENTS & COROUTINES:
    public IEnumerator DoPowerUp(PowerUp.PowerUpType powerType, float waitTime)
    {
        currentPowerUp = powerType;
        if(currentPowerUp == PowerUp.PowerUpType.Invulnerability)
        {
            PlayerController.instance.MakeInvulnerable(settings.InvulnerableTime);
            PlayerController.photonView.RPC("RPC_ChangeMaterial", RpcTarget.Others, 1);
            yield return new WaitForSeconds(settings.InvulnerableTime);
            PlayerController.photonView.RPC("RPC_ChangeMaterial", RpcTarget.Others, -1);
        }
        else if (currentPowerUp == PowerUp.PowerUpType.HeatVision)
        {
            print("Network players being made visible = " + NetworkPlayer.instances.Count);
            foreach (NetworkPlayer player in NetworkPlayer.instances)
            {
                if (player == NetworkManagerScript.localNetworkPlayer) continue;
                player.ChangeNetworkPlayerMaterial(settings.HeatVisMat);
            }
            PlayerController.photonView.RPC("RPC_ChangeMaterial", RpcTarget.Others, 0);
            yield return new WaitForSeconds(settings.HeatVisionTime);
            foreach (NetworkPlayer player in NetworkPlayer.instances)
            {
                if (player == NetworkManagerScript.localNetworkPlayer) continue;
                player.ResetNetworkPlayerMaterials();
            }
            PlayerController.photonView.RPC("RPC_ChangeMaterial", RpcTarget.Others, -1);
        }
        else yield return new WaitForSeconds(waitTime);
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
                if (player == NetworkManagerScript.localNetworkPlayer)
                    continue;
                else
                {
                    player.ResetNetworkPlayerMaterials();
                }
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

        float LevelTimePercent = jumboScript.GetLevelTimer().LevelTimePercentage();
        if (primary == this && PhotonNetwork.IsMasterClient)
        {
            if (LevelTimePercent > 25 && spawnedPowerups < 1)
            {
                StartCoroutine(SpawnAlert());
                SpawnRandomUpgrade();
                spawnedPowerups++;
            }
            else if (LevelTimePercent > 50 && spawnedPowerups < 2)
            {
                StartCoroutine(SpawnAlert());
                SpawnRandomUpgrade();
                spawnedPowerups++;
            }
            else if (LevelTimePercent > 75 && spawnedPowerups < 3)
            {
                StartCoroutine(SpawnAlert());
                SpawnRandomUpgrade();
                spawnedPowerups++;
            }
        }
    }

    //FUNCTIONALITY METHODS:
    public void SpawnUpgrade()
    {
        if (!PhotonNetwork.IsMasterClient) return;


        string resourceName = "PowerUps/" + upgradeResourceNames[Random.Range(0, upgradeResourceNames.Length)];
        PowerUp newUpgrade = PhotonNetwork.Instantiate(resourceName, spawnPoint.position, spawnPoint.rotation).GetComponent<PowerUp>();
        newUpgrade.rb.AddForce(spawnPoint.up * settings.LaunchForce, ForceMode.Impulse);
    }
    public static void SpawnRandomUpgrade()
    {
        if (spawners.Count == 0) return;
        UpgradeSpawner chosenSpawner = spawners[Random.Range(0, spawners.Count)];
        chosenSpawner.SpawnUpgrade();
    }
    public IEnumerator SpawnAlert()
    {
        thisAud.PlayOneShot(settings.AlertSound);
        yield return new WaitForSeconds(0.7f);
        thisAud.PlayOneShot(settings.AlertSound);
        yield return new WaitForSeconds(0.7f);
        thisAud.PlayOneShot(settings.AlertSound);
        yield return new WaitForSeconds(0.7f);
    }
}
