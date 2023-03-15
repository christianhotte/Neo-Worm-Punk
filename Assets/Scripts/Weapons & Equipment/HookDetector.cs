using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookDetector : MonoBehaviour
{
    public Rigidbody hookrb,hitRB;
    private GameObject hookHit;
    public RocketBoost RBScript;
    public Transform hookLead;
    public float hookSpeed = 3, pullSpeed = 20;
    public bool flying = false,pullin=false;
    public LineRenderer cable;

    // Start is called before the first frame update
    void Awake()
    {
        hookrb = this.GetComponent<Rigidbody>();
        Physics.IgnoreCollision(PlayerController.instance.bodyRb.GetComponent<Collider>(), GetComponent<Collider>());

        flying = true;
        cable = this.gameObject.AddComponent<LineRenderer>();
        cable.startColor = Color.black;
        cable.endColor = Color.black;
        cable.material = new Material(Shader.Find("Sprites/Default"));
        cable.startWidth = 0.01f;
        cable.endWidth = 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.GetComponent<LineRenderer>() != null)
        {

            cable.SetPosition(0, RBScript.rocketTip.transform.position);
            cable.SetPosition(1, this.transform.position);
        }
        if (flying)
        {
           // this.transform.LookAt(hookLead);
            hookrb.velocity = (RBScript.rocketTip.forward * hookSpeed);
           // this.transform.position = Vector3.MoveTowards(this.transform.position, hookLead.transform.position, hookSpeed);

        }
        if (pullin)
        {
            this.transform.position = hookHit.transform.position;
            hitRB.velocity = (RBScript.rocketTip.forward * -hookSpeed/4);

            if (RBScript.realDistance > 2)
            {
                pullin = false;
                RBScript.GrappleStop();
            }
        }
       
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 11)
        {
            return;
        }
        else if(collision.collider.tag == "Pullable")
        {
            Debug.Log("triedtopull");
            //this.transform.position = collision.transform.position;
            hookHit = collision.gameObject;
            hitRB = hookHit.GetComponent<Rigidbody>();
            hookrb.isKinematic = true;
            pullin = true;
            flying = false;

        }
        else if (collision.collider.tag != "Blade")
        {
            hookrb.isKinematic = true;
            flying = false;
            //Debug.Log("hit object " + collision.collider.name + " on Layer " + collision.collider.gameObject.layer);
            RBScript.grappleCooldown = false;
            RBScript.HookHit();

        }
       
    }
}
