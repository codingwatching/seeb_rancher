using Dman.Utilities;
using Simulation.Plants;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dman.NarrativeSystem.ConversationTriggers
{
    [CreateAssetMenu(fileName = "SceneIsOpenTrigger", menuName = "Narrative/Triggers/SceneIsOpenTrigger", order = 2)]
    public class SceneIsOpenTrigger : ConversationTrigger
    {
        public SceneReference targetScene;

        public override bool ShouldTrigger(GameNarrative narrative)
        {
            var currentPath = SceneManager.GetActiveScene().path;
            return targetScene.scenePath == currentPath;
        }
    }
}