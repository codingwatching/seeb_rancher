using Assets.Scripts.DataModels;
using Dman.LSystem.SystemRuntime.DOTSRenderer;
using Dman.LSystem.SystemRuntime.LSystemEvaluator;
using Dman.LSystem.SystemRuntime.ThreadBouncer;
using Dman.LSystem.UnityObjects;
using Dman.SceneSaveSystem;
using Genetics.GeneticDrivers;
using System;
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
        public GameObject lSystemPlantPrefab;
        public LSystemObject lSystem;

        public float phaseFractionTillSprout = 1f;
        public int stepsPerPhase = 3;

        public char flowerCharacter = 'C';
        public char seedBearingCharacter = 'D';

        public FloatGeneticDriverToLSystemParameter[] geneticModifiers;

        public override GameObject SpawnNewPlant(Vector3 seedlingPosition, Seed plantedSeed)
        {
            var plantParent = GameObject.FindObjectsOfType<SaveablePrefabParent>().Where(x => x.prefabParentName == "Global Plant Parent").FirstOrDefault();
            if(plantParent == null)
            {
                Debug.LogError("No plant parent found. create a SaveablePrefabParent with a prefabParentName of 'Global Plant Parent'");
            }

            var newPlant = GameObject.Instantiate(lSystemPlantPrefab,seedlingPosition, Quaternion.identity, plantParent.transform);
            var plantController = newPlant.GetComponentInChildren<PlantedLSystem>();
            plantController.InitializeWithSeed(plantedSeed);

            return newPlant;
        }

        public override void ConfigureLSystemWithSeedling(
            LSystemBehavior lSystemContainer,
            CompiledGeneticDrivers geneticDrivers,
            PollinationState pollination)
        {
            // the reset will draw the global parameters from the planted L -system via ILSystemCompileTimeParameterGenerator
            lSystemContainer.SetSystem(lSystem);
            // lSystemContainer.ResetState();

            var steppingHandle = lSystemContainer.steppingHandle;

            steppingHandle.runtimeParameters.SetParameter("hasAnther", pollination.HasAnther ? 1 : 0);
            steppingHandle.runtimeParameters.SetParameter("isPollinated", pollination.IsPollinated ? 1 : 0);

            // step the number of steps required for a seebling to show up immediately
            var seeblingSteps = phaseFractionTillSprout * stepsPerPhase;
            while (steppingHandle.totalSteps < seeblingSteps) {
                steppingHandle.StepSystemImmediate();
            }

            // TODO: the turtle -should- be uneccesary, because it should be hooked into the l-system behaviors updates
            //var completable = targetTurtle.InterpretSymbols(systemState.steppingHandle.currentState.currentSymbols);
            //CompletableExecutor.Instance.RegisterCompletable(completable);

            // TODO: the angles are not saved off anywhere else. the save system should serialize the full transform which places the
            //  l-system in the world somewhere
            var lastAngles = lSystemContainer.transform.parent.localEulerAngles;
            lastAngles.y = UnityEngine.Random.Range(0f, 360f);
            lSystemContainer.transform.parent.localEulerAngles = lastAngles;
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

        public override bool HasFlowers(LSystemBehavior systemManager)
        {
            return systemManager.steppingHandle.currentState.currentSymbols.Data.symbols.Contains((int)flowerCharacter);
        }
        public override bool CanHarvest(LSystemBehavior systemManager)
        {
            // if the plant is done growing always allow harvesting to avoid letting plants with no fruit hang around
            return !systemManager.steppingHandle.lastUpdateChanged || systemManager.steppingHandle.currentState.currentSymbols.Data.symbols.Contains((int)seedBearingCharacter);
        }

        public override bool IsMature(LSystemBehavior systemManager)
        {
            return !systemManager.steppingHandle.lastUpdateChanged;
        }

        protected override int GetHarvestedSeedNumber(LSystemBehavior systemManager)
        {
            return systemManager?.steppingHandle.currentState.currentSymbols.Data.symbols.Sum(symbol => symbol == seedBearingCharacter ? 1 : 0) ?? 0;
        }
        public override IEnumerable<Seed> SimulateGrowthToHarvest(Seed seed)
        {
            // TODO: rework plant growth and breeding simulation
            //  will need to incorporate turtles and sunlight
            //  and basically create an entire virtual space in which the growth 
            //  and breeding takes place in an automated way

            throw new NotImplementedException();
        }
    }
}