using Assets.Scripts.Utilities;
using Assets.Tiling;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Assets.Scripts.Tiling.TileSets
{
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
            newSaveObject.tileValues = newSaveObject.tileKeys.Select(x => tileTypes[x].uniqueID).ToArray();
            return newSaveObject;
        }
    }

    public class UniversalCoordinateSystemMembers : MonoBehaviour
    {
        public TileTypeRegistry tileDefinitions;

        private Dictionary<UniversalCoordinate, int> tileTypes;
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
                        _tileTypesById[tileType.uniqueID] = tileType;
                    }
                }
                return _tileTypesById;
            }
        }

        private void OnDestroy()
        {
        }

        public TileType GetTileType(UniversalCoordinate coordinate)
        {
            if (tileTypes.TryGetValue(coordinate, out var value))
            {
                return TileTypesById[value];
            }
            return null;
        }

        public TileMembersSaveObject GetSaveObject()
        {
            return TileMembersSaveObject.FromDictionary(tileTypes);
        }

        public ReadWriteJobHandleProtector readWriteLock = new ReadWriteJobHandleProtector();

        public void SetupFromSaveObject(TileMembersSaveObject save)
        {
            tileTypes = new Dictionary<UniversalCoordinate, int>(save.tileKeys.Length);
            for (int i = 0; i < save.tileKeys.Length; i++)
            {
                tileTypes[save.tileKeys[i]] = save.tileValues[i];
            }
        }
    }
}
