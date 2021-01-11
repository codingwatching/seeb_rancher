using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Dman.Tiling
{
    [CustomEditor(typeof(TileMember), true)]
    public class GreenhouseMemberEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var self = target as TileMember;
            var newPosition = self.CoordinatePosition;
            newPosition.type = (CoordinateType)EditorGUILayout.EnumPopup(newPosition.type);
            newPosition.CoordinatePlaneID = (short)EditorGUILayout.IntField("Coordinate Plane", newPosition.CoordinatePlaneID);

            switch (newPosition.type)
            {
                case CoordinateType.TRIANGLE:
                    newPosition.triangleDataView.u = EditorGUILayout.IntField("U", newPosition.triangleDataView.u);
                    newPosition.triangleDataView.v = EditorGUILayout.IntField("V", newPosition.triangleDataView.v);
                    newPosition.triangleDataView.R = EditorGUILayout.Toggle("R", newPosition.triangleDataView.R);
                    break;
                case CoordinateType.SQUARE:
                    newPosition.squareDataView.row = EditorGUILayout.IntField("Row", newPosition.squareDataView.row);
                    newPosition.squareDataView.column = EditorGUILayout.IntField("Column", newPosition.squareDataView.column);
                    break;
                default:
                case CoordinateType.INVALID:
                    break;
            }

            DrawDefaultInspector();

        ass:
            if (!newPosition.Equals(self.CoordinatePosition))
            {
                try
                {
                    self.SetPosition(newPosition);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
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
