using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CrusherTrap : MonoBehaviour
{
    public bool cooldown = false;
    internal TrapTrigger triggerScript;
    public GameObject IndicatorLight;
    static List<NetworkPlayer> PlayersInTrap = new List<NetworkPlayer>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

            if (PlayersInTrap.Count > 0)
            {
                IndicatorLight.SetActive(true);
            }
            else
            {
                IndicatorLight.SetActive(false);
            }
       
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "XR Origin" && !cooldown)
        {
            PlayersInTrap.Add(other.GetComponent<NetworkPlayer>());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "XR Origin")
        {
            PlayersInTrap.Remove(other.GetComponent<NetworkPlayer>());
        }
    }
    public void ActivateCrusher()
    {
        cooldown = true;
        foreach (NetworkPlayer player in PlayersInTrap)
        {
            player.RPC_Hit(100, triggerScript.ActivatingPlayer.photonView.ViewID);
        }
        PlayersInTrap.Clear();
        StartCoroutine(CrusherCooldown());
    }
    public IEnumerator CrusherCooldown()
    {
        yield return new WaitForSeconds(5.0f);
        cooldown = false;
        PlayersInTrap.Clear();
        triggerScript.cooldown = false;
    }
}
