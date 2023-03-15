using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;

/// <summary>
/// Base class for any non-hitscan ballistic projectiles.
/// </summary>
public class Projectile : MonoBehaviourPunCallbacks
{
    //Objects & Components:
    private AudioSource audioSource;       //Audiosource component used by this projectile to make sound effects
    private protected TrailRenderer trail; //Renderer component for trail projectile leaves in its wake

    //Settings:
    [Tooltip("Settings object determining base properties of projectile.")]                      public ProjectileSettings settings;
    [SerializeField, Tooltip("When on, projectile will print debug information as it travels.")] private bool printDebug = true;
    [SerializeField, Tooltip("When on, projectile will draw lines showing its homing radius.")]  private bool debugDrawHoming = true;
    [Header("Sounds:")]
    [SerializeField, Tooltip("Material projectile has when it has locked onto a target (for debug purposes).")] private Material homingMat;
    [SerializeField, Tooltip("Sound projectile makes when it's homing in on a player.")]                        private AudioClip homingSound;

    //Runtime Variables:
    private bool dumbFired;             //Indicates that this projectile was fired by a non-player
    internal int originPlayerID;        //PhotonID of player which last fired this projectile
    private protected Vector3 velocity; //Speed and direction at which projectile is traveling
    private protected Transform target; //Transform which projectile is currently homing toward

    internal float totalDistance;              //Total travel distance covered by this projectile
    private float timeAlive;                   //How much time this projectile has been alive for
    private protected float estimatedLifeTime; //Approximate projectile lifetime calculated based on velocity and range

    private protected bool isHook; //Whether or not this projectile is a hook (set by hook script)
    private Vector3 prevTargetPos; //Previous position of target, used for velocity prediction
    private Material origMat;      //Original material projectile had when spawned

    //Utility Variables:
    /// <summary>
    /// How much distance this projectile has left to cover.
    /// </summary>
    private float RemainingRange { get { return settings.range - totalDistance; } }
    /// <summary>
    /// Percentage of distance which has been traveled by this projectile so far.
    /// </summary>
    private float DistancePercent { get { return 1 - (RemainingRange / settings.range); } }

