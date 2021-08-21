using UnityEngine;

namespace UI.Manipulators
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

        public virtual void OnDrawGizmos()
        {

        }
    }

    public interface IAreaSelectManipulator
    {
        void OnAreaSelected(Vector2 origin, Vector2 size);
        void OnDragAreaChanged(Vector2 origin, Vector2 size);
        void SetDragging(bool isDragging);
    }
}
