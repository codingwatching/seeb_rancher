using Assets.Scripts.Plants;
using Dman.ReactiveVariables;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityFx.Outline;

namespace Dman.NarrativeSystem
{
    [CreateAssetMenu(fileName = "HarvestMaturePlantPrompt", menuName = "Narrative/Prompts/HarvestMaturePlantPrompt", order = 1)]
    public class HarvestMaturePlantPrompt : Prompt
    {
        public OutlineLayerCollection layerCollection;
        public int preClickOutlineIndex;

        private PlantedLSystem targetPlant;
        private Conversation currentParentConvo;
        public override void OpenPrompt(Conversation conversation)
        {
            if(targetPlant != null || currentParentConvo != null)
            {
                Debug.LogError("scriptable object prompt already in open state. could lead to unpredictable behavior.");
            }

            var allPlants = GameObject.FindObjectsOfType<PlantedLSystem>();
            targetPlant = allPlants.FirstOrDefault(x => x.IsMatureAndHasSeeds());
            if (targetPlant == null)
            {
                Debug.LogError("no harvestable plant found");
                conversation.PromptClosed();
                return;
            }
            var outlineTarget = targetPlant.gameObject;
            var outlineLayer = layerCollection[preClickOutlineIndex];
            outlineLayer.Add(outlineTarget);

            currentParentConvo = conversation;
            OpenPromptWithSetup();

            targetPlant.OnHarvested += PlantTargetHarvested;
        }

        private void PlantTargetHarvested()
        {
            var plant = targetPlant;
            var convo = currentParentConvo;
            targetPlant = null;
            currentParentConvo = null;

            plant.OnHarvested -= PlantTargetHarvested;
            var outlineLayer = layerCollection[preClickOutlineIndex];
            outlineLayer.Remove(plant.gameObject);
            convo.PromptClosed();
            Destroy(currentPrompt.gameObject);
        }
    }
}
