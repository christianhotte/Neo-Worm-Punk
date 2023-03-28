using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using RootMotion.FinalIK;
using UnityEngine.Events;
using UnityEngine.Rendering;

/// <summary>
/// Manages overall player stats and abilities.
/// </summary>
public class PlayerController : MonoBehaviour
{
    //Objects & Components:
    [Tooltip("Singleton instance of player controller.")]                                    public static PlayerController instance;
    [Tooltip("Singleton instance of this client's photonNetwork (on their NetworkPlayer).")] public static PhotonView photonView;

    [Tooltip("XROrigin component attached to player instance in scene.")]  internal XROrigin xrOrigin;
    [Tooltip("Rigidbody for player's body (the part that flies around).")] internal Rigidbody bodyRb;
    [Tooltip("Settings application script for this player.")]              internal PlayerSetup playerSetup;
    [Tooltip("VR rig for player body.")]                                   internal VRIK bodyRig;
    [Tooltip("Controller component for player's left hand.")]              internal ActionBasedController leftHand;
    [Tooltip("Controller component for player's right hand.")]             internal ActionBasedController rightHand;
    [Tooltip("Equipment which is currently attached to the player")]       internal List<PlayerEquipment> attachedEquipment = new List<PlayerEquipment>();
    [Tooltip("Combat HUD Canvas.")]                                        internal CombatHUDController combatHUD;    
    [SerializeField, Tooltip("Combat Screen GameObject.")]                 internal GameObject combatHUDScreen;

    internal Camera cam;                       //Primary camera for VR rendering, located on player head
    internal PlayerInput input;                //Input manager component used by player to send messages to hands and such
    internal SkinnedMeshRenderer bodyRenderer; //Mesh renderer for player's physical worm body
    internal PlayerBodyManager bodyManager;    //Reference to script on player that manages body collisions and special effects
    private AudioSource audioSource;           //Main player audio source
    private Transform camOffset;               //Object used to offset camera position in case of weirdness
    private InputActionMap inputMap;           //Input map which player uses
    private ScreenShakeVR screenShaker;        //Component used to safely shake player's screen without causing nausea
    private Volume healthVolume;               //Post-processing volume used to visualize player health
    private Transform playerModel;             //Top component in player body hierarchy, contains VRIK component

    //Settings:
    [Header("Components:")]
    [Tooltip("Transform which left-handed primary weapon snaps to when holstered.")]  public Transform leftHolster;
    [Tooltip("Transform which right-handed primary weapon snaps to when holstered.")] public Transform rightHolster;
    [Header("Settings:")]
    [SerializeField, Tooltip("Settings determining player health properties.")] private HealthSettings healthSettings;
    [Space()]
    [SerializeField, Tooltip("How far player head can get from body before it is sent back to center.")]                private float maxHeadDistance;
    [SerializeField, Tooltip("Amount by which to move torso down (allows player to collapse more naturally).")]         private float torsoVerticalOffset = 10f;
    [SerializeField, Tooltip("Makes sure that player torso is always below player head.")]                              private bool keepTorsoCentered = true;
    [SerializeField, Range(0, 1), Tooltip("Amount by which player has to pull the thumb stick in order to snap-turn.")] private float flickStickThreshold;
    [Space()]
    [Tooltip("How quickly player slides down horizontal inclines.")]                                                            public float slipSpeed = 5;
    [MinMaxSlider(0, 90), Tooltip("Minimum and maximum angles for determining whether or not player will slide on a surface.")] public Vector2 slipAngleRange;
    [Header("Sound Settings:")]
    [SerializeField, Tooltip("SFX played when player strikes a target.")] private AudioClip targetHitSound;
    [SerializeField, Tooltip("SFX played when player kills a target.")]   private AudioClip targetKillSound;
    [Header("Debug Options:")]
    [SerializeField, Tooltip("Enables usage of SpawnManager system to automatically position player upon instantiation.")] private bool useSpawnPoint = true;
    [SerializeField, Tooltip("Click to snap camera back to center of player rigidbody (ignoring height).")]                private bool debugCenterCamera;
    [SerializeField, Tooltip("Manually isntantiate a network player.")]                                                    private bool debugSpawnNetworkPlayer;
    [SerializeField, Tooltip("Manually destroy client network player.")]                                                   private bool debugDeSpawnNetworkPlayer;
    [SerializeField, Tooltip("Deals one damage to this player.")]                                                          private bool debugHarm;

