using Assets.Scripts.Plants;
using Assets.Scripts.UI.MarketContracts;
using System.Linq;
using UnityEngine;

namespace Dman.NarrativeSystem.ConversationTriggers
{
    [CreateAssetMenu(fileName = "PollinatingPlantExistsTrigger", menuName = "Narrative/Triggers/PollinatingPlantExistsTrigger", order = 2)]
    public class PollinatingPlantExistsTrigger : ConversationTrigger
    {
        public override bool ShouldTrigger(GameNarrative narrative)
        {
            var allPlants = GameObject.FindObjectsOfType<PlantContainer>();
            return allPlants.Any(x => x.CanPollinate());
        }
    }
}