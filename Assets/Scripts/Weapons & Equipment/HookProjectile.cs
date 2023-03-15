using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Special kind of projectile used for grappling hook system.
/// </summary>
public class HookProjectile : Projectile
{
    //Classes, Enums & Structs:
    /// <summary>
    /// Indicates certain distinct types of hook projectile behavior.
    /// </summary>
    public enum HookState
    {
        /// <summary>Hook is hidden and ready to be fired.</summary>
        Stowed,
        /// <summary>Hook has been launched and is freely traveling through the air.</summary>
        Deployed,
        /// <summary>Hook is not locked on to anything and is retracting back toward player's arm.</summary>
        Retracting,
        /// <summary>Hook is locked into a stationary object (such as a wall) and is pulling the player forward.</summary>
        Hooked,
        /// <summary>Hook is locked into another player and shenanigans are about to ensue.</summary>
        PlayerTethered
    }

    //Objects & Components:
    internal NewGrapplerController controller; //Weapon which controls this hook
    private Transform lockPoint;               //Physical point on projectile which locks onto hit objects. Hook is rotated around this point while it is in pull mode.
    private Transform tetherPoint;             //Physical point on hook which tether line is attached to
    private LineRenderer tether;               //Component which draws the line between projectile and launcher
    internal Transform originPlayerBody;       //Body position of controller's equivalent networkPlayer

    //Runtime Variables:
    /// <summary>
    /// Current behavior state of this hook projectile.
    /// </summary>
    internal HookState state = HookState.Stowed;
    internal NetworkPlayer hitPlayer;  //The player this hook is currently locked into (usually null)
    private Vector3 hitPosition;       //Position this hook is currently locked into (usually irrelevant)
    private Vector3 hitDirection;      //Direction to player when hook is initially locked in
    
    internal float timeInState;   //Indicates how long it has been since hook state has last changed (always counting up)
    private float retractSpeed;   //Current speed (in meters per second) at which hook is being retracted
    internal bool punchWhipped;   //True when projectile has been launched using punch-whip technique
    private float tetherDistance; //Distance between hook and player when hook last locked onto something

    //Utility Variables:
    /// <summary>
    /// Value between 0 and 1 representing player's current progression through grapple.
    /// </summary>
    public float ReelPercent { get { return 1 - Mathf.Clamp01(Vector3.SqrMagnitude(transform.position - controller.barrel.position) / Mathf.Pow(tetherDistance, 2)); } }

