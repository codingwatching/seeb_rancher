using Assets.Scripts.Buildings;
using Assets.Scripts.Plants;
using Dman.LSystem.UnityObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugModeSetup : MonoBehaviour
{
    public GameObject greenhouseParent;

    public GameObject lSystemPrefab;

    public LSystemPlantType plantType;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("debug mode setting up");

        foreach (var door in GameObject.FindObjectsOfType<DoorController>())
        {
            door.gameObject.SetActive(false);
        }

        var i = 0;
        foreach (var plantContainer in greenhouseParent.GetComponentsInChildren<PlantContainer>())
        {
            i++;
            var targetContainer = plantContainer.plantsParent;
            var resultObject = GameObject.Instantiate(lSystemPrefab, targetContainer.transform);
            var lSystem = resultObject.GetComponentInChildren<LSystemBehavior>();
            var generator = lSystem.gameObject.AddComponent<GeneticLSystemParameterGenerator>();
            generator.plantType = plantType;
        }
        Debug.Log($"set up {i} new l systems");
        var developer = greenhouseParent.GetComponent<LSystemDeveloper>();
        developer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
