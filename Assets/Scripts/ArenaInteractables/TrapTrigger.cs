using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using Photon.Pun;
using UnityEngine.Events;

public class TrapTrigger : Targetable
{
    private CrusherTrap crushScript;
    private HoopBoost hoopScript;
    public PowerUp powerScript;
    private int lastPlayerID;
    private TurretTrap turretScript;
    public GameObject indicatorLight,attatchedTrap;
    public GameObject[] MultiTrapsAttacthed;
    internal bool cooldown = false,multiTrigger=false;
    internal NetworkPlayer ActivatingPlayer;
    public UnityEvent onTrapActivated;
    public TrapController netController;

    // Start is called before the first frame update
    void Start()
    {
        netController = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<TrapController>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!cooldown)
        {
            indicatorLight.SetActive(true);
            this.active = true;
        }
        else
        {
            indicatorLight.SetActive(false);
            this.active = false;
        }
    }
    public override void IsHit(int damage, int playerID)
  {
      if (playerID <= 0) return;
      if (PhotonNetwork.GetPhotonView(playerID).TryGetComponent(out NetworkPlayer player))
      {
            ActivatingPlayer = player;
            if (!cooldown)
            {
                lastPlayerID = playerID;
                onTrapActivated.Invoke();
            }
      }
  }
    public void TurretTrap()
    {
        cooldown = true;
        turretScript = attatchedTrap.GetComponent<TurretTrap>();
        turretScript.connectedTrigger = this.GetComponent<TrapTrigger>();
        turretScript.AddShot(lastPlayerID);

    }
    public void CrusherTrap()
    {
        cooldown = true;
        crushScript = attatchedTrap.GetComponent<CrusherTrap>();
        crushScript.connectedTrigger = this.GetComponent<TrapTrigger>();
        crushScript.ActivateCrusher(lastPlayerID);
    }
    public void SlimeHoop()
    {
        cooldown = true;
        if (multiTrigger)
        {
            foreach(GameObject trap in MultiTrapsAttacthed)
            {
                hoopScript = trap.GetComponent<HoopBoost>();
                hoopScript.triggerScript = this.GetComponent<TrapTrigger>();
                hoopScript.slimed = true;
                StartCoroutine(hoopScript.SlimeHoop());
            }
        }
        else
        {
            hoopScript = attatchedTrap.GetComponent<HoopBoost>();
            hoopScript.triggerScript = this.GetComponent<TrapTrigger>();
            hoopScript.slimed = true;
            StartCoroutine(hoopScript.SlimeHoop());
        }
    }
}
