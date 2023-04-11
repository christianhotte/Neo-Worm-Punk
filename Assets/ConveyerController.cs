using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerController : MonoBehaviour
{
    [SerializeField, Tooltip("teehee this is a fake tooltip")] private Transform[] conveyerBeltStopPositions;
    [SerializeField, Tooltip("")] private Transform[] conveyerBeltObjects;

    private int currentConveyerBeltIndex = -1;
    private bool conveyerBeltIsMoving = false;

    private void Start()
    {



    }

    public void MoveConveyer(int nextConveyerBeltIndex)
    {
        Debug.Log("1");
        //if -1 is passed in, it just moves to the next belt index
        if(nextConveyerBeltIndex == -1) { nextConveyerBeltIndex = currentConveyerBeltIndex + 1; }

        //only calls the coroutine if it is not currently running
        if(!conveyerBeltIsMoving) { StartCoroutine(MovingConveyerBelt(nextConveyerBeltIndex)); }
        //safety
        conveyerBeltIsMoving = true;

    }

    
    private IEnumerator MovingConveyerBelt(int nextConveyerBeltIndex)
    {
        conveyerBeltIsMoving = true;

        yield return new WaitForSeconds(1);
        //do actual moving here instead of teleporting
        for(int i = 0; i < conveyerBeltObjects.Length; i++)
        {
            conveyerBeltObjects[i].position = new Vector3(conveyerBeltObjects[i].position.x, conveyerBeltObjects[i].position.y, conveyerBeltStopPositions[nextConveyerBeltIndex].position.z);
        }

        //if(conveyerBeltObjects.Length == 2)
        //{
            //do thing also for the plat
        //}


        currentConveyerBeltIndex++;
        conveyerBeltIsMoving = false;
    }
    


}
