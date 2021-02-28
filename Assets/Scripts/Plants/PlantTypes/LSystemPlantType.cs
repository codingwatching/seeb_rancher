using Assets.Scripts.DataModels;
using Dman.LSystem.UnityObjects;
using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Plants
{

    [System.Serializable]
    public struct FloatGeneticDriverToLSystemParameter
    {
        public FloatGeneticDriver geneticDriver;
        public string lSystemDefineDirectiveName;
    }

    [CreateAssetMenu(fileName = "LSystemPlantType", menuName = "Greenhouse/LSystemPlantType", order = 1)]
    public class LSystemPlantType : BasePlantType
    {
        public TurtleInterpreterBehavior turtleInterpretorPrefab;
        public LSystemObject lSystem;

        public float stepsPerPhase = 3f;

        public char flowerCharacter = 'C';
        public char seedBearingCharacter = 'D';

        public FloatGeneticDriverToLSystemParameter[] geneticModifiers;

        public override PlantState GenerateBaseSate()
        {
            return new LSystemPlantState(lSystem.axiom, 0);
        }

        /// <summary>
        /// In this implemntation, the growth property is used to directly reflect how many phases the plant has aged through.
        ///     during rendering this is converted into L-system steps
        /// This is done because there is no known end-time for the L-system plant, the harvestability and pollination state
        ///     is derived only from the state of the L-system instead of a special value of the growth variable
        /// </summary>
        /// <param name="phaseDiff"></param>
        /// <param name="currentState"></param>
        public override void AddGrowth(int phaseDiff, PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                Debug.LogError("invalid plant state type");
                return;
            }
            systemState.growth += phaseDiff;
        }

        public override void BuildPlantInto(
            Transform targetContainer,
            CompiledGeneticDrivers geneticDrivers,
            PlantState currentState,
            PollinationState pollination)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                Debug.LogError("invalid plant state type");
                return;
            }

            systemState.CompileSystemIfNotCached(geneticModifiers, geneticDrivers, lSystem);
            systemState.runtimeParameters.SetParameter("hasAnther", pollination.HasAnther ? 1 : 0);
            systemState.runtimeParameters.SetParameter("isPollinated", pollination.IsPollinated ? 1 : 0);

            var targetSteps = Mathf.FloorToInt(systemState.growth * stepsPerPhase);
            if (targetSteps > systemState.totalSystemSteps)
            {
                systemState.StepStateUpToSteps(targetSteps);
            }
            else
            {
                systemState.RepeatLastSystemStep();
            }

            var newPlantTarget = Instantiate(turtleInterpretorPrefab, targetContainer);

            newPlantTarget.InterpretSymbols(systemState.lSystemState.currentSymbols);
            var lastAngles = newPlantTarget.transform.parent.localEulerAngles;
            lastAngles.y = systemState.randomRotationAmount;
            newPlantTarget.transform.parent.localEulerAngles = lastAngles;
        }

        public override bool HasFlowers(PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                return false;
            }
            return systemState.lSystemState.currentSymbols.symbols.Contains((int)flowerCharacter);
        }
        public override bool CanHarvest(PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                return false;
            }
            // if the plant is done growing always allow harvesting to avoid letting plants with no fruit hang around
            return !systemState.lastStepChanged || systemState.lSystemState.currentSymbols.symbols.Contains((int)seedBearingCharacter);
        }
        protected override int GetHarvestedSeedNumber(PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                return 0;
            }
            return systemState.lSystemState.currentSymbols.symbols.Sum(symbol => symbol == seedBearingCharacter ? 1 : 0);
        }
        public override IEnumerable<Seed> SimulateGrowthToHarvest(Seed seed)
        {
            var geneticDrivers = genome.CompileGenome(seed.genes);
            var tempState = this.GenerateBaseSate() as LSystemPlantState;

            tempState.CompileSystemIfNotCached(geneticModifiers, geneticDrivers, lSystem);
            // when simulating growth, no anthers are clipped. and it is always pollinated.
            tempState.runtimeParameters.SetParameter("hasAnther", 1);
            tempState.runtimeParameters.SetParameter("isPollinated", 1);

            tempState.StepUntilFirstNoChange();

            var seedCount = GetHarvestedSeedNumber(tempState);
            // TODO: these seed counts are picked out of the blue. Consider running the l-system to get the seed counts
            //  for a more accurate simulation.
            //  more relevent if there are certain trait combinations that can sterilize a seed
            return SelfPollinateSeed(seed, seedCount);
        }
        IEnumerable<Seed> SelfPollinateSeed(Seed seed, int seedCopies)
        {
            for (int i = 0; i < seedCopies; i++)
            {
                yield return new Seed
                {
                    plantType = seed.plantType,
                    genes = new Genetics.Genome(seed.genes, seed.genes)
                };
            }
        }
    }
}
