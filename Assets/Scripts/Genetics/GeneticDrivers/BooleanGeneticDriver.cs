using UnityEngine;

namespace Genetics.GeneticDrivers
{
    [CreateAssetMenu(fileName = "BooleanDriver", menuName = "Genetics/BooleanDriver", order = 10)]
    public class BooleanGeneticDriver : GeneticDriver<bool>
    {
        [Tooltip("Used to describe the state of the driver in the UI")]
        public string outcomeWhenTrue;
        [Tooltip("Used to describe the state of the driver in the UI")]
        public string outcomeWhenFalse;
    }
}
