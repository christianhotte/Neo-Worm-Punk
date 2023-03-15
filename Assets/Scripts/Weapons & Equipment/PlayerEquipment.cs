using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR;
using CustomEnums;
using UnityEngine.InputSystem;
using RootMotion.FinalIK;

/// <summary>
/// Classes which inherit from this will be able to be attached to the player via a configurable joint.
/// </summary>
public class PlayerEquipment : MonoBehaviour
{
    //Classes, Enums & Structs:
    /// <summary>
    /// Describes a complex haptic event used by PlayerEquipment.
    /// </summary>
    [System.Serializable]
    public struct HapticData
    {
        [Min(0), Tooltip("Base intensity of haptic impulse (should be within range 0 - 1).")] public float amplitude;
        [Min(0), Tooltip("Total length (in seconds) of haptic impulse.")]                     public float duration;
        [Tooltip("Curve used to modulate magnitude throughout duration of impulse.")]         public AnimationCurve behaviorCurve;
    }

    //Objects & Components:
    internal PlayerController player;            //Player currently controlling this equipment
    private Transform basePlayerTransform;       //Master player object which all player equipment (and XR Origin) is under
    private protected Transform targetTransform; //Position and orientation for equipment joint to target (should be parent transform)
    private Rigidbody followerBody;              //Transform for object with mimics position and orientation of target equipment joint
    private protected Rigidbody playerBody;      //Rigidbody attached to player XROrigin
    private InputActionMap inputMap;             //Input map which this equipment will use
    private Transform handAnchorMover;           //If this equipment is on a player hand, this transform is used to move the rig when reacting to events such as recoil

    private protected Rigidbody rb;            //Rigidbody component attached to this script's gameobject
    private protected AudioSource audioSource; //Audio source component for playing sounds made by this equipment
    private protected ConfigurableJoint joint; //Physical joint connecting this weapon to the player

    //Settings:
    [Header("Settings:")]
    [SerializeField, Tooltip("Settings defining this equipment's physical joint behavior.")]                                         private protected EquipmentJointSettings jointSettings;
    [SerializeField, Tooltip("Only enable this on equipment which needs it, best practice is to have only one such piece per arm.")] private bool canMoveHandRig = false;
    [SerializeField, Tooltip("Enables constant joint updates for testing purposes.")]                                                private protected bool debugUpdateSettings;

    //Runtime Variables:
    /// <summary>
    /// Secondary modifier to follower offset position, used for animations such as recoil which involve moving IK hands (always aligned to hand orientation).
    /// </summary>
    private protected Vector3 currentAddOffset;
    /// <summary>
    /// Modifier to follower offset rotation, used for animations such as recoil which involve moving IK hands.
    /// </summary>
    private protected Vector3 currentAddRotOffset;
    /// <summary>
    /// Which hand this equipment is associated with (if any).
    /// </summary>
    internal Handedness handedness = Handedness.None;
    /// <summary>
    /// Equipment in stasis will do nothing and check nothing until it is re-equipped to a player.
    /// </summary>
    internal bool inStasis = false;
    /// <summary>
    /// Indicates that this weapon is currently stowed on the player's body and is not in use.
    /// </summary>
    internal bool holstered = false;

    private InputDeviceRole deviceRole = InputDeviceRole.Generic; //This equipment's equivalent device role (used to determine haptic feedback targets)
    private List<Vector3> relPosMem = new List<Vector3>();        //List of remembered relative positions (taken at FixedUpdate) used to calculate current relative velocity (newest entries are first)
    private Transform preferredHolster;                           //Holster which this equipment will go to when holstered
    //private bool holsterTransitioning = fal

