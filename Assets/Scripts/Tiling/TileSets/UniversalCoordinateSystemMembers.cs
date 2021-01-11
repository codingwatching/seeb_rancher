using Assets.Tiling;
using Dman.SceneSaveSystem;
using Dman.Utilities;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Assets.Scripts.Tiling.TileSets
{
    [System.Serializable]
    public class TileMembersSaveObject
    {
        public UniversalCoordinate[] tileKeys;
        public int[] tileValues;

        public static TileMembersSaveObject FromHashMap(NativeHashMap<UniversalCoordinate, int> tileTypes)
        {
            using (var keyValues = tileTypes.GetKeyValueArrays(Allocator.TempJob))
            {
                return new TileMembersSaveObject
                {
                    tileKeys = keyValues.Keys.ToArray(),
                    tileValues = keyValues.Values.ToArray(),
                };
            }
        }
        public static TileMembersSaveObject FromDictionary(IDictionary<UniversalCoordinate, int> tileTypes)
        {
            var newSaveObject = new TileMembersSaveObject
            {
                tileKeys = tileTypes.Keys.ToArray()
            };
            newSaveObject.tileValues = newSaveObject.tileKeys.Select(x => tileTypes[x]).ToArray();
            return newSaveObject;
        }
        public static TileMembersSaveObject FromTileTypeDictionary(IDictionary<UniversalCoordinate, TileType> tileTypes)
        {
            var newSaveObject = new TileMembersSaveObject
            {
                tileKeys = tileTypes.Keys.ToArray()
            };
            newSaveObject.tileValues = newSaveObject.tileKeys.Select(x => tileTypes[x].myId).ToArray();
            return newSaveObject;
        }
    }

    public class UniversalCoordinateSystemMembers : MonoBehaviour, ISaveableData
    {
        public TileTypeRegistry tileDefinitions;

        private Dictionary<UniversalCoordinate, int> tileTypes = new Dictionary<UniversalCoordinate, int>();
        [SerializeField]
        private TileMembersSaveObject tileMembersToSaveInEditMode;
        private Dictionary<int, TileType> _tileTypesById;
        private Dictionary<int, TileType> TileTypesById
        {
            get
            {
                if (_tileTypesById == null)
                {
                    _tileTypesById = new Dictionary<int, TileType>();
                    foreach (var tileType in tileDefinitions.allObjects)
                    {
                        _tileTypesById[tileType.myId] = tileType;
                    }
                }
                return _tileTypesById;
            }
        }

        public void SetTileDataInEditMode(TileMembersSaveObject tileData)
        {
            tileMembersToSaveInEditMode = tileData;
            OverwriteTileTypes(tileData);
        }

        private void Awake()
        {
            OverwriteTileTypes(tileMembersToSaveInEditMode);
        }

        private void OnDestroy()
        {
        }

        public void OverwriteTileTypes(TileMembersSaveObject saveObject)
        {
            tileTypes = new Dictionary<UniversalCoordinate, int>(saveObject.tileKeys.Length);
            for (int i = 0; i < saveObject.tileKeys.Length; i++)
            {
                tileTypes[saveObject.tileKeys[i]] = saveObject.tileValues[i];
            }
        }

        public TileType GetTileType(UniversalCoordinate coordinate)
        {
            if (tileTypes.TryGetValue(coordinate, out var value))
            {
                return TileTypesById[value];
            }
            return null;
        }

        public ReadWriteJobHandleProtector readWriteLock = new ReadWriteJobHandleProtector();

        public string UniqueSaveIdentifier => "CoordinateTiles";

        public object GetSaveObject()
        {
            return TileMembersSaveObject.FromDictionary(tileTypes);
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is TileMembersSaveObject saveObject)
            {
                OverwriteTileTypes(saveObject);
            }
        }

        public ISaveableData[] GetDependencies()
        {
            return new ISaveableData[0];
        }
    }
}
