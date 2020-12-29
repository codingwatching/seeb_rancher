using UnityEngine;
using UnityEditor;

namespace Assets.Scripts.Utilities.SaveSystem.Components
{
    [CreateAssetMenu(fileName = "SaveablePrefabRegistry", menuName = "Saving/SaveablePrefabRegistry", order = 10)]
    public class SaveablePrefabRegistry : UniqueObjectRegistryWithAccess<SaveablePrefabType>
    {
    }
}