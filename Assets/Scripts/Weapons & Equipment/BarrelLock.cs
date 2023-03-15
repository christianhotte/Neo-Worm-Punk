using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelLock : MonoBehaviour
{
    //Objects & Components:
    private NewShotgunController shotgun; //Reference to shotgun relevant to this script

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        shotgun = GetComponentInParent<NewShotgunController>(); //Get shotgun controller script from parent
    }
    private void OnTriggerEnter(Collider other)
    {
        //Old functionality:
        if (shotgun == null) //Compatibility mode for old gun script
        {
            if (GetComponentInParent<FirstGunScript>() != null)
            {
                FirstGunScript fgs = GetComponentInParent<FirstGunScript>();
                if (other.gameObject.tag == "Barrel" && fgs.Ejecting)
                {
                    Debug.Log("Using old system to close weapon breach.");
                    fgs.CloseBreach();
                }
            }
            else Debug.LogWarning("BarrelLock could not find weapon script."); //Log error if neither weapon script could be found
            return;
        }

        //New functionality:
        if (!shotgun.breachOpen) return; //Ignore everything while breach is closed
        if (other.TryGetComponent(out ConfigurableJoint joint)) //Collided object has an attached configurable joint
        {
            if (joint == shotgun.breakJoint) shotgun.Close(); //Close shotgun if it's joint assembly just touched this barrel locker
        }
    }
}