    //Runtime Variables:
    internal float currentHealth; //How much health player currently has
    private bool inCombat;        //Whether the player is actively in combat
    private bool inMenu;          //Whether the player is actively in a menu scene
    private float timeUntilRegen; //Time (in seconds) until health regeneration can begin
    private bool centeredInScene; //Made false whenever player loads into a scene, triggers camera centering in the first update
    internal bool isDead;         //True while player is dead and is kinda in limbo
    private float baseDrag;
    private Vector2 prevRightStick; //Previous position of the right stick

    //Misc:
    internal bool Launchin = false; //NOTE: What references this and where is it modified?

    //Utility Variables:
    /// <summary>
    /// What percentage of maximum player health they currently have.
    /// </summary>
    public float HealthPercent { get { return currentHealth / (float)healthSettings.defaultHealth; } }

    //Events & Coroutines:
    /// <summary>
    /// Controls functions which occur while player is dying.
    /// </summary>
    /// <returns></returns>
    public IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(healthSettings.deathTime); //Wait for designated number of seconds in death zone

        if (SpawnManager.current != null && useSpawnPoint) //Spawn manager is present in scene
        {
            Transform spawnpoint = SpawnManager.current.GetRandomSpawnPoint();                    //Get spawnpoint from spawnpoint manager
            xrOrigin.transform.position = spawnpoint.position;                                    //Move spawned player to target position
            xrOrigin.transform.eulerAngles = Vector3.Project(spawnpoint.eulerAngles, Vector3.up); //Rotate player to designated spawnpoint rotation
        }
        foreach (PlayerEquipment equipment in attachedEquipment) equipment.inputEnabled = true; //Re-enable equipment input
        bodyRb.isKinematic = false; //Re-enable player physics

