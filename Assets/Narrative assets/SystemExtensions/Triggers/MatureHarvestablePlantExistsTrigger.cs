using Assets.Scripts.Plants;
using System.Linq;
using UnityEngine;

namespace Dman.NarrativeSystem.ConversationTriggers
{
    [CreateAssetMenu(fileName = "MatureHarvestablePlantExistsTrigger", menuName = "Narrative/Triggers/MatureHarvestablePlantExistsTrigger", order = 3)]
    public class MatureHarvestablePlantExistsTrigger : ConversationTrigger
    {
        public override bool ShouldTrigger(GameNarrative narrative)
        {
            var allPlants = GameObject.FindObjectsOfType<PlantContainer>();
            return allPlants.Any(x => x.IsMatureAndHasSeeds());
        }
    }
}