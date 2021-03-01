using Assets.Scripts.Plants;
using Assets.Scripts.UI.MarketContracts;
using Dman.ReactiveVariables;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityFx.Outline;
using UniRx;

namespace Dman.NarrativeSystem
{
    [CreateAssetMenu(fileName = "SelectFloweringPlantPrompt", menuName = "Narrative/Prompts/SelectFloweringPlantPrompt", order = 1)]
    public class SelectFloweringPlantPrompt : Prompt
    {
        public OutlineLayerCollection layerCollection;
        public int preClickOutlineIndex;
        public GameObjectVariable selectedObjectVariable;

        public override void OpenPrompt(Conversation conversation)
        {
            var allPlants = GameObject.FindObjectsOfType<PlantContainer>();
            var targetPlant = allPlants.FirstOrDefault(x => x.CanPollinate());
            if(targetPlant == null)
            {
                Debug.LogError("no pollinating plant found");
                conversation.PromptClosed();
                return;
            }
            var outlineTarget = targetPlant.gameObject;
            var outlineLayer = layerCollection[preClickOutlineIndex];
            outlineLayer.Add(outlineTarget);

            OpenPromptWithSetup();

            selectedObjectVariable.Value
                .TakeUntilDestroy(currentPrompt.gameObject)
                .Subscribe(nextValue =>
                {
                    if(nextValue == outlineTarget)
                    {
                        outlineLayer.Remove(outlineTarget);
                        conversation.PromptClosed();
                        Destroy(currentPrompt.gameObject);
                    }
                }).AddTo(currentPrompt.gameObject);
        }
    }
}
