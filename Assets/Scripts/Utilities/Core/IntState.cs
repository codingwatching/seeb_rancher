﻿using UnityEngine;

namespace Assets.Scripts.Utilities.Core
{
    [CreateAssetMenu(fileName = "IntState", menuName = "State/IntState", order = 2)]
    public class IntState : GenericState<int>
    {
        public int defaultState;
        public override GenericVariable<int> GenerateNewVariable()
        {
            var instanced = CreateInstance<IntVariable>();
            instanced.SetValue(defaultState);
            return instanced;
        }

        public override object GetSaveObjectFromVariable(GenericVariable<int> variable)
        {
            return variable.CurrentValue;
        }

        public override void SetSaveObjectIntoVariable(GenericVariable<int> variable, object savedValue)
        {
            var saveValue = (int)savedValue;
            variable.SetValue(saveValue);
        }
    }
}
