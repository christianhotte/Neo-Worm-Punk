using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedArenaElement : MonoBehaviour
{
    public int activatingPlayer;
    public float cooldownTime = 10;
    private Projectile projScript;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ActivateTrap(int Trapper,TurretTrap trap)
    {
        for(int i = 0; i <= trap.shotsStored; i++)
        {
            GameObject projInstace = Instantiate(trap.ProjectilePrefab);
            projInstace.transform.position = trap.barrelPos.position;
            projScript = projInstace.GetComponent<Projectile>();
            projScript.Fire(trap.barrelPos, Trapper);
            StartCoroutine(TrapCooldown(cooldownTime, trap));
        }

    }
    public void ActivateTrap(int Trapper, CrusherTrap trap, List<NetworkPlayer> PlayersInTrap)
    {
        foreach (NetworkPlayer player in PlayersInTrap)
        {


            player.RPC_Hit(100, Trapper);
        }
        PlayersInTrap.Clear();
        StartCoroutine(TrapCooldown(cooldownTime,trap));
    }

    public IEnumerator TrapCooldown(float cooldownTime, CrusherTrap trap)
    {
        yield return new WaitForSeconds(cooldownTime);
        trap.connectedTrigger.cooldown = false;
        trap.cooldown = false;
        trap.PlayersInTrap.Clear();
    }
    public IEnumerator TrapCooldown(float cooldownTime, TurretTrap trap)
    {
        yield return new WaitForSeconds(cooldownTime);
        trap.cooldown = false;
        trap.connectedTrigger.cooldown = false;
        trap.shotsStored = 0;
    }
}
