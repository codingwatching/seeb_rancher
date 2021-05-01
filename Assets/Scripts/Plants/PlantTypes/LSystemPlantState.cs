using Dman.LSystem;
using Dman.LSystem.UnityObjects;
using Genetics.GeneticDrivers;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [System.Serializable]
    public class LSystemPlantState : PlantState
    {
        [System.NonSerialized()]
        public LSystemState<float> lSystemState;
        [System.NonSerialized()]
        public int totalSystemSteps;
        [System.NonSerialized()]
        public bool lastStepChanged;

        //cached stuff
        [System.NonSerialized()]
        private LSystem compiledSystem;
        [System.NonSerialized()]
        public ArrayParameterRepresenation<float> runtimeParameters;
        [System.NonSerialized()]
        private LSystemState<float> lastState;

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
            Func<Dictionary<string, string>> globalParameterGenerator,
            LSystemObject system)
        {
            if (compiledSystem != null)
            {
                return;
            }
            Debug.Log("compiling System");
            compiledSystem = system.CompileWithParameters(globalParameterGenerator());

            runtimeParameters = system.GetRuntimeParameters();
        }

        public void StepUntilFirstNoChange()
        {
            var ruintimeParamValues = runtimeParameters.GetCurrentParameters();
            do
            {
                lastState?.currentSymbols.Dispose();
                lastState = lSystemState;
                lSystemState = compiledSystem.StepSystem(lSystemState, ruintimeParamValues, disposeOldSystem: false);
                lastStepChanged = !lastState.currentSymbols.Equals(lSystemState.currentSymbols);
            } while (lastStepChanged);
        }

        public void StepStateUpToSteps(int targetSteps)
        {
            // TODO: interleave with other steps
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
                    lSystemState = compiledSystem.StepSystem(lSystemState, ruintimeParamValues);
                }
                else
                {
                    // this is the Last step in this series
                    lastState?.currentSymbols.Dispose();
                    lastState = lSystemState;
                    lSystemState = compiledSystem.StepSystem(lSystemState, ruintimeParamValues, disposeOldSystem: false);
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

            lSystemState = compiledSystem.StepSystem(lastState, ruintimeParamValues);
        }
        public override JobHandle Dispose(JobHandle inputDeps)
        {
            if(lSystemState == null)
            {
                if(lastState == null)
                {
                    return inputDeps;
                }else
                {
                    return lastState.currentSymbols.Dispose(inputDeps);
                }
            }else
            {
                if (lastState == null)
                {
                    return lSystemState.currentSymbols.Dispose(inputDeps);
                }
                else
                {
                    return JobHandle.CombineDependencies(
                        lastState.currentSymbols.Dispose(inputDeps),
                        lSystemState.currentSymbols.Dispose(inputDeps)
                        );
                }
            }
        }

        public override void Dispose()
        {
            lastState.currentSymbols.Dispose();
            lSystemState.currentSymbols.Dispose();
        }
    }
}
