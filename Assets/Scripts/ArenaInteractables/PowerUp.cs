using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PowerUp : Targetable
{
    public enum PowerUpType { None, MultiShot, HeatVision }

    public PowerUpType powerType;
    public float PowerupTime=10.0f;
    public int health = 3;
    public float airDrag = 5;
    public float restingSpeed = 10;
    public float bounceForce = 100;
    private MeshRenderer thisModel;
    private int currentHealth;

    private PhotonView photonView;
    internal Rigidbody rb;

    private protected override void Awake()
    {
        base.Awake();
        photonView = GetComponent<PhotonView>();
        thisModel = GetComponent<MeshRenderer>();
        
        currentHealth = health;

        if (photonView.IsMine)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.drag = 5;
        }
    }
    private void FixedUpdate()
    {
        if (rb.drag > 0 && rb.velocity.magnitude < restingSpeed) rb.drag = 0;
    }
    public override void IsHit(int damage, int playerID)
    {
        if (playerID <= 0) return;

        if (currentHealth - damage <= 0) //Give local player an upgrade
        {
            UpgradeSpawner.primary.StartCoroutine(UpgradeSpawner.primary.DoPowerUp(powerType, PowerupTime));
        }

        Vector3 hitForce = Random.insideUnitSphere.normalized * 100; //TEMP
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
                foreach(var player in NetworkPlayer.instances)
                {
                    if (player == NetworkManagerScript.localNetworkPlayer)
                        continue;
                    else
                    {
                        //Do stuff
                    }
                }
            }
        }
    }
    private void Delete()
    {
        PhotonNetwork.Destroy(GetComponent<PhotonView>());
    }

    //RPC METHODS:
    [PunRPC]
    public void RPC_IsHit(int damage, Vector3 hitForce)
    {
        currentHealth -= damage;
        if (photonView.IsMine)
        {
            if (currentHealth <= 0) Delete();
            else
            {
                rb.drag = airDrag;
                rb.AddForce(hitForce, ForceMode.Impulse);
            }
        }
    }
}
