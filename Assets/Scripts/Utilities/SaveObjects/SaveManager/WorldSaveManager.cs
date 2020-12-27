//using System.IO;
//using Unity.Entities;
//using UnityEditor;
//using UnityEngine;

//namespace Assets.WorldObjects.SaveObjects.SaveManager
//{
//    public class WorldSaveManager : MonoBehaviour
//    {
//        public static readonly string GAMEOBJECT_WORLD_ROOT = "objects.dat";
//        public static readonly string ENTITY_COMPONENTS_DATA = "save.dat";
//        public static readonly string SHARED_OBJECT_DATA = "data-objects.dat";

//        public GameObject worldPrefab;
//        public GameObject worldObject;

//        public void Start()
//        {
//            Load();
//        }

//        public void Save()
//        {
//            var saveDataObject = worldObject.GetComponent<ISaveable<WorldSaveObject>>();
//            var data = saveDataObject.GetSaveObject();
//            SaveSystemHooks.TriggerPreSave();
//            try
//            {
//                SaveECSWorld();
//            }
//            catch (System.Exception e)
//            {
//                Debug.LogError("ECS Save error, clearing save files");
//                Debug.LogException(e);
//                ClearECSWorldSaveFiles();
//            }
//            SerializationManager.Save(GAMEOBJECT_WORLD_ROOT, SaveContext.instance.saveName, data);
//            SaveSystemHooks.TriggerPostSave();
//        }

//        public void Load()
//        {
//            SaveSystemHooks.TriggerPreLoad();
//            var loadedData = SerializationManager.Load(GAMEOBJECT_WORLD_ROOT, SaveContext.instance.saveName);
//            try
//            {
//                LoadECSWorld();
//            }
//            catch (System.Exception e)
//            {
//                Debug.LogError("ECS Load error, clearing save files");
//                Debug.LogException(e);
//                ClearECSWorldSaveFiles();
//            }
//            if (loadedData != null && loadedData is WorldSaveObject worldSaveData)
//            {
//                if (worldObject != null)
//                {
//                    Destroy(worldObject);
//                }

//                worldObject = Instantiate(worldPrefab, transform);
//                var saveDataObject = worldObject.GetComponent<ISaveable<WorldSaveObject>>();
//                saveDataObject.SetupFromSaveObject(worldSaveData);
//            }
//            SaveSystemHooks.TriggerPostLoad();
//        }

//        private void LoadECSWorld()
//        {
//            var entityDataPath = SerializationManager.GetSavePath(ENTITY_COMPONENTS_DATA, SaveContext.instance.saveName);
//            if (!File.Exists(entityDataPath))
//            {
//                // NO Ecs data, coming from a map gen
//                // TODO: generate ECS data as part of mapgen
//                return;
//            }
//            var targetWorld = World.DefaultGameObjectInjectionWorld;
//            var targetManager = targetWorld.EntityManager;
//            targetManager.CompleteAllJobs();
//            using (var world = new World("deserialize"))
//            using (var entityDataReader = new Unity.Entities.Serialization.StreamBinaryReader(entityDataPath))
//            {
//                var sourceManager = world.EntityManager;
//                var assetData = SerializationManager.Load(SHARED_OBJECT_DATA, SaveContext.instance.saveName) as SavedAssetArray;
//                var objectAssetData = assetData.GetObjectAssetData();
//                sourceManager.PrepareForDeserialize();

//                var deserializeTransaction = sourceManager.BeginExclusiveEntityTransaction();
//                try
//                {
//                    Unity.Entities.Serialization.SerializeUtility.DeserializeWorld(deserializeTransaction, entityDataReader, objectAssetData);
//                }
//                finally
//                {
//                    sourceManager.EndExclusiveEntityTransaction();
//                }

//                targetManager.DestroyEntity(targetManager.UniversalQuery);
//                targetManager.MoveEntitiesFrom(sourceManager);
//                targetManager.CreateEntity(typeof(DeserializingFlagComponent));
//                var deserializeGroup = targetWorld.GetExistingSystem<PostDeserialzeSystemGroup>();
//                deserializeGroup.Enabled = true;
//            }
//        }

//        private void SaveECSWorld()
//        {
//            object[] sharedObjects;

//            var entityDataPath = SerializationManager.GetSavePath(ENTITY_COMPONENTS_DATA, SaveContext.instance.saveName);
//            using (var writer = new Unity.Entities.Serialization.StreamBinaryWriter(entityDataPath))
//            {
//                var world = World.DefaultGameObjectInjectionWorld;
//                var manager = world.EntityManager;
//                Unity.Entities.Serialization.SerializeUtility.SerializeWorld(manager, writer, out sharedObjects);
//            }
//            // I think because we are not in Project Tiny, this array will always be UnityEngine.Object[]
//            var savedAssets = new SavedAssetArray(sharedObjects as Object[]);
//            SerializationManager.Save(SHARED_OBJECT_DATA, SaveContext.instance.saveName, savedAssets);
//        }

//        public static void ClearECSWorldSaveFiles()
//        {
//            SerializationManager.DeleteAll(SaveContext.instance.saveName, ENTITY_COMPONENTS_DATA, SHARED_OBJECT_DATA);
//        }
//    }

//    [System.Serializable]
//    class SavedAssetArray
//    {
//        private SavedAssetReference[] assetReferences;
//        public SavedAssetArray(Object[] objectData)
//        {
//            assetReferences = new SavedAssetReference[objectData.Length];
//            for (int i = 0; i < objectData.Length; i++)
//            {
//                assetReferences[i] = SavedAssetReference.FromAsset(objectData[i]);
//            }
//        }

//        public Object[] GetObjectAssetData()
//        {
//            var objects = new Object[assetReferences.Length];
//            for (int i = 0; i < objects.Length; i++)
//            {
//                var assReference = assetReferences[i];
//                objects[i] = assReference.ToAsset();
//            }
//            return objects;
//        }
//    }

//    [System.Serializable]
//    class SavedAssetReference
//    {
//        public string path;
//        public System.Type assetType;

//        public static SavedAssetReference FromAsset(Object asset)
//        {
//            return new SavedAssetReference
//            {
//                assetType = asset.GetType(),
//                path = AssetDatabase.GetAssetPath(asset)
//            };
//        }

//        public Object ToAsset()
//        {
//            return AssetDatabase.LoadAssetAtPath(path, assetType);
//        }
//    }
//}
