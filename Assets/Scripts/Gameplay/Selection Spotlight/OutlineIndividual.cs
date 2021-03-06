using UnityEngine;
using UnityFx.Outline;

namespace Gameplay.Selection_Spotlight
{
    [ExecuteInEditMode]
    public class OutlineIndividual : MonoBehaviour
    {
        public OutlineLayerCollection layerCollection;
        public int outlineLayerIndex;

        private OutlineLayer oldLayer;

        private void Start()
        {
            ReAddSelf();
        }

        private void OnValidate()
        {
            ReAddSelf();
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
