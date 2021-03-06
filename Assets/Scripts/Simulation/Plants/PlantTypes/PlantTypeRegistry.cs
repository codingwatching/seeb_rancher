using Dman.ObjectSets;
using UnityEngine;

namespace Simulation.Plants.PlantTypes
{
    [CreateAssetMenu(fileName = "PlantTypeRegistry", menuName = "Greenhouse/PlantTypeRegistry", order = 1)]
    public class PlantTypeRegistry : UniqueObjectRegistryWithAccess<BasePlantType>
    {
    }
}
