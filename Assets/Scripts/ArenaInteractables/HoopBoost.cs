using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopBoost : MonoBehaviour
{
    public Transform hoopCenter;
    private PlayerController PC;
    private Rigidbody playerRB;
    public float boostAmount;
    public bool maintainsOtherVelocity;
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
        if(maintainsOtherVelocity)
        {
            //just add the boostAmount to the velocity of the player
            Vector3 boostVel = Vector3.Project(playerRB.velocity, hoopCenter.forward).normalized * boostAmount;
            playerRB.velocity += boostVel;
        }
        else
        {
            //reset all velocity but in the direction of the hoop and give them a boost in that direction
            Vector3 entryVel = Vector3.Project(playerRB.velocity, hoopCenter.forward);//Gets the velocity on one axis in relation to the hoops forward
            Vector3 exitVel = entryVel + (entryVel.normalized * boostAmount); // adds the projected amount to the players velocity
            playerRB.velocity = exitVel;//actually sets the velocity
        }

        
        yield return new WaitForSeconds(0.2f);//cooldown so only one boost given
        launchin = false;
    }

}
