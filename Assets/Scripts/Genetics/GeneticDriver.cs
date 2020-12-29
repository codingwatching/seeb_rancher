using UnityEngine;

namespace Genetics
{
    public abstract class GeneticDriver : ScriptableObject
    {
        public string DriverName;
    }
    public abstract class GeneticDriver<T> : GeneticDriver
    {
    }
}