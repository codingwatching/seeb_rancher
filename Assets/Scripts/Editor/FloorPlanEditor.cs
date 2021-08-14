using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.GreenhouseLoader.Editor
{
    [CustomEditor(typeof(FloorPlan), true)]
    public class FloorPlanEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("RebuildTiles"))
            {
                var self = serializedObject.targetObject as FloorPlan;

                EditorUtility.SetDirty(self.EditorTriggeredRebuildFloor());
            }
        }
    }
}
