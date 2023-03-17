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
    public GameObject indicatorLight,attatchedTrap;
    public GameObject[] MultiTrapsAttacthed;
    internal bool cooldown = false,multiTrigger=false;
    internal NetworkPlayer ActivatingPlayer;
    public UnityEvent onTrapActivated;

    // Start is called before the first frame update
    void Start()
    {

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
                onTrapActivated.Invoke();
            }
      }
  }
    public void CrusherTrap()
    {
        cooldown = true;
        crushScript = attatchedTrap.GetComponent<CrusherTrap>();
        crushScript.triggerScript = this.GetComponent<TrapTrigger>();
        crushScript.ActivateCrusher();
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
            }
        }
        else
        {
            hoopScript = attatchedTrap.GetComponent<HoopBoost>();
            hoopScript.triggerScript = this.GetComponent<TrapTrigger>();
            hoopScript.slimed = true;
        }
    }
}
