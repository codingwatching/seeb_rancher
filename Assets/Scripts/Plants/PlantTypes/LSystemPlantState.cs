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
        //cached stuff
        [System.NonSerialized()]
        public LSystemSteppingHandle steppingHandle;

        public float randomRotationAmount;

        public LSystemPlantState(
            LSystemObject lSystem,
            float initialGrowth) : base(initialGrowth)
        {
            // used to generate properties for the plant not related to the L-system. such as random rotation
            // Is a duplicate, parallel, random generator to the one used by the l-system
            var ephimeralRandoms = new System.Random(randomSeed);
            randomRotationAmount = (float)(ephimeralRandoms.NextDouble() * 360);

            this.steppingHandle = new LSystemSteppingHandle(lSystem, false);
        }

        public override void AfterDeserialized()
        {
            base.AfterDeserialized();
        }

        /// <summary>
        /// function used to do first-time setup on the state. Should handle cases right after 
        ///     state was deserialized
        /// </summary>
        /// <param name="globalParameterGenerator"></param>
        public void CompileSystemIfNotCached(
            Func<Dictionary<string, string>> globalParameterGenerator,
            LSystemObject lSystem)
        {
            if(steppingHandle == null)
            {
                this.steppingHandle = new LSystemSteppingHandle(lSystem, false);
            }
            if (steppingHandle.HasValidSystem())
            {
                return;
            }
            steppingHandle.RecompileLSystem(globalParameterGenerator());
        }

        public void StepUntilFirstNoChange()
        {
            do
            {
                steppingHandle.StepSystemImmediate();
            } while (steppingHandle.lastUpdateChanged);
        }

        public void StepStateUpToSteps(int targetSteps)
        {
            // TODO: interleave with other steps
            if (steppingHandle.totalSteps >= targetSteps)
            {
                return;
            }
            while (steppingHandle.totalSteps < targetSteps)
            {
                steppingHandle.StepSystemImmediate();
            }
        }
        public void RepeatLastSystemStep()
        {
            steppingHandle.RepeatLastStepImmediate();
        }
        public override JobHandle Dispose(JobHandle inputDeps)
        {
            steppingHandle.Dispose();
            return inputDeps;
        }

        public override void Dispose()
        {
            steppingHandle.Dispose();
        }
    }
}
