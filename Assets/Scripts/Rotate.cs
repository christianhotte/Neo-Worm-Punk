using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public int rotSpeed;
    public Grinder grindScript;
    // Start is called before the first frame update
    void Start()
    {
        grindScript = GetComponentInParent<Grinder>();
        rotSpeed = 200;
    }

    // Update is called once per frame
    void Update()
    {
        if (grindScript.Activated)
        {
            transform.Rotate(0, rotSpeed * Time.deltaTime, 0);
        }

    }
}
