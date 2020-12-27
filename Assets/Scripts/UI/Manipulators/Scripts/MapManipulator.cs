using UnityEngine;

namespace Assets.UI.Manipulators.Scripts
{
    public abstract class MapManipulator : ScriptableObject
    {
        /// <summary>
        /// called when the manipulator is enabled, should active necessary systems
        /// </summary>
        /// <param name="controler"></param>
        public abstract void OnOpen(ManipulatorController controler);
        public abstract void OnUpdate();
        public abstract void OnClose();
    }
}
