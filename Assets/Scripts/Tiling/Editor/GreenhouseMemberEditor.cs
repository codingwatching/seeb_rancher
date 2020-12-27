using System;
using UnityEditor;

namespace Assets.Scripts.Tiling
{
    [CustomEditor(typeof(GreenhouseMemeber), true)]
    public class GreenhouseMemberEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var self = serializedObject.targetObject as GreenhouseMemeber;
            var newPosition = self.CoordinatePosition;
            newPosition.type = (Assets.Tiling.CoordinateType)EditorGUILayout.EnumPopup(newPosition.type);
            newPosition.CoordinatePlaneID = (short)EditorGUILayout.IntField("Coordinate Plane", newPosition.CoordinatePlaneID);

            switch (newPosition.type)
            {
                case Assets.Tiling.CoordinateType.TRIANGLE:
                    newPosition.triangleDataView.u = EditorGUILayout.IntField("U", newPosition.triangleDataView.u);
                    newPosition.triangleDataView.v = EditorGUILayout.IntField("V", newPosition.triangleDataView.v);
                    newPosition.triangleDataView.R = EditorGUILayout.Toggle("R", newPosition.triangleDataView.R);
                    break;
                case Assets.Tiling.CoordinateType.SQUARE:
                    newPosition.squareDataView.row = EditorGUILayout.IntField("Row", newPosition.squareDataView.row);
                    newPosition.squareDataView.column = EditorGUILayout.IntField("Column", newPosition.squareDataView.column);
                    break;
                default:
                case Assets.Tiling.CoordinateType.INVALID:
                    break;
            }

            DrawDefaultInspector();

        ass:
            if (!newPosition.Equals(self.CoordinatePosition))
            {
                try
                {
                    self.SetPosition(newPosition);
                }
                catch (IndexOutOfRangeException)
                {
                    newPosition.CoordinatePlaneID = self.CoordinatePosition.CoordinatePlaneID;
                    goto ass;
                }
            }
        }
    }
}
