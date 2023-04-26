using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
public class PowerUp : Targetable
{
    public enum PowerUpType { None, MultiShot, HeatVision, InfiniShot, Invulnerability }

    public PowerUpType powerType;
    public float PowerupTime=10.0f;
    public int health = 3;
    public float airDrag = 5;
    public float restingSpeed = 10;
    public float bounceForce = 100;
    private MeshRenderer thisModel;
    private int currentHealth;
    private AudioSource powerUpAud;
    public AudioClip powerUpHit;
    private PhotonView photonView;
    internal Rigidbody rb;
    public bool dummyPowerup = false;
    internal float roomPowerUpTime;
    private protected override void Awake()
    {
        base.Awake();
        photonView = GetComponent<PhotonView>();
        thisModel = GetComponent<MeshRenderer>();
        powerUpAud = this.GetComponent<AudioSource>();
        currentHealth = health;
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (photonView.IsMine)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.drag = 5;
        }

    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PhotonNetwork.InRoom)
        {
            roomPowerUpTime = (float)PhotonNetwork.CurrentRoom.CustomProperties["UpgradeLength"];
        }
    }
    private void FixedUpdate()
    {
        if (photonView.IsMine && rb.drag > 0 && rb.velocity.magnitude < restingSpeed) rb.drag = 0;
    }
    public override void IsHit(int damage, int playerID, Vector3 velocity)
    {
        if (playerID <= 0) return;

        if (currentHealth - damage <= 0) //Give local player an upgrade
        {
            UpgradeSpawner.primary.StartCoroutine(UpgradeSpawner.primary.DoPowerUp(powerType, roomPowerUpTime));
        }

        Vector3 hitForce = velocity.normalized * bounceForce;
        photonView.RPC("RPC_IsHit", RpcTarget.All, damage, hitForce);
    }
    private void OnCollisionEnter(Collision collision)
    {
  
        if (PhotonNetwork.IsMasterClient)
        {
            rb.drag = airDrag;
            foreach (ContactPoint point in collision.contacts)
            {
                Vector3 newVel = (2 * (Vector3.Dot(rb.velocity, Vector3.Normalize(point.normal))) * Vector3.Normalize(point.normal) - rb.velocity) * -1;
                newVel = newVel.normalized * bounceForce;
                rb.velocity = newVel;
               
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (dummyPowerup)
        {
            if (other.name == "XR Origin")
            {
                UpgradeSpawner.primary.StartCoroutine(UpgradeSpawner.primary.DoPowerUp(powerType, roomPowerUpTime));
                PlayerController.instance.combatHUD.AddToUpgradeInfo(powerType, roomPowerUpTime);   //Adds the power up to the player's HUD
                this.gameObject.SetActive(false);
            }
        }
    }
    private void Delete()
    {
        PlayerController.instance.combatHUD.AddToUpgradeInfo(powerType, roomPowerUpTime);   //Adds the power up to the player's HUD
        PhotonNetwork.Destroy(GetComponent<PhotonView>());
    }

    //RPC METHODS:
    [PunRPC]
    public void RPC_IsHit(int damage, Vector3 hitForce)
    {
        currentHealth -= damage;
        if (photonView.IsMine)
        {
           // powerUpAud.PlayOneShot(powerUpHit);   THis is only client side
            if (currentHealth <= 0) Delete();
            else
            {
                rb.drag = airDrag;
                rb.AddForce(hitForce, ForceMode.Impulse);
            }
        }
    }
}
