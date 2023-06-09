using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class NewChainsawController : PlayerEquipment
{
    //Classes, Enums & Structs:
    /// <summary>
    /// Behavioral phases which chainsaw can be in.
    /// </summary>
    public enum BladeMode
    {
        Sheathed,
        Extending,
        Extended,
        Retracting,
        Deflecting
    }

    //Objects & Components:
    [Header("Components:")]
    [SerializeField, Tooltip("End of blade, sticks out of the front of the sheath (retains its scale).")]                                        private Transform bladeTip;
    [SerializeField, Tooltip("Joins blade to wrist sheath, seamlessly scalable on the Z axis (Z scale is proportional to blade tip distance).")] private Transform bladeExtender;
    [SerializeField, Tooltip("Secondary blade extension for the backend of the weapon, can be whatever length (Z scale).")]                      private Transform bladeExtenderBack;
    [SerializeField, Tooltip("Jointed component on front of system which moves forward and rotates during blade activation.")]                   private Transform wrist;
    [SerializeField, Tooltip("Rotating assembly which allows the wrist to be turned downward for reverse grip mode.")]                           private Transform wristPivot;
    [SerializeField, Tooltip("Invisible transform which indicates the extent to which blade tracks for grinding and hitting players.")]          private Transform bladeEnd;
    [SerializeField, Tooltip("")]                                                                                                                private Transform bladeBackTip;
    [SerializeField, Tooltip("Position chainsaw fires deflected projectiles out of.")]                                                           private Transform barrel;
    [SerializeField, Tooltip("")]                                                                                                                private Transform deflectChargeNeedle;
    [Space()]
    [SerializeField, Tooltip("")] private ParticleSystem jawParticles;
    [SerializeField, Tooltip("")] private ParticleSystem grindParticles;

    private Transform hand;             //Real position of player hand used by this equipment
    private PlayerEquipment handWeapon; //Player weapon held in the same hand as this chainsaw
    private NewGrapplerController otherHandGrapple;

    //Settings:
    [Header("Settings:")]
    [SerializeField, Tooltip("Settings describing behavior specific to chainsaw functionality.")] private ChainsawSettings settings;

    //Runtime Variables:
    /// <summary>
    /// Blade's current behavior state.
    /// </summary>
    internal BladeMode mode = BladeMode.Sheathed;
    private BladeMode prevMode = BladeMode.Sheathed; //Previous mode blade was in
    internal bool grinding;          //Whether or not player is currently grinding on a surface
    private float grindTime = 0;     //Amount of time chainsaw has been grinding on surface for
    private float deflectTime = 0;   //Amount of time chainsaw is able to deflect for (recharges when not in use)
    private RaycastHit lastGrindHit; //Data for last surface chainsaw has ground on
    private float timeInMode;        //How long weapon has been in current mode for
    private float timeUntilPulse;    //Time until next haptic pulse should be triggered (used for repeated pulses while saw is active)
    private bool reverseGrip;        //Indicates that player is pressing the input for reverse grip mode (only valid while chainsaw is active)
    private float gripValue;         //Latest value from grip input
    private float triggerValue;      //Latest value from trigger input
    private bool deflectDeactivated; //Becomes true when player exhausts their deflect meter and needs to wait for it to recharge

    private Vector3 bladeOriginPos;    //Initial local (sheathed) position of blade
    private Vector3 wristOriginPos;    //Initial local (sheathed) position of wrist assembly
    private float bladeOriginSize;     //Initial length of blade extender
    private float bladeBackOriginSize; //Initial length of rear blade extender
    private float afterKillCountdown;

    private int currentRapidDeflects;
    private float currentDeflectCooldown;
    private bool rapidDeflectCheckActive;

    //RUNTIME METHODS:
    /// <summary>
    /// lmao nerd
    /// </summary>
    private protected override void Awake()
    {
        //Initialization:
        base.Awake(); //Do base awake setup

        //Get objects & components:


        //Get runtime vars:
        bladeOriginPos = bladeTip.localPosition;              //Get starting local position of blade assembly
        wristOriginPos = wrist.localPosition;                 //Get starting local position of wrist assembly
        bladeOriginSize = bladeExtender.localScale.z;         //Get starting local Z scale of blade extender
        bladeBackOriginSize = bladeExtenderBack.localScale.z; //Get starting local Z scale of rear blade extender
    }
    private protected override void Start()
    {
        base.Start(); //Call base start stuff

        //Late object & component get:
        hand = (handedness == 0 ? player.leftHand : player.rightHand).transform; //Get a reference to the relevant player hand
        deflectTime = settings.deflectTime;                                      //Set deflect time to max
        foreach (PlayerEquipment equipment in player.attachedEquipment) //Iterate through all equipment attached to player
        {
            if (equipment != this && equipment.handedness == handedness) { handWeapon = equipment; break; } //Try to get weapon used by same hand
            if (equipment.GetComponent<NewGrapplerController>() != null) otherHandGrapple = equipment.GetComponent<NewGrapplerController>();
        }
    }
    private protected override void Update()
    {
        base.Update(); //Call base equipment update method

        //Update timers:
        timeInMode += Time.deltaTime; //Increment mode time tracker
        if (afterKillCountdown > 0) afterKillCountdown = Mathf.Max(afterKillCountdown - Time.deltaTime, 0);
        if (mode == BladeMode.Extended || mode == BladeMode.Extending) //Blade is in extended mode
        {
            timeUntilPulse -= Time.deltaTime; //Increment pulse time tracker
            if (timeUntilPulse <= 0 && afterKillCountdown <= 0) //It is time for the next haptic pulse
            {
                PlayerController.HapticData newPulse = settings.activeHapticPulse;                                                                  //Get values from settings for active pulse
                newPulse.amplitude += Random.Range(-settings.activeHapticMagnitudeVariance, settings.activeHapticMagnitudeVariance);                //Add a little bit of random variation to the pulse
                SendHapticImpulse(newPulse);                                                                                                        //Play new haptic pulse
                timeUntilPulse = newPulse.duration + Random.Range(-settings.activeHapticFrequencyVariance, settings.activeHapticFrequencyVariance); //Schedule new pulse with slight variation in activation time
            }
        }
        if (mode != BladeMode.Deflecting)
        {
            if (deflectTime < settings.deflectTime)
            {
                deflectTime = Mathf.Min(deflectTime + (Time.deltaTime * settings.deflectCooldownRate), settings.deflectTime);
                if (deflectTime == settings.deflectTime)
                {
                    deflectDeactivated = false;
                }
            }
        }
        else if (deflectTime > 0)
        {
            deflectTime = Mathf.Max(deflectTime - Time.deltaTime, 0);
            if (deflectTime <= 0)
            {
                //Switch mode:
                prevMode = mode;            //Record previous blade mode
                mode = BladeMode.Extending; //Indicate that blade is no longer in deflect mode
                timeInMode = 0;             //Reset mode time tracker
                deflectTime = 0;            //Always fully reset deflect time tracker
                deflectDeactivated = true;
                if (PhotonNetwork.IsConnected) PlayerController.photonView.RPC("RPC_Deflect", RpcTarget.All, 2);
                audioSource.clip = settings.runningSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        if (grinding) grindTime += Time.deltaTime; //Update grind time tracker

        //If the rapid deflect check is active
        if (rapidDeflectCheckActive)
        {
            if(currentDeflectCooldown > settings.rapidDeflectCooldown)
            {
                rapidDeflectCheckActive = false;
                currentDeflectCooldown = 0;
                currentRapidDeflects = 0;
            }
            else
            {
                currentDeflectCooldown += Time.deltaTime;
            }
        }

        //Move deflect needle:
        if (deflectChargeNeedle != null && !inStasis)
        {
            float newAngle = Mathf.Lerp(settings.deflectNeedleRange.x, settings.deflectNeedleRange.y, deflectTime / settings.deflectTime);
            deflectChargeNeedle.localEulerAngles = Vector3.up * newAngle;
        }

        //Extend/Retract blade:
        if (mode == BladeMode.Sheathed && gripValue >= settings.triggerThresholds.y) //Grip has been squeezed enough to activate the chainsaw
        {
            //Switch modes:
            if (handWeapon != null) handWeapon.Holster();     //Holster weapon held in hand if possible
            if (settings.extendSound != null) audioSource.PlayOneShot(settings.extendSound); //Play extend sound
            SendHapticImpulse(settings.extendHaptics);        //Play extend haptics
            prevMode = mode;                                  //Record previous blade mode
            mode = BladeMode.Extending;                       //Indicate that blade is now extending
            timeInMode = 0;                                   //Reset mode time tracker
            timeUntilPulse = settings.extendHaptics.duration; //Set pulse timer to begin pulsing as soon as extend haptics have finished

            //Play sound:
            audioSource.clip = settings.runningSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if ((mode == BladeMode.Extended || mode == BladeMode.Deflecting) && gripValue < settings.triggerThresholds.x) //Grip has been released enough to re-sheath the chainsaw (always check in case of early release)
        {
            //Switch mode:
            if (handWeapon != null) handWeapon.Holster(false); //Un-holster weapon held in hand if possible
            if (settings.sheathSound != null) audioSource.PlayOneShot(settings.sheathSound); //Play sheath sound
            SendHapticImpulse(settings.retractHaptics);        //Play haptic impulse
            prevMode = mode;                                   //Record previous blade mode
            mode = BladeMode.Retracting;                       //Indicate that blade is now retracting
            timeInMode = 0;                                    //Reset mode time tracker
            jawParticles.gameObject.SetActive(false);
            grindParticles.gameObject.SetActive(false);
            if (PhotonNetwork.IsConnected) PlayerController.photonView.RPC("RPC_Deflect", RpcTarget.All, 2);

            //Grinding disengagement:
            if (grinding) //Player is currently grinding on a surface
            {
                player.bodyRb.AddForce(lastGrindHit.normal * settings.disengageForce, ForceMode.Impulse); //Bounce player away from surface when ending grind
                grinding = false;                                                                         //Indicate player can no longer be grinding on a surface
                grindTime = 0;                                                                            //Reset grind time tracker
            }
        }
        else if ((mode == BladeMode.Extended || mode == BladeMode.Extending) && triggerValue >= settings.triggerThresholds.y && deflectTime > 0 && !deflectDeactivated) //Activate deflect mode when player squeezes the trigger
        {
            //Switch mode:
            prevMode = mode;                           //Record previous blade mode
            mode = BladeMode.Deflecting;               //Indicate that blade is now deflecting
            wrist.localRotation = Quaternion.identity; //Reset local rotation of the wrist
            timeInMode = 0;                            //Reset mode time tracker
            jawParticles.gameObject.SetActive(false);
            if (PhotonNetwork.IsConnected) PlayerController.photonView.RPC("RPC_Deflect", RpcTarget.All, 1);

            //Play sound:
            audioSource.clip = settings.deflectIdleSound;
            audioSource.loop = true;
            audioSource.Play();

            //Grinding disengagement:
            if (grinding) //Player is currently grinding on a surface
            {
                grinding = false; //Indicate that player is no longer grinding
                grindTime = 0;    //Reset grind time tracker
            }
        }
        else if (mode == BladeMode.Deflecting && triggerValue < settings.triggerThresholds.x && timeInMode >= settings.minimumDeflectTime) //End deflect mode when player releases the trigger
        {
            //Switch mode:
            prevMode = mode;            //Record previous blade mode
            mode = BladeMode.Extending; //Indicate that blade is no longer in deflect mode
            timeInMode = 0;             //Reset mode time tracker

            //Play sound:
            audioSource.clip = settings.runningSound;
            audioSource.loop = true;
            audioSource.Play();

            //End deflect effect:
            if (PhotonNetwork.IsConnected) PlayerController.photonView.RPC("RPC_Deflect", RpcTarget.All, 2);
        }

        //Blade movement:
        if (mode == BladeMode.Extending || mode == BladeMode.Retracting) //Blade is moving between primary modes
        {
            //Initialize:
            float timeInterpolant = timeInMode / settings.bladeExtendTime;                                                  //Get interpolant value representing percentage of blade animation completed so far
            float gripInterpolant = settings.bladePreRetractCurve.Evaluate(gripValue / settings.triggerThresholds.y);       //Get multiplier to apply to retraction distance for true retracted position
            Vector3 bladeExtendPos = bladeOriginPos + (Vector3.forward * settings.bladeTraverseDistance);                   //Get target position blade is extending to (or retracting from)
            Vector3 bladeRetractPos = bladeOriginPos + (gripInterpolant * settings.bladePreRetractDistance * Vector3.back); //Get target position blade is retracting to (or extending from)
            Vector3 wristStartPos = wristOriginPos;                                                                         //Get starting position of wrist
            Vector3 wristExtendPos = wristOriginPos + (Vector3.forward * settings.wristExtendDistance);                     //Get extended target position of wrist
            if (prevMode == BladeMode.Deflecting)
            {
                wristStartPos = wristOriginPos + (Vector3.forward * settings.deflectWristExtend);
                if (mode == BladeMode.Extending) bladeRetractPos = bladeOriginPos + (Vector3.forward * settings.deflectRadius);
                else if (mode == BladeMode.Retracting) bladeExtendPos = bladeOriginPos + (Vector3.forward * settings.deflectRadius);
            }

            //Move blade tip:
            if (mode == BladeMode.Extending) bladeTip.localPosition = Vector3.LerpUnclamped(bladeRetractPos, bladeExtendPos, settings.bladeExtendCurve.Evaluate(timeInterpolant)); //Move extending blade to interpolated position
            else bladeTip.localPosition = Vector3.LerpUnclamped(bladeExtendPos, bladeRetractPos, settings.bladeRetractCurve.Evaluate(timeInterpolant));                            //Move retracting blade to interpolated position

            //Move wrist:
            if (timeInterpolant <= settings.wristDeployPeriod) //Wrist deploys during first part of blade extension/retraction
            {
                float wristInterpolant = Mathf.InverseLerp(0, settings.wristDeployPeriod, timeInterpolant);                           //Get special interpolant for wrist deployment
                wristInterpolant = settings.wristDeployCurve.Evaluate(wristInterpolant);                                              //Feed interpolant through animation curve
                if (mode == BladeMode.Extending) wrist.localPosition = Vector3.Lerp(wristStartPos, wristExtendPos, wristInterpolant); //Move wrist toward extended position
                else wrist.localPosition = Vector3.Lerp(wristExtendPos, wristStartPos, wristInterpolant);                             //Move wrist toward extended position
            }
            else if (mode == BladeMode.Extending && wrist.localPosition != wristExtendPos) wrist.localPosition = wristExtendPos;  //Make sure wrist reaches its final position
            else if (mode == BladeMode.Retracting && wrist.localPosition != wristOriginPos) wrist.localPosition = wristOriginPos; //Make sure wrist reaches its final position

            //Check for end:
            if (timeInMode >= settings.bladeExtendTime) //Blade extension/retraction animation has completed
            {
                bladeTip.localPosition = (mode == BladeMode.Extending ? bladeExtendPos : bladeRetractPos); //Move blade to target position
                wrist.localPosition = (mode == BladeMode.Extending ? wristExtendPos : wristOriginPos);     //Move wrist to target position
                if (mode == BladeMode.Retracting) //Weapon has just finished retracting
                {
                    wrist.localRotation = Quaternion.identity;      //Return wrist to base rotation
                    wristPivot.localRotation = Quaternion.identity; //Return wrist pivot to base rotation
                }
                prevMode = mode;                                                                //Record previous blade mode
                mode = (mode == BladeMode.Extending ? BladeMode.Extended : BladeMode.Sheathed); //Progress mode to stable state
                timeInMode = 0;                                                                 //Reset mode timer
            }

            //Cleanup:
            UpdateBladeExtender(); //Automatically update blade extender and blade end system
        }

        //Blade rotations:
        if (mode == BladeMode.Extended || mode == BladeMode.Extending) //Blade is currently deployed or is being deployed
        {
            //Update wrist rotation:
            Quaternion targetWristRot = Quaternion.LookRotation(hand.forward, hand.right * (handedness == CustomEnums.Handedness.Right ? -1 : 1)); //Get target wrist rotation from rotation of hand (in order to make weapon more controllable)
            targetWristRot = Quaternion.RotateTowards(wrist.parent.rotation, targetWristRot, settings.maxWristAngle);                              //Clamp rotation to set angular limit
            wrist.rotation = Quaternion.Lerp(wrist.rotation, targetWristRot, settings.wristLerpRate * Time.deltaTime);                             //Lerp wrist toward target rotation

            //Reverse grip:
            float targetPivotRot = reverseGrip ? -settings.reverseGripAngle : 0; //Get target Y rotation for wrist pivot
            if (wristPivot.localEulerAngles.y != targetPivotRot)
            {
                Vector3 newPivotRot = wristPivot.localEulerAngles;                                                             //Get current eulers from pivot
                newPivotRot.y = Mathf.LerpAngle(newPivotRot.y, targetPivotRot, settings.reverseGripLerpRate * Time.deltaTime); //Get new lerped y rotation value for pivot
                wristPivot.localEulerAngles = newPivotRot;                                                                     //Apply new eulers to pivot
            }
            if (jawParticles.gameObject.activeSelf != reverseGrip)
            {
                jawParticles.gameObject.SetActive(reverseGrip);
                if (reverseGrip) jawParticles.Play();
            }

            //Wall grinding:
            Vector3 bladeOffset = wristPivot.right * settings.bladeWidth; //Get distance of offset for secondary blade cast
            Vector3 bladeDir = wristPivot.position - bladeEnd.position;
            if ((Physics.Linecast(wristPivot.position, bladeEnd.position, out RaycastHit hitInfo, settings.grindLayers) ||                  //Check for obstacles intersecting back of the blade
                Physics.Linecast(wristPivot.position + bladeOffset, bladeEnd.position + bladeOffset, out hitInfo, settings.grindLayers)) && //Check for obstacles intersecting front of the blade
                Vector3.Angle(bladeDir, hitInfo.normal) <= settings.maxGrindAngle)
            {
                //Adjust player velocity:
                Vector3 grindDirection = Vector3.Cross(hitInfo.normal, wrist.up).normalized;                                 //Get target direction of grind
                if (handedness == CustomEnums.Handedness.Left) grindDirection *= -1;                                         //Grind in opposite direction if blade is flipped
                float grindTimeInterpolant = Mathf.Clamp01(grindTime / settings.grindAccelTime);                             //Get interpolant for how long player has been grinding
                float grindSpeed = Mathf.Lerp(settings.grindSpeedRange.x, settings.grindSpeedRange.y, grindTimeInterpolant); //Determine grind speed based on how long player has been grinding for
                playerBody.velocity = grindDirection * grindSpeed;                                                           //Modify player velocity based on grind values

                //Grind beginning:
                if (!grinding) //Player was not previously grinding
                {
                    if (otherHandGrapple != null && otherHandGrapple.hook != null && otherHandGrapple.hook.state != HookProjectile.HookState.Stowed) otherHandGrapple.hook.Release();
                    grindParticles.gameObject.SetActive(true);
                    grindParticles.Play();
                }

                //Particles:
                grindParticles.transform.position = Vector3.MoveTowards(hitInfo.point, wristPivot.position, settings.sparkGap);
                grindParticles.transform.rotation = Quaternion.LookRotation(-wristPivot.right, hitInfo.normal);

                //Sweet spot attraction:
                if (settings.grindGlueForce > 0)
                {
                    float penetration = 1 - (Mathf.Min(hitInfo.distance, settings.bladeTraverseDistance) / settings.bladeTraverseDistance); //Get penetration depth as a percentage of total blade length
                    if (penetration < settings.grindSweetSpot.x || penetration > settings.grindSweetSpot.y) //Blade is not pushed far enough into surface
                    {
                        //Get corrective force value:
                        float correctionValue = penetration < settings.grindSweetSpot.x ? Mathf.InverseLerp(settings.grindSweetSpot.x, 0, penetration) : -Mathf.InverseLerp(settings.grindSweetSpot.y, 1, penetration); //Get linear value representing how far blade is from sweet spot
                        print("CorrectionValue = " + correctionValue);
                        correctionValue = Mathf.Sign(correctionValue) * settings.grindGlueCurve.Evaluate(Mathf.Abs(correctionValue)); //Apply curve to value so it's a bit more smoothed-out/consistent (sign-independent)
                        correctionValue *= settings.grindGlueForce;                          //Get final correction value from glue force

                        //Apply force:
                        Vector3 correctionForce = correctionValue * Time.deltaTime * wrist.transform.forward; //Get force being applied this frame to keep blade locked in surface
                        playerBody.AddForce(correctionForce, ForceMode.Force);                                //Apply corrective force to player rigidbody
                    }
                    else //Blade is within sweet spot and wants to stay there
                    {
                        //Apply brakes:

                    }
                }

                //Cleanup:
                lastGrindHit = hitInfo; //Store hit info for later
                grinding = true;        //Indicate that player is now grinding
            }
            else if (grinding) //Blade has just stopped touching a wall
            {
                //Grind ending:
                grinding = false; //Indicate that grind is no longer occurring
                grindTime = 0;    //Reset grind time tracker
                grindParticles.gameObject.SetActive(false);
            }

            //Player killing:
            if (Physics.Linecast(wristPivot.position, bladeEnd.position, out hitInfo, LayerMask.GetMask("Player")))
            {
                NetworkPlayer hitPlayer = hitInfo.collider.GetComponentInParent<NetworkPlayer>(); //Try to get networkplayer from hit
                if (hitPlayer != null && !hitPlayer.photonView.IsMine) //Player (other than self) has been hit by blade
                {
                    //If you chainsaw someone with the same color as you (same team), do not kill
                    if ((int)hitPlayer.photonView.Owner.CustomProperties["Color"] == (int)NetworkManagerScript.localNetworkPlayer.photonView.Owner.CustomProperties["Color"])
                    {
                        Debug.Log("You chainsawed your own teammate.");
                    }

                    // Chainsaw somebody with a different color (different teams).
                    else
                    {
                        hitPlayer.photonView.RPC("RPC_Hit", RpcTarget.AllBuffered, 100, PlayerController.photonView.ViewID, Vector3.zero, (int)DeathCause.CHAINSAW); //Hit target
                        Debug.Log("ChainsawKill");
                        SendHapticImpulse(settings.killHaptics);
                        PlayerController.instance.audioSource.PlayOneShot(settings.KillSound);
                        afterKillCountdown = settings.killHaptics.duration;
                    }
                }
            }
        }

        else if (mode == BladeMode.Retracting) //Blade is currently retracting
        {
            //Return to base rotation:
            wrist.localRotation = Quaternion.RotateTowards(wrist.localRotation, Quaternion.identity, settings.wristRotReturnRate * Time.deltaTime);              //Have wrist return to base rotation
            wristPivot.localRotation = Quaternion.RotateTowards(wristPivot.localRotation, Quaternion.identity, settings.reverseGripReturnRate * Time.deltaTime); //Have wrist pivot return to base rotation
        }
        else if (mode == BladeMode.Deflecting) //Blade is currently in deflect mode
        {
            //Transition period:
            float wristRotAngle = settings.deflectRotRate * Time.deltaTime; //Get initial value for how quickly blade will rotate this frame
            if (timeInMode <= settings.deflectTransTime) //Blade is currently transitioning into deflection mode
            {
                //Initialization:
                float timeInterpolant = Mathf.Clamp01(timeInMode / settings.deflectTransTime); //Get interpolant value representing time spent in deflection mode
                wristRotAngle *= timeInterpolant;                                               //Apply time interpolant to wrist rotation rate in order to make it spin up

                //Reverse grip transition:
                if (wristPivot.localEulerAngles.y != -90) //Wrist is still transitioning into deflection mode
                {
                    Vector3 newPivotRot = wristPivot.localEulerAngles;   //Get current eulers from pivot
                    newPivotRot.y = Mathf.Lerp(0, -90, timeInterpolant); //Get target pivot rotation by lerping over time
                    wristPivot.localEulerAngles = newPivotRot;           //Set new wrist pivot rotation
                }

                //Change blade length:
                Vector3 bladeExtendPos = bladeOriginPos + (Vector3.forward * settings.bladeTraverseDistance); //Get origin position blade is moving from
                Vector3 bladeDeflectPos = bladeOriginPos + (Vector3.forward * settings.deflectRadius);        //Get target position blade is moving to
                bladeTip.localPosition = Vector3.Lerp(bladeExtendPos, bladeDeflectPos, timeInterpolant);      //Move blade tip to target position
                UpdateBladeExtender();                                                                        //Update blade middle

                //Extend wrist:
                if (settings.wristExtendDistance > 0)
                {
                    Vector3 wristStartPos = wristOriginPos + (Vector3.forward * settings.wristExtendDistance);
                    Vector3 wristExtendPos = wristOriginPos + (Vector3.forward * (settings.deflectWristExtend + settings.wristExtendDistance));
                    wrist.localPosition = Vector3.Lerp(wristStartPos, wristExtendPos, timeInterpolant);
                }
            }

            //Rotate wrist:
            Quaternion targetWristRot = wrist.localRotation * Quaternion.AngleAxis(wristRotAngle, Vector3.forward);
            wrist.localRotation = targetWristRot;

            //Pull player forward:
            float interpolant = Mathf.Min(settings.deflectTime, timeInMode) / settings.deflectTime;
            float pullForceMultiplier = settings.deflectPullForce * Time.deltaTime * settings.deflectPullForceCurve.Evaluate(interpolant);
            player.bodyRb.AddForce(transform.forward * pullForceMultiplier, ForceMode.Force); //Add force to player body

            //targetWristRot = Quaternion.RotateTowards(wrist.parent.rotation, targetWristRot, settings.maxWristAngle);  //Clamp rotation to set angular limit
            //wrist.rotation = Quaternion.Lerp(wrist.rotation, targetWristRot, settings.wristLerpRate * Time.deltaTime); //Lerp wrist toward target rotation

            //Reverse grip:
            if (wristPivot.localEulerAngles.y != -settings.reverseGripAngle)
            {
                Vector3 newPivotRot = wristPivot.localEulerAngles;                                                                         //Get current eulers from pivot
                newPivotRot.y = Mathf.LerpAngle(newPivotRot.y, -settings.reverseGripAngle, settings.reverseGripLerpRate * Time.deltaTime); //Get new lerped y rotation value for pivot
                wristPivot.localEulerAngles = newPivotRot;                                                                                 //Apply new eulers to pivot
            }

            //Player killing:
            if (Physics.Linecast(wristPivot.position, bladeEnd.position, out RaycastHit hitInfo, LayerMask.GetMask("Player")))
            {
                NetworkPlayer hitPlayer = hitInfo.collider.GetComponentInParent<NetworkPlayer>(); //Try to get networkplayer from hit
                if (hitPlayer != null && !hitPlayer.photonView.IsMine) //Player (other than self) has been hit by blade
                {
                    hitPlayer.photonView.RPC("RPC_Hit", RpcTarget.AllBuffered, 3, PlayerController.photonView.ViewID, Vector3.zero, (int)DeathCause.CHAINSAW); //Hit target
                    Debug.Log("DeflectingChainsawKill");
                    SendHapticImpulse(settings.killHaptics);
                    PlayerController.instance.audioSource.PlayOneShot(settings.KillSound);
                    afterKillCountdown = settings.killHaptics.duration;
                }
            }
        }
    }
    
    private protected override void FixedUpdate()
    {
        base.FixedUpdate(); //Call base equipment update method
    }

    private protected override void InputActionTriggered(InputAction.CallbackContext context)
    {
        //Determine input target:
        switch (context.action.name) //Determine behavior depending on action name
        {
            case "Grip":
                gripValue = context.ReadValue<float>() + GameSettings.inputSensitivityBuffer; //Get current amount by which player is squeezing the grip
                if (mode == BladeMode.Sheathed) //Blade is currently sheathed
                {
                    float gripInterpolant = settings.bladePreRetractCurve.Evaluate(gripValue / settings.triggerThresholds.y);      //Get multiplier to apply to retraction distance in order to pull blade back slightly
                    bladeTip.localPosition = bladeOriginPos + (gripInterpolant * settings.bladePreRetractDistance * Vector3.back); //Set blade tip position based on how much player is squeezing the grip
                }
                break;
            case "Trigger":
                triggerValue = context.ReadValue<float>() + GameSettings.inputSensitivityBuffer; //Get current amount by which player is squeezing the trigger
                break;
            case "AButton":
                if (context.started) reverseGrip = true;   //Indicate that player is pressing reverse grip button
                if (context.canceled) reverseGrip = false; //Indicate that player has released the reverse grip button
                break;
            default: break; //Ignore unrecognized actions
        }
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Restores blade to sheathed position.
    /// </summary>
    /// <param name="disableInputTime">Also disables player input for this number of seconds (0 does not disable player input, less than 0 disables it indefinitely)</param>
    public override void Shutdown(float disableInputTime = 0)
    {
        base.Shutdown(disableInputTime); //Call base functionality
        reverseGrip = false;             //Clear reverse grip input
        gripValue = 0;                   //Clear grip input (chainsaw will begin sheathing)
        triggerValue = 0;                //Clear trigger input
        if (mode != BladeMode.Sheathed) mode = BladeMode.Retracting; //Skip grinding disengagement mode

        jawParticles.gameObject.SetActive(false);
        grindParticles.gameObject.SetActive(false);
    }

    //UTILITY METHODS:
    /// <summary>
    /// Updates position of blade extender to match position of blade tip.
    /// </summary>
    private void UpdateBladeExtender()
    {
        Vector3 newExtenderScale = bladeExtender.localScale; newExtenderScale.z = (bladeTip.localPosition.z * 5);                      //Match Z scale of extender to Z position of blade tip (should be fine if everything is set up right)
        bladeExtender.localScale = newExtenderScale;                                                                                   //Set blade extender's local scale so that it reaches and connects with blade tip
        newExtenderScale = bladeExtenderBack.localScale;                                                                               //Switch to modifying scale of rear blade extender
        float backExtenderInterpolant = Mathf.InverseLerp(bladeOriginPos.z, settings.bladeTraverseDistance, bladeTip.localPosition.z); //Get interpolant for back extender downscaling based on current blade length percentage
        newExtenderScale.z = Mathf.Lerp(bladeBackOriginSize, 0, backExtenderInterpolant);                                              //Use interpolant to scale down back extender as blade gets longer
        bladeExtenderBack.localScale = newExtenderScale;                                                                               //Apply new scale to back extender
        Vector3 newBackTipPos = bladeBackTip.localPosition; newBackTipPos.z = bladeExtenderBack.localScale.z / 2.7657f;
        bladeBackTip.localPosition = newBackTipPos;
    }
    /// <summary>
    /// Checks to see whether or not projectile striking player at given incoming direction can be deflected by chainsaw, then fires out a deflected projectile if so.
    /// </summary>
    /// <param name="incomingDirection">Direction projectile is coming at the chainsaw from.</param>
    /// <param name="projectileName">The full resource path for the incoming projectile.</param>
    public bool TryDeflect(Vector3 incomingDirection, string projectileName)
    {
        if (mode != BladeMode.Deflecting) return false; //Chainsaw cannot deflect while not deflecting
        float alignment = Vector3.Angle(wrist.forward, -incomingDirection);
        if (alignment <= settings.deflectionAngle)
        {
            barrel.rotation = Quaternion.LookRotation(-incomingDirection, Vector3.up);
            Projectile newProjectile; //Initialize reference container for spawned projectile
            if (!PhotonNetwork.IsConnected) //Weapon is in local fire mode
            {
                newProjectile = ((GameObject)Instantiate(Resources.Load(projectileName))).GetComponent<Projectile>(); //Instantiate projectile
                newProjectile.FireKindaDumb(barrel);
            }
            else //Weapon is firing on the network
            {
                newProjectile = PhotonNetwork.Instantiate(projectileName, barrel.position, barrel.rotation).GetComponent<Projectile>();        //Instantiate projectile on network
                newProjectile.photonView.RPC("RPC_Fire", RpcTarget.All, barrel.position, barrel.rotation, PlayerController.photonView.ViewID); //Initialize all projectiles simultaneously
            }
            audioSource.PlayOneShot(settings.deflectSound); //Play deflect sound
            if (PhotonNetwork.IsConnected) PlayerController.photonView.RPC("RPC_Deflect", RpcTarget.All, 0);

            TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
            if (tutorialManager != null && tutorialManager.GetCurrentTutorialSegment() == TutorialManager.Tutorial.PARRY)
                tutorialManager.IncrementTutorialProgress();

            //If the player deflects during a match, unlock an achievement
            if (PhotonNetwork.InRoom)
            {
                if (!AchievementListener.Instance.IsAchievementUnlocked(1))
                    AchievementListener.Instance.UnlockAchievement(1);

                NetworkManagerScript.localNetworkPlayer.networkPlayerStats.successfulDeflects++;

                rapidDeflectCheckActive = true;
                currentRapidDeflects++;

                //If the player has deflected 5 projectiles in a rapid succession, unlock an achievement
                if(currentRapidDeflects == 5)
                {
                    if (!AchievementListener.Instance.IsAchievementUnlocked(22))
                        AchievementListener.Instance.UnlockAchievement(22);
                }

                //If the player has successfully deflected 22 times in a match, unlock an achievement
                if (NetworkManagerScript.localNetworkPlayer.networkPlayerStats.successfulDeflects == 22)
                {
                    if (!AchievementListener.Instance.IsAchievementUnlocked(21))
                        AchievementListener.Instance.UnlockAchievement(21);
                }
            }

            return true; //Indicate that projectile was deflected
        }
        else return false; //Indicate that projectile was not deflected
    }
}
