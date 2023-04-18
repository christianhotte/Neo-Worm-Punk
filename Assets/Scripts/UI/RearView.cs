using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class RearView : MonoBehaviour
{
    private PlayerController playerRef;
    private List<NetworkPlayer> otherPlayers = new List<NetworkPlayer>();
    // Start is called before the first frame update
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        if (!GameManager.Instance.InMenu())
        {
            Transform playerCam = PlayerController.instance.cam.transform;
            foreach (NetworkPlayer otherPlayer in otherPlayers)
            {
                Vector3 PlayerToNet = (playerCam.position - otherPlayer.GetComponentInChildren<Targetable>().targetPoint.position);
                PlayerToNet = Vector3.ProjectOnPlane(PlayerToNet, playerCam.up);
                Vector3 facingDir = playerCam.forward;
                float playerAngle = Vector3.SignedAngle(facingDir, PlayerToNet, playerCam.up);
                Debug.Log(playerAngle);

               // Color playerColor = PlayerSettingsController.playerColors[(int)otherPlayer.GetComponent<PhotonView>().Owner.CustomProperties["Color"]];
            }
        }
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerRef = PlayerController.instance;
        otherPlayers.AddRange(FindObjectsOfType<NetworkPlayer>());
        otherPlayers.Remove(PlayerController.photonView.GetComponent<NetworkPlayer>());
    }
    public void PlaceIndicator(float Pos,float Size,Color PlayerCol)
    {

    }
}
