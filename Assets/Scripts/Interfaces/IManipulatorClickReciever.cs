using UnityEngine;

namespace UI.Manipulators
{
    public interface IManipulatorClickReciever
    {
        bool SelfClicked(uint clickedObjectID);
        bool IsSelectable();
        GameObject GetOutlineObject();
    }
}
