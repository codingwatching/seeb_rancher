using Dman.ObjectSets;
using UnityEngine;

namespace Dman.Tiling.TileSets
{
    [CreateAssetMenu(fileName = "TileTypeRegistry", menuName = "Greenhouse/TileTypeRegistry", order = 3)]
    public class TileTypeRegistry : UniqueObjectRegistryWithAccess<TileType>
    {
    }
}
