using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Plants.Editor
{
    [CustomEditor(typeof(PlantContainer), true)]
    public class PlantContainerEditor : UnityEditor.Editor
    {
        private PlantContainer plantTarget;

        public override void OnInspectorGUI()
        {
            plantTarget = target as PlantContainer;
            var modified = ChangeGrowth();

            modified |= DrawDefaultInspector();

            if (modified)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }

        private bool ChangeGrowth()
        {
            var newGrowth = plantTarget.Growth;
            newGrowth = EditorGUILayout.Slider("Growth", newGrowth, 0, 1);
            if (newGrowth != plantTarget.Growth)
            {
                try
                {
                    plantTarget.UpdateGrowth(newGrowth);
                }
                catch(System.Exception e)
                {
                    Debug.LogException(e);
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
