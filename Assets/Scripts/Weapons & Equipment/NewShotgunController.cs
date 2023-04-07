using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using CustomEnums;
using Photon.Pun;

/// <summary>
/// New controller for player shotgun, fires a single slug.
/// </summary>
public class NewShotgunController : PlayerEquipment
{
    //Objects & Components:
    internal ConfigurableJoint breakJoint;  //Joint controlling weapon's break action
    internal NewShotgunController otherGun; //Shotgun in the player's other hand

    private ParticleSystem shotParticles; //Particle system which activates each time weapon is fired

    //Settings:
    [SerializeField, Tooltip("Transforms representing position and direction of weapon barrels.")] private Transform[] barrels;
    [Tooltip("Settings object which determines general weapon behavior.")]                         public ShotgunSettings gunSettings;
    [Header("Animated Components:")]
    [SerializeField, Tooltip("The forward part of the weapon which jolts backward when weapon fires.")] private Transform reciprocatingAssembly;
    [SerializeField, Tooltip("Part on the side of the weapon which indicates barrel load status.")]     private Transform leftEjectorAssembly;
    [SerializeField, Tooltip("Part on the side of the weapon which indicates barrel load status.")]     private Transform rightEjectorAssembly;
    [SerializeField, Tooltip("Pin on the back of the weapon which indicates when it is fired.")]        private Transform leftFiringPin;
    [SerializeField, Tooltip("Pin on the back of the weapon which indicates when it is fired.")]        private Transform rightFiringPin;
    [Header("Debug Settings:")]
    [SerializeField, Tooltip("Makes it so that weapon fires from the gun itself and not on the netwrok.")] private bool debugFireLocal = false;

    //Runtime Variables:
    private int currentBarrelIndex = 0;  //Index of barrel currently selected as next to fire
    private int loadedShots = 0;         //Number of shots weapon is able to fire before needing to reload again
    internal bool breachOpen = false;    //Indicates whether or not weapon breach is swung open
    private float breachOpenTime = 0;    //Time breach has been open for (zero if breach is closed)
    private float timeSinceFiring = 0;   //Time since weapon was last fired
    private bool triggerPulled = false;  //Whether or not the trigger is currently pulled
    private float doubleFireWindow = 0;  //Above zero means that weapon has just been fired and firing another weapon will cause a double fire
    internal bool locked = false;        //Lets the other equipment disable the guns
    private float recoilRotOffset = 0;   //Current rotational offset along the X axis due to weapon recoil sequence
    private string projResourceName;     //Calculated at start, the name/directory of projectile this weapon uses in the Resources folder

    internal bool reverseFiring = false;     //True when reverse fire input is pressed
    private float reverseTime = 0;           //Time until reverse fire phase is complete (used to make sure guns always rotate in the correct direction)
    private float reverseButtonCooldown = 0; //Prevents the reverse button from being spammed while above zero
    private bool flipSwingUp = true;         //Indicates whether player swung up or down while pressing reverse button (system defaults to upward)

    private Vector3 baseScale;       //Initial scale of weapon
    private Vector3 baseReciproPos;  //Base position of reciprocating barrel assembly
    private Vector3 baseEjectorLPos; //Base position of left ejector
    private Vector3 baseEjectorRPos; //Base position of right ejector
    private Vector3 basePinPos;      //Base position of both firing pins

