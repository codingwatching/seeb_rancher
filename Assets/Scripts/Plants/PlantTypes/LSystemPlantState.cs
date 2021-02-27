using Dman.LSystem;
using Dman.LSystem.UnityObjects;
using Genetics.GeneticDrivers;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [System.Serializable]
    public class LSystemPlantState : PlantState
    {
        [System.NonSerialized()]
        public DefaultLSystemState lSystemState;
        [System.NonSerialized()]
        public int totalSystemSteps;
        [System.NonSerialized()]
        public bool lastStepChanged;

        //cached stuff
        [System.NonSerialized()]
        private LSystem<double> compiledSystem;
        [System.NonSerialized()]
        public ArrayParameterRepresenation<double> runtimeParameters;
        [System.NonSerialized()]
        private DefaultLSystemState lastState;

        public float randomRotationAmount;
        private string axiom;

        public LSystemPlantState(string axiom, float growth) : base(growth)
        {
            // used to generate properties for the plant not related to the L-system. such as random rotation
            // Is a duplicate, parallel, random generator to the one used by the l-system
            var ephimeralRandoms = new System.Random(randomSeed);
            totalSystemSteps = 0;
            randomRotationAmount = (float)(ephimeralRandoms.NextDouble() * 360);

            this.axiom = axiom;
            lSystemState = new DefaultLSystemState(this.axiom, randomSeed);
        }

        public override void AfterDeserialized()
        {
            base.AfterDeserialized();
            lSystemState = new DefaultLSystemState(axiom, randomSeed);
            totalSystemSteps = 0;
        }

        public void CompileSystemIfNotCached(
            FloatGeneticDriverToLSystemParameter[] geneticModifiers,
            CompiledGeneticDrivers geneticDrivers,
            LSystemObject system)
        {
            if (compiledSystem != null)
            {
                return;
            }
            Debug.Log("compiling System");
            var geneticModifiedParameters = geneticModifiers
                .Select(x =>
                {
                    if (geneticDrivers.TryGetGeneticData(x.geneticDriver, out var driverValue))
                    {
                        return new { x.lSystemDefineDirectiveName, driverValue };
                    }
                    return null;
                })
                .Where(x => x != null)
                .ToDictionary(x => x.lSystemDefineDirectiveName, x => x.driverValue.ToString());
            compiledSystem = system.CompileWithParameters(geneticModifiedParameters);

            runtimeParameters = system.GetRuntimeParameters();
        }

        public void StepStateUpToSteps(int targetSteps)
        {
            var ruintimeParamValues = runtimeParameters.GetCurrentParameters();
            if (totalSystemSteps >= targetSteps)
            {
                return;
            }
            while (totalSystemSteps < targetSteps)
            {
                totalSystemSteps++;

                if (totalSystemSteps >= targetSteps)
                {
                    compiledSystem.StepSystem(lSystemState, ruintimeParamValues);
                }
                else
                {
                    // this is the Last step in this series
                    // use Clone Constructor to prevent sharing array pointers. necessary to check if a change occured, and to restore back to this state later.
                    lastState = new DefaultLSystemState(lSystemState);
                    compiledSystem.StepSystem(lSystemState, ruintimeParamValues);
                    lastStepChanged = !lastState.currentSymbols.Equals(lSystemState.currentSymbols);
                }
            }
        }
        public void RepeatLastSystemStep()
        {
            if (lastState == null)
            {
                return;
            }
            var ruintimeParamValues = runtimeParameters.GetCurrentParameters();

            lSystemState = lastState;
            lastState = new DefaultLSystemState(lSystemState); // clone again to make sure the last state stays the same.
            compiledSystem.StepSystem(lSystemState, ruintimeParamValues);
        }
    }
}
