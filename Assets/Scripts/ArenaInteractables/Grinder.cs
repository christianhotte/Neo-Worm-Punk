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
    public bool Activated = false, Closed = true;
    public float doorSpeed = 3;
    public float LevelTimePercent;

    private Jumbotron jumbotronObject;

    // Start is called before the first frame update
    void Start()
    {
        jumbotronObject = FindObjectOfType<Jumbotron>();
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
        if (!Activated && Closed)
        {
            LevelTimePercent = jumbotronObject.GetLevelTimer().LevelTimePercentage();
            if (LevelTimePercent >= 75)
            {
                Activated = true;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //PlayerController.photonView.ViewID;
        if (other.name == "XR Origin"&& Activated)
        {
            // hitPlayer = other.GetComponent<PlayerController>();
            netPlayer = PlayerController.photonView.GetComponent<NetworkPlayer>();
            // Debug.Log(netPlayer.name);
            netPlayer.photonView.RPC("RPC_Hit", RpcTarget.All, 100, netPlayer.photonView.ViewID);
        }
    }

}
