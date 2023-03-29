using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretTrap : NetworkedArenaElement 
{
    public int shotCap = 6,shotsStored=0;
    public bool firing = false, cooldown = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        
    }
}
