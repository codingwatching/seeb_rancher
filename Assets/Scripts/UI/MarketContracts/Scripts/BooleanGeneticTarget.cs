using Dman.ObjectSets;
using Genetics;
using Genetics.GeneticDrivers;
using System.Runtime.Serialization;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts
{
    [System.Serializable]
    public class BooleanGeneticTarget : ISerializable
    {
        public BooleanGeneticDriver targetDriver;
        public bool targetValue;

        public BooleanGeneticTarget(BooleanGeneticDriver driver)
            : this(driver, Random.Range(0f, 1f) > .5f)
        {
        }

        public BooleanGeneticTarget(BooleanGeneticDriver driver, bool targetValue)
        {
            targetDriver = driver;
            this.targetValue = targetValue;
        }

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
