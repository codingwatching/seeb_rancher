using System;
using UnityEngine;

namespace Assets.Scripts.Utilities.Core
{
    public abstract class GenericSelector<T> : ScriptableObject
    {
        public abstract T GetCurrentValue(VariableInstantiator instancer);
        public abstract IObservable<T> ValueChanges(VariableInstantiator instancer);
    }
}
