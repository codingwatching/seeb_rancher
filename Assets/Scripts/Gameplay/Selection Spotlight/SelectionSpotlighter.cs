using Dman.ReactiveVariables;
using Simulation.Plants;
using UniRx;
using UnityEngine;

namespace Gameplay.Selection_Spotlight
{
    public class SelectionSpotlighter : MonoBehaviour
    {
        public GameObjectVariable selectedGameObject;
        public GameObject highlighterObject;
        public GameObject pollinationRangeObject;

        private void Awake()
        {
            selectedGameObject.Value
                .TakeUntilDestroy(this)
                .Subscribe(newObject =>
                    SpotlightObject(newObject)).AddTo(this);
        }

        private void SpotlightObject(GameObject spotlightedObject)
        {
            if (spotlightedObject == null)
            {
                highlighterObject.SetActive(false);
                pollinationRangeObject.SetActive(false);
            }
            else
            {
                transform.position = spotlightedObject.transform.position;
                highlighterObject.SetActive(true);
                var plantContainer = spotlightedObject.GetComponentInParent<PlantedLSystem>();
                if (plantContainer != null && plantContainer.CanPollinate())
                {
                    pollinationRangeObject.transform.localScale = new Vector3(
                        plantContainer.PollinationRadius,
                        plantContainer.PollinationRadius,
                        pollinationRangeObject.transform.localScale.z);
                    pollinationRangeObject.SetActive(true);
                }
                else
                {
                    pollinationRangeObject.SetActive(false);
                }
            }
        }
    }
}