using Genetics.GeneticDrivers;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Plants
{
    [CreateAssetMenu(fileName = "GeneticMaterialSwitch", menuName = "Genetics/Effectors/MaterialSwitch", order = 10)]
    public class GeneticMaterialSwitch : GeneticDrivenModifier
    {
        public BooleanGeneticDriver switchDriver;
        public Material materialWhenTrue;
        public Material materialWhenFalse;

        public override void ModifyObject(GameObject target, CompiledGeneticDrivers geneticDrivers)
        {
            if (!geneticDrivers.TryGetGeneticData(switchDriver, out var switchValue))
            {
                Debug.LogError($"No data found for driver {switchDriver} when evaluating switch {this}");
                return;
            }
            var sourceMaterial = switchValue ? materialWhenFalse : materialWhenTrue;
            var targetMaterial = switchValue ? materialWhenTrue : materialWhenFalse;
            var sharedMaterialList = new List<Material>();
            foreach (var renderer in target.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.GetSharedMaterials(sharedMaterialList);
                var index = sharedMaterialList.FindIndex(x => x == sourceMaterial);
                if (index >= 0)
                {
                    sharedMaterialList[index] = targetMaterial;
                    renderer.sharedMaterials = sharedMaterialList.ToArray();
                }
                sharedMaterialList.Clear();
            }
        }
    }
}
