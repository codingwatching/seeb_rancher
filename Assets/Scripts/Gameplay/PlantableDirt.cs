using Assets.Scripts.UI.Manipulators.Scripts;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    /// <summary>
    /// marks a mesh with a collider as something which can be planted onto
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlantableDirt : MonoBehaviour, IManipulatorClickReciever
    {
        public GameObject GetOutlineObject()
        {
            return gameObject;
        }

        public bool IsSelectable()
        {
            return false;
        }

        public bool SelfClicked(uint clickedObjectID)
        {
            return false;
        }
    }
}
