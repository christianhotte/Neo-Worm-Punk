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
    private int spawnedPowerups=0;
    //Settings:
    [Header("Settings:")]
    public PowerUpSettings settings;
    [SerializeField] private string upgradeResourceName = "PowerUpTest";
    [SerializeField] private float ejectForce = 10;
    [Space()]
    [SerializeField] private bool debugSpawn;

    //Runtime Variables:
    public PowerUp.PowerUpType currentPowerUp;

    //EVENTS & COROUTINES:
    public IEnumerator DoPowerUp(PowerUp.PowerUpType powerType, float waitTime)
    {
        currentPowerUp = powerType;
        yield return new WaitForSeconds(waitTime);
        currentPowerUp = PowerUp.PowerUpType.None;
    }

    //RUNTIME METHODS:
    private void Awake()
    {
        primary = this;
        spawners.Add(this);

        if (spawnPoint == null) spawnPoint = transform;
        jumboScript = FindObjectOfType<Jumbotron>();
    }
    private void OnDestroy()
    {
        if (spawners.Contains(this)) spawners.Remove(this);
    }
    private void Update()
    {
        if (debugSpawn)
        {
            debugSpawn = false;
            SpawnUpgrade();
        }
        float LevelTimePercent = jumboScript.GetLevelTimer().LevelTimePercentage();
        if (primary == this && PhotonNetwork.IsMasterClient)
        {
            if (LevelTimePercent > 25 && spawnedPowerups < 1)
            {
                SpawnRandomUpgrade();
                spawnedPowerups++;
            }
            else if (LevelTimePercent > 50 && spawnedPowerups < 2)
            {
                SpawnRandomUpgrade();
                spawnedPowerups++;
            }
            else if (LevelTimePercent > 75 && spawnedPowerups < 3)
            {
                SpawnRandomUpgrade();
                spawnedPowerups++;
            }
        }
    }

    //FUNCTIONALITY METHODS:
    public void SpawnUpgrade()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PowerUp newUpgrade = PhotonNetwork.Instantiate("PowerUps/" + upgradeResourceName, spawnPoint.position, spawnPoint.rotation).GetComponent<PowerUp>();
        newUpgrade.rb.AddForce(spawnPoint.up * ejectForce, ForceMode.Impulse);
    }
    public static void SpawnRandomUpgrade()
    {
        if (spawners.Count == 0) return;
        UpgradeSpawner chosenSpawner = spawners[Random.Range(0, spawners.Count)];
        chosenSpawner.SpawnUpgrade();
    }
}
