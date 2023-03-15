using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    }

    //Objects & Components:
    [Header("Components:")]
    [SerializeField, Tooltip("End of blade, sticks out of the front of the sheath (retains its scale).")]                                        private Transform bladeTip;
    [SerializeField, Tooltip("Joins blade to wrist sheath, seamlessly scalable on the Z axis (Z scale is proportional to blade tip distance).")] private Transform bladeExtender;
    [SerializeField, Tooltip("Secondary blade extension for the backend of the weapon, can be whatever length (Z scale).")]                      private Transform bladeExtenderBack;
    [SerializeField, Tooltip("Jointed component on front of system which moves forward and rotates during blade activation.")]                   private Transform wrist;
    [SerializeField, Tooltip("Rotating assembly which allows the wrist to be turned downward for reverse grip mode.")]                           private Transform wristPivot;
    [SerializeField, Tooltip("Invisible transform which indicates the extent to which blade tracks for grinding and hitting players.")]          private Transform bladeEnd;

    private Transform hand;             //Real position of player hand used by this equipment
    private PlayerEquipment handWeapon; //Player weapon held in the same hand as this chainsaw

    //Settings:
    [Header("Settings:")]
    [SerializeField, Tooltip("Settings describing behavior specific to chainsaw functionality.")] private ChainsawSettings settings;

    //Runtime Variables:
    /// <summary>
    /// Blade's current behavior state.
    /// </summary>
    internal BladeMode mode = BladeMode.Sheathed;
    internal bool grinding;       //Whether or not player is currently grinding on a surface
    private float timeInMode;     //How long weapon has been in current mode for
    private float timeUntilPulse; //Time until next haptic pulse should be triggered (used for repeated pulses while saw is active)
    private bool reverseGrip;     //Indicates that player is pressing the input for reverse grip mode (only valid while chainsaw is active)
    private float gripValue;      //Latest value from grip input
    private float triggerValue;   //Latest value from trigger input

    private Vector3 bladeOriginPos;    //Initial local (sheathed) position of blade
    private Vector3 wristOriginPos;    //Initial local (sheathed) position of wrist assembly
    private float bladeBackOriginSize; //Initial length of rear blade extender

    //RUNTIME METHODS:
    private protected override void Awake()
    {
        //Initialization:
        base.Awake(); //Do base awake setup

        //Get objects & components:


        //Get runtime vars:
        bladeOriginPos = bladeTip.localPosition;              //Get starting local position of blade assembly
        wristOriginPos = wrist.localPosition;                 //Get starting local position of wrist assembly
        bladeBackOriginSize = bladeExtenderBack.localScale.z; //Get starting local Z scale of rear blade extender
    }
    private protected override void Start()
    {
        base.Start(); //Call base start stuff

        //Late object & component get:
        hand = (handedness == 0 ? player.leftHand : player.rightHand).transform; //Get a reference to the relevant player hand
        foreach (PlayerEquipment equipment in player.attachedEquipment) //Iterate through all equipment attached to player
        {
            if (equipment != this && equipment.handedness == handedness) { handWeapon = equipment; break; } //Try to get weapon used by same hand
        }
    }
    private protected override void Update()
    {
        base.Update(); //Call base equipment update method

        //Update timers:
        timeInMode += Time.deltaTime; //Increment mode time tracker
        if (mode == BladeMode.Extended || mode == BladeMode.Extending) //Blade is in extended mode
        {
            timeUntilPulse -= Time.deltaTime; //Increment pulse time tracker
            if (timeUntilPulse <= 0) //It is time for the next haptic pulse
            {
                HapticData newPulse = settings.activeHapticPulse;                                                                                   //Get values from settings for active pulse
                newPulse.amplitude += Random.Range(-settings.activeHapticMagnitudeVariance, settings.activeHapticMagnitudeVariance);                //Add a little bit of random variation to the pulse
                SendHapticImpulse(newPulse);                                                                                                        //Play new haptic pulse
                timeUntilPulse = newPulse.duration + Random.Range(-settings.activeHapticFrequencyVariance, settings.activeHapticFrequencyVariance); //Schedule new pulse with slight variation in activation time
            }
        }

        //Extend/Retract blade:
        if (mode == BladeMode.Sheathed && gripValue >= settings.triggerThreshold) //Grip has been squeezed enough to activate the chainsaw
        {
            //Switch modes:
            if (handWeapon != null) handWeapon.Holster();     //Holster weapon held in hand if possible
            PlaySFX(settings.extendSound);                    //Play extend sound
            SendHapticImpulse(settings.extendHaptics);        //Play extend haptics
            mode = BladeMode.Extending;                       //Indicate that blade is now extending
            timeInMode = 0;                                   //Reset mode time tracker
            timeUntilPulse = settings.extendHaptics.duration; //Set pulse timer to begin pulsing as soon as extend haptics have finished
        }
        else if (mode == BladeMode.Extended && gripValue < settings.releaseThreshold) //Grip has been released enough to re-sheath the chainsaw (always check in case of early release)
        {
            //Switch mode:
            if (handWeapon != null) handWeapon.Holster(false); //Un-holster weapon held in hand if possible
            PlaySFX(settings.sheathSound);                     //Play retraction sound
            SendHapticImpulse(settings.retractHaptics);        //Play haptic impulse
            mode = BladeMode.Retracting;                       //Indicate that blade is now retracting
            timeInMode = 0;                                    //Reset mode time tracker
            grinding = false;                                  //Indicate player can no longer be grinding on a surface
        }
        if (mode == BladeMode.Extending || mode == BladeMode.Retracting) //Blade is moving between primary modes
        {
            //Initialize:
            float timeInterpolant = timeInMode / settings.bladeExtendTime;                                                  //Get interpolant value representing percentage of blade animation completed so far
            float gripInterpolant = settings.bladePreRetractCurve.Evaluate(gripValue / settings.triggerThreshold);          //Get multiplier to apply to retraction distance for true retracted position
            Vector3 bladeExtendPos = bladeOriginPos + (Vector3.forward * settings.bladeTraverseDistance);                   //Get target position blade is extending to (or retracting from)
            Vector3 bladeRetractPos = bladeOriginPos + (gripInterpolant * settings.bladePreRetractDistance * Vector3.back); //Get target position blade is retracting to (or extending from)
            Vector3 wristExtendPos = wristOriginPos + (Vector3.forward * settings.wristExtendDistance);                     //Get extended target position of wrist

            //Move blade tip:
            if (mode == BladeMode.Extending) bladeTip.localPosition = Vector3.LerpUnclamped(bladeRetractPos, bladeExtendPos, settings.bladeExtendCurve.Evaluate(timeInterpolant)); //Move extending blade to interpolated position
            else bladeTip.localPosition = Vector3.LerpUnclamped(bladeExtendPos, bladeRetractPos, settings.bladeRetractCurve.Evaluate(timeInterpolant));                            //Move retracting blade to interpolated position

            //Move wrist:
            if (timeInterpolant <= settings.wristDeployPeriod) //Wrist deploys during first part of blade extension/retraction
            {
                float wristInterpolant = Mathf.InverseLerp(0, settings.wristDeployPeriod, timeInterpolant);                            //Get special interpolant for wrist deployment
                wristInterpolant = settings.wristDeployCurve.Evaluate(wristInterpolant);                                               //Feed interpolant through animation curve
                if (mode == BladeMode.Extending) wrist.localPosition = Vector3.Lerp(wristOriginPos, wristExtendPos, wristInterpolant); //Move wrist toward extended position
                else wrist.localPosition = Vector3.Lerp(wristExtendPos, wristOriginPos, wristInterpolant);                             //Move wrist toward extended position
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
                mode = (mode == BladeMode.Extending ? BladeMode.Extended : BladeMode.Sheathed);            //Progress mode to stable state
                timeInMode = 0;                                                                            //Reset mode timer
            }

            //Stretch blade extender:
            Vector3 newExtenderScale = bladeExtender.localScale; newExtenderScale.z = bladeTip.localPosition.z;                            //Match Z scale of extender to Z position of blade tip (should be fine if everything is set up right)
            bladeExtender.localScale = newExtenderScale;                                                                                   //Set blade extender's local scale so that it reaches and connects with blade tip
            newExtenderScale = bladeExtenderBack.localScale;                                                                               //Switch to modifying scale of rear blade extender
            float backExtenderInterpolant = Mathf.InverseLerp(bladeOriginPos.z, settings.bladeTraverseDistance, bladeTip.localPosition.z); //Get interpolant for back extender downscaling based on current blade length percentage
            newExtenderScale.z = Mathf.Lerp(bladeBackOriginSize, 0, backExtenderInterpolant);                                              //Use interpolant to scale down back extender as blade gets longer
            bladeExtenderBack.localScale = newExtenderScale;                                                                               //Apply new scale to back extender
        }

        //Blade rotations:
        if (mode == BladeMode.Extended || mode == BladeMode.Extending) //Blade is currently deployed or is being deployed
        {
            //Update wrist rotation:
            Quaternion targetWristRot = Quaternion.LookRotation(hand.forward, -hand.right);                            //Get target wrist rotation from rotation of hand (in order to make weapon more controllable)
            targetWristRot = Quaternion.RotateTowards(wrist.parent.rotation, targetWristRot, settings.maxWristAngle);  //Clamp rotation to set angular limit
            wrist.rotation = Quaternion.Lerp(wrist.rotation, targetWristRot, settings.wristLerpRate * Time.deltaTime); //Lerp wrist toward target rotation

            //Reverse grip:
            float targetPivotRot = reverseGrip ? -settings.reverseGripAngle : 0; //Get target Y rotation for wrist pivot
            if (wristPivot.localEulerAngles.y != targetPivotRot)
            {
                Vector3 newPivotRot = wristPivot.localEulerAngles;                                                             //Get current eulers from pivot
                newPivotRot.y = Mathf.LerpAngle(newPivotRot.y, targetPivotRot, settings.reverseGripLerpRate * Time.deltaTime); //Get new lerped y rotation value for pivot
                wristPivot.localEulerAngles = newPivotRot;                                                                     //Apply new eulers to pivot
            }

            //Wall grinding:
            Vector3 bladeOffset = wristPivot.right * settings.bladeWidth; //Get distance of offset for secondary blade cast
            if (Physics.Linecast(wristPivot.position, bladeEnd.position, out RaycastHit hitInfo, settings.grindLayers) ||                //Check for obstacles intersecting back of the blade
                Physics.Linecast(wristPivot.position + bladeOffset, bladeEnd.position + bladeOffset, out hitInfo, settings.grindLayers)) //Check for obstacles intersecting front of the blade
            {
                //Adjust player velocity:
                Vector3 grindDirection = Vector3.Cross(hitInfo.normal, wrist.up).normalized; //Get target direction of grind
                float grindSpeed = settings.grindSpeed;                                      //Get base speed for grinding
                grindSpeed *= Mathf.Lerp(1, settings.triggerGrindMultiplier, triggerValue);  //Modify grind speed by multiplier depending on how much player is squeezing the trigger
                playerBody.velocity = grindDirection * grindSpeed;                           //Modify player velocity based on grind values

                //Cleanup:
                Debug.DrawRay(hand.position, grindDirection, Color.cyan);
                grinding = true;
            }
            else //Blade is not touching a wall
            {
                //Cleanup:
                grinding = false;
            }

            //Player killing:
            if (Physics.Linecast(wristPivot.position, bladeEnd.position, out hitInfo, LayerMask.GetMask("Player")))
            {
                NetworkPlayer hitPlayer = hitInfo.collider.GetComponentInParent<NetworkPlayer>(); //Try to get networkplayer from hit
                if (hitPlayer != null && !hitPlayer.photonView.IsMine) //Player (other than self) has been hit by blade
                {
                    hitPlayer.photonView.RPC("RPC_Hit", Photon.Pun.RpcTarget.AllBuffered, 3, PlayerController.photonView.ViewID); //Hit target
                }
            }
        }
        else if (mode == BladeMode.Retracting) //Blade is currently retracting
        {
            //Return to base rotation:
            wrist.localRotation = Quaternion.RotateTowards(wrist.localRotation, Quaternion.identity, settings.wristRotReturnRate * Time.deltaTime);              //Have wrist return to base rotation
            wristPivot.localRotation = Quaternion.RotateTowards(wristPivot.localRotation, Quaternion.identity, settings.reverseGripReturnRate * Time.deltaTime); //Have wrist pivot return to base rotation
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
                gripValue = context.ReadValue<float>(); //Get current amount by which player is squeezing the grip
                if (mode == BladeMode.Sheathed) //Blade is currently sheathed
                {
                    float gripInterpolant = settings.bladePreRetractCurve.Evaluate(gripValue / settings.triggerThreshold);         //Get multiplier to apply to retraction distance in order to pull blade back slightly
                    bladeTip.localPosition = bladeOriginPos + (gripInterpolant * settings.bladePreRetractDistance * Vector3.back); //Set blade tip position based on how much player is squeezing the grip
                }
                break;
            case "Trigger":
                triggerValue = context.ReadValue<float>(); //Get current amount by which player is squeezing the trigger
                break;
            case "AButton":
                if (context.started) reverseGrip = true;   //Indicate that player is pressing reverse grip button
                if (context.canceled) reverseGrip = false; //Indicate that player has released the reverse grip button
                break;
            default: break; //Ignore unrecognized actions
        }
    }
}
