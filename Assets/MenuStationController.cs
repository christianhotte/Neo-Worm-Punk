using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuStationController : MonoBehaviour
{
    [SerializeField, Tooltip("An array of the menu station transforms")] private Transform[] menuStationTransforms;

    //should be replaced with animation curves soon
    [SerializeField, Tooltip("The time it should take to activate menu UI")] private float activationTime;
    [SerializeField, Tooltip("The time it should take to deactivate menu UI")] private float deactivationTime;

    private Station[] menuStations;

    private struct Station
    {
        public bool isActive;
        public Transform stationTransform;
        public Vector3 activePosition;
        public Vector3 inactivePosition;
        public bool isMoving;
    }

    private void Start()
    {
        //setting up the temporary menu station positions
        menuStations = new Station[menuStationTransforms.Length];
        for (int i = 0; i < menuStations.Length; i++)
        {
            menuStations[i].stationTransform = menuStationTransforms[i];
            menuStations[i].activePosition = menuStationTransforms[i].position;
            menuStations[i].stationTransform.position += new Vector3(0, 5, 0);
            menuStations[i].inactivePosition = menuStations[i].stationTransform.position;
            menuStations[i].isActive = false;
            menuStations[i].isMoving = false;
        }
        menuStations[0].isActive = true;
        menuStations[0].stationTransform.position += new Vector3(0, -5, 0);
        menuStations[0].inactivePosition -= new Vector3(0, 10, 0);
        //teleport station to correct positon
        //by default the first one should actually be set to true, bc that's the menu color select etc.
    }


    public void ActivateStation(int menuStationIndex)
    {
        StartCoroutine(ActivatingStation(menuStationIndex));
        //safety
        menuStations[menuStationIndex].isMoving = true;
    }

    private IEnumerator ActivatingStation(int menuStationIndex)
    {
        menuStations[menuStationIndex].isMoving = true;

        //TEMPORARY________________________________________________________________________________________________________________________________________________________________________
        //menuStations[menuStationIndex].stationTransform.position = menuStations[menuStationIndex].activePosition;

        //set initial time to 0
        float timeElapsed = 0;

        while(timeElapsed < activationTime)
        {
            //smooth lerp duration alg
            float t = timeElapsed / activationTime;
            t = t * t * (3f - 2f * t);

            menuStations[menuStationIndex].stationTransform.position = Vector3.Lerp(menuStations[menuStationIndex].inactivePosition, menuStations[menuStationIndex].activePosition, t);

            //advance time
            timeElapsed += Time.deltaTime;

            //= new Vector3(menuStations[menuStationIndex].stationTransform.position.x, menuStations[menuStationIndex].stationTransform.position.x, m);

            //just in case
            if (GameManager.Instance.levelTransitionActive) { break; }

            yield return null;
        }
        //TEMPORARY________________________________________________________________________________________________________________________________________________________________________





        //Add animations here





        menuStations[menuStationIndex].isActive = true;
        menuStations[menuStationIndex].isMoving = false;

    }




    public void DeactivateAllOtherStations(int menuStationIndex)
    {
        for(int i = 0; i < menuStations.Length; i++)
        {
            if(i != menuStationIndex && menuStations[i].isActive)
            {
                StartCoroutine(DeactivatingStation(i));
            }
        }
    }

    private IEnumerator DeactivatingStation(int menuStationIndex)
    {
        menuStations[menuStationIndex].isMoving = true;


        //TEMPORARY________________________________________________________________________________________________________________________________________________________________________
        //menuStations[menuStationIndex].stationTransform.position = menuStations[menuStationIndex].inactivePosition;

        //set initial time to 0
        float timeElapsed = 0;

        while (timeElapsed < deactivationTime)
        {
            //smooth lerp duration alg
            float t = timeElapsed / deactivationTime;
            t = t * t * (3f - 2f * t);

            menuStations[menuStationIndex].stationTransform.position = Vector3.Lerp(menuStations[menuStationIndex].activePosition, menuStations[menuStationIndex].inactivePosition, t);

            //advance time
            timeElapsed += Time.deltaTime;

            //just in case
            if (GameManager.Instance.levelTransitionActive) { break; }

            yield return null;
        }
        //TEMPORARY________________________________________________________________________________________________________________________________________________________________________





        //Add animations here





        menuStations[menuStationIndex].isActive = false;
        menuStations[menuStationIndex].isMoving = false;
    }


    public bool GetMenuStationIsMoving(int menuStationIndex) => menuStations[menuStationIndex].isMoving;


}