    //Events & Coroutines:
    /// <summary>
    /// Weapon recoil procedure (triggers a number of effects throughout recoil phase).
    /// </summary>
    public IEnumerator DoRecoil()
    {
        //Initialize:
        SetWobble(gunSettings.triggerDamper / 2);                                     //Super lock down weapon swinginess
        Vector3 maxOffset = gunSettings.recoilDistance * Vector3.back;                //Get greatest offset value which weapon will reach during recoil phase
        //Quaternion maxRotation = Quaternion.Euler(-gunSettings.recoilRotation, 0, 0); //Get greatest rotation value which weapon will reaach during recoil phase
        Vector3 maxScale = gunSettings.recoilScale * baseScale;                       //Get greatest scale value which weapon will reach during recoil phase
        Vector3 barrelTargetPos = (gunSettings.barrelReciprocationDistance * Vector3.back) + baseReciproPos; //Get target position barrels reciprocate to

        //Linear recoil & scaling procedure:
        for (float totalTime = 0; totalTime < gunSettings.recoilTime; totalTime += Time.fixedDeltaTime) //Iterate once each fixed update for duration of recoil phase
        {
            //Initialize:
            float timeValue = totalTime / gunSettings.recoilTime; //Get value representing progression through recoil phase

            //Main recoil animations:
            SetWobble(Mathf.Lerp(gunSettings.triggerDamper / 2, jointSettings.angularDriveDamper, timeValue * 2));                                             //Slowly release weapon tightness so that recoil wobble may begin
            currentAddOffset = Vector3.LerpUnclamped(Vector3.zero, maxOffset, gunSettings.recoilCurve.Evaluate(timeValue));                                    //Modify follower offset so that weapon is moved backwards/forwards
            recoilRotOffset = Mathf.LerpUnclamped(0, -gunSettings.recoilRotation, gunSettings.recoilRotationCurve.Evaluate(timeValue));                        //Modify follower rotation so that weapon is rotated upwards
            //currentAddRotOffset = Quaternion.LerpUnclamped(Quaternion.identity, maxRotation, gunSettings.recoilRotationCurve.Evaluate(timeValue)).eulerAngles; //Modify follower rotation so that weapon is rotated upwards
            transform.localScale = Vector3.LerpUnclamped(baseScale, maxScale, gunSettings.recoilScaleCurve.Evaluate(timeValue));                               //Adjust scale based on settings and curve

            //Secondary animations:
            reciprocatingAssembly.localPosition = Vector3.LerpUnclamped(baseReciproPos, barrelTargetPos, gunSettings.barrelReciproCurve.Evaluate(timeValue)); //Move reciprocating barrel assembly back and forth along curve

            //Cleanup:
            yield return new WaitForFixedUpdate(); //Wait for next fixed update
        }

        //Cleanup:
        SetWobble(jointSettings.angularDriveDamper);          //Return wobble to its base amount
        reciprocatingAssembly.localPosition = baseReciproPos; //Return reciprocating barrels to base position
        currentAddOffset = Vector3.zero;                      //Return system to base position
        recoilRotOffset = 0;                                  //Return system to base rotation
        transform.localScale = baseScale;                     //Reset weapon to base scale
    }
    /// <summary>
    /// Moves ejector on designated side to forward or backward position.
    /// </summary>
    /// <param name="side">Which ejector to move, moves both if side is None.</param>
    /// <param name="forward">True moves ejector(s) forward, false moves it/them backward.</param>
    public IEnumerator MoveEjector(Handedness side, bool forward)
    {
        //validity checks:
        bool leftInPlace = (leftEjectorAssembly.localPosition == baseEjectorLPos) != forward;   //Check whether or not left ejector is in target position
        bool rightInPlace = (rightEjectorAssembly.localPosition == baseEjectorRPos) != forward; //Check whether or not right ejector is in target position
        if (side == Handedness.None && leftInPlace && rightInPlace) yield return null; //End if both ejectors are already in target positions
        if (side == Handedness.Left && leftInPlace) yield return null;                 //End if left ejector is already in target position
        if (side == Handedness.Right && rightInPlace) yield return null;               //End if right ejector is already in target position
        Vector3 originAdd = forward ? Vector3.zero : (gunSettings.ejectorTraverseDistance * Vector3.forward); //Get amount to add to origin for each ejector
        Vector3 targetAdd = forward ? (gunSettings.ejectorTraverseDistance * Vector3.forward) : Vector3.zero; //Get amount to add to target for each ejector

        for (float totalTime = 0; totalTime < gunSettings.ejectorTraverseTime; totalTime += Time.fixedDeltaTime) //Iterate once each fixed update for duration of ejector phase
        {
            //Initialization:
            float timeValue = totalTime / gunSettings.ejectorTraverseTime;      //Get value representing progression through recoil phase
            Vector3 currentAdd = Vector3.Lerp(originAdd, targetAdd, timeValue); //Get current positional value to add to base ejector positions

            //Movement:
            if ((side == Handedness.Left || side == Handedness.None) && !leftInPlace) leftEjectorAssembly.localPosition = baseEjectorLPos + currentAdd;    //Move left side ejector
            if ((side == Handedness.Right || side == Handedness.None) && !rightInPlace) rightEjectorAssembly.localPosition = baseEjectorRPos + currentAdd; //Move right side ejector
        }

        //Cleanup:
        if ((side == Handedness.Left || side == Handedness.None) && !leftInPlace) leftEjectorAssembly.localPosition = baseEjectorLPos + targetAdd;    //Make sure left side ejector is at its destination
        if ((side == Handedness.Right || side == Handedness.None) && !rightInPlace) rightEjectorAssembly.localPosition = baseEjectorRPos + targetAdd; //Make sure right side ejector is at its destination
    }