        photonView.RPC("RPC_MakeVisible", RpcTarget.Others); //Unhide trailrenderers for all other players
        isDead = false;                                      //Indicate that player is no longer dead
        CenterCamera();                                      //Center camera (this is worth doing during any major transition)
    }

    //RUNTIME METHODS:
    private void Awake()
    {
        //Check validity / get objects & components:
        if (instance != null) { print("Replacing player " + instance.gameObject.name + " with player " + gameObject.name + " from previous scene"); } instance = this; //Use newest instance of PlayerController script as authoritative version, and indicate when an old playerController script is being replaced

        if (!TryGetComponent(out input)) { Debug.LogError("PlayerController could not find PlayerInput component!"); Destroy(gameObject); }                                    //Make sure player input component is present on same object
        xrOrigin = GetComponentInChildren<XROrigin>(); if (xrOrigin == null) { Debug.LogError("PlayerController could not find XROrigin in children."); Destroy(gameObject); } //Make sure XROrigin is present inside player
        bodyRb = xrOrigin.GetComponent<Rigidbody>(); if (bodyRb == null) { Debug.LogError("PlayerController could not find Rigidbody on XR Origin."); Destroy(gameObject); }   //Make sure player has a rigidbody on origin
        playerSetup = GetComponent<PlayerSetup>(); if (playerSetup == null) playerSetup = gameObject.AddComponent<PlayerSetup>();                                              //Make sure player has a settings configuration component
        cam = GetComponentInChildren<Camera>(); if (cam == null) { Debug.LogError("PlayerController could not find camera in children."); Destroy(gameObject); }               //Make sure system has camera
        audioSource = cam.GetComponent<AudioSource>(); if (audioSource == null) audioSource = cam.gameObject.AddComponent<AudioSource>();                                      //Make sure system has an audio source
        bodyRig = GetComponentInChildren<VRIK>(); if (bodyRig == null) { Debug.LogWarning("PlayerController could not find VRIK rig in children."); }                          //Make sure system has access to VR rig component
        bodyRenderer = GetComponentInChildren<SkinnedMeshRenderer>();                                                                                                          //Get renderer component for player's physical body
        camOffset = cam.transform.parent;                                                                                                                                      //Get camera offset object
        inputMap = GetComponent<PlayerInput>().actions.FindActionMap("XRI Generic Interaction");                                                                               //Get generic input map from PlayerInput component
        combatHUD = GetComponentInChildren<CombatHUDController>();                                                                                                             //Get the combat HUD canvas
        screenShaker = cam.GetComponent<ScreenShakeVR>();                                                                                                                      //Get screenshaker script from camera object
        playerModel = GetComponentInChildren<VRIK>().transform;                                                                                                                //Get player model component
        bodyManager = GetComponentInChildren<PlayerBodyManager>(); if (bodyManager == null) bodyRb.gameObject.AddComponent<PlayerBodyManager>();                               //Make sure player has a body manager component
        foreach (Volume volume in GetComponentsInChildren<Volume>()) //Iterate through Volume components in children
        {
            if (volume.name.Contains("Health")) healthVolume = volume; //Get health volume
        }

        //Get hands:
        ActionBasedController[] hands = GetComponentsInChildren<ActionBasedController>();                                    //Get both hands in player object
        if (hands[0].name.Contains("Left") || hands[0].name.Contains("left")) { leftHand = hands[0]; rightHand = hands[1]; } //First found component is on left hand
        else { rightHand = hands[0]; leftHand = hands[1]; }                                                                  //Second found component is on right hand
        if (leftHolster == null) leftHolster = leftHand.transform;                                                           //Use hand as holster if none is provided
        if (rightHolster == null) rightHolster = rightHand.transform;                                                        //Use hand as holster if none is provided

        //Check settings:
        if (healthSettings == null) //No health settings were provided
        {
            Debug.Log("PlayerController is missing HealthSettings, using system defaults.");          //Log warning in case someone forgot
            healthSettings = (HealthSettings)Resources.Load("DefaultSettings/DefaultHealthSettings"); //Load default settings from Resources folder
        }

        //Setup runtime variables:
        currentHealth = healthSettings.defaultHealth; //Set base health value
        baseDrag = bodyRb.drag;                       //Store base drag value

        inCombat = true;
        UpdateWeaponry();

        //Event subscription:
        inputMap.actionTriggered += OnInputTriggered; //Subscribe to generic input event
        SceneManager.sceneLoaded += OnSceneLoaded;    //Subscribe to scene loaded event
    }
    private void Start()
    {
        //Late setup:
        playerSetup.ApplyAllSettings(); //Make sure settings are all updated on this player instance

        //Move to spawnpoint:
        if (SpawnManager.current != null && useSpawnPoint) //Spawn manager is present in scene
        {
            Transform spawnpoint = SpawnManager.current.GetRandomSpawnPoint(); //Get spawnpoint from spawnpoint manager
            xrOrigin.transform.position = spawnpoint.position;           //Move spawned player to target position
        }

        //Hide equipment in menus:
        if (GameManager.Instance.InMenu())
        {
            inMenu = true;
            UpdateWeaponry();
        }
        else
        {
            inMenu = false;
        }
            
    }
    private void Update()
    {
        //Make sure player is centered:
        if (!centeredInScene) //Player has not been centered in this scene yet
        {
            centeredInScene = true; //Indicate that player has been centered
            CenterCamera();         //Make sure player camera is in dead center of rigidbody
        }
        if (keepTorsoCentered) playerModel.transform.position = cam.transform.position + (Vector3.down * torsoVerticalOffset); //Center model to player body position and apply vertical offset

        //Debug functions:
        if (Application.isEditor) //Debug settings updates are enabled (only necessary while running in Unity Editor)
        {
            if (debugSpawnNetworkPlayer)
            {
                debugSpawnNetworkPlayer = false;
                NetworkPlayerSpawn.instance.SpawnNetworkPlayer();
            }
            if (debugDeSpawnNetworkPlayer)
            {
                debugDeSpawnNetworkPlayer = false;
                NetworkPlayerSpawn.instance.DeSpawnNetworkPlayer();
            }
            if (debugCenterCamera) //Camera is being manually centered
            {
                debugCenterCamera = false; //Immediately unpress button
                CenterCamera();            //Center camera to rigidbody
            }
            if (debugHarm)
            {
                debugHarm = false;
                IsHit(1);
            }
        }

        //Update health:
        if (healthSettings.regenSpeed > 0) //Only do health regeneration if setting is on
        {
            if (timeUntilRegen > 0) { timeUntilRegen = Mathf.Max(timeUntilRegen - Time.deltaTime, 0); } //Update health regen countdown whenever relevant
            else if (currentHealth < healthSettings.defaultHealth) //Regen wait time is zero and player has lost health
            {
                currentHealth = Mathf.Min(currentHealth + (healthSettings.regenSpeed * Time.deltaTime), healthSettings.defaultHealth); //Regenerate until player is back to default health
                healthVolume.weight = 1 - HealthPercent;                                                                               //Update health visualization
            }
        }
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CenterCamera(); //Center player camera whenever a new scene is loaded
    }
    private void OnDestroy()
    {
        //Cleanup:
        instance = null; //Clear reference on player destruction

        //Event unsubscription:
        inputMap.actionTriggered -= OnInputTriggered; //Unsubscribe from generic input event
        SceneManager.sceneLoaded -= OnSceneLoaded;    //Unsubscribe from scene loaded event
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Applies current PlayerSettings to this player and synchronizes them across the network.
    /// </summary>
    public void ApplyAndSyncSettings()
    {
        playerSetup.ApplyAllSettings();                                              //Apply all settings to this player
        if (photonView != null) photonView.GetComponent<NetworkPlayer>().SyncData(); //Sync settings data across the network (if able)
    }
    /// <summary>
    /// Uses camera offset to snap camera back to center of rigidbody.
    /// </summary>
    private void CenterCamera()
    {
        camOffset.localPosition = Vector3.zero;                                               //Zero out offset position
        Vector3 offsetAmt = xrOrigin.transform.InverseTransformPoint(cam.transform.position); //Get true amount by which camera is offset from rigidbody center
        offsetAmt.y = 0;                                                                      //Ignore discrepancy along Y axis (so operation doesn't try to flatten the player)
        camOffset.localPosition = -offsetAmt;                                                 //Use camera offset to center player to rigidbody

        print("Centering player camera."); //Indicate that camera is being centered
    }
    /// <summary>
    /// Updates the weaponry so that the player can / can't fight under certain conditions.
    /// </summary>
    public void UpdateWeaponry()
    {
        if (inMenu)
        {
            inCombat = false;
        }

        bodyRb.isKinematic = !inCombat;

        foreach (var weapon in attachedEquipment)
            foreach (var renderer in weapon.GetComponentsInChildren<Renderer>())
                renderer.enabled = inCombat;

        foreach (NewGrapplerController grappler in GetComponentsInChildren<NewGrapplerController>())
        {
            if (grappler.hook != null)
            {
                foreach (Renderer renderer in grappler.hook.GetComponentsInChildren<Renderer>())
                    renderer.enabled = inCombat;
            }
        }

        combatHUDScreen.GetComponent<MeshRenderer>().enabled = inCombat;
    }

    //INPUT METHODS:
    public void OnInputTriggered(InputAction.CallbackContext context)
    {
        switch (context.action.name) //Determine behavior based on input action
        {
            case "RightStickPress": if (context.started) { CenterCamera(); } break; //Center camera when player presses the right stick
            case "RightStick":
                Vector2 stickValue = context.ReadValue<Vector2>(); //Get input value from thumbstick
                if (stickValue.magnitude > flickStickThreshold && prevRightStick.magnitude < flickStickThreshold) //Flick stick action has just begun
                {
                    Vector3 stickDir = new Vector3(stickValue.x, 0, stickValue.y);                   //Convert stick value to a flat vector3
                    Quaternion playerRotator = Quaternion.FromToRotation(Vector3.forward, stickDir); //Get a rotation that points the player in the direction of the stick
                    bodyRb.transform.rotation = playerRotator * bodyRb.transform.rotation;           //Rotate the player
                    Vector3 newEulers = bodyRb.transform.eulerAngles;
                    newEulers.z = 0; newEulers.x = 0;
                    bodyRb.transform.eulerAngles = newEulers;
                }
                prevRightStick = stickValue; //Store stick value for later
                break;
        }
    }
    /// <summary>
    /// Called when player hits an enemy with a projectile.
    /// </summary>
    public void HitEnemy()
    {
        if (targetHitSound != null) audioSource.PlayOneShot(targetHitSound); //Play hit sound when player shoots (or damages) a target
    }
    /// <summary>
    /// Called when player hits and kills an enemy with a projectile.
    /// </summary>
    public void KilledEnemy()
    {
        if (targetKillSound != null) audioSource.PlayOneShot(targetKillSound); //Play kill sound when player kills a target
    }
    /// <summary>
    /// Method called when this player is hit by a projectile.
    /// </summary>
    public bool IsHit(int damage)
    {
        //Hit effects:
        currentHealth -= Mathf.Max((float)damage, 0);                           //Deal projectile damage, floor at 0
        healthVolume.weight = 1 - HealthPercent;                                //Update health visualization
        print(damage + " damage dealt to player with ID " + photonView.ViewID); //Indicate that damage has been dealt

        //Death check:
        if (currentHealth <= 0) //Player is being killed by this projectile hit
        {
            IsKilled(); //Indicate that player has been killed
            return true;
        }
        else //Player is being hurt by this projectile hit
        {
            audioSource.PlayOneShot(healthSettings.hurtSound != null ? healthSettings.hurtSound : (AudioClip)Resources.Load("Sounds/Default_Hurt_Sound")); //Play hurt sound
            if (healthSettings.regenSpeed > 0) timeUntilRegen = healthSettings.regenPauseTime;                                                             //Optionally begin regeneration sequence
            return false;
        }
    }
    /// <summary>
    /// Method called when something kills this player.
    /// </summary>
    public void IsKilled()
    {
        //Validity checks:
        if (isDead) return; //Do not allow dead players to be killed

        //Effects:
        audioSource.PlayOneShot(healthSettings.deathSound != null ? healthSettings.deathSound : (AudioClip)Resources.Load("Sounds/Temp_Death_Sound")); //Play death sound
        
        //Weapon cleanup:
        foreach (NewGrapplerController hookShot in GetComponentsInChildren<NewGrapplerController>()) //Iterate through any hookshots player may have equipped
        {
            if (hookShot.hook != null) hookShot.hook.Stow(); //Stow hook to make sure it doesn't get lost
        }

        //Put player in limbo:
        photonView.RPC("RPC_MakeInvisible", RpcTarget.Others);                           //Hide trailrenderers for all other players
        bodyRb.velocity = Vector3.zero;                                                  //Reset player velocity
        CenterCamera();                                                                  //Center camera (this is worth doing during any major transition)
        foreach (PlayerEquipment equipment in attachedEquipment) equipment.Shutdown(-1); //Stow and disable all equipment on player
        bodyRb.isKinematic = true;                                                       //Disable body physics

        //Cleanup:
        isDead = true; //Indicate that this player is dead
        xrOrigin.transform.position = SpawnManager.current.deathZone.position; //Move player to death zone
        xrOrigin.transform.rotation = Quaternion.identity;                     //Zero out player rotation
        StartCoroutine(DeathSequence());                                       //Begin death sequence
        currentHealth = healthSettings.defaultHealth;                          //Reset to max health
        healthVolume.weight = 0;                                               //Reset health volume weight
        timeUntilRegen = 0;                                                    //Reset regen timer
        print("Local player has been killed!");
    }
    private void MakeNotWiggly()
    {
        //foreach (Rigidbody rigidbody in playerModel.GetComponentsInChildren<Rigidbody>()).
    }
    /// <summary>
    /// Safely shakes the player's eyeballs.
    /// </summary>
    public void ShakeScreen(float intensity, float time) { screenShaker.Shake(intensity, time); }
    /// <summary>
    /// Safely shakes the player's eyeballs, but this time with a convenient vector.
    /// </summary>
    public void ShakeScreen(Vector2 shakeSettings) { screenShaker.Shake(shakeSettings.x, shakeSettings.y); }

    //UTILITY METHODS:
    public bool InCombat() => inCombat;
    public bool InMenu() => inMenu;
    public void SetCombat(bool combat)
    {
        inCombat = combat;
        UpdateWeaponry();
    }
    public void SetInMenu(bool menu) => inMenu = menu;
    /// <summary>
    /// Generic method to make sure UnityActions always have something subscribed to them.
    /// </summary>
    private void SubscriptionDummy() { }
    public void ResetDrag() { if (bodyRb != null) bodyRb.drag = baseDrag; }
}
