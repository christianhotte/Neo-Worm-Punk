using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LookTarget : MonoBehaviour
{
    [SerializeField, Tooltip("The sprite for the target when the target is successfully looked at.")] private Sprite lookSuccessSprite;
    [SerializeField, Tooltip("The maximum distance the player needs to be at to look at the target.")] private float distance;
    [SerializeField, Tooltip("The angle between the player's head and the target needed to register that the player is looking at the target.")] private float lookAngleRange;
    public UnityEvent OnLookSuccessful;
    private bool lookedAt = false;


    // Update is called once per frame
    void Update()
    {
        if (!lookedAt)
        {
            Vector3 lookDirection = transform.position - PlayerController.instance.cam.transform.position;
            float lookAngle = Vector3.Angle(lookDirection, PlayerController.instance.cam.transform.forward);
            float lookDistance = Vector3.Distance(transform.position, PlayerController.instance.cam.transform.position);

            //Debug.Log(gameObject.name + " Look Angle: " + lookAngle + ", Expected Angle: " + lookAngleRange + ", Successful: " + (lookAngle < lookAngleRange).ToString());
            //Debug.Log(gameObject.name + " Look Distance: " + lookDistance + ", Expected Distance: " + distance + ", Successful: " + (distance < lookDistance).ToString());

            //If the target is visble, the player is looking at the target, and is close enough to it
            if ((GetComponent<Renderer>().isVisible) && lookAngle < lookAngleRange && lookDistance < distance)
            {
                lookedAt = true;
                if(lookSuccessSprite != null)
                    GetComponent<SpriteRenderer>().sprite = lookSuccessSprite;
                OnLookSuccessful.Invoke();
            }
        }
    }
}
