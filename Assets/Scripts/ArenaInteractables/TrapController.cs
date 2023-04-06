using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    internal NetworkedArenaElement trapScript;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ExecuteTrapOnNetwork(int Trapper, TurretTrap trap)
    {
        trapScript = trap;
    }
    public void ExecuteTrapOnNetwork(int Trapper, CrusherTrap trap)
    {
        trapScript = trap;
    }
    public void ExecuteTrapOnNetwork(HoopBoost trap)
    {
        trapScript = trap;
    }
}
