using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerController : MonoBehaviour
{
    [SerializeField, Tooltip("teehee this is a fake tooltip")] private Transform[] conveyerBeltStopPositions;
    [SerializeField, Tooltip("")] private Transform[] conveyerBeltObjects;
    [SerializeField, Tooltip("How much time it takes for objects to get from conveyerbelt stop positions")] private float transportTime;
    [SerializeField, Tooltip("Refference to MenuStationController")] private MenuStationController menuStationControllerRef;
    [SerializeField, Tooltip("Indicates whether or not this is the player's conveyerbelt")] private bool isPlayerBelt;

    private int currentConveyerBeltIndex = -1;
    private bool conveyerBeltIsMoving = false;
    private float[] beginningZBiases;

    private void Start()
    {
        //if this is not the conveyerbelt start repeatedely calling the conveyerbelt to move
        if(!isPlayerBelt) { InvokeRepeating("DisplayBeltMove", Random.Range(0.5f, 3f), 3f); }
        


    }

    public void MoveConveyer(int nextConveyerBeltIndex)
    {
        //if -1 is passed in, it just moves to the next belt index
        if(nextConveyerBeltIndex == -1) { nextConveyerBeltIndex = currentConveyerBeltIndex + 1; }

        //only calls the coroutine if it is not currently running
        if(!conveyerBeltIsMoving) { StartCoroutine(MovingConveyerBelt(nextConveyerBeltIndex)); }
        //safety
        conveyerBeltIsMoving = true;
    }

    
    private IEnumerator MovingConveyerBelt(int nextConveyerBeltIndex)
    {
        //safety first folks
        conveyerBeltIsMoving = true;

        //only do if this is the player's belt
        if(isPlayerBelt)
        {
            //retract all ui that is down except for the next one
            menuStationControllerRef.DeactivateAllOtherStations(nextConveyerBeltIndex);
        }


        //wait a tiny bit
        yield return new WaitForSeconds(0.4f);                          //do other things in this time like retract stuffs

        //make arrays to store starting + ending positions
        Vector3[] startPositions = new Vector3[conveyerBeltObjects.Length];
        Vector3[] endPositions = new Vector3[conveyerBeltObjects.Length];

        //assign the arrays wit the correct starting and ending positions
        for(int i = 0; i < conveyerBeltObjects.Length; i++)
        {
            Debug.Log("i = " + i);
            Debug.Log("next Conv = " + nextConveyerBeltIndex);
            startPositions[i] = conveyerBeltObjects[i].position;
            if(isPlayerBelt)
            {
                endPositions[i] = new Vector3(conveyerBeltObjects[i].position.x, conveyerBeltObjects[i].position.y, conveyerBeltStopPositions[nextConveyerBeltIndex].position.z);
            }
            else
            {
                endPositions[i] = new Vector3(conveyerBeltObjects[i].position.x, conveyerBeltObjects[i].position.y, conveyerBeltStopPositions[nextConveyerBeltIndex].position.z);
            }
            
            
        }
        


        //set initial time to 0
        float timeElapsed = 0;

        //lerp the objects from start to end positions
        while (timeElapsed < transportTime)
        {
            //smooth lerp duration alg
            float t = timeElapsed / transportTime;
            t = t * t * (3f - 2f * t);

            //loops through all of the conveyer belt objects and lerps them from start to end
            for(int i = 0; i < conveyerBeltObjects.Length; i++)
            {
                conveyerBeltObjects[i].position = Vector3.Lerp(startPositions[i], endPositions[i], t);
                //conveyerBeltObjects[i].position = endPositions[i];
            }

            //advance time
            timeElapsed += Time.deltaTime;
            //timeElapsed = 100;

            //just in case
            if(GameManager.Instance.levelTransitionActive) { break; }


            //do animation if under 0.25 seconds from ending, and the station isn't already moving, and it is the player's conveyer
            if(isPlayerBelt)
            {
                if (timeElapsed > (transportTime - 0.25f) && !menuStationControllerRef.GetMenuStationIsMoving(nextConveyerBeltIndex))
                {
                    menuStationControllerRef.ActivateStation(nextConveyerBeltIndex);
                }
            }
            

            yield return null;
        }

        
        

        currentConveyerBeltIndex = nextConveyerBeltIndex;
        conveyerBeltIsMoving = false;
    }
    
    private void DisplayBeltMove()
    {
        //first teleport objects that need to go from one end to the other to actually do that
        for(int i = 0; i < conveyerBeltObjects.Length; i++)
        {
            if(conveyerBeltObjects[i].position.z >= conveyerBeltStopPositions[conveyerBeltStopPositions.Length - 1].position.z)
            {
                conveyerBeltObjects[i].position = conveyerBeltStopPositions[0].position;
            }
        }

        //then call conveyerbelt
        currentConveyerBeltIndex = MyUtils.MyMod(currentConveyerBeltIndex + 1, conveyerBeltStopPositions.Length);
        Debug.Log("YEE_________________________________________________________________________________________________________ " + currentConveyerBeltIndex);
        MoveConveyer(currentConveyerBeltIndex);
        
    }

}
