using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

public class Grinder : MonoBehaviour
{
    public const byte PlayGrinderEffectEventCode = 101;
    public enum GrinderEffect { Death }

    public static Grinder instance;
    private PlayerController hitPlayer;
    public GameObject leftDoorStart, rightDoorStart, leftDoorEnd, rightDoorEnd;
    private NetworkPlayer netPlayer;
    public bool Activated = false, Closed = true, Enabled = true;
    public float doorSpeed = 3;
    public float LevelTimePercent;
    internal AudioSource GrinderAud;
    public AudioClip GrindnerSound;
    //private Jumbotron jumbotronObject;
    private RoundManager roundManager;
    [Header("Effects:")]
    public ParticleSystem deathEffect;
    public AudioClip deathSound;

    // Start is called before the first frame update
    void Awake()
    {
        //jumbotronObject = FindObjectOfType<Jumbotron>();
        instance = this;
        GrinderAud = this.GetComponent<AudioSource>();
        PhotonView masterPV = PhotonView.Find(17);
        roundManager = masterPV.GetComponent<RoundManager>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //check to see if activated in room setting
        if (PhotonNetwork.IsConnected) Enabled = (bool)PhotonNetwork.CurrentRoom.CustomProperties["HazardsActive"];


    }
    // Update is called once per frame
    void Update()
    {
        if (Activated && Closed)
        {
            rightDoorStart.transform.position = Vector3.MoveTowards(rightDoorStart.transform.position, rightDoorEnd.transform.position, doorSpeed);
            leftDoorStart.transform.position = Vector3.MoveTowards(leftDoorStart.transform.position, leftDoorEnd.transform.position, doorSpeed);
        }
        if (leftDoorStart.transform.position == leftDoorEnd.transform.position)
        {
            Closed = true;
        }
        if (!Activated && Closed && Enabled)
        {
            //LevelTimePercent = jumbotronObject.GetLevelTimer().LevelTimePercentage();
            LevelTimePercent = roundManager.LevelTimePercentage();
            if (LevelTimePercent >= 60)
            {
                Activated = true;
                if (GrindnerSound != null) GrinderAud.PlayOneShot(GrindnerSound);
            }
        }
    }
    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code; //Get event code
        if (eventCode == PlayGrinderEffectEventCode) //UPDATE CANNON OCCUPATION STATUS
        {
            GrinderEffect effectType = (GrinderEffect)((object[])photonEvent.CustomData)[0];
            switch (effectType)
            {
                case GrinderEffect.Death:
                    PlayDeathEffect();
                    break;
                default: break;
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.name == "XR Origin" && Activated && Enabled)
        {
            netPlayer = PlayerController.photonView.GetComponent<NetworkPlayer>();
            netPlayer.photonView.RPC("RPC_Hit", RpcTarget.All, 100, netPlayer.photonView.ViewID, Vector3.zero, (int)DeathCause.TRAP);
            PlayGrinderEffectEvent(GrinderEffect.Death);
        }
    }
    private void PlayGrinderEffectEvent(GrinderEffect effect)
    {
        object[] content = new object[] { effect };                                                                 //Get object array containing identifyer for this spawnpoint and the player occupying it (or vacating it)
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };              //Indicate that this event is getting sent to every connected user
        PhotonNetwork.RaiseEvent(PlayGrinderEffectEventCode, content, raiseEventOptions, SendOptions.SendReliable); //Raise event across network
    }
    public void PlayDeathEffect()
    {
        if (deathEffect != null) deathEffect.Play(true);
        if (deathSound != null) GrinderAud.PlayOneShot(deathSound);
    }
}
