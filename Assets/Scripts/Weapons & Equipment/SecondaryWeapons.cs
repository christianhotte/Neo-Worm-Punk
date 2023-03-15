using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Photon;
using Photon.Pun;
public class SecondaryWeapons : PlayerEquipment
{
    public GameObject blade,hand,ProjectilePrefab,energyBlade,rightGun;
    public GameObject[] StoredShots;
    public Rigidbody playerRB;
    public Transform headpos,attachedHand, bladeSheethed, bladeDeployed,bladeTip,stowedTip,rayStartPoint,bulletSpredPoint,bladeImpulsePosition,EnergyBladeStowed,EnergyBladeExtended;
    public float activationTime, activationSpeed, timeAtSpeed, grindSpeed = 10, grindRange = 2, deploySpeed = 5,blockRadius=4,sawDistance,rayHitDistance,maxSpreadAngle=4,energySpeed=5, maxPossibleHandSpeed=10, minPossibleHandSpeed= 1,maxBladeReductSpeed = 1,explosiveForce =5;
    public AnimationCurve deployMotionCurve, deployScaleCurve, sheathMotionCurve, sheathScaleCurve;
    public bool deployed = false,cooldown=false,grindin=false,deflectin=false;
    public Vector3 prevHandPos, tipPos, storedScale, energyBladeBaseScale, energyTargetScale,energyCurrentScale,energyBladeStartSize;
    [Space()]
    [SerializeField, Range(0, 1)] private float gripThreshold = 1;
    public Projectile projScript;
    private NewShotgunController NSC;
    private bool gripPressed = false,shootin=false,stabbin=false;
    public int shotsHeld = 0, shotCap = 3,shotsToFire,shotsCharged=0;
    public AudioSource sawAud;
    public AudioClip punchSound,chainsawDeploy,chainsawSheethe;
    int num;
    private float prevInterpolant;

