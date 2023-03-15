using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class Leaderboards : MonoBehaviourPunCallbacks
{
    //Objects & Components:
    [Header("Components:")]
    [SerializeField, Tooltip("Displays which rank each player placed in.")]              private TMP_Text ranks;
    [SerializeField, Tooltip("Displays name of each player who participated in match.")] private TMP_Text names;
    [SerializeField, Tooltip("Displays how many kills each player got.")]                private TMP_Text kills;
    [SerializeField, Tooltip("Displays how many time player died.")]                     private TMP_Text deaths;
    [SerializeField, Tooltip("Displays player's K/D ratio.")]                            private TMP_Text ratios;

    //Settings:
    [Header("Settings:")]
    [SerializeField, Tooltip("How far apart each new line is (in Y units).")]                                     private float lineSeparation;
    [Range(0, 1), SerializeField, Tooltip("How light player colors are (increase for consistency/readability).")] private float playerColorGamma;
    [SerializeField, Tooltip("If true, system will reset player stats as they leave the scene.")]                 private bool clearStats = true;
    
    //Runtime Vars:
    private bool showingLeaderboard; //True when leaderboard is enabled for the scene (only when players are coming back from combat)

    //RUNTIME METHODS:
    private void Awake()
    {
        //Event subscription:
        SceneManager.sceneUnloaded += OnSceneUnloaded; //Subscribe to scene unload method
    }
    private void OnDestroy()
    {
        //Event unsubscription:
        SceneManager.sceneUnloaded -= OnSceneUnloaded; //Unsubscribe from scene unload method
    }
    void Start()
    {
        //Initialization:
        if (PhotonNetwork.LocalPlayer.ActorNumber != GetComponentInParent<LockerTubeController>().tubeNumber) { gameObject.SetActive(false); return; } //Hide board if it does not correspond with player's tube

        //Check scene state:
        foreach (NetworkPlayer player in NetworkPlayer.instances) { if (player.networkPlayerStats.numOfKills > 0) { showingLeaderboard = true; break; } } //Show leaderboard if any players have any kills
        if (showingLeaderboard) //Leaderboard is being shown this scene
        {
            //Rank players:
            List<NetworkPlayer> rankedPlayers = new List<NetworkPlayer>(); //Create a new list for sorting network players by rank
            foreach (NetworkPlayer player in NetworkPlayer.instances) //Iterate through each network player in scene
            {
                if (rankedPlayers.Count == 0) { rankedPlayers.Add(player); continue; } //Add first player immediately to list

                //Get stats:
                PlayerStats stats = player.networkPlayerStats; //Get current player's stats from last round
                float currentH = stats.numOfKills;             //Get KD of current player

                //Rank against competitors:
                for (int x = 0; x < rankedPlayers.Count; x++) //Iterate through ranked player list
                {
                    PlayerStats otherStats = rankedPlayers[x].networkPlayerStats;      //Get stats from other player
                    float otherH = otherStats.numOfKills;                              //Get KD of other player
                    if (otherH < currentH) { rankedPlayers.Insert(x, player); break; } //Insert current player above the first player it outranks
                }
                if (!rankedPlayers.Contains(player)) rankedPlayers.Add(player); //Add player in last if it doesn't outrank anyone
            }

            //Display lists:
            for (int x = 0; x < rankedPlayers.Count; x++) //Iterate through list of ranked players
            {
                //Initialization:
                float yHeight = x * -lineSeparation;                                  //Get target Y height for all new text assets
                PlayerStats stats = rankedPlayers[x].networkPlayerStats;              //Get stats from current player
                Color playerColor = rankedPlayers[x].currentColor;                    //Get color to make text (from synched player settings)
                playerColor = Color.Lerp(playerColor, Color.white, playerColorGamma); //Make color a bit lighter so it is more readable

                //Place rank number:
                TMP_Text newRank = Instantiate(ranks, ranks.transform.parent).GetComponent<TMP_Text>(); //Instantiate new text object
                newRank.rectTransform.localPosition -= Vector3.down * yHeight;                          //Move text to target position
                newRank.text = "#" + (x + 1) + ":";                                                     //Display ranks in order by number
                newRank.color = playerColor;                                                            //Set text color to given player color

                //Place name:
                TMP_Text newName = Instantiate(names, names.transform.parent).GetComponent<TMP_Text>(); //Instantiate new text object
                newName.rectTransform.localPosition -= Vector3.down * yHeight;                          //Move text to target position
                List<char> nameCharacters = new List<char>();
                foreach (char c in rankedPlayers[x].GetName().ToCharArray())
                {
                    if (c == '#') continue;
                    if (c == '0') continue;
                    if (c == '1') continue;
                    if (c == '2') continue;
                    if (c == '3') continue;
                    if (c == '4') continue;
                    if (c == '5') continue;
                    if (c == '6') continue;
                    if (c == '7') continue;
                    if (c == '8') continue;
                    if (c == '9') continue;
                    nameCharacters.Add(c);
                }
                newName.text = new string(nameCharacters.ToArray());                              //Display player name
                newName.color = playerColor;                                                      //Set text color to given player color
                if (rankedPlayers[x].photonView.IsMine) newName.fontStyle = FontStyles.Underline; //Underline local player's name

                //Place kills:
                TMP_Text newKills = Instantiate(kills, kills.transform.parent).GetComponent<TMP_Text>(); //Instantiate new text object
                newKills.rectTransform.localPosition -= Vector3.down * yHeight;                          //Move text to target position
                newKills.text = stats.numOfKills.ToString();                                             //Display killcount
                newKills.color = playerColor;                                                            //Set text color to given player color

                //Place kills:
                TMP_Text newDeaths = Instantiate(deaths, deaths.transform.parent).GetComponent<TMP_Text>(); //Instantiate new text object
                newDeaths.rectTransform.localPosition -= Vector3.down * yHeight;                            //Move text to target position
                newDeaths.text = stats.numOfDeaths.ToString();                                              //Display death count
                newDeaths.color = playerColor;                                                              //Set text color to given player color

                //Place K/D:
                TMP_Text newKD = Instantiate(ratios, ratios.transform.parent).GetComponent<TMP_Text>();        //Instantiate new text object
                newKD.rectTransform.localPosition -= Vector3.down * yHeight;                                   //Move text to target position
                float KD = ((float)stats.numOfKills / (stats.numOfDeaths > 0 ? (float)stats.numOfDeaths : 1));
                newKD.text = KD.ToString("F2");                                                                //Display KD ratio
                newKD.color = playerColor;                                                                     //Set text color to given player color
            }

            //Clear list references:
            ranks.enabled = false;  //Hide original ranking display
            names.enabled = false;  //Hide original name display
            kills.enabled = false;  //Hide original kill display
            deaths.enabled = false; //Hide original death display
            ratios.enabled = false; //Hide original ratio display
        }
        else gameObject.SetActive(false); // If the player did not come back from a game, we don't want to show the leaderboards.
    }
    /// <summary>
    /// Called whenever current scene is unloaded.
    /// </summary>
    public void OnSceneUnloaded(Scene scene)
    {
        if (clearStats) //Leaderboard is set to clear local player stats
        {
            NetworkPlayer localPlayer = PlayerController.photonView.GetComponent<NetworkPlayer>(); //Get reference to local network player
            localPlayer.networkPlayerStats.numOfKills = 0;                                         //Reset kill counter
            localPlayer.networkPlayerStats.numOfDeaths = 0;                                        //Reset death counter
            localPlayer.SyncStats();                                                               //Sync cleared stats over network
        }
    }
}