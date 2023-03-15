using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CreateRoomController : MonoBehaviour
{
    private void OnEnable()
    {
        TMP_InputField inputField = GetComponentInChildren<TMP_InputField>();
        inputField.text = "";
    }
}
