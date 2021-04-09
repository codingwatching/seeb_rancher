using Dman.Tiling;
using UnityEngine;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    public abstract class MapManipulator : ScriptableObject
    {
        /// <summary>
        /// called when the manipulator is enabled, should active necessary systems
        /// </summary>
        /// <param name="controler"></param>
        public abstract void OnOpen(ManipulatorController controler);
        /// <summary>
        /// return true to keep the manipulator open. return false to close the manipulator
        /// </summary>
        /// <returns>whether or not to keep the manipulator alive</returns>
        public abstract bool OnUpdate();
        public abstract void OnClose();
    }

    public interface IAreaSelectManipulator
    {
        void OnAreaSelected(UniversalCoordinateRange range);
        void OnDragAreaChanged(UniversalCoordinateRange range);
        void SetDragging(bool isDragging);
    }
}
