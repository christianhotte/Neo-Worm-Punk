using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopBoost : MonoBehaviour
{
    public Transform hoopCenter;
    private PlayerController PC;
    private Rigidbody playerRB;
    public float boostAmount;
    internal bool launchin = false;
    // Start is called before the first frame update
    void Start()
    {        
    }
    // Update is called once per frame
    void Update()
    {          
    }
    public IEnumerator HoopLaunch(Collider hitPlayer)
    {
        launchin = true;
        PC = PlayerController.instance;
        playerRB = PC.bodyRb;
        Vector3 entryVel = Vector3.Project(playerRB.velocity, hoopCenter.forward);//Gets the velocity on one axis in relation to the hoops forward
        Vector3 exitVel = entryVel + (entryVel.normalized * boostAmount); // adds the projected amount to the players velocity
        playerRB.velocity = exitVel;//actually sets the velocity
        yield return new WaitForSeconds(0.2f);//cooldown so only one boost given
        launchin = false;
    }

}
