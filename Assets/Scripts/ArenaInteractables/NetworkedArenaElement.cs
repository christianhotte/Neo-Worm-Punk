using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkedArenaElement : MonoBehaviour
{
    public int activatingPlayer;
    public float cooldownTime = 10;
    private Projectile projScript;
    internal bool cooldown = false;
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
        cooldown = true;
        StartCoroutine(TurretDump(Trapper, trap));
    }
    public void ActivateTrap(HoopBoost hoopScript)
    {
        hoopScript.slimed = true;
        hoopScript.StartCoroutine(hoopScript.SlimeHoop());
    }
    public void ActivateTrap(int Trapper, CrusherTrap trap, List<NetworkPlayer> PlayersInTrap)
    {
        foreach (NetworkPlayer player in PlayersInTrap)
        {


            player.RPC_Hit(100, Trapper, Vector3.zero, (int)DeathCause.TRAP);
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
    public IEnumerator TurretDump(int Trapper, TurretTrap trap)
    {
        for (int i = 0; i <= trap.shotsStored; i++)
        {
            Projectile newProjectile; //Initialize reference container for spawned projectile
            if (!PhotonNetwork.InRoom) //Weapon is in local fire mode
            {
                newProjectile = ((GameObject)Instantiate(Resources.Load("Projectiles/HotteProjectile"))).GetComponent<Projectile>(); //Instantiate projectile
                newProjectile.FireDumb(trap.barrelPos);                                                                              //Initialize projectile
            }
            else //Weapon is firing on the network
            {
                newProjectile = PhotonNetwork.Instantiate("Projectiles/HotteProjectile", trap.barrelPos.position, trap.barrelPos.rotation).GetComponent<Projectile>(); //Instantiate projectile on network
                newProjectile.photonView.RPC("RPC_Fire", RpcTarget.All, trap.barrelPos.position, trap.barrelPos.rotation, PlayerController.photonView.ViewID);         //Initialize all projectiles simultaneously
            }

            //GameObject projInstace = Instantiate(trap.ProjectilePrefab);
            //projInstace.transform.position = trap.barrelPos.position;
            //projScript = projInstace.GetComponent<Projectile>();
            //projScript.Fire(trap.barrelPos, Trapper);
            StartCoroutine(TrapCooldown(cooldownTime, trap));
            yield return new WaitForSeconds(0.5f);
        }
        cooldown = false;
    }
}
