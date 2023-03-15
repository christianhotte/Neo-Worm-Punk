using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using CustomEnums;

/// <summary>
/// Used by player to lock onto walls and move rapidly through the level. Launches a special type of projectile which is always tracked on the network.
/// </summary>
public class NewGrapplerController : PlayerEquipment
{
    //Objects & Components:
    internal HookProjectile hook;        //The projectile this weapon fires and recalls to perform its actions (never destroyed)
    internal Transform stowPoint;        //Point on launcher at which hook projectile is invisibly stowed
    private Transform hand;              //Real position of player hand used by this equipment
    internal Transform barrel;           //Position hook is launched from, and point where the line begins
    internal PlayerEquipment handWeapon; //Player weapon held in the same hand as this chainsaw

    //Settings:
    [Tooltip("Mechanical properties describing hookshot functionality and effects.")]                                         public HookshotSettings settings;
    [Tooltip("When hook is stowed, hook will be visible and will be childed to barrel (weapon will not need a stow point).")] public bool hookVisibleOnBarrel;
    [Header("Debug Stuff:")]
    [SerializeField, Tooltip("Launches grappling hook.")] private bool debugLaunch;
    [SerializeField, Tooltip("Releases grappling hook.")] private bool debugRelease;

    //Runtime Variables:
    internal Vector3 hookedHandPos; //Player hand position (relative to body) when hook lands on surface

    //EVENTS & COROUTINES:
    /// <summary>
    /// NOTE: This should be replaced with something better. This is here because the hook can only be spawned once the player is in a room (which shouldn't be a problem once weapon spawning/equipping is sorted out).
    /// </summary>
    public IEnumerator TryToInitialize()
    {
        yield return new WaitUntil(() => PlayerController.photonView != null);                                                                       //Wait until player's network player has been spawned
        hook = PhotonNetwork.Instantiate("Projectiles/" + settings.hookResourceName, barrel.position, hand.rotation).GetComponent<HookProjectile>(); //Instantiate hook projectile on the network
        hook.Stow(this);                                                                                                                             //Immediately do a stow initialization on new projectile
    }

    //RUNTIME METHODS:
    private protected override void Awake()
    {
        //Initialization:
        base.Awake(); //Call base awake method

        //Get objects & components:
        if (handedness == Handedness.None) { Debug.LogError("GrapplingHook cannot recognize which side it is on. Make sure parent has the word Left or Right in its name."); Destroy(this); } //Make sure grappling hook knows which hand it is associated with
        if (!hookVisibleOnBarrel) { stowPoint = transform.Find("StowPoint"); if (stowPoint == null) stowPoint = transform; } //Get stow position (use own transform if none is given)
        barrel = transform.Find("Barrel"); if (barrel == null) barrel = transform;                                           //Get barrel position (use own transform if none is given)

        //Set up projectile:
        StartCoroutine(TryToInitialize()); //Begin trying to spawn hook (NOTE: will break if player leaves a room)
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
        //Debug functions:
        if (debugLaunch) { debugLaunch = false; Launch(); }    //Launch hook manually
        if (debugRelease) { debugRelease = false; Release(); } //Release hook manually

        //Cleanup:
        base.Update(); //Always perform base update method
    }
    private protected override void OnDestroy()
    {
        base.OnDestroy();                                         //Call base destruction method
        if (hook != null) PhotonNetwork.Destroy(hook.gameObject); //Destroy hook object when destroying this equipment
    }

    //INPUT METHODS:
    private protected override void InputActionTriggered(InputAction.CallbackContext context)
    {
        //Determine input target:
        switch (context.action.name) //Determine behavior depending on action name
        {
            case "Grip":
                float gripValue = context.ReadValue<float>();                                                                                                                //Get current position of grip axis
                if (hook == null) break;                                                                                                                                     //Do not try to launch hook before it has been spawned
                if (hook.state == HookProjectile.HookState.Stowed && gripValue >= settings.deployThreshold) Launch();                                                        //Launch hook when grip is squeezed
                if (hook.state != HookProjectile.HookState.Stowed && hook.state != HookProjectile.HookState.Retracting && gripValue <= settings.releaseThreshold) Release(); //Begin retracting hook when grip is released
                break;
            default: break; //Ignore unrecognized actions
        }
    }

