using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopTrigger : MonoBehaviour
{
    private HoopBoost HoopScript;
    // Start is called before the first frame update
    void Start()
    {
        HoopScript = gameObject.GetComponentInParent<HoopBoost>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("SomethingHit");
        if (other.name == "XR Origin"&&!HoopScript.launchin)
        {
            StartCoroutine(HoopScript.HoopLaunch(other));
            Debug.Log("CallingBoost");
        }
    }
}
