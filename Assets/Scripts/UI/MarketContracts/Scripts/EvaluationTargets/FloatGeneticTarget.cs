using Dman.ObjectSets;
using Genetics;
using Genetics.GeneticDrivers;
using System.Runtime.Serialization;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts.EvaluationTargets
{

    [System.Serializable] // unity inspector
    public class FloatGeneticTargetGenerator
    {
        public FloatGeneticDriver driver;
        [Header("Extremes which the range will be confined inside")]
        public float absoluteMin;
        public float absoluteMax;
        [Header("Range for the width of the driver valid range")]
        public float rangeMin;
        public float rangeMax;
        public FloatGeneticTarget GenerateTarget()
        {
            var range = UnityEngine.Random.Range(rangeMin, rangeMax);
            var minValue = UnityEngine.Random.Range(0f, absoluteMax - absoluteMin - range);
            return new FloatGeneticTarget
            {
                minValue = Mathf.Round(minValue * 10) / 10f,
                maxValue = Mathf.Round((minValue + range) * 10) / 10f,
                targetDriver = driver
            };
        }
    }

    [System.Serializable]// odin and unity inspector
    public class FloatGeneticTarget : ISerializable, IContractTarget
    {
        public GeneticDriver<float> targetDriver;
        public float minValue;
        public float maxValue;

        public FloatGeneticTarget()
        {

        }

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