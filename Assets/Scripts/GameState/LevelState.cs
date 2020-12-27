using Assets.Scripts.Utilities.Core;

namespace Assets.Scripts.GreenhouseLoader
{
    [System.Serializable]
    public class LevelState
    {
        public IntReference currentPhase;

        public void AdvancePhase()
        {
            currentPhase.SetValue(currentPhase.CurrentValue + 1);
        }
    }
}