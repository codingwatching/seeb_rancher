using UnityEngine;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    public interface IManipulatorClickReciever
    {
        public bool SelfHit(RaycastHit hit);
    }
}
