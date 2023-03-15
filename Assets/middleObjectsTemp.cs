using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class middleObjectsTemp : MonoBehaviour
{
    private Transform middleObject;
    private float newYLevel;
    [SerializeField] private float maxYLevel;
    [SerializeField] private float movePerUpdate;
    void Update()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            middleObject = transform.GetChild(i);
            if(middleObject.position.y <= 0)
            {
                newYLevel = maxYLevel;
            }
            else
            {
                newYLevel = middleObject.position.y - movePerUpdate;
            }
            middleObject.position = new Vector3(middleObject.position.x, newYLevel, middleObject.position.z);
        }
    }
}
