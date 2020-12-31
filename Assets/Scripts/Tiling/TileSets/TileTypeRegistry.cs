using Assets.Scripts.Utilities;
using Assets.Scripts.Utilities.ScriptableObjectRegistries;
using UnityEngine;

namespace Assets.Scripts.Tiling.TileSets
{
    [CreateAssetMenu(fileName = "TileTypeRegistry", menuName = "Greenhouse/TileTypeRegistry", order = 3)]
    public class TileTypeRegistry : UniqueObjectRegistryWithAccess<TileType>
    {
    }
}
