using Assets.Scripts.Utilities;
using Assets.Scripts.Utilities.ScriptableObjectRegistries;
using UnityEngine;

namespace Genetics
{
    [CreateAssetMenu(fileName = "GeneticDriverRegistry", menuName = "Genetics/GeneticDriverRegistry", order = 20)]
    public class GeneticDriverRegistry : UniqueObjectRegistryWithAccess<GeneticDriver>
    {
    }
}