    //RUNTIME METHODS:
    private protected override void Awake()
    {
        //Validation & component get:
        if (barrels.Length == 0) Debug.LogWarning("Shotgun " + " has no assigned barrel transforms!");                                                                        //Warn player if no barrels have been assigned to shotgun
        breakJoint = GetComponentInChildren<ConfigurableJoint>(); if (breakJoint == null) { Debug.LogWarning("Shotgun does not have Configurable Joint for break action!"); } //Get configurable break joint (before spawning another one in base method)
        shotParticles = GetComponentInChildren<ParticleSystem>();                                                                                                             //Get particle system from children (fix this if multiple systems end up getting added)
        base.Awake();                                                                                                                                                         //Run base awake method

        //Check settings:
        if (gunSettings == null) //No weapon settings were provided
        {
            Debug.Log("Weapon " + name + " is missing Gun Settings, using system defaults.");        //Log warning in case someone forgot
            gunSettings = (ShotgunSettings)Resources.Load("DefaultSettings/DefaultShotgunSettings"); //Use default settings from Resources
        }

        //Setup runtime variables:
        //positionMemoryReference = barrels[0];                                   //Use first barrel as position memory reference
        projResourceName = "Projectiles/" + gunSettings.projectileResourceName; //Put together resource name from setting
        loadedShots = gunSettings.maxLoadedShots;                               //Fully load weapon on start
        baseScale = transform.localScale;                     //Get base scale
        baseReciproPos = reciprocatingAssembly.localPosition; //Get base local position of reciprocating barrel assembly
        baseEjectorLPos = leftEjectorAssembly.localPosition;  //Get base local position of left ejector assembly
        baseEjectorRPos = rightEjectorAssembly.localPosition; //Get base local position of right ejector assembly
        basePinPos = leftFiringPin.localPosition;             //Get base local position of both firing pins
    }
    private protected override void Start()
    {
        base.Start(); //Call base start stuff

        //Late component get:
        if (player != null) //Only try this if weapon is attached to a player
        {
            foreach (PlayerEquipment equipment in player.attachedEquipment) //Iterate through equipment currently attached to player
            {
                if (equipment.TryGetComponent(out NewShotgunController other) && other != this) otherGun = other; //Try to get other shotgun controller
            }
        }
    }
    private protected override void Update()
    {
        //Initialization:
        base.Update(); //Run base update method

        //Update timers:
        if (breachOpen) //Breach is currently open
        {
            breachOpenTime += Time.deltaTime;                                                                     //Increment breach time tracker
            if (loadedShots < gunSettings.maxLoadedShots && breachOpenTime >= gunSettings.cooldownTime) Reload(); //Reload weapon once cooldown time has been reached
        }
        if (doubleFireWindow > 0) doubleFireWindow = Mathf.Max(doubleFireWindow - Time.deltaTime, 0);                //Decrement time tracker and floor at zero
        if (reverseButtonCooldown > 0) reverseButtonCooldown = Mathf.Max(reverseButtonCooldown - Time.deltaTime, 0); //Decrement reverse button time tracker and floor at zero
        timeSinceFiring += Time.deltaTime; //Always update timeSinceFiring tracker (whatever state weapon is in)
        if (gunSettings.emptyEjectWait >= 0 && !breachOpen && loadedShots == 0 && timeSinceFiring >= gunSettings.emptyEjectWait) Eject(); //Do auto-eject sequence

        //Perform swing-closing:
        if (breachOpen && breachOpenTime >= gunSettings.swingCloseWait) //Breach is currently open and swing warmup time has been passed
        {
            Vector3 currentHandVel = RelativeVelocity;             //Get current velocity of hand relative to body
            Transform currentBarrel = barrels[currentBarrelIndex]; //Get transform for active barrel
            if (Vector3.Angle(currentBarrel.up, currentHandVel) < 90) //Make sure player is swinging weapon upward (down is too easy)
            {
                currentHandVel = Vector3.Project(RelativeVelocity, currentBarrel.up);                                                        //Constrain value of currentHandVel to just velocity along upward axis
                Vector3 closeForce = currentHandVel.sqrMagnitude * gunSettings.closerForce * 100 * Time.deltaTime * breakJoint.transform.up; //Apply relevant multipliers to get force to apply to end of barrel
                breakJoint.GetComponent<Rigidbody>().AddForceAtPosition(closeForce, currentBarrel.position, ForceMode.Force);                //Exert closing force on jointed barrel assembly
            }
        }
    }
    private protected override void FixedUpdate()
    {
        base.FixedUpdate(); //Call base fixed update stuff

        //Update rotation offset:
        if (currentAddRotOffset != Vector3.zero || recoilRotOffset != 0 || reverseTime > 0 || reverseFiring) //Rotation offset needs to be modified this update
        {
            //Initialization:
            reverseTime = Mathf.Max(reverseTime - Time.fixedDeltaTime, 0); //Update reverse time tracker

            //Set new rotational offset:
            Vector3 newRotOffset = Vector3.zero;                                                                                      //Initialize value to store new rotational offset eulers in
            newRotOffset.x += recoilRotOffset;                                                                                        //Apply current recoil rotation to offset
            float reverseTimeInterp = 1 - Mathf.Clamp01(reverseTime / gunSettings.reverseSpeed);                                      //Get value representing progression through reversal procedure
            float reverseOffsetAdd = reverseFiring ? Mathf.Lerp(0, 180, reverseTimeInterp) : Mathf.Lerp(180, 360, reverseTimeInterp); //Determine how to lerp added amount based on which reverse fire phase weapon is in
            if (flipSwingUp) reverseOffsetAdd *= -1;                                                                                  //Make gun flip upward if designated
            newRotOffset.x += reverseOffsetAdd;                                                                                       //Apply modifier to overall rotational offset
            currentAddRotOffset = newRotOffset;                                                                                       //Set new rotational offset
        }
    }

