using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigDoorController : MonoBehaviour
{
    [SerializeField] private Transform[] doors;
    [SerializeField] private Vector3[] openDoorPos;
    [SerializeField] private float OpenCloseTime;
    private Vector3[] closedDoorPos = new Vector3[2];
    

    private void Awake()
    {
        closedDoorPos[0] = doors[0].localPosition;
        closedDoorPos[1] = doors[1].localPosition;
    }

    public void EnableDoors(float initialWaitTime)
    {
        StartCoroutine(OpenAndCloseDoors(initialWaitTime));
    }

    IEnumerator OpenAndCloseDoors(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        float currentCounter = 0;
        while (currentCounter < OpenCloseTime)
        {
            currentCounter += Time.deltaTime;

            doors[0].transform.localPosition = Vector3.Lerp(doors[0].transform.localPosition, openDoorPos[0], Mathf.InverseLerp(0, OpenCloseTime, currentCounter));
            doors[1].transform.localPosition = Vector3.Lerp(doors[1].transform.localPosition, openDoorPos[1], Mathf.InverseLerp(0, OpenCloseTime, currentCounter));

            yield return null;
        }
        doors[0].transform.localPosition = openDoorPos[0];
        doors[1].transform.localPosition = openDoorPos[1];

        //stay open time
        yield return new WaitForSeconds(0.25f);

        currentCounter = 0;
        while (currentCounter < OpenCloseTime)
        {
            currentCounter += Time.deltaTime;

            doors[0].transform.localPosition = Vector3.Lerp(doors[0].transform.localPosition, closedDoorPos[0], Mathf.InverseLerp(0, OpenCloseTime, currentCounter));
            doors[1].transform.localPosition = Vector3.Lerp(doors[1].transform.localPosition, closedDoorPos[1], Mathf.InverseLerp(0, OpenCloseTime, currentCounter));

            yield return null;
        }
        doors[0].transform.localPosition = closedDoorPos[0];
        doors[1].transform.localPosition = closedDoorPos[1];
    }
}