using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class RearView : MonoBehaviour
{
    private PlayerController playerRef;
    private List<NetworkPlayer> otherPlayers = new List<NetworkPlayer>();
    private Transform[] playerDots;
    public GameObject playerMarker;
    public Transform markerStartPos;
    internal bool scannerOff = false;
    [Header("Settings:")]
    [SerializeField, Range(0, 360)] private float detectionAngle;
    [SerializeField, Min(0)] private float mirrorWidth = 300;
    [SerializeField, Min(0)] private float maxDotScale = 0.6f;
    [SerializeField, Min(0)] private float detectRange = 100;

    // Start is called before the first frame update
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        //mirrorWidth = GetComponentInChildren<RectTransform>().rect.width;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.InMenu() && PhotonNetwork.IsConnected)
        {
            Transform playerCam = PlayerController.instance.cam.transform;
            float sqrMaxDist = Mathf.Pow(detectRange, 2);
            for (int x = 0; x < otherPlayers.Count; x++)
            {
                NetworkPlayer otherPlayer = otherPlayers[x];
                Vector3 PlayerToNet = (otherPlayer.GetComponentInChildren<Targetable>().targetPoint.position - playerCam.position);
                float sqrPlayerDist = PlayerToNet.sqrMagnitude;
                PlayerToNet = Vector3.ProjectOnPlane(PlayerToNet, playerCam.up);
                Vector3 facingDir = playerCam.forward;
                float playerAngle = Vector3.SignedAngle(facingDir, PlayerToNet, playerCam.up);
                //Debug.Log(playerAngle);
                Debug.Log(sqrPlayerDist);
                
                Vector3 newPos = Vector3.zero;

                if (!scannerOff&&(playerAngle >= -(detectionAngle / 2) || playerAngle <= detectionAngle / 2) && sqrPlayerDist < sqrMaxDist)
                {
                    //Dot should be visible
                    //playerDots[x];
                    
                    //Dot position:
                    float angleInterp = Mathf.InverseLerp(-(detectionAngle / 2), detectionAngle / 2, playerAngle);
                    float dotPosX = Mathf.Lerp(-(mirrorWidth / 2), mirrorWidth / 2, angleInterp);
                    newPos.x = dotPosX;

                    //Dot scale:
                    float distInterp = Mathf.InverseLerp(sqrMaxDist, 0, sqrPlayerDist);
                    print(distInterp);
                    float sclValue = Mathf.Lerp(0.075f, maxDotScale, distInterp);
                    playerDots[x].localScale = Vector3.one * sclValue;
                    //float playerDist = Vector3.Distance(playerCam.position, otherPlayer.GetComponentInChildren<Targetable>().targetPoint.position);
                }
                else
                {
                    //Dot should not be visible

                    newPos.x = 1000;
                    playerDots[x].localScale = Vector3.zero;
                }
                playerDots[x].localPosition = newPos;
            }
        }
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameSettings.arenaScene)
        {
            if (PhotonNetwork.IsConnected)
            {
                playerRef = PlayerController.instance;
                otherPlayers.AddRange(FindObjectsOfType<NetworkPlayer>());
                otherPlayers.Remove(PlayerController.photonView.GetComponent<NetworkPlayer>());

                List<Transform> playerDotList = new List<Transform>();
                foreach (NetworkPlayer otherPlayer in otherPlayers)
                {
                    Transform dot = Instantiate(playerMarker, transform.GetChild(0), markerStartPos).GetComponent<Transform>();
                    //Transform dot = //Instantiate dot prefab and get its transform
                    //Move dot to a convenient position
                    dot.localPosition = new Vector3(1000, 0, 0);
                    //Set dot to player color
                    Color playerColor = PlayerSettingsController.playerColors[(int)otherPlayer.GetComponent<PhotonView>().Owner.CustomProperties["Color"]];
                    dot.GetComponent<Image>().color = playerColor;
                    // Color playerColor = PlayerSettingsController.playerColors[(int)otherPlayer.GetComponent<PhotonView>().Owner.CustomProperties["Color"]];
                    dot.localScale = Vector3.one * 0.2f;
                    playerDotList.Add(dot);
                }
                playerDots = playerDotList.ToArray();
            }
        }
    }
    public IEnumerator DisableScanner(float waitTime)
    {
        Debug.Log("DisableStart");
        scannerOff = true;
        yield return new WaitForSeconds(waitTime);
        scannerOff = false;
        Debug.Log("DisableStop");
    }
    public void ScannerStop()
    {
        scannerOff = true;
    }
    public void ScannerStart()
    {
        scannerOff = false;
    }
}