    //INPUT METHODS:
    private protected override void InputActionTriggered(InputAction.CallbackContext context)
    {
        //Determine input target:
        switch (context.action.name) //Determine behavior depending on action name
        {
            case "Trigger":
                float triggerPosition = context.ReadValue<float>(); //Get current position of trigger as a value
                if (timeSinceFiring >= gunSettings.recoilTime) //Make sure weapon is able to wobble during recoil phase (also disincentivises firing rapidly)
                {
                    //Tighten aim:
                    //NOTE: This system will not work while debugUpdateSettings is enabled
                    float triggerInterpolant = Mathf.InverseLerp(0, gunSettings.triggerThreshold, triggerPosition);                //Get value representing progress of trigger toward firing position
                    float newDamper = Mathf.Lerp(jointSettings.angularDriveDamper, gunSettings.triggerDamper, triggerInterpolant); //Use trigger interpolant to get a proportional damper value
                    SetWobble(newDamper);                                                                                          //Apply new damper value to change weapon wobbliness
                }

                //Check pull state:
                if (!triggerPulled) //Trigger has not yet been pulled
                {
                    if (triggerPosition >= gunSettings.triggerThreshold) //Trigger has just been pulled
                    {
                        triggerPulled = true; //Indicate that trigger is now pulled
                        Fire();               //Begin firing sequence
                    }
                }
                else //Trigger is currently pulled
                {
                    if (triggerPosition < gunSettings.triggerThreshold) //Trigger has been released
                    {
                        triggerPulled = false; //Indicate that trigger is now released
                    }
                }
                break;
            case "BButton":
                if (context.started && !breachOpen) //Button has just been pressed and breach is currently closed
                {
                    Eject(); //Open breach and eject shells
                }
                break;
            case "AButton":
                if (context.started && !reverseFiring && reverseButtonCooldown == 0) //Reverse fire button has just been pressed
                {
                    //Determine flip properties:
                    /*flipSwingUp = true;                                                          //Default to upward flip
                    Vector3 currentBarrelVel = Vector3.Project(RelativeVelocity, barrels[0].up); //Get current velocity of barrels (only along vertical axis)
                    float swingSpeed = currentBarrelVel.magnitude;                               //Get speed at which weapon is being swung upwards/downwards
                    if (swingSpeed > gunSettings.directionalReverseSwingSpeed) //Player is swinging weapon fast enough to reverse in a particular direction
                    {
                        if (Vector3.Angle(barrels[0].up, currentBarrelVel) > 90) flipSwingUp = false; //Flip downward if player is swinging gun downward
                    }*/

                    //Begin rotation procedure:
                    reverseTime = Mathf.Min(reverseTime + gunSettings.reverseSpeed, gunSettings.reverseSpeed * 2); //Initialize reversal sequence by adding time to the reversal clock
                    reverseFiring = true;                                      //Indicate that weapon is moving to reverse fire mode
                    if (breachOpen) Close();                                   //Close breach on button press if possible
                    reverseButtonCooldown = gunSettings.reverseButtonCooldown; //Disable button for a brief period

                    if (gunSettings.flipSound != null) audioSource.PlayOneShot(gunSettings.flipSound); //Play flipping sound effect
                }
                else if (context.canceled && reverseFiring) //Reverse fire button has just been released
                {
                    //Continue rotation procedure:
                    reverseTime = Mathf.Min(reverseTime + gunSettings.reverseSpeed, gunSettings.reverseSpeed * 2); //Initialize reversal sequence by adding time to the reversal clock
                    reverseFiring = false;   //Indicate that weapon is no longer in reverse fire mode
                    if (breachOpen) Close(); //Close breach on button release if possible

                    if (gunSettings.flipSound != null) audioSource.PlayOneShot(gunSettings.flipSound); //Play flipping sound effect
                }
                break;
            default: break; //Ignore unrecognized actions
        }
    }
    /// <summary>
    /// Shoots the gun (instantiates projectiles in network if possible).
    /// </summary>
    public Projectile[] Fire()
    {
        //Validation & initialization:
        if (loadedShots <= 0) { DryFire(); return new Projectile[0]; }                                                           //Dry-fire if weapon is out of shots
        if (breachOpen) { DryFire(); return new Projectile[0]; }                                                                 //Dry-fire if weapon breach is open
        if (gunSettings.noFireDuringRecoil && timeSinceFiring < gunSettings.recoilTime) { DryFire(); return new Projectile[0]; } //Dry-fire if weapon has not finished firing cooldown
        if (locked) return new Projectile[0];                                                                                    //Return if locked by another weapon
        Transform currentBarrel = barrels[currentBarrelIndex];     //Get reference to active barrel
        Vector3 origBarrelEulers = currentBarrel.localEulerAngles; //Get original local euler angles for current barrel

        //Put weapon at default position:
        if (!reverseFiring) //Only snap weapon while in normal fire mode
        {
            currentAddOffset = Vector3.zero;                      //Reset positional offset
            recoilRotOffset = 0;                                  //Reset rotational offset (due to recoil)
            transform.localScale = baseScale;                     //Set shotgun back to base scale
            reciprocatingAssembly.localPosition = baseReciproPos; //Return reciprocating barrels to base position
            transform.position = targetTransform.position;        //Move to exact position of target transform
            transform.rotation = targetTransform.rotation;        //Rotate to exact orientation of target transform
        }

        //Fire projectile(s):
        List<Projectile> spawnedProjectiles = new List<Projectile>(); //Initialize list of projectiles spawned by shot
        int shots = gunSettings.projectilesPerShot;
        float spread = gunSettings.shotSpread;
        string activeProjName = projResourceName;
        if (UpgradeSpawner.primary != null) //Upgrade system modifiers
        {
            switch (UpgradeSpawner.primary.currentPowerUp)
            {
                case PowerUp.PowerUpType.MultiShot:
                    shots *= UpgradeSpawner.primary.settings.MS_projectileMultiplier;
                    spread += UpgradeSpawner.primary.settings.MS_spreadAdd;
                    break;
                case PowerUp.PowerUpType.HeatVision:
                    activeProjName = "Projectiles/" + UpgradeSpawner.primary.settings.heatSeekerPrefabName;
                    break;
            }
        }
        for (int x = 0; x < shots; x++) //Iterate for number of projectiles spawned by shot
        {
            //Reposition barrel:
            Vector2 randomAngles = Random.insideUnitCircle * spread;                                            //Get random angles of shot spread
            currentBarrel.localEulerAngles = origBarrelEulers + new Vector3(randomAngles.x, randomAngles.y, 0); //Rotate barrel using random angular values

            //Generate new projectiles:
            Projectile newProjectile; //Initialize reference container for spawned projectile
            if (debugFireLocal || !PhotonNetwork.InRoom) //Weapon is in local fire mode
            {
                newProjectile = ((GameObject)Instantiate(Resources.Load(activeProjName))).GetComponent<Projectile>(); //Instantiate projectile
                newProjectile.FireDumb(currentBarrel);                                                                //Initialize projectile
            }
            else //Weapon is firing on the network
            {
                newProjectile = PhotonNetwork.Instantiate(activeProjName, currentBarrel.position, currentBarrel.rotation).GetComponent<Projectile>();        //Instantiate projectile on network
                newProjectile.photonView.RPC("RPC_Fire", RpcTarget.All, currentBarrel.position, currentBarrel.rotation, PlayerController.photonView.ViewID); //Initialize all projectiles simultaneously
            }
            spawnedProjectiles.Add(newProjectile); //Add projectile to spawn list
        }
        currentBarrel.localEulerAngles = origBarrelEulers; //Move barrel back to base euler angles

        //Fire particle effect:
        if (shotParticles != null) //Player has shot particle system (particles need to be shot before recoil scaling occurs)
        {
            //NOTE: Re-tool this system to spawn a prefab effect which lingers in the air
            shotParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); //Reset particle system
            shotParticles.Play();                                                      //Play particle effect
        }

