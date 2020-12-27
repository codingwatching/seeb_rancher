using Assets.Scripts.Utilities.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Assets.Scripts.Core.Editor
{
    [CustomPropertyDrawer(typeof(FloatReference))]
    public class FloatReferenceDrawer : GenericReferenceDrawer
    {
        protected override List<string> GetValidNamePaths(VariableInstantiator instantiator)
        {
            return instantiator.floatStateConfig.Select(x => x.IdentifierInInstantiator).ToList();
        }
    }
}
