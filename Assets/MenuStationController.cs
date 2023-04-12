using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuStationController : MonoBehaviour
{
    [SerializeField, Tooltip("An array of the menu station transforms")] private Transform[] menuStationTransforms;

    private Station[] menuStations;

    private struct Station
    {
        public bool activeStatus;
        public Transform stationTransform;
        public Vector3 activePosition;
        public Vector3 inactivePosition;
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
            menuStations[i].activeStatus = false;
        }

        menuStations[0].activeStatus = true;
        ActivateStation(0);
        //by default the first one should actually be set to true, bc that's the menu color select etc.
    }


    public void ActivateStation(int menuStationIndex)
    {
        StartCoroutine(ActivatingStation(menuStationIndex));
    }

    private IEnumerator ActivatingStation(int menuStationIndex)
    {
        yield return new WaitForSeconds(0.1f);  //random wait time

        menuStations[menuStationIndex].stationTransform.position = menuStations[menuStationIndex].activePosition;
        menuStations[menuStationIndex].activeStatus = true;

    }




    public void DeactivateAllOtherStations(int menuStationIndex)
    {
        for(int i = 0; i < menuStations.Length; i++)
        {
            if(i != menuStationIndex && menuStations[i].activeStatus)
            {
                StartCoroutine(DeactivatingStation(i));
            }
        }
    }

    private IEnumerator DeactivatingStation(int menuStationIndex)
    {
        yield return new WaitForSeconds(0.1f);  //random wait time

        menuStations[menuStationIndex].stationTransform.position = menuStations[menuStationIndex].inactivePosition;
        menuStations[menuStationIndex].activeStatus = false;
    }



}