    //Utility Variables:
    /// <summary>
    /// Current position of transform target relative to player body.
    /// </summary>
    internal Vector3 RelativePosition
    {
        get
        {
            if (playerBody == null) return Vector3.zero;                                      //Return nothing if equipment is not attached to a real player
            else return playerBody.transform.InverseTransformPoint(targetTransform.position); //Use inverse transform point to determine the position of the weapon relative to its player body
        }
    }
    /// <summary>
    /// Current velocity (in units per second) of transform target relative to player body.
    /// </summary>
    internal Vector3 RelativeVelocity
    {
        get
        {
            if (relPosMem.Count < 1) return Vector3.zero;                                                          //Return nothing if there is no positional memory to go off of
            else if (relPosMem.Count == 1) return (RelativePosition - relPosMem[0]) / Time.fixedDeltaTime;         //Return difference between single memory entry and current relative position if necessary
            else return (relPosMem[0] - relPosMem[relPosMem.Count - 1]) / (Time.fixedDeltaTime * relPosMem.Count); //Return difference between newest and oldest entries over time if more than two positions are in memory
        }
    }

    //EVENTS & COROUTINES:
    /// <summary>
    /// Plays a haptic sequence using an AnimationCurve to modify amplitude over time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HapticEvent(AnimationCurve hapticCurve, float maxAmplitude, float fullDuration)
    {
        //Validity checks:
        if (fullDuration <= 0) { Debug.LogWarning("Tried to play haptic event with duration " + fullDuration + " . This is too small."); yield break; } //Do not run if duration is zero

        //Get valid devices:
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>(); //Initialize list to store input devices
        #pragma warning disable CS0618                                                     //Disable obsolescence warning
        UnityEngine.XR.InputDevices.GetDevicesWithRole(deviceRole, devices);               //Find all input devices counted as right hand
        #pragma warning restore CS0618                                                     //Re-enable obsolescence warning
        for (int x = 0; x < devices.Count;) //Iterate through list of input devices (manually indexing iterator)
        {
            var device = devices[x];                                                                                                       //Get current device
            if (device.TryGetHapticCapabilities(out HapticCapabilities capabilities)) if (capabilities.supportsImpulse) { x++; continue; } //Skip devices which are good to go
            devices.RemoveAt(x);                                                                                                           //Remove incompatible devices from list
        }

        //Play haptic event over time:
        for (float timePassed = 0; timePassed <= fullDuration; timePassed += Time.fixedDeltaTime) //Iterate over time for full duration of haptic event
        {
            float currentInterpolant = timePassed / fullDuration;                                               //Get interpolant based on current progression of time
            float currentAmplitude = hapticCurve.Evaluate(currentInterpolant) * maxAmplitude;                   //Use given curve to get an amplitude value
            foreach (var device in devices) device.SendHapticImpulse(0, currentAmplitude, Time.fixedDeltaTime); //Send a brief haptic pulse at current amplitude (duration is only until next update)
            yield return new WaitForFixedUpdate();                                                              //Wait for next fixed update
        }
    }
    /// <summary>
    /// Moves preferred holster transform to position of actual holster or back to hand, then updates holster status if moving back to hand.
    /// </summary>
    /// <param name="holster">Pass true if holstering weapon, false if returning it to hand.</param>
    /// <returns></returns>
    private IEnumerator MoveHolster(bool holster = true)
    {
        //Initialize:
        Transform localSpaceParent = player.cam.transform.parent;                                               //Use camera offset as local space because hands are childed to it
        preferredHolster.parent = localSpaceParent;                                                             //Child holster to local space so we can use those local transforms
        Transform actualHolster = handedness == Handedness.Left ? player.leftHolster : player.rightHolster;     //Get acutal holster based on equipment handedness
        Vector3 relativeHolsterPos = localSpaceParent.InverseTransformPoint(actualHolster.position);            //Get position of holster relative to player body
        Quaternion relativeHolsterRot = Quaternion.Inverse(localSpaceParent.rotation) * actualHolster.rotation; //Get rotation of holster relative to player body
        Vector3 initialOriginPos = holster ? targetTransform.localPosition : relativeHolsterPos;                //Get initial position to lerp from (which will stay the same throughout process)
        Quaternion initialOriginRot = holster ? targetTransform.localRotation : relativeHolsterRot;             //Get initial rotation to lerp from (which will stay the same throughout process)

        //Move holster:
        for (float timePassed = 0; timePassed < jointSettings.holsterSpeed; timePassed += Time.fixedDeltaTime) //Iterate on update until holster time has passed
        {
            float timeInterpolant = timePassed / jointSettings.holsterSpeed;                                                                                                                                       //Get interpolant representing progression through holstering animation
            timeInterpolant = jointSettings.holsterCurve.Evaluate(timeInterpolant);                                                                                                                                //Evaluate time over curve to get a more complex animation
            preferredHolster.localPosition = Vector3.Lerp(initialOriginPos, holster ? localSpaceParent.InverseTransformPoint(actualHolster.position) : targetTransform.localPosition, timeInterpolant);            //Move weapon toward holster (or toward hand)
            preferredHolster.localRotation = Quaternion.Lerp(initialOriginRot, holster ? Quaternion.Inverse(localSpaceParent.rotation) * actualHolster.rotation : targetTransform.localRotation, timeInterpolant); //Rotate weapon toward holster (or toward hand)
            yield return new WaitForFixedUpdate(); //Wait for next fixed update step
        }

        //Cleanup:
        if (holster) //Equipment has just finished being holstered
        {
            preferredHolster.parent = actualHolster;              //Child transform to actual holster (so it doesn't move around)
            preferredHolster.localPosition = Vector3.zero;        //Clear any remaining movement holster has
            preferredHolster.localRotation = Quaternion.identity; //Clear any remaining rotation holster has
        }
        holstered = holster; //Update holstered status to false once done, so weapon can resume normal followbody control
    }

