using Assets.Scripts.Utilities.SaveSystem.Objects;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Utilities.SaveSystem.Components
{
    /// <summary>
    /// Attatch to the parent of any prefab that should be instantiated multiple times as part of loading
    ///     Will group all save data underneath it as part of the prefab, and not as part of the scene save data
    /// Requires a <see cref="SaveablePrefabParent"/> on the parent transform to uniquely identify the parent in the scene
    ///     which is needed to ensure the prefab is instantiated in the correct context when save loaded
    /// </summary>
    public class SaveablePrefab : MonoBehaviour
    {
        public SaveablePrefabType myPrefabType;

        public SavedPrefab GetPrefabSaveData()
        {
            var saveDataList = GetComponentsInChildren<ISaveableData>()
                    .Select(x => new SaveData
                    {
                        savedSerializableObject = x.GetSaveObject(),
                        uniqueSaveDataId = x.UniqueSaveIdentifier,
                        saveDataIDDependencies = x.GetDependencies().Select(x => x.UniqueSaveIdentifier).ToArray()
                    }).ToList();
            WorldSaveManager.SortSavedDatasBasedOnInterdependencies(saveDataList);
            return new SavedPrefab
            {
                prefabParentId = GetComponentInParent<SaveablePrefabParent>().prefabParentName,
                prefabTypeId = myPrefabType.prefabID,
                saveData = saveDataList.ToArray()
            };
        }
    }
}