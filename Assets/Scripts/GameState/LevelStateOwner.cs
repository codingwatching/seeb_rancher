using Assets.Scripts.Utilities.Core;
using UnityEngine;

namespace Assets.Scripts.GreenhouseLoader
{
    public class LevelStateOwner : MonoBehaviour
    {
        public LevelState levelState;
        public EventGroup phaseAdvanceTrigger;

        private void Awake()
        {
            phaseAdvanceTrigger.OnEvent += AdvancePhase;
        }

        private void AdvancePhase()
        {
            levelState.AdvancePhase();
        }
    }
}