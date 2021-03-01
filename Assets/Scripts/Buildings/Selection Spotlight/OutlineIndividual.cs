using UnityEngine;
using UnityFx.Outline;

namespace Assets.Scripts.Buildings.Selection_Spotlight
{
    [ExecuteInEditMode]
    public class OutlineIndividual : MonoBehaviour
    {
        public OutlineLayerCollection layerCollection;
        public int outlineLayerIndex;

        private OutlineLayer oldLayer;

        private void Start()
        {
            this.ReAddSelf();
        }

        private void OnValidate()
        {
            this.ReAddSelf();
        }

        private void ReAddSelf()
        {
            if (oldLayer != null)
            {
                oldLayer.Remove(gameObject);
            }
            oldLayer = layerCollection[outlineLayerIndex];
            oldLayer.Add(gameObject);
        }

        private void OnDestroy()
        {
            oldLayer.Remove(gameObject);
        }
    }
}
