using Assets.Scripts.Utilities.ScriptableObjectRegistries;
using Genetics;
using Genetics.GeneticDrivers;
using System.Runtime.Serialization;

namespace Assets.Scripts.UI.MarketContracts
{
    [System.Serializable]
    public class BooleanGeneticTarget: ISerializable
    {
        public BooleanGeneticDriver targetDriver;
        public bool targetValue;
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("targetValue", targetValue);
            info.AddValue("driverReference", new IDableSavedReference(targetDriver));
        }
        // The special constructor is used to deserialize values.
        private BooleanGeneticTarget(SerializationInfo info, StreamingContext context)
        {
            targetValue = info.GetBoolean("targetValue");
            var savedReference = info.GetValue("driverReference", typeof(IDableSavedReference)) as IDableSavedReference;
            targetDriver = savedReference?.GetObject<GeneticDriver>() as BooleanGeneticDriver;
            if (targetDriver == null)
            {
                throw new SerializationException($"Could not deserialize a value for ${nameof(targetDriver)}");
            }
        }

        public string GetDescriptionOfTarget()
        {
            return targetValue ? targetDriver.outcomeWhenTrue : targetDriver.outcomeWhenFalse;
        }
    }
}
