using UnityEngine;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    public interface IManipulatorClickReciever
    {
        bool SelfHit(RaycastHit hit);
        bool IsSelectable();
        GameObject GetOutlineObject();
    }
}
