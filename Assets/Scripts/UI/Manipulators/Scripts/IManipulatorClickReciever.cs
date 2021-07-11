using UnityEngine;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    public interface IManipulatorClickReciever
    {
        bool SelfClicked(uint clickedObjectID);
        bool IsSelectable();
        GameObject GetOutlineObject();
    }
}
