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

        public KeyCode escapeKey = KeyCode.Escape;

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
            if (Input.GetKeyDown(escapeKey))
            {
                manipulatorVariable.SetValue(null);
            }
            if (activeManipulator != null && !activeManipulator.OnUpdate())
            {
                manipulatorVariable.SetValue(null);
            }
        }
        private void OnDrawGizmos()
        {
            activeManipulator?.OnDrawGizmos();
        }


        public void OnAreaSelected(Vector2 origin, Vector2 size)
        {
            if (activeManipulator is IAreaSelectManipulator areaManipulator)
            {
                areaManipulator.OnAreaSelected(origin, size);
            }
        }
        public void OnDragAreaChanged(Vector2 origin, Vector2 size)
        {
            if (activeManipulator is IAreaSelectManipulator areaManipulator)
            {
                areaManipulator.OnDragAreaChanged(origin, size);
            }
        }
        public void SetDragging(bool isDragging)
        {
            if (activeManipulator is IAreaSelectManipulator areaManipulator)
            {
                areaManipulator.SetDragging(isDragging);
            }
        }
    }
}
