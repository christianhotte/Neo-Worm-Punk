using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RocketBoost : PlayerEquipment  //renametograpple
{
    public Transform rocketTip,rocketTipReset,hookedPos,hookStartPos;
    public GameObject HookModel,HookInstance,Player,lineCheckModel,lineCheckInstance;
    public Vector3 rayHitPoint, rocketTipStart;
    public float rocketPower=20,grappleDistance=10,releasePower=5,realDistance=0,hookSpeed=5, rayToHitDistance;
    public bool Grapplin = false,grappleCooldown=true,grapplinWall=false,shootinHook=false,Pullin=false,checkinLine=false;
    public int hitType = 0;
    public MeshRenderer Rocket;
    public HookDetector hookScript;
    public LineRenderer cable;
    public SecondaryWeapons SawScript;
    private HookLineChecker HLC;
    public AudioSource grapAud;
    public AudioSource hookHit;
    [Space()]
    public float sidePullStrength = 5f, fowardPullStrength = 10;

    Vector3 hitHandPos;

    // Start is called before the first frame update
    private protected override void Awake()
    {
        Destroy(this.gameObject.GetComponent<LineRenderer>());
        base.Awake();
       
    }
    private protected override void FixedUpdate()
    {

       
        base.FixedUpdate();
    }
    // Update is called once per frame
    private protected override void Update()
    {
        // if(realDistance!= null) Debug.Log(realDistance);

        

        if (shootinHook)
        {
            if (HookInstance != null) //Put this here to MissingReferenceException error
            {
                if (!checkinLine)
                {
                    //checkinLine = true;
                    //lineCheckInstance = Instantiate(lineCheckModel);
                    //HLC = lineCheckInstance.GetComponent<HookLineChecker>();
                    //HLC.GrapScrip = this.GetComponent<RocketBoost>();
                    //lineCheckInstance.transform.position = hookStartPos.position;
                }
            }

        }
        if (HookInstance != null)
        {
            realDistance = Vector3.Distance(rocketTip.position, HookInstance.transform.position); // gets distance to the hit
            rayToHitDistance = Vector3.Distance(rocketTip.position, HookInstance.transform.position);
            RaycastHit checkSaw;
            var sawRay = Physics.Raycast(rocketTip.position, rocketTip.forward, out checkSaw, 9999999, ~LayerMask.GetMask("PlayerWeapon", "Player", "Bullet", "EnergyBlade", "Hitbox"));
            rayToHitDistance = 999;

            if (checkSaw.collider == null) return;
            rayToHitDistance = Vector3.Distance(rocketTip.position, checkSaw.transform.position);
            if (checkSaw.collider.gameObject.layer == 10)
            {
                SawScript = checkSaw.collider.GetComponentInParent<SecondaryWeapons>();


                if (SawScript.deployed)
                {
                    Debug.Log("cut");
                    GrappleStop();
                }
            }
            if (rayToHitDistance < realDistance && grappleCooldown)
            {
                Debug.Log(checkSaw.collider.name);
                HookInstance.transform.position = checkSaw.point;
            }
        }
        if (Grapplin && !grappleCooldown)
        {
            Vector3 newHandPos = player.xrOrigin.transform.InverseTransformPoint(targetTransform.position);
            Vector3 diff = hitHandPos - newHandPos;
            float forwardPullAmt = Vector3.Project(diff, targetTransform.forward).magnitude;
            float sidePullAmt = Vector3.ProjectOnPlane(diff, targetTransform.forward).magnitude;

            Vector3 newPlayerVelocity = (HookInstance.transform.position - transform.position).normalized * rocketPower;
            newPlayerVelocity += diff.normalized * (forwardPullAmt * fowardPullStrength);
            newPlayerVelocity += diff.normalized * (sidePullAmt * sidePullStrength);
            playerBody.velocity = newPlayerVelocity;//move the player
        }
        if (!grappleCooldown && realDistance < 2.5 && grapplinWall)
        {
            playerBody.velocity = (rocketTip.up * releasePower); //the bounce after grapple is released
            GrappleStop();
        }
        else if (!grappleCooldown && realDistance < 2.5 && !grapplinWall)
        {
            // playerBody.velocity = (rocketTip.forward * releasePower); //the bounce after grapple is released
            GrappleStop();
        }


        base.Update();
    }

    //This is the new system for processing equipment inputs, equipment now automatically subscribes to the event for you so now you don't have to manually assign it in the inspector
    private protected override void InputActionTriggered(InputAction.CallbackContext context)
    {
        if (context.action.name == "Grip") OnGripInput(context);
    }
    public void OnGripInput(InputAction.CallbackContext context)
    {
        //Vector3 hitNormal = Vector3.up;
        //AnimationCurve bounceCurve = new AnimationCurve();

        //float groundness = 1 - (Mathf.Min(Vector3.Angle(Vector3.up, hitNormal), 90) / 90);
        //float realReleasePower = bounceCurve.Evaluate(groundness) * releasePower;

        float gripValue = context.ReadValue<float>();//value 0-1 of how much pulled
        // print("Context: " + context.phase + "| Value: " + gripValue);
        if (gripValue > 0.95 && !Grapplin)//is 95% pressed or more grapple
        {
            GrappleStart();
        }
        if (gripValue == 0 && Grapplin)//if released stop grapple
        {
            GrappleStop();
        }
        
    }
    public void GrappleStart()
    {
        if (!Grapplin)
        {
            RaycastHit hit;
            var ray = Physics.Raycast(rocketTip.position, rocketTip.forward, out hit, grappleDistance);
           // if (hit.collider == null) return;

            HookInstance = Instantiate(HookModel);
            HookDetector detector = HookInstance.GetComponent<HookDetector>();
            detector.RBScript = this.gameObject.GetComponent<RocketBoost>();
            HookInstance.transform.position = hookStartPos.position;
           // hookedPos = hit.transform;
           // Debug.Log(hit.normal);
           // rayHitPoint = hit.point;
            // StartCoroutine(GrappleLaunch());
            
            //cable = this.gameObject.AddComponent<LineRenderer>();
            //cable.startColor = Color.black;
            //cable.endColor = Color.black;
            //cable.material = new Material(Shader.Find("Sprites/Default"));
            //cable.startWidth = 0.01f;
            //cable.endWidth = 0.01f;
            Grapplin = true;
            shootinHook = true;
            if (hit.collider == null) return;
            //if (hit.normal.y!=0&&hit.normal!=Vector3.zero)//if hit ground, also stopps iff null
            //{
            //    grapplinWall = false;
            //}
            //else if (hit.normal != Vector3.zero)//if hit wall, also stopps iff null
            //{
            //    grapplinWall = true;
            //}
        }
      
    }
    public void HookHit()
    {
        hitHandPos = player.xrOrigin.transform.InverseTransformPoint(targetTransform.position);
        shootinHook = false;
    }
    public void GrappleStop()
    {
        //Debug.Log("release");
        //Rocket.enabled = true;
        Destroy(this.gameObject.GetComponent<LineRenderer>());
        hitType = 0;
      //  Rocket.enabled = true;
        Grapplin = false;
        grappleCooldown = true;
        rocketTip.LookAt(rocketTipReset);
        Destroy(HookInstance);


    }
    public IEnumerator GrappleLaunch()
    {
        yield return new WaitForSeconds(0.35f);//delay before grapple impulse begins after detecting hit
       // Rocket.enabled = false;
        grappleCooldown = false;

    }
}
