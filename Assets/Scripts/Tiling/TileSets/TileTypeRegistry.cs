using Dman.ObjectSets;
using UnityEngine;

namespace Assets.Scripts.Tiling.TileSets
{
    [CreateAssetMenu(fileName = "TileTypeRegistry", menuName = "Greenhouse/TileTypeRegistry", order = 3)]
    public class TileTypeRegistry : UniqueObjectRegistryWithAccess<TileType>
    {
    }
}
