using Dman.ReactiveVariables;
using Dman.Tiling;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    public class ManipulatorController : MonoBehaviour
    {
        public ScriptableObjectVariable manipulatorVariable;
        [NonSerialized]
        public MapManipulator activeManipulator;

        public MapManipulator defaultManipulator;


        private void Awake()
        {
            manipulatorVariable.SetValue(defaultManipulator);
            manipulatorVariable.Value
                .TakeUntilDestroy(this)
                .Select(x => x == null ? defaultManipulator : x)
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
            if (activeManipulator != null && !activeManipulator.OnUpdate())
            {
                manipulatorVariable.SetValue(null);
            }
        }

        public void OnAreaSelected(UniversalCoordinateRange range)
        {
            if(activeManipulator is IAreaSelectManipulator areaManipulator)
            {
                areaManipulator.OnAreaSelected(range);
            }
        }
    }
}
