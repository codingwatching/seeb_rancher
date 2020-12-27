using System;

namespace Assets.Scripts.Utilities.Core
{
    [Serializable]
    public class FloatReference : GenericReference<float>
    {
        public FloatReference(float value) : base(value)
        {
        }

        public override GenericVariable<float> GetFromInstancer(VariableInstantiator Instancer, string NamePath)
        {
            return Instancer.GetFloatValue(NamePath);
        }
    }
}
