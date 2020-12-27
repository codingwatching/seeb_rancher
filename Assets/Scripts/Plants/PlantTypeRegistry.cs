using Assets.Scripts.Utilities;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [CreateAssetMenu(fileName = "PlantTypeRegistry", menuName = "Greenhouse/PlantTypeRegistry", order = 1)]
    public class PlantTypeRegistry : UniqueObjectRegistryWithAccess<PlantType>
    {
    }
}
