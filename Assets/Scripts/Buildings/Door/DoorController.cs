using Assets.Scripts.UI.Manipulators.Scripts;
using UnityEngine;

namespace Assets.Scripts.Buildings
{
    public class DoorController : MonoBehaviour, IManipulatorClickReciever
    {
        public GameObject NextPhaseUI;

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
    }
}