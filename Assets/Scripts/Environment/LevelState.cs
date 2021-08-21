using Dman.ReactiveVariables;

namespace Environment
{
    [System.Serializable]
    public class LevelState
    {
        public IntReference currentWave;
        public FloatReference money;

        public void AdvanceWave()
        {
            currentWave.SetValue(currentWave.CurrentValue + 1);
        }
    }
}