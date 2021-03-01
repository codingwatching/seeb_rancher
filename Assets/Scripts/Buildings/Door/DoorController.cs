using Assets.Scripts.UI.Manipulators.Scripts;
using Dman.ReactiveVariables;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Buildings
{
    public class DoorController : MonoBehaviour, IManipulatorClickReciever
    {
        public GameObject NextPhaseUI;
        public GameObjectVariable selectedGameObject;

        private void Awake()
        {
            selectedGameObject.Value.TakeUntilDestroy(gameObject)
                .Subscribe(x =>
                {
                    NextPhaseUI.SetActive(false);
                }).AddTo(gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public bool SelfHit(RaycastHit hit)
        {
            NextPhaseUI.SetActive(true);
            return true;
        }
        public GameObject GetOutlineObject()
        {
            return gameObject;
        }

        public bool IsSelectable()
        {
            return true;
        }
    }
}