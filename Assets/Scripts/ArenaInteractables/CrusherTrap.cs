using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CrusherTrap : NetworkedArenaElement
{
    public bool cooldown = false;
    internal TrapTrigger connectedTrigger;
    public GameObject IndicatorLight;
    public List<NetworkPlayer> PlayersInTrap = new List<NetworkPlayer>();
    private NetworkedArenaElement networkScript;

    // Start is called before the first frame update
    void Start()
    {
        networkScript = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkedArenaElement>();
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
    public void ActivateCrusher(int PlayerID)
    {
        if (!cooldown)
        {
            cooldown = true;
            networkScript.ActivateTrap(PlayerID, this, PlayersInTrap);
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "XR Origin" && !cooldown)
        {
            if (!PlayersInTrap.Contains(other.GetComponent<NetworkPlayer>()))
            {
                PlayersInTrap.Add(other.GetComponent<NetworkPlayer>());
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "XR Origin")
        {
            PlayersInTrap.Remove(other.GetComponent<NetworkPlayer>());
        }
    }
}
