using Assets.Scripts.Utilities.Core;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.UI.Manipulators.Scripts
{
    public class ManipulatorController : MonoBehaviour
    {
        public ScriptableObjectVariable manipulatorVariable;
        public MapManipulator activeManipulator;

        private void Awake()
        {
            manipulatorVariable.Value
                .TakeUntilDestroy(this)
                .Subscribe((nextValue) =>
                {
                    activeManipulator?.OnClose();
                    activeManipulator = nextValue as MapManipulator;
                    activeManipulator?.OnOpen(this);
                })
                .AddTo(this);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown((int)MouseButton.RightMouse))
            {
                manipulatorVariable.SetValue(null);
            }
            activeManipulator?.OnUpdate();
        }
    }
}
