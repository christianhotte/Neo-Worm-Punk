using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private Transform lookAt;
    [SerializeField] private Transform transformToFollow;
    [SerializeField] private float followSpeed;
    [SerializeField] private float maxFollowDist;

    private Transform objectTransform;

    // Start is called before the first frame update
    void Start()
    {
        objectTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        objectTransform.LookAt(lookAt, Vector3.up);

        Vector3 newPos = objectTransform.position;
        Vector3 followPos = transformToFollow.position;

        newPos = Vector3.MoveTowards(followPos, Vector3.Lerp(newPos, followPos, followSpeed * Time.deltaTime), maxFollowDist);
        transform.position = newPos;
    }
}
