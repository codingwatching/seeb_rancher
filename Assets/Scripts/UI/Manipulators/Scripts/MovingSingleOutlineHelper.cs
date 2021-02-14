using UnityEngine;
using UnityFx.Outline;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    public class MovingSingleOutlineHelper
    {
        private GameObject lastOutlined;
        private OutlineLayerCollection layerCollection;
        private int layerIndex;

        public MovingSingleOutlineHelper(OutlineLayerCollection layerCollection, int targetOutlineLayer = 0)
        {
            this.layerCollection = layerCollection;
            layerIndex = targetOutlineLayer;
        }
        public void UpdateOutlineObject(GameObject nextOutlined)
        {
            if (nextOutlined == lastOutlined)
            {
                return;
            }
            var outlineLayer = layerCollection[layerIndex];
            ClearOutlinedObject();
            if (nextOutlined != null)
            {
                Debug.Log((object)("outlining " + nextOutlined.name));
                outlineLayer.Add((GameObject)nextOutlined);
                lastOutlined = nextOutlined;
            }
        }

        public void ClearOutlinedObject()
        {
            if (lastOutlined != null)
            {
                var outlineLayer = layerCollection[layerIndex];
                if (outlineLayer.Contains(lastOutlined))
                {
                    Debug.Log("removing " + lastOutlined.name);
                    outlineLayer.Remove(lastOutlined);
                }
            }
            lastOutlined = null;
        }
    }
}
