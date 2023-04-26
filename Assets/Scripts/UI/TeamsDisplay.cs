using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamsDisplay : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private RectTransform teamColorDisplay;
    [SerializeField] private RectTransform teamNameDisplay;

    [Header("Containers")]
    [SerializeField] private Transform teamColorContainer;

    private Dictionary<int, string> playerTeams = new Dictionary<int, string>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void InitializePlayerTeams()
    {
        foreach(var player in NetworkManagerScript.instance.GetPlayerList())
        {
            if (!playerTeams.ContainsValue(player.NickName))
                playerTeams.Add((int)player.CustomProperties["Color"], player.NickName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdatePlayerTeams()
    {

    }
}
