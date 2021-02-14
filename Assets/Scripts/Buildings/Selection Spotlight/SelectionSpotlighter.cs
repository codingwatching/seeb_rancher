using Dman.ReactiveVariables;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Buildings
{
    public class SelectionSpotlighter : MonoBehaviour
    {
        public GameObjectVariable selectedGameObject;
        public float spotlightHeightFromSelection = 5f;
        public GameObject highlighterObject;

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
            }
            else
            {
                highlighterObject.SetActive(true);
                highlighterObject.transform.position = spotlightedObject.transform.position + Vector3.up * spotlightHeightFromSelection;
            }
        }
    }
}