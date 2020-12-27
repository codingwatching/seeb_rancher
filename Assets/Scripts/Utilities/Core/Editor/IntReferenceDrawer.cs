using Assets.Scripts.Utilities.Core;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Scripts.Core.Editor
{
    [CustomPropertyDrawer(typeof(IntReference))]
    public class IntReferenceDrawer : GenericReferenceDrawer
    {
        protected override List<string> GetValidNamePaths(VariableInstantiator instantiator)
        {
            throw new NotImplementedException();
        }
    }
}
