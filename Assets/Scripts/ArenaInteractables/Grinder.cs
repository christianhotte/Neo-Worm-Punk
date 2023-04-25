using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.XR.CoreUtils;

public class Grinder : MonoBehaviour
{
    private PlayerController hitPlayer;
    public GameObject leftDoorStart, rightDoorStart, leftDoorEnd, rightDoorEnd;
    private NetworkPlayer netPlayer;
    public bool Activated = false, Closed = true,Enabled=true;
    public float doorSpeed = 3;
    public float LevelTimePercent;
    internal AudioSource GrinderAud;
    public AudioClip GrindnerSound;
    //private Jumbotron jumbotronObject;
    private RoundManager roundManager;

    // Start is called before the first frame update
    void Start()
    {
        //jumbotronObject = FindObjectOfType<Jumbotron>();
        GrinderAud = this.GetComponent<AudioSource>();
        PhotonView masterPV = PhotonView.Find(17);
        roundManager = masterPV.GetComponent<RoundManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Activated && Closed)
        {
            rightDoorStart.transform.position = Vector3.MoveTowards(rightDoorStart.transform.position, rightDoorEnd.transform.position, doorSpeed);
            leftDoorStart.transform.position = Vector3.MoveTowards(leftDoorStart.transform.position,leftDoorEnd.transform.position,doorSpeed);
        }
        if (leftDoorStart.transform.position == leftDoorEnd.transform.position)
        {
            Closed = true;
        }
        if (!Activated && Closed&&Enabled)
        {
            //LevelTimePercent = jumbotronObject.GetLevelTimer().LevelTimePercentage();
            LevelTimePercent = roundManager.LevelTimePercentage();
            if (LevelTimePercent >= 60)
            {
                Activated = true;
                if(GrindnerSound!=null)GrinderAud.PlayOneShot(GrindnerSound);
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.name == "XR Origin"&& Activated&&Enabled)
        {
            netPlayer = PlayerController.photonView.GetComponent<NetworkPlayer>();
            netPlayer.photonView.RPC("RPC_Hit", RpcTarget.All, 100, netPlayer.photonView.ViewID, Vector3.zero, (int)DeathCause.TRAP);
        }
    }

}