        //Effects:
        if (gunSettings.fireSound != null) audioSource.PlayOneShot(gunSettings.fireSound); //Play sound effect
        StartCoroutine(DoRecoil());                                                        //Begin recoil phase
        if (loadedShots == 2) //Weapon is firing its first shot
        {
            StartCoroutine(MoveEjector(handedness, true)); //Move inner ejector when one shot is fired
            MovePin(handedness, true);
        }
        else if (loadedShots == 1) //Weapon is firing its second shot
        {
            StartCoroutine(MoveEjector(otherGun.handedness, true)); //Move outer ejector when both shots are fired
            MovePin(otherGun.handedness, true);
        }
        if (player != null) //Effects which need a playerController
        {
            //Direct player feedback:
            SendHapticImpulse(gunSettings.fireHaptics);      //Play haptic impulse
            player.ShakeScreen(gunSettings.fireScreenShake); //Shake screen (gently)

            //Generic exit velocity modifiers:
            float effectiveFireVel = gunSettings.fireVelocity;                                            //Store fire velocity so it can be optionally modified
            if (otherGun != null && otherGun.doubleFireWindow > 0 &&                                      //Player is firing both weapons simultaneously...
                otherGun.reverseFiring == reverseFiring) effectiveFireVel *= gunSettings.doubleFireBoost; //And both weapons are in the same firing mode, apply double-fire boost
            if (reverseFiring) effectiveFireVel *= gunSettings.reverseFireBoost;                          //Add reverse firing boost

            //Wall boost calculations
            bool hitWall = Physics.Raycast(currentBarrel.position, currentBarrel.forward, out RaycastHit hit, gunSettings.maxWallBoostDist, gunSettings.wallBoostLayers); //Determine whether or not play is shooting at a wall
            if (!hitWall) hitWall = Physics.CheckSphere(transform.position, gunSettings.wallCheckSphereRadius, gunSettings.wallBoostLayers);
            if (hitWall) //Weapon is firing at a nearby wall
            {
                effectiveFireVel *= gunSettings.maxWallBoost; //Apply multiplier to shot power based on how close player is to the wall
                if (!gunSettings.alwaysMaxWallBoost) //Wall boost power is being interpolated based on proximity to the wall
                {
                    float distInterpolant = 1 - (hit.distance / gunSettings.maxWallBoostDist); //Get interpolant value representing how close weapon barrel is to a wall (the closer the higher)
                    effectiveFireVel *= distInterpolant;                                       //Apply distance interpolant
                }
            }

            //Apply velocity to player
            if (!hitWall && gunSettings.wallBoostOnly && !reverseFiring) //Additive boost system is being used
            {
                if (gunSettings.neutralFireBoost) player.bodyRb.AddForce(-currentBarrel.forward * effectiveFireVel, ForceMode.Impulse); //Add impulse force to player's existing velocity
            }
            else //Override boost system is being used
            {
                Vector3 newVelocity = -currentBarrel.forward * effectiveFireVel;               //Store new velocity for player (always directly away from barrel that fired latest shot, unless reverse firing)
                float velocityAngleDelta = Vector3.Angle(newVelocity, player.bodyRb.velocity); //Get angle between current velocity and new velocity
                if (velocityAngleDelta <= gunSettings.additiveVelocityMaxAngle) //Player is firing to push themself in the direction they are generally already going
                {
                    float velAngleInterpolant = 1 - (velocityAngleDelta / gunSettings.additiveVelocityMaxAngle); //Get interpolant value for closeness of velocity angles
                    float addVel = gunSettings.additiveVelocityCurve.Evaluate(velAngleInterpolant);              //Get multiplier for component of new player velocity
                    addVel *= effectiveFireVel * gunSettings.additiveVelocityMultiplier;                         //Apply both multipliers to base velocity magnitude
                    newVelocity = newVelocity.normalized * (player.bodyRb.velocity.magnitude + addVel);          //Keep exact direction of new velocity but use existing speed and add some previous speed
                }
                player.bodyRb.velocity = newVelocity; //Launch player instantaneously (no acceleration)
            }
        }

