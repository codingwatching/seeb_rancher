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

        private void SpotlightObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                highlighterObject.SetActive(false);
            }
            else
            {
                highlighterObject.SetActive(true);
                highlighterObject.transform.position = gameObject.transform.position + Vector3.up * spotlightHeightFromSelection;
            }
        }
    }
}