using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class WormHoleTrigger : MonoBehaviour
{
    private WormHole WHS;
    internal bool exiting = false,flashin=false,reset=false;
    public GameObject light,WormHole;
    private Animator holeAnim;
    // Start is called before the first frame update
    void Start()
    {
        WHS = gameObject.GetComponentInParent<WormHole>();
        holeAnim = WormHole.GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
       if(exiting && !flashin)  //If someones coming out of this hole and the light isnt on
        {
            StartCoroutine(FlashLight());
        }
        if (exiting) //set by the parent when designated as the exit and player is in wormhole
        {
            holeAnim.SetBool("Locked",true);//Tells the wormhole to close
        }
        if (reset)//Set by the parent script when player exits the wormholes
        {
            holeAnim.SetBool("Locked", false);//Tells the wormhole to open
            reset = false;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
     
        if (other.TryGetComponent(out XROrigin playerOrigin) && !WHS.locked) // make sure it hit the player, and the wormhole isnt locked
        {
            GameObject playerRb = PlayerController.instance.bodyRb.gameObject;//gets player reference to send to the wormhole script
            holeAnim.SetBool("Locked", true);
            StartCoroutine(WHS.StartWormhole(this.gameObject, playerRb)); //Tells the wormhole to start the loop 
            return;
        }
    }

    public IEnumerator FlashLight()
    {
        flashin = true;
        light.SetActive(true);//turns light off
        yield return new WaitForSeconds(0.2f);
        light.SetActive(false);//turns light back on
        flashin = false;
    }
}
