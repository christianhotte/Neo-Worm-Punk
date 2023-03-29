using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoomSettingsDisplay : MonoBehaviour
{
    private TextMeshProUGUI roomSettingsText;   //The room settings that are displayed

    // Start is called before the first frame update
    void Start()
    {
        roomSettingsText = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
