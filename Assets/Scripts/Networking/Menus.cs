using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code was used from https://youtu.be/zPZK7C5_BQo?t=848

public class Menus : MonoBehaviour
{
    public string menuName;
    public bool open;

    // Opens the menu UI
    public void Open()
    {
        open = true;
        gameObject.SetActive(true);
    }

    // Closes the menu UI
    public void Close()
    {
        open = false;
        gameObject.SetActive(false);

        if (menuName == "tutorials")
        {
            open = true;
            gameObject.SetActive(true);
        }
    }
}