        //Cleanup:
        timeSinceFiring = 0;                           //Reset firing timer
        doubleFireWindow = gunSettings.doubleFireTime; //Open double fire window so that other weapon can check for it
        if (barrels.Length > 1) //Only index barrel if there is more than one
        {
            currentBarrelIndex += 1;                                          //Index current barrel number by one
            if (currentBarrelIndex >= barrels.Length) currentBarrelIndex = 0; //Overflow barrel index if relevant
        }
        if (!(UpgradeSpawner.primary != null && UpgradeSpawner.primary.currentPowerUp == PowerUp.PowerUpType.InfiniShot)) loadedShots = Mathf.Max(loadedShots - 1, 0); //Spend one shot (floor at zero)
        return spawnedProjectiles.ToArray(); //Return reference to the master script of the projectile(s) spawned
    }
    /// <summary>
    /// Opens weapon breach and ejects shells.
    /// </summary>
    public void Eject()
    {
        //Validity checks:
        if (breachOpen||locked) //Breach is already open or gun is locked
        {
            //SOUND EFFECT
            return; //Ignore everything else
        }

        //Open joint:
        breakJoint.angularXMotion = ConfigurableJointMotion.Limited;                          //Unlock pivot rotation
        breakJoint.targetRotation = Quaternion.Euler(Vector3.right * gunSettings.breakAngle); //Set target joint rotation to break angle
        SoftJointLimit newJointLimit = breakJoint.highAngularXLimit;                          //Copy current joint limit setting
        newJointLimit.limit = gunSettings.breakAngle;                                         //Set break angle to open position
        breakJoint.highAngularXLimit = newJointLimit;                                         //Apply new joint limit

        //Effects:
        StartCoroutine(MoveEjector(Handedness.None, false)); //Move ejectors back to forward positions
        MovePin(Handedness.None, false);                     //Move pins to backward positions

        //Cleanup:
        if (gunSettings.ejectSound != null) audioSource.PlayOneShot(gunSettings.ejectSound); //Play sound effect
        SendHapticImpulse(gunSettings.ejectHaptics);                                         //Play haptic impulse
        breachOpen = true;                                                                   //Indicate that breach is now open
    }
    /// <summary>
    /// Closes weapon breach and makes weapon prepared to fire.
    /// </summary>
    public void Close()
    {
        //Validity checks:
        if (!breachOpen) return; //Do not attempt to close if breach is open

        //Close joint:
        breakJoint.angularXMotion = ConfigurableJointMotion.Locked;  //Lock pivot rotation
        breakJoint.targetRotation = Quaternion.Euler(Vector3.zero);  //Set target angle of break joint to zero
        SoftJointLimit newJointLimit = breakJoint.highAngularXLimit; //Copy current joint limit setting
        newJointLimit.limit = 0;                                     //Set break angle to closed value
        breakJoint.highAngularXLimit = newJointLimit;                //Apply new joint limit

        //Cleanup:
        if (gunSettings.lockSound != null) audioSource.PlayOneShot(gunSettings.lockSound); //Play sound effect
        SendHapticImpulse(gunSettings.closeHaptics);                                       //Play haptic impulse
        breachOpenTime = 0;                                                                //Reset breach open time tracker
        breachOpen = false;                                                                //Indicate that breach is now closed
    }
    /// <summary>
    /// Fully reloads weapon to max ammo capacity.
    /// </summary>
    public void Reload()
    {
        //Cleanup:
        currentBarrelIndex = 0;                   //Reset barrel index
        loadedShots = gunSettings.maxLoadedShots; //Reset shot counter to maximum
    }
    /// <summary>
    /// Called whenever player tries to fire weapon but weapon cannot be fired for some reason.
    /// </summary>
    public void DryFire()
    {

    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Moves shotgun to or from player hip.
    /// </summary>
    public override void Holster(bool holster = true)
    {
        base.Holster(holster); //Call base holster functionality
        if (holster) //Reset stuff affecting weapon rotation when holstering
        {
            if (breachOpen) Close();            //Close weapon if open
            Reload();                           //Make sure weapon is fully loaded
            reverseFiring = false;              //Make sure weapon is not in reverse fire mode
            reverseTime = 0;                    //Clear reverse fire timer
            currentAddRotOffset = Vector3.zero; //Reset rotation offset
        }
    }
    /// <summary>
    /// Restores shotgun to base state (fully-loaded and closed).
    /// </summary>
    /// <param name="disableInputTime">Also disables player input for this number of seconds (0 does not disable player input, less than 0 disables it indefinitely)</param>
    public override void Shutdown(float disableInputTime = 0)
    {
        base.Shutdown(disableInputTime);    //Call base functionality
        if (breachOpen) Close();            //Close weapon if open
        Reload();                           //Make sure weapon is fully loaded
        triggerPulled = false;              //Reset trigger value tracker
        reverseFiring = false;              //Make sure weapon is not in reverse fire mode
        reverseTime = 0;                    //Clear reverse fire timer
        currentAddRotOffset = Vector3.zero; //Reset rotation offset
    }

    //UTILITY METHODS:
    /// <summary>
    /// Modifies how wobbly gun currently is.
    /// </summary>
    /// <param name="damperValue">The higher this value is, the less wobbly the gun is.</param>
    private void SetWobble(float damperValue)
    {
        JointDrive newDrive = joint.angularXDrive;                       //Duplicate current angular X drive setting
        newDrive.positionDamper = damperValue;                           //Apply interpolated damper value to drive setting
        joint.angularXDrive = newDrive; joint.angularYZDrive = newDrive; //Apply damper change to all angular drive axes
    }
    /// <summary>
    /// Moves one or both firing pins on weapon to given position.
    /// </summary>
    /// <param name="side">Which pin to move (moves both if None is given).</param>
    /// <param name="inward">Pass true to move pin inward, false to move it back out.</param>
    private void MovePin(Handedness side, bool inward)
    {
        Vector3 targetPinPos = basePinPos + (inward ? (Vector3.forward * gunSettings.pinTraverseDistance) : Vector3.zero); //Get position pin(s) are being moved to
        if (side == Handedness.Left || side == Handedness.None) leftFiringPin.localPosition = targetPinPos;                //Left pin is being moved
        if (side == Handedness.Right || side == Handedness.None) rightFiringPin.localPosition = targetPinPos;              //Right pin is being moved
    }
}
