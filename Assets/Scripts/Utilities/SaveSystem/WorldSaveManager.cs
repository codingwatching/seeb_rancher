using Assets.Scripts.Utilities.SaveSystem.Components;
using Assets.Scripts.Utilities.SaveSystem.Objects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Utilities.SaveSystem
{
    public class WorldSaveManager : MonoBehaviour
    {
        public static readonly string GAMEOBJECT_WORLD_ROOT = "objects.dat";
        public static readonly string ENTITY_COMPONENTS_DATA = "save.dat";
        public static readonly string SHARED_OBJECT_DATA = "data-objects.dat";

        public SaveablePrefabRegistry saveablePrefabRegistry;

        public void Start()
        {
            //Load();
        }

        public void Save()
        {
            var saveDataObject = GetMasterSaveObject(SceneManager.GetActiveScene());
            SaveSystemHooks.TriggerPreSave();
            SerializationManager.Save(GAMEOBJECT_WORLD_ROOT, SaveContext.instance.saveName, saveDataObject);
            SaveSystemHooks.TriggerPostSave();
        }

        public void Load()
        {
            StartCoroutine(LoadCoroutine());
        }

        private IEnumerator LoadCoroutine()
        {
            var loadedData = SerializationManager.Load(GAMEOBJECT_WORLD_ROOT, SaveContext.instance.saveName);
            if (loadedData == null || !(loadedData is MasterSaveObject worldSaveData))
            {
                yield break;
            }

            SaveSystemHooks.TriggerPreLoad();
            DontDestroyOnLoad(gameObject);
            var loadingScene = SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, new LoadSceneParameters(LoadSceneMode.Single));
            yield return new WaitUntil(() => loadingScene.isLoaded);
            LoadFromMasterSaveObjectIntoScene(worldSaveData, loadingScene, saveablePrefabRegistry);
            SaveSystemHooks.TriggerPostLoad();
            Destroy(gameObject);
        }

        public static MasterSaveObject GetMasterSaveObject(Scene sceneToSave)
        {
            var prefabsToSave = new List<SaveablePrefab>();

            var rootObjects = sceneToSave.GetRootGameObjects();

            var sceneSaveData = rootObjects
                .SelectMany(x => GetSaveablesForParent(x.transform, prefabsToSave))
                .Select(x => new SaveData
                {
                    savedSerializableObject = x.GetSaveObject(),
                    uniqueSaveDataId = x.UniqueSaveIdentifier,
                    saveDataIDDependencies = x.GetDependencies().Select(x => x.UniqueSaveIdentifier).ToArray()
                }).ToList();
            SortSavedDatasBasedOnInterdependencies(sceneSaveData);

            var savedPrefabData = prefabsToSave
                .Select(x => x.GetPrefabSaveData());

            return new MasterSaveObject
            {
                sceneSaveData = sceneSaveData.ToArray(),
                sceneSavedPrefabInstances = savedPrefabData.ToArray()
            };
        }
        public static void LoadFromMasterSaveObjectIntoScene(MasterSaveObject saveObject, Scene sceneToLoadTo, SaveablePrefabRegistry prefabRegistry)
        {
            Debug.Log($"loading from save post-scene-reload");
            var rootObjects = sceneToLoadTo.GetRootGameObjects();

            foreach (var prefabRootExistingInScene in rootObjects.SelectMany(x => x.GetComponentsInChildren<SaveablePrefab>()))
            {
                DestroyImmediate(prefabRootExistingInScene.gameObject);
            }
            AssignSaveDataToChildren(rootObjects.Select(x => x.transform), saveObject.sceneSaveData);

            var prefabParentDictionary = rootObjects
                .SelectMany(x => x.GetComponentsInChildren<SaveablePrefabParent>())
                .ToDictionary(x => x.prefabParentName);
            foreach (var savedPrefab in saveObject.sceneSavedPrefabInstances)
            {
                if (!prefabParentDictionary.TryGetValue(savedPrefab.prefabParentId, out var prefabParent))
                {
                    Debug.LogError($"No prefab parent found of ID {savedPrefab.prefabParentId}");
                }
                var prefab = prefabRegistry.GetUniqueObjectFromID(savedPrefab.prefabTypeId);
                var newInstance = Instantiate(prefab.prefab, prefabParent.transform);

                AssignSaveDataToChildren(new[] { newInstance.transform }, savedPrefab.saveData);
            }
        }

        private static IEnumerable<ISaveableData> GetSaveablesForParent(Transform initialTransform, List<SaveablePrefab> prefabList = null)
        {
            return DepthFirstRecurseTraverseAvoidingPrefabs(initialTransform, prefabList).SelectMany(x => x);
        }

        private static IEnumerable<ISaveableData[]> DepthFirstRecurseTraverseAvoidingPrefabs(Transform initialTransform, List<SaveablePrefab> prefabList = null)
        {
            var transformIterationStack = new Stack<Transform>();
            transformIterationStack.Push(initialTransform);

            while(transformIterationStack.Count > 0)
            {
                var currentTransform = transformIterationStack.Pop();

                var saveable = currentTransform.GetComponents<ISaveableData>();
                if (saveable != null && saveable.Length > 0)
                {
                    yield return saveable;
                }
                foreach (Transform child in currentTransform)
                {
                    var childPrefab = child.GetComponent<SaveablePrefab>();
                    if (childPrefab != null)
                    {
                        prefabList?.Add(childPrefab);
                        continue;
                    }
                    transformIterationStack.Push(child);
                }
            }
        }

        public static void SortSavedDatasBasedOnInterdependencies(List<SaveData> datas)
        {
            datas.Sort((a, b) =>
            {
                var aDependsOnB = a.saveDataIDDependencies.Contains(b.uniqueSaveDataId);
                var bDependsOnA = b.saveDataIDDependencies.Contains(a.uniqueSaveDataId);
                if (aDependsOnB && bDependsOnA)
                {
                    throw new System.Exception("Circular dependency detected!");
                }
                if (aDependsOnB)
                {
                    return -1;
                }
                if (bDependsOnA)
                {
                    return 1;
                }
                return 0;
            });
        }


        private static void AssignSaveDataToChildren(IEnumerable<Transform> roots, SaveData[] orderedSaveData)
        {
            var saveableChildren = roots.SelectMany(x => GetSaveablesForParent(x)).ToDictionary(x => x.UniqueSaveIdentifier);
            foreach (var saveData in orderedSaveData)
            {
                if (!saveableChildren.TryGetValue(saveData.uniqueSaveDataId, out var saveable))
                {
                    Debug.LogError($"No matching saveable for {saveData.uniqueSaveDataId}");
                }
                saveable.SetupFromSaveObject(saveData.savedSerializableObject);
            }
        }
    }
}
