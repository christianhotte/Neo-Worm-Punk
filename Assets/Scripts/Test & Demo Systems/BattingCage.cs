using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattingCage : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public Transform spawnPos;
    public bool cooldown = false;
    public Projectile projScript;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!cooldown)
        {
            StartCoroutine(Shoot());
        }
    }
    public IEnumerator Shoot()
    {
        cooldown = true;
        GameObject projInstance= Instantiate(ProjectilePrefab);
        projInstance.transform.position = spawnPos.position;
        projScript = projInstance.GetComponent<Projectile>();
        projScript.FireDumb(spawnPos.position,spawnPos.rotation);
        //projScript.localOnly = true;
        yield return new WaitForSeconds(2.0f);
        cooldown = false;
    }

}
