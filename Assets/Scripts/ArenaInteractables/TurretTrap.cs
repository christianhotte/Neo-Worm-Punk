using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretTrap : NetworkedArenaElement 
{
    public int shotCap = 6,shotsStored=0,playerID;
    public bool firing = false, cooldown = false;
    public Transform barrelPos;
    public TrapTrigger connectedTrigger;
    public GameObject ProjectilePrefab;
    private NetworkedArenaElement NetworkController;
    // Start is called before the first frame update
    void Start()
    {
        NetworkController = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkedArenaElement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddShot(int TriggeringPlayerID)
    {
        shotsStored++;
        if (shotsStored == shotCap)
        {
            NetworkController.ActivateTrap(TriggeringPlayerID, this);
        }
    }
}
