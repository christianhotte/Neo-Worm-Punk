using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon;
using Photon.Pun;

public class FirstGunScript : PlayerEquipment
{
    public ConfigurableJoint breakJoint;
    private PlayerInput input;
    public GameObject projectile;
    //private GameObject player;
    public bool Ejecting = false, Cooldown=false;
    public int Barrels = 2, shotsLeft,pellets=30;
    public float maxSpreadAngle=7,projectileSpeed=5,gunCooldown=1,gunBoost=20,recoilForce=20;
    [SerializeField, Range(0, 90), Tooltip("Angle at which barrels will rest when breach is open")] private float breakAngle;
    public Transform BarreTran;

    private protected override void Awake()
    {
        input = GetComponentInParent<PlayerInput>();
        //player = GameObject.Find("XR Origin");
        base.Awake();
        shotsLeft = Barrels;
    }

    // Start is called before the first frame update
    private protected override void Start()
    {
        base.Start();
        //StartCoroutine(WaitandClose());
        SoftJointLimit angleCap = new SoftJointLimit();
        angleCap = breakJoint.highAngularXLimit;
        Debug.Log(angleCap.limit);
    }

    // Update is called once per frame
    private protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    public void Eject()
    {
        Ejecting = true;
        Debug.Log("ejecting");
        breakJoint.angularXMotion = ConfigurableJointMotion.Limited;
        breakJoint.targetRotation = Quaternion.Euler(Vector3.right * breakAngle);
    }
    public void CloseBreach()
    {
        if (!Ejecting) return;
        breakJoint.targetRotation = Quaternion.Euler(Vector3.zero);
        //float num = breakJoint.co
        breakJoint.angularXMotion = ConfigurableJointMotion.Locked; 
        Ejecting = false;
        shotsLeft = Barrels;
    }   

    public IEnumerator WaitandClose()
    {
        yield return new WaitForSeconds(5f);
        Debug.Log("closing");
        CloseBreach();
    }
    
    // Calls the fire method.
    public void Fire()
    {
        //Validate firing sequence:
        if (Cooldown) return;
        if (Ejecting) return;
        if (!player.GetComponentInParent<PlayerController>().InCombat()) return;

        StartCoroutine(CooldownTime(gunCooldown));
        Vector3 SpawnPoint = BarreTran.localEulerAngles;
        List<Projectile> projectiles = new List<Projectile>();
        if (shotsLeft <= 0) return;

        for (int i = 0; i < pellets; i++)
        {
            Vector3 exitAngles = Random.insideUnitCircle * maxSpreadAngle;
            BarreTran.localEulerAngles = new Vector3(SpawnPoint.x + exitAngles.x, SpawnPoint.y + exitAngles.y, SpawnPoint.z + exitAngles.z);
            Vector3 projVel = BarreTran.forward * projectileSpeed;

            Projectile newProjectile = PhotonNetwork.Instantiate("DavidProjectile1", BarreTran.position, Quaternion.Euler(-BarreTran.forward)).GetComponent<Projectile>();
            projectiles.Add(newProjectile);
            newProjectile.transform.position = BarreTran.transform.position;
            //float newProjSpeed = newProjectile.velocity.magnitude;
            //newProjectile.velocity = -BarreTran.forward * newProjSpeed;
            Rigidbody playerrb = player.GetComponent<Rigidbody>();
            Rigidbody gunrb = gameObject.GetComponent<Rigidbody>();
            Vector3 gunTorque = recoilForce * BarreTran.up;
            gunrb.AddForceAtPosition(gunTorque, BarreTran.position, ForceMode.Impulse);
            playerrb.velocity = BarreTran.forward * gunBoost;
        }

        //Cleanup:
        Cooldown = true;
        shotsLeft--;
        Debug.Log("Shots fired");
    }
    public IEnumerator CooldownTime(float time)
    {
        yield return new WaitForSeconds(time);
        Cooldown = false;
    }
}