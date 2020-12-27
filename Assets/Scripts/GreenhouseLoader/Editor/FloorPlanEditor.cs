using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.GreenhouseLoader.Editor
{
    [CustomEditor(typeof(FloorPlan), true)]
    public class FloorPlanEditor: UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("RebuildTiles"))
            {
                var self = serializedObject.targetObject as FloorPlan;
                self.GenerateFloorPlan();
                self.GetComponent<GreenhouseBuilder>().RebuildTiles();
            }
        }
    }
}
