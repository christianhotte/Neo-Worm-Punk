using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretTrap : NetworkedArenaElement 
{
    public int shotCap = 6,shotsStored=0,playerID;
    public bool firing = false, Reset = false, hasTarget = false;
    public Transform barrelPos,barrelReset;
    public TrapTrigger connectedTrigger;
    public GameObject turret,spotLight;
    private NetworkedArenaElement NetworkController;
    public GameObject[] PlayersInRange;

    // Start is called before the first frame update
    void Start()
    {
        PlayersInRange = new GameObject[6];
        

    }

    // Update is called once per frame
    void Update()
    {
        if (hasTarget)
        {
            if (PlayersInRange[0] != null)
            {
                turret.transform.LookAt(PlayersInRange[0].transform);
                Reset = false;
            }
        }
      
    }
    public void AddShot(int TriggeringPlayerID)
    {
        shotsStored++;
        if (shotsStored == shotCap)
        {
            NetworkController.ActivateTrap(TriggeringPlayerID, this);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "XR Origin")
        {
            Debug.Log("PlayerAdded");
            hasTarget = true;
            Debug.Log(other.name);
            PlayersInRange[0] = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "XR Origin")
        {
            Debug.Log("PlayerRemoved");
            hasTarget = false;
            turret.transform.LookAt(barrelReset);
            PlayersInRange[0] = null;
        }
    }
}
