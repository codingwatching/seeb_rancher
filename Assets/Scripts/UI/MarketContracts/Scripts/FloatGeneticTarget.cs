using Dman.ObjectSets;
using Genetics;
using System.Runtime.Serialization;

namespace Assets.Scripts.UI.MarketContracts
{
    [System.Serializable]
    public class FloatGeneticTarget : ISerializable
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
            minValue = info.GetInt32("minValue");
            maxValue = info.GetInt32("maxValue");
            var savedReference = info.GetValue("driverReference", typeof(IDableSavedReference)) as IDableSavedReference;
            targetDriver = savedReference?.GetObject<GeneticDriver>() as GeneticDriver<float>;
            if (targetDriver == null)
            {
                throw new SerializationException($"Could not deserialize a value for ${nameof(targetDriver)}");
            }
        }
    }
}