using UnityEngine;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    public interface IManipulatorClickReciever
    {
        public void SelfHit(RaycastHit hit);
    }
}
