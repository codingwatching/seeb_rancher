using Dman.ReactiveVariables;

namespace Assets.Scripts.GreenhouseLoader
{
    [System.Serializable]
    public class LevelState
    {
        public IntReference currentPhase;
        public FloatReference money;

        public void AdvancePhase()
        {
            currentPhase.SetValue(currentPhase.CurrentValue + 1);
        }
    }
}