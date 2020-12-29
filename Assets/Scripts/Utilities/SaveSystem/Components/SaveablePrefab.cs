using Assets.Scripts.Utilities.SaveSystem.Objects;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Utilities.SaveSystem.Components
{
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