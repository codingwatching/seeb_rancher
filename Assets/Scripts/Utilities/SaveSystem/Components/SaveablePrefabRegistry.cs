using Assets.Scripts.Utilities.ScriptableObjectRegistries;
using UnityEngine;

namespace Assets.Scripts.Utilities.SaveSystem.Components
{
    [CreateAssetMenu(fileName = "SaveablePrefabRegistry", menuName = "Saving/SaveablePrefabRegistry", order = 10)]
    public class SaveablePrefabRegistry : UniqueObjectRegistryWithAccess<SaveablePrefabType>
    {
    }
}