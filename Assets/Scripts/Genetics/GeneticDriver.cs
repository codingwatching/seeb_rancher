using Assets.Scripts.Utilities;
using Assets.Scripts.Utilities.ScriptableObjectRegistries;

namespace Genetics
{
    public abstract class GeneticDriver : IDableObject
    {
        public string DriverName;
    }
    public abstract class GeneticDriver<T> : GeneticDriver
    {
    }
}