    //RUNTIME METHODS:
    private protected override void Awake()
    {
        //Initialization:
        base.Awake(); //Call base awake method

        //Get objects & components:
        lockPoint = transform.Find("LockPoint"); if (lockPoint == null) lockPoint = transform;                                               //Get lock point transform (use own transform if not given)
        tetherPoint = transform.Find("TetherPoint"); if (tetherPoint == null) tetherPoint = transform;                                       //Get tether point transform (use own transform if not given)
        tether = GetComponent<LineRenderer>(); if (tether == null) { Debug.LogWarning("Grappling Hook projectile needs a line renderer."); } //Get line renderer component

        //Initialize runtime vars:
        isHook = true; //Indicate that this is a hook projectile (for homing purposes)
    }
    private protected override void Update()
    {
        //Update timers:
        timeInState += Time.deltaTime; //Increment timer tracking time spent in current behavior state
        if (photonView.IsMine && state == HookState.Retracting) //Hook is currently being retracted
        {
            retractSpeed += controller.settings.retractAcceleration * Time.deltaTime; //Add acceleration to retraction speed
        }

        //Update position:
        switch (state) //Determine update behavior of projectile depending on which state it is in
        {
            case HookState.Deployed: //Hook has been launched and is flying through the air
                base.Update(); //Use normal projectile movement and homing
                if (photonView.IsMine && controller.settings.travelIntersectBehavior != HookshotSettings.LineIntersectBehavior.Ignore) //Hook needs to check if anything is intersecting its line
                {
                    if (Physics.Linecast(controller.barrel.position, transform.position, out RaycastHit hitInfo, controller.settings.lineCheckLayers)) //Check along line for collisions
                    {
                        if (HitsOwnPlayer(hitInfo)) { Debug.LogWarning("Grappling hook line just tried to hit player for some reason, check lineCheckLayers."); break; } //Super make sure line can't hit own player
                        if (controller.settings.travelIntersectBehavior == HookshotSettings.LineIntersectBehavior.Release) { Release(); break; }                         //Behavior is set to release on line intersection
                        if (controller.settings.travelIntersectBehavior == HookshotSettings.LineIntersectBehavior.Grab) { HitObject(hitInfo); break; }                   //Behavior is set to grab on line intersection
                    }
                }
                break;
            case HookState.Retracting: //Hook is released and is being pulled back toward player
                //Point away from player:
                Vector3 retractionTarget = photonView.IsMine ? controller.barrel.position : originPlayerBody.position; //Determine which point to retract toward depending on whether or not projectile is remote
                Vector3 pointDirection = (retractionTarget - transform.position).normalized; //Get direction from projectile to player
                transform.forward = -pointDirection;                                                   //Always point grappling hook in direction of player

                //Move toward player:
                transform.position = Vector3.MoveTowards(transform.position, retractionTarget, retractSpeed * Time.deltaTime); //Retract toward player at given speed
                if (photonView.IsMine && transform.position == retractionTarget) Stow();                                       //Stow once hook has gotten close enough to destination
                break;
            case HookState.Hooked: //Grappling hook is attached to a stationary object
                PointLock(hitPosition); //Rotate hook toward controlling player, maintaining world position of lock point

                //Move player:
                if (photonView.IsMine) //Only master version needs to be able to pull the player
                {
                    float effectivePullSpeed = controller.settings.basePullSpeed * (punchWhipped ? controller.settings.punchWhipBoost : 1); //Initialize value to pass as player pull speed (increase if hook was punch-whipped)
                    Vector3 newVelocity = (lockPoint.position - controller.barrel.position).normalized * effectivePullSpeed;                //Get base speed at which grappling hook pulls you toward target
                    Quaternion playerRotation = controller.player.bodyRb.rotation;                                                          //Get current rotation of player body
                    Vector3 handDiff = (playerRotation * controller.RelativePosition) - (playerRotation * controller.hookedHandPos);        //Get difference between current position of hand and position when it initially hooked something
                    if (Vector3.Angle(handDiff, hitDirection) > 90) //Player is yanking
                    {
                        Vector3 addVel = Vector3.Project(handDiff, hitDirection) * controller.settings.yankForce; //Get additional velocity to player based on how much they are pulling their arm back
                        newVelocity -= addVel;                                                                    //Apply additional velocity (rotated based on player orientation)
                    }
                    if (!punchWhipped) //Player is not in punch-whip mode
                    {
                        float maneuverMultiplier = controller.settings.lateralManeuverForce;                  //Initialize value for lateral maneuver force multiplier
                        Vector3 addVel = Vector3.ProjectOnPlane(handDiff, hitDirection) * maneuverMultiplier; //Get additional velocity to player based on how much they are pulling their arm to the side
                        newVelocity -= addVel;                                                                //Apply additional velocity (rotated based on player orientation)
                    }
                    controller.player.bodyRb.velocity = newVelocity; //Apply new velocity
                }
                break;
            case HookState.PlayerTethered: //Grappling hook is attached to an enemy player
                PointLock(hitPlayer.GetComponent<Targetable>().targetPoint.position); //Rotate hook toward controlling player, maintaining position at center mass of tethered player

                //Move player:
                if (photonView.IsMine) //NOTE: This is temporarily an exact copy of the normal pull logic
                {
                    float effectivePullSpeed = controller.settings.basePullSpeed * (punchWhipped ? controller.settings.punchWhipBoost : 1);                 //Initialize value to pass as player pull speed (increase if hook was punch-whipped)
                    Vector3 newVelocity = (lockPoint.position - controller.barrel.position).normalized * effectivePullSpeed;                                //Get base speed at which grappling hook pulls you toward target
                    Vector3 handDiff = controller.RelativePosition - controller.hookedHandPos;                                                              //Get difference between current position of hand and position when it initially hooked something
                    if (Vector3.Angle(handDiff, hitDirection) > 90) newVelocity -= Vector3.Project(handDiff, hitDirection) * controller.settings.yankForce; //Apply additional velocity to player based on how much they are pulling their arm back
                    if (!punchWhipped) newVelocity -= Vector3.ProjectOnPlane(handDiff, hitDirection) * controller.settings.lateralManeuverForce;            //Apply additional velocity to player based on how much they are pulling their arm to the side
                    controller.player.bodyRb.velocity = newVelocity;                                                                                        //Apply new velocity
                }
                break;
            default: break;
        }
        if (photonView.IsMine && (state == HookState.Hooked || state == HookState.PlayerTethered)) //Hook is hooked onto something
        {
            if (controller.settings.hookedIntersectBehavior != HookshotSettings.LineIntersectBehavior.Ignore) //Hook needs to check if anything is intersecting its line
            {
                if (Physics.Linecast(controller.barrel.position, tetherPoint.position, out RaycastHit hitInfo, controller.settings.lineCheckLayers)) //Check along line for collisions
                {
                    if (HitsOwnPlayer(hitInfo)) Debug.LogWarning("Grappling hook line just tried to hit player for some reason, check lineCheckLayers."); //Super make sure line can't hit own player
                    else if (controller.settings.hookedIntersectBehavior == HookshotSettings.LineIntersectBehavior.Release) Release();                    //Behavior is set to release on line intersection
                    else if (controller.settings.hookedIntersectBehavior == HookshotSettings.LineIntersectBehavior.Grab) HitObject(hitInfo);              //Behavior is set to grab on line intersection
                }
            }
        }

        //Update tether:
        if (state != HookState.Stowed) //Hook is currently out and about
        {
            tether.SetPosition(0, photonView.IsMine ? controller.barrel.position : originPlayerBody.position); //Move start of line to current position of launcher (on player arm)
            tether.SetPosition(1, tetherPoint.position);                                                       //Move end of line to current position of this projectile
        }
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Un-stows and fires grappling hook (mostly like a normal projectile).
    /// </summary>
    public override void Fire(Vector3 startPosition, Quaternion startRotation, int playerID)
    {
        //Deployment:
        transform.parent = null;                                                                                             //Un-child from stow point
        transform.localScale = Vector3.one;                                                                                  //Make sure scale is right after unchilding
        base.Fire(startPosition, startRotation, playerID);                                                                   //Use base fire method to physically launch hook
        estimatedLifeTime = -1;                                                                                              //Do not use lifetime system for this type of projectile
        if (photonView.IsMine) photonView.RPC("RPC_Fire", RpcTarget.OthersBuffered, startPosition, startRotation, playerID); //Fire hooks on the network
        else originPlayerBody = PhotonNetwork.GetPhotonView(playerID).GetComponent<Targetable>().targetPoint;                //Have remote projectiles get their player body from given playerID

        //Initialize tether:
        tether.SetPosition(0, photonView.IsMine ? controller.barrel.position : originPlayerBody.position); //Move start of line to current position of launcher (on player arm)
        tether.SetPosition(1, tetherPoint.position);                                                       //Move end of line to current position of this projectile
        tether.enabled = true;                                                                             //Make tether visible
        if (trail != null) { trail.enabled = true; trail.Clear(); }                                        //Enable and clear particle trail

        //Cleanup:
        ChangeVisibility(true);     //Make projectile visible
        punchWhipped = false;       //Clear punch-whipped status (used for cooldown during Stowed state)
        state = HookState.Deployed; //Indicate that hook is now deployed
        timeInState = 0;            //Reset state timer
    }
    public override void Fire(Transform barrel, int playerID)
    {
        Fire(barrel.position, barrel.rotation, playerID);
    }
    /// <summary>
    /// Hides and stows this projectile on a designated position on player hand.
    /// </summary>
    public void Stow()
    {
        //Move to position:
        if (photonView.IsMine) //This is the master version of this hook
        {
            if (hitPlayer != null) //Hook is currently latched onto a player which needs to be released
            {
                hitPlayer = null; //Indicate that hook is no longer tethered to a player
            }
            transform.parent = controller.hookVisibleOnBarrel ? controller.stowPoint : controller.barrel; //Child hook to stow point
            photonView.RPC("RPC_Stow", RpcTarget.OthersBuffered);                                         //Stow remote hooks
            if (!controller.handWeapon.holstered) controller.handWeapon.Holster(false); //Unholster gun after grappling (if it hasn't been already)
        }
        else transform.parent = originPlayerBody; //Child remote hooks to networkplayer's center mass
        transform.localPosition = Vector3.zero;    //Zero out position relative to stow point
        transform.localEulerAngles = Vector3.zero; //Zero out rotation relative to stow point

        //Cleanup:
        if (trail != null) trail.enabled = false;                     //Hide trail
        tether.enabled = false;                                       //Hide tether
        if (!controller.hookVisibleOnBarrel) ChangeVisibility(false); //Immediately make projectile invisible
        state = HookState.Stowed;                                     //Indicate that projectile is stowed
        timeInState = 0;                                              //Reset state timer
        retractSpeed = 0;                                             //Reset retraction speed
    }
    /// <summary>
    /// Version of stow method meant to be the first thing called on new hook projectile by its controller. Passes along the reference so everything runs smoothly.
    /// </summary>
    public void Stow(NewGrapplerController newController)
    {
        controller = newController; //Store controller reference
        Stow();                     //Call base method
    }
    /// <summary>
    /// Causes hook to let go of whatever it is attached to and begin retracting back toward the player.
    /// </summary>
    public void Release(bool bounce = false)
    {
        if (photonView.IsMine) //Master-only release procedure
        {
            //Player release:
            if (hitPlayer != null) //Hook is currently tethered to a player
            {
                hitPlayer = null; //Indicate to player that it is no longer tethered
            }

            //Begin retraction:
            retractSpeed = controller.settings.baseRetractSpeed;                   //Get retraction speed from controller settings
            if (punchWhipped) retractSpeed *= controller.settings.punchWhipBoost;  //Increase retraction speed if player is using a punch-whip
            photonView.RPC("RPC_Release", RpcTarget.OthersBuffered, retractSpeed); //Tell remote hooks to release and feed them the calculated release speed

            //Effects:
            if (bounce) controller.Bounced();     //Indicate to controller that hook has bounced off of something
            else controller.ForceReleased();      //Indicate to controller that hook has been released
            controller.handWeapon.Holster(false); //Unholster gun after grappling
        }

        //Cleanup:-
        transform.localScale = Vector3.one; //Make sure projectile is at its base scale
        state = HookState.Retracting;       //Indicate that hook is now returning to its owner
        timeInState = 0;                    //Reset state timer
    }
    /// <summary>
    /// Hook has hit something.
    /// </summary>
    private protected override void HitObject(RaycastHit hitInfo)
    {
        //Initialization:
        target = null; //Clear target

        //Check for bounce:
        if (controller.settings.bounceLayers == (controller.settings.bounceLayers | (1 << hitInfo.collider.gameObject.layer))) //Hook is bouncing off of a non-hookable layer
        {
            Release(true); //Release hook immediately
            return;        //Hit resolution has finished
        }

        //Check hook type:
        hitDirection = (transform.position - controller.barrel.position).normalized;       //Get direction from hook to hit object
        hitPlayer = hitInfo.collider.GetComponentInParent<NetworkPlayer>();                //Try to get network player from hit collider
        if (hitPlayer == null) hitPlayer = hitInfo.collider.GetComponent<NetworkPlayer>(); //Try again for network player if it was not initially gotten
        if (hitPlayer != null) HookToPlayer(hitPlayer);                                    //Hook is attaching to a player
        else HookToPoint(hitInfo.point);                                                   //Hook to given point

        //Cleanup:
        tetherDistance = Vector3.Distance(hitInfo.point, controller.barrel.position); //Get exact max length of tether
        if (trail != null) trail.enabled = false;                                     //Hide trail on hit
    }
    /// <summary>
    /// Makes projectile think that it has hit target object.
    /// </summary>
    public void AutoHitObject(RaycastHit hitInfo) { HitObject(hitInfo); }
    /// <summary>
    /// Immediately attaches grappling hook to given point.
    /// </summary>
    public void HookToPoint(Vector3 point)
    {
        hitPosition = point; //Mark position of hit
        PointLock(point);    //Lock hook to point
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_HookToPoint", RpcTarget.OthersBuffered, point); //Indicate to remote projectiles that they are being hooked to given point
            controller.HookedObstacle();                                        //Indicate to controller that an obstacle has been successfully hooked
        }
        state = HookState.Hooked; //Indicate that hook is now latched onto a surface
        timeInState = 0;          //Reset state timer
    }
    /// <summary>
    /// Immediately attaches grappling hook to given player.
    /// </summary>
    public void HookToPlayer(NetworkPlayer player)
    {
        //Validity checks:
        hitPlayer = player;                                                                                                                                    //Store reference to hit player
        if (hitPlayer.photonView.ViewID == PlayerController.photonView.ViewID) { print("Grappling hook hit own player, despite it all."); Release(); return; } //Prevent hook from ever hitting its own player

        //Move to target:
        Transform targetPoint = hitPlayer.GetComponent<Targetable>().targetPoint; //Get target point from hit player (should be approximately center mass)
        transform.parent = targetPoint;                                           //Child hook to player target point
        PointLock(targetPoint.position);                                          //Lock hook to position on target

        //Cleanup
        print("Hooked player!");
        if (photonView.IsMine) //Master-only cleanup for hooking a player
        {
            controller.HookedPlayer();                                                              //Indicate to controller that a player has been successfully hooked
            photonView.RPC("RPC_HookToPlayer", RpcTarget.OthersBuffered, player.photonView.ViewID); //Call remote hook method
        }
        state = HookState.PlayerTethered; //Indicate that a player has been successfully tethered
        timeInState = 0;                  //Reset state timer
        return;                           //Hit resolution has finished
    }
    /// <summary>
    /// Hooks are never destroyed and begin retracting when they run out of range (instead of burning out)
    /// </summary>
    private protected override void BurnOut()
    {
        Release(); //Just have hook release itself when it runs out of range.
    }