    //RUNTIME METHODS:
    private protected virtual void Awake()
    {
        //Flexible system setup:
        player = GetComponentInParent<PlayerController>(); //Try to get player controlling this equipment
        if (player != null) //Equipment is childed to a player (normal operation)
        {
            //Validity checks:
            PlayerInput playerInput = player.GetComponent<PlayerInput>(); if (playerInput == null) { Debug.LogError("PlayerEquipment " + name + " could not find a PlayerInput on Player"); Destroy(gameObject); } //Make sure equipment can find player input component
            XROrigin origin = GetComponentInParent<XROrigin>();                                                                                                                                                    //Try to get player XR origin
            if (origin == null) { Debug.LogError("PlayerEquipment " + name + " is not childed to an XR Origin and must be destroyed."); Destroy(gameObject); }                                                     //Call error message and abort if player could not be found
            if (!origin.TryGetComponent(out playerBody)) { Debug.LogError("PlayerEquipment " + name + " could not find player rigidbody and must be destroyed."); Destroy(gameObject); }                           //Call error message and abort if player rigidbody could not be found
            player.attachedEquipment.Add(this);                                                                                                                                                                    //Indicate that this equipment has now been attached to player

            //Initial component setup:
            basePlayerTransform = origin.transform.parent; //Get root player transform (above XR Origin)
            targetTransform = transform.parent;            //Use current parent as target transform
            transform.parent = basePlayerTransform;        //Reparent equipment to base player transform
            InitializeFollower(basePlayerTransform);       //Initialize rigidbody follower system

            //Check handedness & setup input:
            if (targetTransform.name.Contains("Left") || targetTransform.name.Contains("left")) //Equipment is being attached to the left hand/side
            {
                inputMap = playerInput.actions.FindActionMap("XRI LeftHand Interaction"); //Get left hand input map
                handedness = Handedness.Left;                                             //Indicate left-handedness
                deviceRole = InputDeviceRole.LeftHanded;                                  //Indicate that equipment uses left-handed devices
            }
            else if (targetTransform.name.Contains("Right") || targetTransform.name.Contains("right")) //Equipment is being attached to the right hand/side
            {
                inputMap = playerInput.actions.FindActionMap("XRI RightHand Interaction"); //Get right hand input map
                handedness = Handedness.Right;                                             //Indicate right-handedness
                deviceRole = InputDeviceRole.RightHanded;                                  //Indicate that equipment uses right-handed devices
            }
            else //Equipment is not being attached to an identifiable side
            {
                inputMap = playerInput.actions.FindActionMap("XRI Generic Interaction"); //Get generic input map
                handedness = Handedness.None;                                            //Indicate that equipment is not attached to a side
            }
            if (inputMap != null) inputMap.actionTriggered += TryGiveInput; //Otherwise, subscribe to input triggered event
            else Debug.LogWarning("PlayerEquipment " + name + " could not get its desired input map, make sure PlayerInput's actions are set up properly."); //Post warning if input get was unsuccessful
            
            //Set up hand retargeting:
            if (canMoveHandRig && handedness != Handedness.None) //Equipment is attached to a player hand and needs to be able to artificially move it
            {
                //Initialization:
                if (player.bodyRig == null) player.bodyRig = player.GetComponentInChildren<VRIK>();                                                      //Make sure player has a reference to its own body rig
                Transform realHandAnchor = handedness == Handedness.Left ? player.bodyRig.solver.leftArm.target : player.bodyRig.solver.rightArm.target; //Get hand IK target from body rig (use ternary to differentiate between hands)
                
                //Set up custom anchor parent:
                if (realHandAnchor.parent.name == "HandAnchorMover") //IK target has already been set up by a piece of playerEquipment on the same arm
                {
                    handAnchorMover = realHandAnchor.parent; //Use pre-existing mover as parent
                }
                else //No anchor mover has been previously been set up with this target
                {
                    handAnchorMover = new GameObject("HandAnchorMover").transform; //Create empty transform to move anchor (without modifying actual anchor or its parent)
                    handAnchorMover.SetParent(realHandAnchor.parent, false);       //Child anchor mover to same parent as true hand anchor
                    realHandAnchor.SetParent(handAnchorMover, true);               //Child true anchor target to mover
                }
            }
        }
        else //Equipment is not being controlled by a player (probably for demo purposes)
        {
            //Initial component setup:
            if (transform.parent.TryGetComponent(out DemoEquipmentMount mount)) { mount.equipment = this; } //Send reference to equipment mount if relevant
            targetTransform = transform.parent;                                                             //Make parent the effective target handle
            transform.parent = null;                                                                        //Unchild weapon from parent
            InitializeFollower(null);                                                                       //Initialize rigidbody follower system, unchilding system from any parent
        }

        //Universal component setup:
        if (!TryGetComponent(out audioSource)) audioSource = gameObject.AddComponent<AudioSource>(); //Make sure equipment has audio source

        //Setup rigidbody:
        if (!TryGetComponent(out rb)) rb = gameObject.AddComponent<Rigidbody>();                                      //Make sure system has a rigidbody
        else { Debug.Log("NOTE: The rigidbody on PlayerEquipment " + name + " may have been modified at runtime."); } //Post a note just in case the following messes with someone's rigidbody
        rb.drag = 0;                                                                                                  //Turn off linear drag
        rb.angularDrag = 0;                                                                                           //Turn off angular drag
        rb.useGravity = false;                                                                                        //Turn off rigidbody gravity
        rb.isKinematic = false;                                                                                       //Make sure rigidbody is not kinematic
        rb.interpolation = RigidbodyInterpolation.Interpolate;                                                        //Enable interpolation
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;                                         //Enable continuous dynamic collisions
        rb.maxAngularVelocity = jointSettings.maxAngularSpeed;                                                        //Set base max rotation value

        //Check for settings:
        if (jointSettings == null) //No joint settings were provided
        {
            Debug.Log("PlayerEquipment " + name + " is missing jointSettings, using system defaults.");              //Log warning in case someone forgot
            jointSettings = (EquipmentJointSettings)Resources.Load("DefaultSettings/DefaultEquipmentJointSettings"); //Load default settings from Resources folder
        }

        //Setup configurable joint:
        joint = gameObject.AddComponent<ConfigurableJoint>();   //Instantiate a configurable joint on this equipment gameobject
        joint.connectedBody = followerBody;                     //Connect joint to follower transform
        joint.xMotion = ConfigurableJointMotion.Limited;        //Enable X axis linear motion limits
        joint.yMotion = ConfigurableJointMotion.Limited;        //Enable Y axis linear motion limits
        joint.zMotion = ConfigurableJointMotion.Limited;        //Enable Z axis linear motion limits
        joint.angularXMotion = ConfigurableJointMotion.Limited; //Enable X axis angular motion limits
        joint.angularYMotion = ConfigurableJointMotion.Limited; //Enable Y axis angular motion limits
        joint.angularZMotion = ConfigurableJointMotion.Limited; //Enable Z axis angular motion limits
        ConfigureJoint();                                       //Perform the remainder of joint configuration in a separate function

        //Ignore player collisions:
        if (playerBody != null) //System is attached to a player with a rigidbody
        {
            foreach (Collider collider in GetComponentsInChildren<Collider>()) //Iterate through each collider in this equipment
            {
                Physics.IgnoreCollision(playerBody.GetComponent<Collider>(), collider, true); //Make physics ignore collisions between equipment colliders and player
            }
        }
    }
    private protected virtual void Start()
    {
        //Set up holster:
        preferredHolster = new GameObject(name + "_HolsterMover").transform; //Instantiate an empty transform to serve as preferred holster
        preferredHolster.parent = playerBody.transform;                      //Child holster to player body
    }
    private protected virtual void Update()
    {
        if (debugUpdateSettings && Application.isEditor) ConfigureJoint(); //Reconfigure joint every update if debug setting is selected (only necessary in Unity Editor)
    }
    private protected virtual void FixedUpdate()
    {
        //Update position memory:
        relPosMem.Insert(0, RelativePosition);                                                       //Add current relative position to beginning of memory list
        if (relPosMem.Count > jointSettings.positionMemory) relPosMem.RemoveAt(relPosMem.Count - 1); //Keep list size constrained to designated amount (removing oldest entries)

        //Cleanup:
        PerformFollowerUpdate(); //Update follower transform
    }
    private protected virtual void OnPreRender()
    {
        PerformFollowerUpdate(); //Update follower transform
    }
    private protected virtual void OnDestroy()
    {
        //Unsubscribe from events:
        if (inputMap != null) inputMap.actionTriggered -= TryGiveInput; //Unsubscribe from input event
    }
    private void TryGiveInput(InputAction.CallbackContext context)
    {
        //Input exception states:
        if (!player.InCombat()) return; //Ignore equipment input while not in combat
        if (holstered) return;          //Ignore input while equipment is holstered

        InputActionTriggered(context); //Pass along input
    }
    private protected virtual void InputActionTriggered(InputAction.CallbackContext context) { }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Equips equipment onto target transform (should be under a player's XR Origin).
    /// </summary>
    /// <param name="target">Needs to be under a specific player's XR Origin, named "Left..." or "Right..." if it's on a specific side of the player.</param>
    public void Equip(Transform target)
    {

    }
    /// <summary>
    /// Detaches equipment from player and puts it into stasis.
    /// </summary>
    public void UnEquip()
    {
        //Cleanup:
        if (player != null) player.attachedEquipment.Remove(this); //Remove this item from player's running list of attached equipment
        inStasis = true;                                           //Indicate that equipment is now safely in stasis and will not messily try to update itself
    }
    /// <summary>
    /// Holsters or un-holsters weapon. Disables inputs to this piece of equipment, and stows it in appropriate transform on player.
    /// </summary>
    /// <param name="holster">Pass true to hoster this equipment, false to un-holster it.</param>
    public void Holster(bool holster = true)
    {
        if (holster) //Weapon is being holstered
        {
            holstered = true;//Immediately indicate holster status if holstering weapon
        }
        else //Weapon is being unholstered
        {
            if (TryGetComponent(out NewShotgunController gun)) gun.reverseFireStage = 0; //Make sure weapon does not become unholstered in reverse fire mode
        }
        StartCoroutine(MoveHolster(holster)); //Move holster to designated position over time
    }

    //UTILITY METHODS:
    /// <summary>
    /// Initializes rigidbody follower system as child of target parent.
    /// </summary>
    private void InitializeFollower(Transform followerParent)
    {
        Transform followerTransform = new GameObject(name + "Follower").transform; //Instantiate empty gameobject as follower
        followerTransform.parent = followerParent;                                 //Child follower to target parent
        followerTransform.position = targetTransform.position;                     //Set followBody position to exact position of target
        followerTransform.rotation = targetTransform.rotation;                     //Set followBody orientation to exact orientation of target
        followerBody = followerTransform.gameObject.AddComponent<Rigidbody>();     //Give follower a rigidbody component (and save a reference to it)
        followerBody.isKinematic = true;                                           //Make follower rigidbody kinematic
        followerBody.useGravity = false;                                           //Ensure follower body is not affected by gravity
    }
    /// <summary>
    /// Applies current joint settings to local ConfigurableJoint.
    /// </summary>
    private void ConfigureJoint()
    {
        //Validity checks:
        if (joint == null) { Debug.LogWarning("PlayerEquipment " + name + " tried to update joint before joint was instantiated."); return; } //Log warning and abort if there is no joint to update
        if (jointSettings == null) { Debug.LogWarning("PlayerEquipment " + name + " tried to update joint without jointSettings."); return; } //Log warning and abort if there are no joint settings to reference

        //Apply limit springs:
        SoftJointLimitSpring spring = new SoftJointLimitSpring(); //Initialize variable for setting spring values
        spring.spring = jointSettings.angularSpringiness;         //Set angular X limit springiness
        spring.damper = jointSettings.angularDampening;           //Set angular X limit dampening
        joint.angularXLimitSpring = spring;                       //Apply changes to angular X spring
        //spring.damper = 0; //Try uncommenting this if joint feels weird
        joint.angularYZLimitSpring = spring;                      //Apply changes to anglular YZ spring

        //Apply limits:
        SoftJointLimit limit = new SoftJointLimit();       //Initialize variable for setting limit values
        limit.limit = (jointSettings.limitAngle / 2) * -1; //Set lower angular X limit (to half of full setting because it is split between two separate limits)
        limit.bounciness = jointSettings.limitBounciness;  //Set lower angular X limit bounciness
        joint.lowAngularXLimit = limit;                    //Apply changes to low angular X limit
        limit.limit *= -1;                                 //Set upper angular X limit (positive half of lower limit)
        //limit.bounciness = 0; //Try uncommenting this if joint feels weird
        joint.highAngularXLimit = limit;                   //Apply changes to high angular X limit
        limit.limit *= 2;                                  //Set angular Y and Z limits (double to get back to full setting)
        //limit.bounciness = jointSettings.limitBounciness; //Uncomment this if you uncommented the previous commented line
        joint.angularYLimit = limit;                       //Apply changes to angular Y limit
        joint.angularZLimit = limit;                       //Apply changes to angular Z limit

        //Apply drives:
        JointDrive drive = new JointDrive();                     //Initialize variable for setting drive values
        drive.positionSpring = jointSettings.linearDrive;        //Set linear drive spring force
        drive.maximumForce = joint.xDrive.maximumForce;          //Get maximum drive from configurableJoint default setting
        joint.xDrive = drive;                                    //Apply setting to linear X drive
        joint.yDrive = drive;                                    //Apply setting to linear Y drive
        joint.zDrive = drive;                                    //Apply setting to linear Z drive
        drive.positionSpring = jointSettings.angularDrive;       //Set angular drive spring force
        drive.positionDamper = jointSettings.angularDriveDamper; //Set angular drive dampening effect
        joint.angularXDrive = drive;                             //Apply setting to angular X drive
        joint.angularYZDrive = drive;                            //Apply setting to angular YZ drive
    }
    /// <summary>
    /// Updates position of rigidbody follower to match position of target.
    /// </summary>
    private void PerformFollowerUpdate()
    {
        //Calculate follower position:
        Vector3 targetPos = holstered ? preferredHolster.position : targetTransform.position;                                                     //Get base target position for rigidbody follower (either hand or holster)
        if (jointSettings.velocityCompensation > 0 && playerBody != null) targetPos += playerBody.velocity * jointSettings.velocityCompensation; //Apply target velocity compensation to account for rigidbody lag
        if (jointSettings.offset != Vector3.zero) targetPos += transform.rotation * jointSettings.offset;                                        //Apply constant offset to keep equipment in desired position relative to player
        if (currentAddOffset != Vector3.zero) targetPos += transform.rotation * currentAddOffset;                                                //Apply secondary offset to target position, used by some equipment animations such as shotgun recoil

        //Calculate follower rotation:
        Quaternion targetRot = holstered ? preferredHolster.rotation : targetTransform.rotation; //Get base target rotation for rigidbody follower
        if (currentAddRotOffset != Vector3.zero) //Weapon rotation is currently being affected by an offset
        {
            targetRot *= Quaternion.AngleAxis(currentAddRotOffset.x, Vector3.right);
        }

        //Apply follower transforms:
        followerBody.MovePosition(targetPos); //Apply target position through follower rigidbody
        followerBody.MoveRotation(targetRot); //Apply target rotation through follower rigidbody
        if (handAnchorMover != null && canMoveHandRig) //Player hand re-targeting is enabled
        {
            handAnchorMover.localPosition = currentAddOffset;                                                               //Artificially add movement to player hand
            if (currentAddRotOffset.magnitude <= 90) handAnchorMover.localRotation = Quaternion.Euler(currentAddRotOffset); //Artificially add rotation to player hand (if it won't break wrist rig)
            else handAnchorMover.localRotation = Quaternion.identity;                                                       //Use normal rotation otherwise
        }
    }
    /// <summary>
    /// Sends a haptic impulse to this equipment's associated controller.
    /// </summary>
    /// <param name="amplitude">Strength of vibration (between 0 and 1).</param>
    /// <param name="duration">Duration of vibration (in seconds).</param>
    public void SendHapticImpulse(float amplitude, float duration)
    {
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>(); //Initialize list to store input devices
        #pragma warning disable CS0618                                                     //Disable obsolescence warning
        UnityEngine.XR.InputDevices.GetDevicesWithRole(deviceRole, devices);               //Find all input devices counted as right hand
        #pragma warning restore CS0618                                                     //Re-enable obsolescence warning
        foreach (var device in devices) //Iterate through list of devices identified as right hand
        {
            if (device.TryGetHapticCapabilities(out HapticCapabilities capabilities)) //Device has haptic capabilities
            {
                if (capabilities.supportsImpulse) device.SendHapticImpulse(0, amplitude, duration); //Send impulse if supported by device
            }
        }
    }
    public void SendHapticImpulse(Vector2 properties) { SendHapticImpulse(properties.x, properties.y); }
    public void SendHapticImpulse(HapticData properties)
    {
        if (properties.duration == 0 || properties.amplitude == 0) return;                                           //Do nothing if player has given a null haptic setting
        if (properties.behaviorCurve.keys.Length <= 1) SendHapticImpulse(properties.amplitude, properties.duration); //Use simpler impulse method if no curve is given
        else StartCoroutine(HapticEvent(properties.behaviorCurve, properties.amplitude, properties.duration));       //Use coroutine to deploy more complex haptic impulses
    }
    /// <summary>
    /// Plays one-shot of given sound, taking into account current volume settings (specific to SFX) (also checks if sound is null so you don't have to).
    /// </summary>
    public void PlaySFX(AudioClip sound) { if (sound != null) audioSource.PlayOneShot(sound, PlayerPrefs.GetFloat("MasterVolume") * PlayerPrefs.GetFloat("SFXVolume")); }
}