    // Start is called before the first frame update
    private protected override void Awake()
    {
        attachedHand = hand.transform;
        sawAud = this.GetComponent<AudioSource>();
        energyBladeBaseScale = energyBlade.transform.localScale;
        energyBlade.transform.localScale = energyBladeStartSize;
        base.Awake();
        NSC = rightGun.GetComponent<NewShotgunController>();
    }
    // Update is called once per frame
    private protected override void Update()
    {
        //if (shotsToFire > 0 && !shootin)
        //{
        //    //StartCoroutine(ShootAbsorbed());
        //}
        //if (deployed)
        //{
        //    tipPos = bladeTip.transform.position;
        //    GameObject[] bullethits = GameObject.FindGameObjectsWithTag("Bullet");
        //    foreach (var hit in bullethits)
        //    {
        //        float bulletDistance = Vector3.Distance(bladeTip.position, hit.transform.position);
        //        projScript = hit.gameObject.GetComponent<Projectile>();
        //        if (hit.gameObject.TryGetComponent(out HookProjectile hook)) continue;
        //        if (bulletDistance <= blockRadius&&shotsHeld<shotCap)
        //        {
        //            if (projScript != null && projScript.originPlayerID == PlayerController.photonView.ViewID) return;
        //            Destroy(hit);
        //            shotsHeld++;
        //            StoredShots[shotsHeld - 1].SetActive(true);
        //            break;
        //        }
        //    }
        //}
        tipPos = bladeTip.transform.position;
        Collider[] hits = Physics.OverlapSphere(tipPos, grindRange, ~LayerMask.GetMask("PlayerWeapon", "Player", "Bullet", "EnergyBlade","Blade", "Hitbox"));
        grindin = false;
        foreach (var hit in hits)
        {
                grindin = true;
                break;            
        }
        if (deployed)
        {
            sawDistance = Vector3.Distance(rayStartPoint.position, bladeTip.position);
            var sawRay = Physics.Raycast(rayStartPoint.position, rayStartPoint.forward, out RaycastHit checkBlade, sawDistance + 1, ~LayerMask.GetMask("PlayerWeapon", "Blade"));// ~LayerMask.GetMask("PlayerWeapon"));
            if (checkBlade.collider == null) return;
            rayHitDistance = 999;
            rayHitDistance = Vector3.Distance(rayStartPoint.position, checkBlade.point);
            if (rayHitDistance < sawDistance&&checkBlade.collider.tag!="Blade"&&checkBlade.collider.tag!="Player"&&checkBlade.collider.tag!="Barrel"&&checkBlade.collider.name != "RightHand Controller"&& checkBlade.collider.name != "LeftHand Controller") // The detection for chainsaw movement
            {
               Debug.Log(checkBlade.collider.name);
                grindin = true;
            }
            else if (rayHitDistance > sawDistance)
            {
                grindin = false;

            }
        }
        if (grindin&&deployed)
        {
            playerRB.velocity = bladeImpulsePosition.forward * grindSpeed;
            
        }
        Vector3 handPos,handMotion;
        handPos = attachedHand.localPosition; 
        handMotion = handPos - prevHandPos;
        float punchSpeed = handMotion.magnitude / Time.deltaTime;
        if (deployed)
        {
            NSC.locked = true;
            energyBlade.SetActive(true);
            float targetInterpolant = Mathf.Clamp01(Mathf.InverseLerp(minPossibleHandSpeed, maxPossibleHandSpeed, punchSpeed));
            if (targetInterpolant < prevInterpolant) targetInterpolant = Mathf.MoveTowards(prevInterpolant, targetInterpolant, maxBladeReductSpeed * Time.deltaTime);

            Vector3 targetPosition = Vector3.Lerp(EnergyBladeStowed.position, EnergyBladeExtended.position, targetInterpolant);
            Vector3 targetScale = Vector3.Lerp(EnergyBladeStowed.localScale, EnergyBladeExtended.localScale, targetInterpolant);
            energyBlade.transform.position = targetPosition;
            energyBlade.transform.localScale = targetScale;
            prevInterpolant = targetInterpolant;
        }
        else
        {
            NSC.locked = false;
            energyBlade.transform.position = EnergyBladeStowed.position; //Vector3.Lerp(energyBlade.transform.localScale, energyBladeStartSize, energySpeed);
            energyBlade.transform.localScale = EnergyBladeStowed.localScale; //Vector3.Lerp(energyBlade.transform.localScale, energyBladeStartSize, energySpeed);
            energyBlade.SetActive(false);
        }
        prevHandPos = handPos;
    }
    private protected override void InputActionTriggered(InputAction.CallbackContext context)
    {
        if (context.action.name == "Grip")
        {
            float gripPosition = context.ReadValue<float>(); //Get current position of trigger as a value
            if (!gripPressed) //Trigger has not yet been pulled
            {
               // Debug.Log(gripPosition);
                if (gripPosition >= gripThreshold) //Trigger has just been pulled
                {
                    gripPressed = true; //Indicate that trigger is now pulled
                    Deploy();
                }
            }
            else //Trigger is currently pulled
            {
                if (gripPosition < gripThreshold) //Trigger has been released
                {
                    gripPressed = false; //Indicate that trigger is now released
                    Sheethe();
                }
            }
        }
    }
    public void Deploy()
    {
        blade.transform.position = Vector3.MoveTowards(blade.transform.position, bladeDeployed.transform.position, deploySpeed);
        blade.transform.localRotation = bladeDeployed.transform.localRotation;
        deployed = true;
        sawAud.PlayOneShot(chainsawDeploy, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f));       
        StartCoroutine(StartCooldown());
    }
    public void Sheethe()
    {
        blade.transform.position = Vector3.MoveTowards(blade.transform.position, bladeSheethed.transform.position, deploySpeed);
        deployed = false;
        sawAud.PlayOneShot(chainsawSheethe, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f));
        StartCoroutine(StartCooldown());
        if (shotsHeld > 0)
        {
            ClearAbsorbed();
        }
    }
    public IEnumerator StartCooldown()
    {
        cooldown = true;
        yield return new WaitForSeconds(1.0f);
        cooldown = false;
    }
    public IEnumerator ShootAbsorbed()
    {
        shootin = true;
        for (; shotsToFire > 0; shotsToFire--)
        {            
            Vector3 exitAngles = Random.insideUnitCircle * maxSpreadAngle;
            bulletSpredPoint.localEulerAngles = new Vector3(bladeTip.position.x + exitAngles.x, bladeTip.position.y + exitAngles.y, bladeTip.position.z + exitAngles.z);
            //NOTE: Hotte modified this so that it'd work with the new slightly different projectile deployment system
            Projectile projectile = PhotonNetwork.Instantiate("Projectiles/HotteProjectile1", bulletSpredPoint.position, bulletSpredPoint.rotation).GetComponent<Projectile>(); //Instantiate projectile on network
            projectile.photonView.RPC("RPC_Fire", RpcTarget.All, bulletSpredPoint.position, bulletSpredPoint.rotation, PlayerController.photonView.ViewID);                     //Initialize all projectiles simultaneously
            StoredShots[shotsToFire - 1].SetActive(false);
            yield return new WaitForSeconds(.05f);
        }
        shootin = false;
    }
    public void ClearAbsorbed()
    {
        float prevExplosiveForce=explosiveForce;
        for(; shotsHeld > 0; shotsHeld--)
        {
            StoredShots[shotsHeld - 1].SetActive(false);
            explosiveForce *= 1.5f;
        }
        playerRB.velocity = stowedTip.forward * -explosiveForce;
        explosiveForce = prevExplosiveForce;
    }
    public IEnumerator DeflectTime()
    {
        deflectin = true;
        yield return new WaitForSeconds(0.3f);
        deflectin = false;
         Sheethe();
    }
    public IEnumerator BladeSlice()
    {      
        yield return new WaitForSeconds(1.5f);
        energyBlade.SetActive(false);
        energyBlade.transform.localScale = energyBladeStartSize;
        stabbin = false;
    }
}
