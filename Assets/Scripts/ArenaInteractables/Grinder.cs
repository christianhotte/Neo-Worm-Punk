using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement;

public class Grinder : MonoBehaviour
{
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
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //check to see if activated in room setting
        Enabled = (bool)PhotonNetwork.CurrentRoom.CustomProperties["HazardsActive"];


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
    private void OnTriggerStay(Collider other)
    {
        if (other.name == "XR Origin" && Activated && Enabled)
        {
            netPlayer = PlayerController.photonView.GetComponent<NetworkPlayer>();
            netPlayer.photonView.RPC("RPC_Hit", RpcTarget.All, 100, netPlayer.photonView.ViewID, Vector3.zero, (int)DeathCause.TRAP);
            netPlayer.photonView.RPC("RPC_TriggerEffect", RpcTarget.All, 0);
        }
    }
    public void PlayDeathEffect()
    {
        if (deathEffect == null) return;
        deathEffect.Play(true);
    }
}
