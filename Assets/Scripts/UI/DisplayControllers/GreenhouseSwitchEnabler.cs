using Dman.ReactiveVariables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace UI.DisplayControllers
{
    public class GreenhouseSwitchEnabler : MonoBehaviour
    {
        public BooleanReference areEnemiesClear;
        public IntReference currentWave;

        public BooleanReference canSwitch;

        [Tooltip("How many waves between when the option to go to the greenhouse is enabled")]
        public int wavesPerGreenhouseTrigger = 5;

        // Start is called before the first frame update
        void Start()
        {
            currentWave.ValueChanges
                .AsUnitObservable()
                .Merge(areEnemiesClear.ValueChanges.AsUnitObservable())
                .TakeUntilDestroy(this)
                .Subscribe((_) =>
                {
                    var nextValue = areEnemiesClear.CurrentValue && ((currentWave.CurrentValue % wavesPerGreenhouseTrigger) == 0);
                    canSwitch.SetValue(nextValue);
                })
                .AddTo(this);

        }
    }
}