    //REMOTE METHODS:
    [PunRPC]
    public void RPC_Stow() { Stow(); }
    [PunRPC]
    public void RPC_Release(float speed)
    {
        retractSpeed = speed; //Get retraction speed from master version (calculated based on local controller settings)
        Release();            //Call base release method (already protected for remote versions)
    }
    [PunRPC]
    public void RPC_HookToPoint(Vector3 point) { HookToPoint(point); }
    [PunRPC]
    public void RPC_HookToPlayer(int playerID) { HookToPlayer(PhotonNetwork.GetPhotonView(playerID).GetComponent<NetworkPlayer>()); }

    //UTILITY METHODS:
    /// <summary>
    /// Hides or shows all renderers.
    /// </summary>
    private void ChangeVisibility(bool enable)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = enable; //Enable or disable all renderers
    }
    /// <summary>
    /// Places lockPoint transform position at given world coordinates, and rotates the rest of the hook to point back toward tethered player.
    /// </summary>
    private void PointLock(Vector3 lockPosition)
    {
        Vector3 pointDirection = ((photonView.IsMine ? controller.barrel.position : originPlayerBody.position) - lockPosition).normalized; //Get direction from lock point to end of player grappling arm
        transform.forward = -pointDirection;                                                                                               //Always point grappling hook in direction of player
        transform.position = lockPosition - (transform.rotation * lockPoint.localPosition);                                                //Move hook so that lockPoint is at target position
    }
}
