using Simulation.Plants.PlantData;
using Simulation.Plants.PlantTypes;
using UI.SeedInventory;
using UnityEngine;
using UnityEngine.Events;

namespace Dman.NarrativeSystem
{
    [CreateAssetMenu(fileName = "CreateSeebsPrompt", menuName = "Narrative/Prompts/CreateSeebsPrompt", order = 1)]
    public class CreateSeebsPrompt : Prompt
    {
        public UnityEvent onOpened;
        public UnityEvent onCompleted;

        [Header("seed gen params")]
        public BasePlantType plantType;
        public int seedNum;
        public string seedBucketDescription;


        public override void OpenPrompt(Conversation conversation)
        {
            onOpened?.Invoke();

            var newSeeds = new Seed[seedNum];
            for (int i = 0; i < newSeeds.Length; i++)
            {
                newSeeds[i] = plantType.GenerateRandomSeed();
            }
            var seedBucket = new SeedBucket(newSeeds);

            var seedReceiver = SeedInventoryController.Instance.CreateSeedStack(new SeedBucketUI
            {
                bucket = seedBucket,
                description = seedBucketDescription
            });
            if (seedReceiver == null)
            {
                throw new System.Exception("No available bucket to put seeds in, unhandled problem");
            }

            OpenPromptWithSetup(() =>
            {
                onCompleted?.Invoke();
                conversation.PromptClosed();
                Destroy(currentPrompt.gameObject);
            });
        }
    }
}
