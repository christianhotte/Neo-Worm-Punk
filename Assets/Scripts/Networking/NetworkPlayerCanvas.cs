using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Voice.PUN;

[RequireComponent(typeof(Canvas))]
public class NetworkPlayerCanvas : MonoBehaviour
{
    private Canvas canvas;
    private PhotonVoiceView photonVoiceView;

    [SerializeField, Tooltip("The image to show when the player is recording.")] private Image recorderSprite;
    [SerializeField, Tooltip("The image to show when the player is speaking.")] private Image speakerSprite;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;

        photonVoiceView = GetComponentInParent<PhotonVoiceView>();
    }


    // Update is called once per frame
    private void Update()
    {
        //Shows the recording or speaker sprite when either action is occurring
        recorderSprite.enabled = photonVoiceView.IsRecording;
        speakerSprite.enabled = photonVoiceView.IsSpeaking;
    }

    private void LateUpdate()
    {
        if (canvas == null || canvas.worldCamera == null)
            return;

        //Rotates the canvas based on the position of the player
        transform.rotation = Quaternion.Euler(0f, canvas.worldCamera.transform.eulerAngles.y, 0f);
    }
}