    //FEEDBACK METHODS:
    /// <summary>
    /// Called when hook successfully locks onto a surface.
    /// </summary>
    public void HookedObstacle()
    {
        hookedHandPos = RelativePosition; //Get position of hand at moment of contact

        //Effects:
        if (settings.hitSound != null) audioSource.PlayOneShot(settings.hitSound, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f)); //Play sound effect
        SendHapticImpulse(settings.hitHaptics);                                    //Play haptic impulse
    }
    /// <summary>
    /// Called when hook successfully locks onto a player.
    /// </summary>
    public void HookedPlayer()
    {
        hookedHandPos = RelativePosition; //Get position of hand at moment of contact

        if (settings.playerHitSound != null) audioSource.PlayOneShot(settings.playerHitSound, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f)); //Play sound effect
        SendHapticImpulse(settings.hitHaptics);                                                //Play haptic impulse
    }
    /// <summary>
    /// Called when hook automatically releases itself for some reason.
    /// </summary>
    public void ForceReleased()
    {
        if (settings.releaseSound != null) audioSource.PlayOneShot(settings.releaseSound, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f)); //Play sound effect
        SendHapticImpulse(settings.releaseHaptics);                                        //Play haptic impulse
    }
    /// <summary>
    /// Called when hook bounces off of something it can't dig into.
    /// </summary>
    public void Bounced()
    {
        if (settings.bounceSound != null) audioSource.PlayOneShot(settings.bounceSound, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f)); //Play sound effect
        SendHapticImpulse(settings.releaseHaptics);                                      //Play haptic impulse
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Launches hookshot in direction hand is currently facing.
    /// </summary>
    private void Launch()
    {
        //Validation & initialization:
        if (hook == null) { print("Tried to launch without hook!"); return; }           //Do not try to launch hook if it does not exist yet (for some reason)
        if (hook.state != HookProjectile.HookState.Stowed) return;                      //Do not allow hook to be launched if it is not stowed
        if (hook.punchWhipped && hook.timeInState < settings.punchWhipCooldown) return; //Do not allow hook to be launched before punch-whip cooldown has ended

        //Fire hook:
        hook.Fire(barrel.position, hand.rotation, PlayerController.photonView.ViewID); //Fire using the position of barrel and rotation of hand (to allow for more directional flexibility)

        //Effects:
        Vector3 forwardHandVel = Vector3.Project(RelativeVelocity, hand.forward); //Get forward hand velocity (relative to barrel)
        if (Vector3.Angle(forwardHandVel, hand.forward) < 90 && forwardHandVel.magnitude > settings.punchWhipSpeed) //Player is punching hard/fast enough to trigger instant grab
        {
            if (Physics.Raycast(barrel.position, barrel.forward, out RaycastHit hit, hook.settings.range, ~hook.settings.ignoreLayers)) //Player can immediately hit something with the hook
            {
                hook.totalDistance = hit.distance; //Indicate to projectile how far it has traveled
                hook.AutoHitObject(hit);           //Have projectile automatically hit target object
                /*if (hit.distance > settings.minPunchWhipDist) //Punch whip will hit something within range
                {
                    float whipDist = hit.distance - settings.minPunchWhipDist; //Get distance whip action is causing projectile to instantly travel
                    hook.transform.position += barrel.forward * whipDist;      //Place hook just in front of punch whip target
                    hook.totalDistance = whipDist;                             //Make sure projectile knows exactly how far it has traveled
                }*/
            }
            else //Punch whip hit absolutely nothing
            {
                hook.transform.position += hand.forward * hook.settings.range; //Have hook instantaneously travel its entire range
                hook.totalDistance = hook.settings.range;                      //Indicate that hook has travelled its entire range
            }

            hook.punchWhipped = true;                                                    //Indicate to hook that it has been punch=whipped
            if (settings.whipSound != null) audioSource.PlayOneShot(settings.whipSound, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f)); //Play sound effect
        }
        else //Normal launch effects
        {
            if (settings.launchSound != null) audioSource.PlayOneShot(settings.launchSound, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f)); //Play sound effect
        }
        if (settings.holstersWeapon) handWeapon.Holster(); //Holster gun while grappling (if set to do so)
        SendHapticImpulse(settings.launchHaptics);         //Play haptic impulse
    }
    /// <summary>
    /// Releases whatever hookshot is currently hooked on to (if anything) and begins reeling it back in.
    /// </summary>
    private void Release()
    {
        //Validation & initialization:
        if (hook.state == HookProjectile.HookState.Retracting || hook.state == HookProjectile.HookState.Stowed) return; //Do not try to release hook after it has already been released

        //Release hook:
        hook.Release(); //Indicate to hook that it is being released and needs to return to player
    }
}
