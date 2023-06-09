using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Special script which equipment can attach to and receive input from (without needing a playerController). Used for debug equipment testing.
/// </summary>
public class DemoEquipmentMount : MonoBehaviour
{
    //Objects & Components:
    internal PlayerEquipment equipment;   //Reference to base script for attached equipment (should be handed over by equipment itself)
    private NewShotgunController shotgun; //Shotgun currently attached to equipment mount

    //Settings:
    [Header("Inputs:")]
    [SerializeField, Tooltip("Fires weapon.")]                                      private bool fire;
    [SerializeField, Tooltip("Fires weapon automatically at set rate.")]            private bool fireContinuous;
    [Min(0), SerializeField, Tooltip("Shots per second when continuously firing.")] private float continuousFireRate;
    [Range(0, 1), SerializeField] private float playerFollowRate = 1;
    [Min(0), SerializeField] private float playerFollowDist = 100;
    [Header("Debug Options:")]
    [SerializeField, Tooltip("Firing weapon does not spend ammunition.")] private bool infiniteAmmo;

    //Runtime Variables:
    float timeSinceFiring = -1;

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        if (equipment == null) equipment = GetComponentInChildren<PlayerEquipment>();
        shotgun = equipment.GetComponent<NewShotgunController>(); //Get shotgun script if equipment is a shotgun
    }
    private void Update()
    {
        //Check debug input:
        if (shotgun != null) //Shotgun debug inputs
        {
            if (fire) //Fire command is called
            {
                shotgun.Fire();                     //Fire shotgun
                if (infiniteAmmo) shotgun.Reload(); //Reload shotgun immediately if infinite ammo is set
                fire = false;                       //Immediately unpress button
                print("Firing.");                   //Indicate that button was successfully pressed
            }
            if (fireContinuous)
            {
                timeSinceFiring += Time.deltaTime;
                if (timeSinceFiring > 1 / continuousFireRate)
                {
                    shotgun.Fire();
                    if (infiniteAmmo) shotgun.Reload();
                    timeSinceFiring = 0;
                }
            }
            else if (timeSinceFiring > 0) timeSinceFiring = -1;
        }
    }
    private void FixedUpdate()
    {
        if (playerFollowRate > 0 && PlayerController.instance != null && Vector3.Distance(transform.position, PlayerController.instance.cam.transform.position) <= playerFollowDist)
        {
            Vector3 playerDir = (PlayerController.instance.cam.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(playerDir, Vector3.up);
            if (playerFollowRate < 1)
            {
                targetRotation = Quaternion.Slerp(transform.rotation, targetRotation, playerFollowRate);
            }
            transform.rotation = targetRotation;
        }
    }
}
