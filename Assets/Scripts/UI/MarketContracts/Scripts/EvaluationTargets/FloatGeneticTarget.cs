using Dman.ObjectSets;
using Genetics;
using Genetics.GeneticDrivers;
using System.Runtime.Serialization;

namespace Assets.Scripts.UI.MarketContracts.EvaluationTargets
{
    [System.Serializable]// odin and unity inspector
    public class FloatGeneticTarget : ISerializable, IContractTarget
    {
        public GeneticDriver<float> targetDriver;
        public float minValue;
        public float maxValue;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("minValue", minValue);
            info.AddValue("maxValue", maxValue);
            info.AddValue("driverReference", new IDableSavedReference(targetDriver));
        }


        // The special constructor is used to deserialize values.
        private FloatGeneticTarget(SerializationInfo info, StreamingContext context)
        {
            minValue = info.GetSingle("minValue");
            maxValue = info.GetSingle("maxValue");
            var savedReference = info.GetValue("driverReference", typeof(IDableSavedReference)) as IDableSavedReference;
            targetDriver = savedReference?.GetObject<GeneticDriver>() as GeneticDriver<float>;
            if (targetDriver == null)
            {
                throw new SerializationException($"Could not deserialize a value for ${nameof(targetDriver)}");
            }
        }

        public bool Matches(CompiledGeneticDrivers geneticDrivers)
        {
            if (!geneticDrivers.TryGetGeneticData(targetDriver, out var floatValue))
            {
                return false;
            }
            if (floatValue < minValue || floatValue > maxValue)
            {
                return false;
            }
            return true;
        }

        public string GetDescriptionOfTarget()
        {
            return $"{targetDriver.DriverName} between {minValue} and {maxValue}";
        }
    }
}