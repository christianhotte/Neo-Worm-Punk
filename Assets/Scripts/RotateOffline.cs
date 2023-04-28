using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOffline : MonoBehaviour
{
    public int rotSpeed;
    // Start is called before the first frame update
    void Start()
    {

        //rotSpeed = 200;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, rotSpeed * Time.deltaTime, 0);
    }
}