    //COROUTINES:
    /// <summary>
    /// Performs target acquisition check on set timestep until projectile has its desired target (also resets targeting state).
    /// </summary>
    /// <returns></returns>
    IEnumerator DoTargetAcquisition()
    {
        //Initialization:
        if (printDebug) print("Homing Initiated!");               //Have projectile log that it has begun homing
        target = null;                                            //Reset current target
        List<Transform> potentialTargets = new List<Transform>(); //Create list for storing viable targets
        float targetHeuristic = 0;                                //Heuristic value for current target (the higher the better)

        //Populate potential targets list:
        foreach (Targetable targetable in Targetable.instances) //Iterate through list of targetables
        {
            //Eliminate non-viable targets:
            if (targetable.type == Targetable.TargetType.BulletsOnly && isHook || //Targetable object is only targetable by bullets
                targetable.type == Targetable.TargetType.HooksOnly && !isHook)    //Targetable object is only targetable by hooks
            {
                if (printDebug) print("Target ignored, incompatible type."); //Indicate reason target was ignored
                continue;                                                    //Ignore target
            }
            Vector3 targetSep = targetable.targetPoint.position - transform.position; //Get distance and direction from projectile to target
            float targetDist = targetSep.magnitude;                                   //Distance from projectile to target
            if (targetDist > RemainingRange)                                          //Target is outside projectile's potential range
            {
                if (printDebug) print("Target ignored, too far away. Distance: " + targetDist); //Indicate reason target was ignored
                continue;                                                                       //Ignore target
            }
            float targetAngle = Vector3.Angle(targetSep, transform.forward); //Get angle between target direction and projectile movement direction
            if (targetAngle > settings.targetDesignationAngle.y) //Target is outside projectile's viable targeting angle
            {
                if (printDebug) print("Target ignored, outside angle. Angle from projectile: " + targetAngle); //Indicate reason target was ignored
                continue;                                                                                      //Ignore target
            }
            if (targetable.TryGetComponent(out NetworkPlayer targetPlayer)) //Target is a player
            {
                if (targetPlayer.photonView.ViewID == originPlayerID) //This is the player who shot this projectile
                {
                    if (printDebug) print("Target ignored, projectile cannot target player who shot it."); //Indicate reason target was ignored
                    continue;                                                                              //Ignore target
                }
            }

            //Cleanup:
            potentialTargets.Add(targetable.targetPoint); //Add valid targets to list of targets to check
        }
        if (printDebug) print("Potential Targets: " + potentialTargets.Count + " / " + Targetable.instances.Count); //Indicate number of potential targets vs actual target options

        //Look for targets:
        while (true) //Run forever
        {
            //Initialization:
            if (potentialTargets.Count == 0) break;                                                                               //Stop when projectile has run out of targets
            float realDesignationAngle = settings.targetAngleCurve.Evaluate(DistancePercent) * settings.targetDesignationAngle.x; //Apply multiplier for target designation angle based on evaluation of curve over range
            float realTargetingDistance = settings.targetingDistanceCurve.Evaluate(DistancePercent) * settings.targetingDistance; //Apply multiplier for target designation distance based on evaluation of curve over range

            //Targeting sequence:
            for (int x = 0; x < potentialTargets.Count;) //Iterate through list of potential targets (allow internal functionality to manually index to next target)
            {
                //Eliminate non-viable targets:
                Transform potentialTarget = potentialTargets[x];                   //Get reference to current target
                Vector3 targetSep = potentialTarget.position - transform.position; //Get distance and direction from projectile to target
                float targetDist = targetSep.magnitude;                            //Distance from projectile to target
                float targetAngle = Vector3.Angle(targetSep, transform.forward);   //Get angle between target direction and projectile movement direction
                if (targetAngle > settings.targetDesignationAngle.y)               //Angle to potential target is so steep that projectile will likely never hit
                {
                    potentialTargets.RemoveAt(x); //Remove this target from list of potential targets
                    if (potentialTarget == target) //Active target has just been passed
                    {
                        LoseTarget();        //Clear current target
                        targetHeuristic = 0; //Clear current target heuristic
                    }
                    if (printDebug) print("Potential target ignored, outside angle. Angle to projectile: " + targetAngle); //Indicate why target is being ignored
                    continue; //Skip to next potential target
                }

                //Check for viable targets:
                if (targetDist <= realTargetingDistance && targetAngle <= realDesignationAngle) //Current targetable is within range and within acquisition angle
                {
                    //Check for obstructions:
                    if (settings.LOSTargeting) //System is using line-of-sight targeting
                    {
                        if (Physics.Linecast(transform.position, potentialTarget.position, ~settings.targetingIgnoreLayers)) //Object is obstructed
                        {
                            if (potentialTarget == target) //Active target has been obstructed
                            {
                                LoseTarget();        //Clear current target
                                targetHeuristic = 0; //Clear current target heuristic
                            }
                            x++; continue; //Move to next potential target
                        }
                    }

                    //Check target viability:
                    float currentHeuristic = (1 - settings.angleDistancePreference) * Mathf.InverseLerp(realDesignationAngle, 0, targetAngle); //Calculate angle preference score for this target
                    currentHeuristic += settings.angleDistancePreference * Mathf.InverseLerp(realTargetingDistance, 0, targetDist);            //Calculate proximity preference score for this target

                    //Update current target:
                    if (target == potentialTarget) targetHeuristic = currentHeuristic; //Update current target heuristic whenever current target is re-acquired
                    else if (target == null || currentHeuristic > targetHeuristic) //Either projectile has no current target or potential target is more viable than current target
                    {
                        AcquireTarget(potentialTarget);     //Update target
                        targetHeuristic = currentHeuristic; //Update target heuristic
                    }
                }

                //Cleanup:
                x++; //Move to next potential target
            }

            //Cleanup:
            if (target != null && !settings.alwaysLookForTarget) break; //Stop homing if permanent target has been found
            yield return new WaitForFixedUpdate();                      //Wait until next update
        }
    }

