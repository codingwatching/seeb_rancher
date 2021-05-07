using Assets.Scripts.DataModels;
using Dman.LSystem.SystemRuntime.DOTSRenderer;
using Dman.LSystem.SystemRuntime.ThreadBouncer;
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

        public float phaseFractionTillSprout = 1f;
        public float stepsPerPhase = 3f;

        public char flowerCharacter = 'C';
        public char seedBearingCharacter = 'D';

        public FloatGeneticDriverToLSystemParameter[] geneticModifiers;

        public override PlantState GenerateBaseStateAndHookTo()
        {
            return new LSystemPlantState(
                lSystem,
                phaseFractionTillSprout);
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

            var targetTurtle = targetContainer.GetComponentInChildren<TurtleInterpreterBehavior>();
            if(targetTurtle == null)
            {
                targetTurtle = Instantiate(turtleInterpretorPrefab, targetContainer);
            }


            systemState.CompileSystemIfNotCached(
                () => CompileToGlobalParameters(geneticDrivers),
                lSystem);
            systemState.steppingHandle.runtimeParameters.SetParameter("hasAnther", pollination.HasAnther ? 1 : 0);
            systemState.steppingHandle.runtimeParameters.SetParameter("isPollinated", pollination.IsPollinated ? 1 : 0);

            var targetSteps = Mathf.FloorToInt(systemState.growth * stepsPerPhase);
            if (targetSteps > systemState.steppingHandle.totalSteps)
            {
                systemState.StepStateUpToSteps(targetSteps);
            }
            else
            {
                systemState.RepeatLastSystemStep();
            }


            var completable = targetTurtle.InterpretSymbols(systemState.steppingHandle.currentState.currentSymbols);
            CompletableExecutor.Instance.RegisterCompletable(completable);
            var lastAngles = targetTurtle.transform.parent.localEulerAngles;
            lastAngles.y = systemState.randomRotationAmount;
            targetTurtle.transform.parent.localEulerAngles = lastAngles;
        }

        public Dictionary<string, string> CompileToGlobalParameters(CompiledGeneticDrivers geneticDrivers)
        {
            return geneticModifiers
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
        }

        public override bool HasFlowers(PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                return false;
            }
            return systemState.steppingHandle.currentState.currentSymbols.Data.symbols.Contains((int)flowerCharacter);
        }
        public override bool CanHarvest(PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                return false;
            }
            // if the plant is done growing always allow harvesting to avoid letting plants with no fruit hang around
            return !systemState.steppingHandle.lastUpdateChanged || systemState.steppingHandle.currentState.currentSymbols.Data.symbols.Contains((int)seedBearingCharacter);
        }

        public override bool IsMature(PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                return false;
            }
            return !systemState.steppingHandle.lastUpdateChanged;
        }

        protected override int GetHarvestedSeedNumber(PlantState currentState)
        {
            if (!(currentState is LSystemPlantState systemState))
            {
                return 0;
            }
            return systemState.steppingHandle.currentState.currentSymbols.Data.symbols.Sum(symbol => symbol == seedBearingCharacter ? 1 : 0);
        }
        public override IEnumerable<Seed> SimulateGrowthToHarvest(Seed seed)
        {
            var geneticDrivers = genome.CompileGenome(seed.genes);
            using var tempState = GenerateBaseStateAndHookTo() as LSystemPlantState;

            tempState.CompileSystemIfNotCached(
                () => CompileToGlobalParameters(geneticDrivers),
                lSystem);
            // when simulating growth, no anthers are clipped. and it is always pollinated.
            tempState.steppingHandle.runtimeParameters.SetParameter("hasAnther", 1);
            tempState.steppingHandle.runtimeParameters.SetParameter("isPollinated", 1);

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
