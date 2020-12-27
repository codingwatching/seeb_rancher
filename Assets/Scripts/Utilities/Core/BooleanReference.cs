using System;

namespace Assets.Scripts.Utilities.Core
{
    [Serializable]
    public class BooleanReference : GenericReference<bool>
    {
        public BooleanReference(bool value) : base(value)
        {
        }

        public override GenericVariable<bool> GetFromInstancer(VariableInstantiator Instancer, string NamePath)
        {
            return Instancer.GetBooleanValue(NamePath);
        }
    }
}