    //RUNTIME METHODS:
    private protected virtual void Awake()
    {
        //Check settings:
        if (settings == null) //Settings were not provided
        {
            Debug.LogWarning("Projectile " + name + " is missing settings, using system defaults.");    //Log warning in case someone forgot
            settings = (ProjectileSettings)Resources.Load("DefaultSettings/DefaultProjectileSettings"); //Load default settings from Resources folder
        }

        //Objects & components:
        if (!TryGetComponent(out audioSource)) audioSource = gameObject.AddComponent<AudioSource>(); //Make sure projectile has an audiosource component
        if (!TryGetComponent(out trail)) trail = GetComponentInChildren<TrailRenderer>();            //Try to get projectile trail renderer
        origMat = GetComponentInChildren<Renderer>().material;                                       //Get original version of material on projectile
        
    }
    private protected virtual void Update()
    {
        //Draw homing debugs:
        if (debugDrawHoming) //Projectile is in debug mode
        {
            //Draw targeting cones:
            float targetingAngle = settings.targetAngleCurve.Evaluate(DistancePercent) * settings.targetDesignationAngle.x;                   //Get current angle at which projectile can track targets
            Vector3 centerLineEnd = settings.targetingDistanceCurve.Evaluate(DistancePercent) * settings.targetingDistance * Vector3.forward; //Create a vector pointing in world forward direction with length equal to current projectile homing distance
            Vector3 lineEnd1 = Quaternion.Euler(0, targetingAngle, 0) * centerLineEnd;                                                        //Rotate vector to point at edge of target designation cone
            Vector3 lineEnd2 = Quaternion.Euler(0, settings.targetDesignationAngle.y, 0) * centerLineEnd * 0.5f;                              //Rotate vector to point at edge of target exclusion cone (make half as large to cut down on clutter)
            Quaternion forwardRotator = Quaternion.FromToRotation(Vector3.forward, transform.forward);                                        //Get a quaternion which aligns vectors to projectile orientation
            Vector3[] innerPositions = new Vector3[10]; Vector3[] outerPositions = new Vector3[10];                                           //Initialize arrays to store endpoint positions
            for (int x = 0; x < 10; x++) //Iterate 10 times
            {
                //Draw main cones:
                Quaternion currentRotator = Quaternion.Euler(0, 0, x * 36);                              //Get quaternion which will rotate lines around forward axis
                innerPositions[x] = (forwardRotator * (currentRotator * lineEnd1)) + transform.position; //Calculate and store world position of inner targeting cone point
                outerPositions[x] = (forwardRotator * (currentRotator * lineEnd2)) + transform.position; //Calculate and store world position of outer targeting cone point
                Debug.DrawLine(transform.position, innerPositions[x], Color.green);                      //Draw line for this side of inner targeting cone
                Debug.DrawLine(transform.position, outerPositions[x], Color.red);                        //Draw line for this side of outer targeting cone

                //Draw cone outlines:
                if (x == 0) continue;                                                  //Skip if there are no two points to connect yet
                Debug.DrawLine(innerPositions[x], innerPositions[x - 1], Color.green); //Draw inner outlines
                Debug.DrawLine(outerPositions[x], outerPositions[x - 1], Color.red);   //Draw outer outlines
                if (x == 9) //Process is on its final loop, an additional line needs to be drawn to complete circle
                {
                    Debug.DrawLine(innerPositions[x], innerPositions[0], Color.green); //Wrap around to connect circle
                    Debug.DrawLine(outerPositions[x], outerPositions[0], Color.red);   //Wrap around to connect circle
                }
            }
        }

        //Modify velocity:
        if (settings.drop > 0) velocity.y -= settings.drop * Time.deltaTime; //Perform bullet drop (downward acceleration) if relevant
        if (target != null) //Projectile has a target
        {
            //Get effective target position:
            Vector3 targetPos = target.position; //Initialize target position marker
            if (settings.predictionStrength > 0) //Projectile is using velocity prediction
            {
                Vector3 targetVelocity = (targetPos - prevTargetPos) / Time.deltaTime;                         //Approximate velocity of target object
                float currentSpeed = velocity.magnitude;                                                       //Get current speed here so it only has to be calculated once
                float distanceToTarget = Vector3.Distance(transform.position, targetPos);                      //Get distance between projectile and target
                float timeToTarget = distanceToTarget / currentSpeed;                                          //Approximate time it would take to reach target
                targetPos = target.position + ((targetVelocity * timeToTarget) * settings.predictionStrength); //Adjust effective target position to where target will be when reached (dampening with prediction strength)
                prevTargetPos = target.position;                                                               //Update target position tracker
            }

            //Draw debug:
            if (debugDrawHoming) Debug.DrawLine(transform.position, targetPos, Color.red); //Draw a line from projectile to target position

            //Curve to meet target:
            Vector3 newVelocity = (targetPos - transform.position).normalized * velocity.magnitude;                      //Get velocity which would point projectile directly at target
            float realHomingStrength = settings.homingStrengthCurve.Evaluate(DistancePercent) * settings.homingStrength; //Evaluate strength curve to get actual current projectile homing strength
            velocity = Vector3.MoveTowards(velocity, newVelocity, realHomingStrength * Time.deltaTime);                  //Incrementally adjust toward target velocity
        }

        //Get target position:
        Vector3 newPosition = transform.position + (velocity * Time.deltaTime);   //Get target projectile position
        float travelDistance = Vector3.Distance(transform.position, newPosition); //Get distance this projectile is moving this update

        //Check range:
        totalDistance += travelDistance; //Add motion to total distance traveled (NOTE: may briefly end up being greater than actual distance traveled)
        if (settings.range > 0 && totalDistance > settings.range) //Distance about to be traveled exceeds range
        {
            //Backtrack target:
            float backtrackAmt = totalDistance - settings.range;                              //Determine length by which to backtrack projectile
            travelDistance -= backtrackAmt;                                                   //Adjust travel distance to account for limited range
            newPosition = Vector3.MoveTowards(newPosition, transform.position, backtrackAmt); //Backtrack target position by given amount (ensuring projectile travels exact range)
            totalDistance = settings.range;                                                   //Indicate that projectile has now traveled exactly its full range
        }

        //Check for hit:
        if (photonView.IsMine || dumbFired) //Only check for hits on local projectile
        {
            //Thin collision check:
            //NOTE: This is intended to allow projectiles to be better at making it around walls
            if (Physics.Linecast(transform.position, newPosition, out RaycastHit hitInfo, ~settings.ignoreLayers)) //Do a simple linecast, only ignoring designated layers
            {
                if (!HitsOwnPlayer(hitInfo)) //Make sure projectile is not hitting its own player
                {
                    totalDistance -= velocity.magnitude - hitInfo.distance; //Update totalDistance to reflect actual distance traveled at exact point of contact
                    HitObject(hitInfo);                                     //Trigger hit procedure
                    return;                                                 //Do nothing else
                }
            }

            //Thick collision check:
            if (settings.radius > 0) //Projectile has a radius
            {
                RaycastHit[] hits = Physics.SphereCastAll(transform.position, settings.radius, velocity, travelDistance, ~settings.radiusIgnoreLayers); //Do a spherecast along bullet trajectory, get all colliders (in case it collides with own player)
                foreach (RaycastHit hit in hits) //Iterate through each thing projectile has hit along path
                {
                    if (!HitsOwnPlayer(hit)) //Do not acnowledge hits on own player
                    {
                        totalDistance -= velocity.magnitude - hit.distance; //Update totalDistance to reflect actual distance traveled at exact point of contact
                        HitObject(hit);                                     //Trigger hit procedure
                        return;                                             //Do nothing else
                    }
                }
            }
        }

        //Perform move:
        transform.position = newPosition;                       //Move projectile to target position
        transform.rotation = Quaternion.LookRotation(velocity); //Rotate projectile to align with current velocity

        //Burnout check:
        if (photonView.IsMine || dumbFired) ////Only check if projectile is authoritative version
        {
            timeAlive += Time.deltaTime;                                               //Update time tracker
            if (estimatedLifeTime > 0 && timeAlive > estimatedLifeTime) BurnOut();     //Burn projectile out if it has been alive for too long (if lifetime is being used)
            else if (settings.range > 0 && totalDistance >= settings.range) BurnOut(); //Destroy projectile if it has reached its maximum range
        }
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Call this method to fire this projectile from designated barrel.
    /// </summary>
    /// <param name="barrel">Determines starting position, orientation and velocity of projectile.</param>
    /// <param name="playerID">The ViewID number of the player who fired this projectile.</param>
    public virtual void Fire(Transform barrel, int playerID)
    {
        Fire(barrel.position, barrel.rotation, playerID); //Break transform apart and perform normal firing operation
    }
    /// <summary>
    /// Call this method if projectile needs to be fired without an object reference (safe for remote projectiles).
    /// </summary>
    /// <param name="playerID">The ViewID number of the player who fired this projectile.</param>
    public virtual void Fire(Vector3 startPosition, Quaternion startRotation, int playerID)
    {
        //Initialize values:
        Vector3 targetPosition = startPosition;                  //Initialize value for ideal starting position
        transform.rotation = startRotation;                      //Rotate to initial orientation
        velocity = transform.forward * settings.initialVelocity; //Give projectile initial velocity (aligned with forward direction of barrel)
        originPlayerID = playerID;                               //Record ID of player firing this projectile
        totalDistance = 0;                                       //Reset total distance value (in case this is not the first time projectile has been fired)

        //Check barrel gap:
        if (settings.barrelGap > 0) //Projectile is spawning slightly ahead of barrel
        {
            //Perform a mini position update:
            targetPosition += transform.forward * settings.barrelGap; //Get target starting position (with barrel gap)
            if (photonView.IsMine) //Only have master projectile check for homing
            {
                if (Physics.Linecast(startPosition, targetPosition, out RaycastHit hitInfo, ~settings.ignoreLayers)) //Barrel gap movement would cause projectile to hit something
                {
                    if (!HitsOwnPlayer(hitInfo)) { HitObject(hitInfo); return; }     //Make sure hit object is not the projectile's own player, then if not, hit it immediately
                    if (settings.range <= settings.barrelGap) { BurnOut(); return; } //Burn projectile out in the unlikely event that the barrel gap is greater than its range
                }
            }
            totalDistance += settings.barrelGap; //Include distance in total distance traveled
        }

        //Cleanup:
        if (printDebug) print("Projectile Fired!"); //Print debug signal if called for
        transform.position = targetPosition;        //Move to initial position
        if (photonView.IsMine || dumbFired) //This is the authoritative version of the projectile
        {
            if (settings.homingStrength > 0) StartCoroutine(DoTargetAcquisition()); //Begin doing target acquisition if projectile does homing
            estimatedLifeTime = settings.range / settings.initialVelocity;          //Calculate approximate lifetime based on range and velocity
        }
    }
    /// <summary>
    /// Overload for FireDumb which safely fires projectile with no player reference or specific barrel transform.
    /// </summary>
    public void FireDumb(Vector3 startPosition, Quaternion startRotation)
    {
        dumbFired = true;                       //Indicate that projectile was fired without player
        Fire(startPosition, startRotation, -1); //Do normal firing procedure (give negative playerID to make sure systems know this is a non-player projectile)
    }
    /// <summary>
    /// Safely fires projectile with no player reference.
    /// </summary>
    public void FireDumb(Transform barrel)
    {
        FireDumb(barrel.position, barrel.rotation); //Pass to more granular FireDumb method
    }
    /// <summary>
    /// Called whenever projectile strikes an object.
    /// </summary>
    /// <param name="hitInfo">Data about object struck.</param>
    /// <summary>
    /// Sets this projectile to home in toward designated target.
    /// </summary>
    public void AcquireTarget(Transform newTarget)
    {
        //Target initialization:
        target = newTarget;              //Set potential target as chosen target
        prevTargetPos = target.position; //Initialize target position tracker

        //Effects:
        if (homingMat != null) GetComponentInChildren<Renderer>().material = homingMat; //Change color to indicate that it has successfully locked on to target
        audioSource.loop = true;                                                        //Make audiosource loop
        audioSource.clip = homingSound;                                                 //Set audiosource to play homing sound
        if (audioSource.clip != null) audioSource.Play();                               //Play homing sound on loop

        //Have other projectiles acquire target:
        if (dumbFired) return;
        if (!target.TryGetComponent(out PhotonView targetView)) targetView = target.GetComponentInParent<PhotonView>(); //Try to get photonView from target
        if (targetView != null) //Target has a photon view component
        {
            photonView.RPC("RPC_AcquireTarget", RpcTarget.Others, targetView.ViewID); //Use view ID to lock other projectiles onto this component
            if (printDebug) print("Target Acquired: " + targetView.name);             //Indicate that a target has been acquired
        }
        else //Targeted object is not on network (in this case it should ideally be stationary)
        {
            Collider[] checkColliders = Physics.OverlapSphere(target.position, settings.dumbTargetAquisitionRadius); //Get list of colliders currently overlapping target position (hopefully just target)
            foreach (Collider collider in checkColliders) //Iterate through identified colliders within target area
            {
                if (collider.transform == target) //Target can be acquired with this solution
                {
                    photonView.RPC("RPC_AcquireTargetDumb", RpcTarget.Others, target.position); //Send position of target as identifying acquisition data
                    if (printDebug) print("Dumb Target Acquired: " + target.name);              //Indicate that a target has been acquired
                    break;                                                                      //Exit collider iteration
                }
            }
        }
        photonView.RPC("RPC_Move", RpcTarget.Others, transform.position); //Sync up position and velocity between all versions of networked projectile
    }
    /// <summary>
    /// Makes projectile lose its current target.
    /// </summary>
    public void LoseTarget()
    {
        //Effects:
        if (homingMat != null) GetComponentInChildren<Renderer>().material = origMat; //Change color back to original setting
        audioSource.Stop();                                                           //Stop playing homing sound

        //Cleanup:
        target = null;                                                      //Clear active target
        if (!dumbFired) photonView.RPC("RPC_LostTarget", RpcTarget.Others); //Indicate to all other projectiles that target has been lost
    }
    private protected virtual void HitObject(RaycastHit hitInfo)
    {
        //Initialization:
        transform.position = hitInfo.point;                               //Move projectile to its hit position
        totalDistance += hitInfo.distance;                                //Add the final amount of distance projectile had to travel to get there
        photonView.RPC("RPC_Move", RpcTarget.Others, transform.position); //Move all networked projectiles to hit position

        //Look for strikeable scripts:
        NetworkPlayer targetPlayer = hitInfo.collider.GetComponentInParent<NetworkPlayer>();     //Try to get network player from hit collider
        if (targetPlayer == null) targetPlayer = hitInfo.collider.GetComponent<NetworkPlayer>(); //Try again for network player if it was not initially gotten
        if (targetPlayer != null) //Hit object was a player
        {
            if (targetPlayer.photonView.ViewID == originPlayerID) { print("Projectile tried to hit own player (despite it all)."); return; } //Do one last hail mary check for player self-collision

            //Hit using player:
            print("Projectile with origin ID " + originPlayerID + " hit player with ID " + targetPlayer.photonView.ViewID);
            targetPlayer.photonView.RPC("RPC_Hit", RpcTarget.All, settings.damage, originPlayerID);                                //Indicate to player that it has been hit
            if (!dumbFired && originPlayerID != 0) PhotonNetwork.GetPhotonView(originPlayerID).RPC("RPC_HitEnemy", RpcTarget.All); //Indicate to origin player that it has shot something
            if (settings.knockback > 0) //Projectile has player knockback
            {
                Vector3 knockback = transform.forward * settings.knockback;          //Get value by which to launch hit player
                targetPlayer.photonView.RPC("RPC_Launch", RpcTarget.All, knockback); //Add force to player rigidbody based on knockback amount
            }
        }
        else //Hit object is not a player
        {
            //Hit through targetable:
            Targetable targetObject = hitInfo.collider.GetComponent<Targetable>(); //Try to get targetable script from target
            if (targetObject != null) targetObject.IsHit(settings.damage);         //Indicate to targetable that it has been hit

            //Surface explosion:
            if (settings.explosionPrefab != null) //Only explode if projectile has an explosion prefab
            {
                ExplosionController explosion = Instantiate(settings.explosionPrefab, transform.position, transform.rotation).GetComponent<ExplosionController>(); //Instantiate the explosion prefab and get reference to its script
                explosion.originPlayerID = originPlayerID;                                                                                                         //Make sure explosion can't hit its own player
                photonView.RPC("RPC_Explode", RpcTarget.Others);                                                                                                   //Create explosions from networked projectiles
            }
        }

        //Cleanup:
        if (printDebug) print("Projectile hit " + hitInfo.collider.name); //Indicate what the projectile hit if debug logging is on
        Delete();                                                         //Destroy projectile
    }
    /// <summary>
    /// Called if projectile range is exhausted and projectile hasn't hit anything.
    /// </summary>
    private protected virtual void BurnOut()
    {
        //Mid-air explosion:
        if (!photonView.IsMine) return; //Make sure non-main projectiles cannot burn out
        if (settings.explosionPrefab != null) //Only explode if projectile has an explosion prefab
        {
            ExplosionController explosion = Instantiate(settings.explosionPrefab, transform.position, transform.rotation).GetComponent<ExplosionController>(); //Instantiate an explosion at burnout point
            explosion.originPlayerID = originPlayerID;                                                                                                         //Make sure explosion can't hit its own player
            photonView.RPC("RPC_Explode", RpcTarget.Others);                                                                                                   //Create explosions from networked projectiles
        }

        //Cleanup:
        Delete(); //Destroy projectile
    }

    //REMOTE METHODS:
    /// <summary>
    /// Moves projectile to target position (used to move remote projectiles).
    /// </summary>
    [PunRPC]
    public void RPC_Move(Vector3 newPosition)
    {
        transform.position = newPosition; //Move to new position
    }
    [PunRPC]
    public void RPC_Fire(Vector3 barrelPos, Quaternion barrelRot, int playerID)
    {
        Fire(barrelPos, barrelRot, playerID); //Initialize projectile (passing along playerID information)
    }
    /// <summary>
    /// Locks remote projectile onto target identified by its PhotonView ID.
    /// </summary>
    [PunRPC]
    public void RPC_AcquireTarget(int targetViewID)
    {
        PhotonView targetView = PhotonNetwork.GetPhotonView(targetViewID);                                                        //Get photonView from ID
        if (!targetView.TryGetComponent(out Targetable targetable)) targetable = targetView.GetComponentInChildren<Targetable>(); //Get targetable script from photonView
        if (targetable != null) //Target has been successfully acquired
        {
            target = targetable.targetPoint;                                                //Get target from script
            if (homingMat != null) GetComponentInChildren<Renderer>().material = homingMat; //Change color to indicate that it has successfully locked on to target
            audioSource.loop = true;                                                        //Make audiosource loop
            audioSource.clip = homingSound;                                                 //Set audiosource to play homing sound
            audioSource.Play();                                                             //Play homing sound on loop
            if (printDebug) print("Target Remotely Acquired: " + targetable.name);          //Indicate to local user that a remote projectile has successfully acquired a target
        }
    }
    /// <summary>
    /// Attempts to acquire stationary, non-networked target based on position.
    /// </summary>
    [PunRPC]
    public void RPC_AcquireTargetDumb(Vector3 targetPos)
    {
        Collider[] checkColliders = Physics.OverlapSphere(targetPos, settings.dumbTargetAquisitionRadius); //Get list of colliders currently overlapping target position (hopefully just target)
        foreach (Collider collider in checkColliders) //Iterate through colliders found by targeting solution
        {
            if (!collider.TryGetComponent(out Targetable targetable)) targetable = collider.GetComponentInParent<Targetable>(); //Try very hard to get targetable controller from collider
            if (targetable != null) //Target has been successfully acquired
            {
                target = targetable.targetPoint;                                                //Lock onto target
                if (homingMat != null) GetComponentInChildren<Renderer>().material = homingMat; //Change color to indicate that it has successfully locked on to target
                audioSource.loop = true;                                                        //Make audiosource loop
                audioSource.clip = homingSound;                                                 //Set audiosource to play homing sound
                audioSource.Play();                                                             //Play homing sound on loop
                if (printDebug) print("Dumb target acquisition successful!");                   //Indicate to local user that a remote projectile has successfully acquired a target
                break;                                                                          //Ignore everything else (risky)
            }
        }
    }
    /// <summary>
    /// Causes remote projectile to clear target field.
    /// </summary>
    [PunRPC]
    public void RPC_LostTarget()
    {
        target = null;                                         //Indicate that target has been lost
        GetComponentInChildren<Renderer>().material = origMat; //Set material back to original color
        audioSource.Stop();                                    //Stop playing homing sound
    }
    /// <summary>
    /// Generates projectile explosion on remote client but does not destroy projectile.
    /// </summary>
    [PunRPC]
    public void RPC_Explode()
    {
        ExplosionController explosion = Instantiate(settings.explosionPrefab, transform.position, transform.rotation).GetComponent<ExplosionController>(); //Instantiate an explosion at burnout point
        explosion.originPlayerID = originPlayerID;                                                                                                         //Make sure explosion can't hit its own player
    }

    //UTILITY METHODS:
    /// <summary>
    /// Returns true if given RaycastHit is a hit on this projectile's own player.
    /// </summary>
    /// <returns></returns>
    private protected bool HitsOwnPlayer(RaycastHit hit)
    {
        //Check network player:
        NetworkPlayer hitPlayer = hit.collider.GetComponentInParent<NetworkPlayer>();        //Try to get network player controller from collision
        if (hitPlayer != null && hitPlayer.photonView.ViewID == originPlayerID) return true; //Return true if raycast hits projectile's own player
        
        //Check demo player:
        PlayerController hitRealPlayer = hit.collider.GetComponentInParent<PlayerController>();         //Try to get real player controller from collision
        if (hitRealPlayer == null) hitRealPlayer = hit.collider.GetComponent<PlayerController>();       //Try again to get real player controller from collision
        if (hitRealPlayer != null && PlayerController.photonView.ViewID == originPlayerID) return true; //Return true if raycast hits projectile's own demo player

        return false; //No possible self-hit could be detected, return false
    }
    private void Delete()
    {
        if (!dumbFired && photonView.IsMine) PhotonNetwork.Destroy(photonView);               //Destroy networked projectiles on the network
        else if (dumbFired) { print("Destroying projectile locally."); Destroy(gameObject); } //Use normal destruction for non-networked projectiles
        //NOTE: Remote networked projectiles cannot delete themselves, they must be deleted from the network by their master version
    }
}