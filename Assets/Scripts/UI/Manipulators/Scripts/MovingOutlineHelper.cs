using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityFx.Outline;

namespace UI.Manipulators
{
    public class MovingOutlineHelper
    {
        private ISet<GameObject> lastOutlined;
        private OutlineLayerCollection layerCollection;
        private int layerIndex;

        public MovingOutlineHelper(OutlineLayerCollection layerCollection, int targetOutlineLayer = 0)
        {
            lastOutlined = new HashSet<GameObject>();
            this.layerCollection = layerCollection;
            layerIndex = targetOutlineLayer;
        }
        public void UpdateOutlineObject(GameObject nextOutlined)
        {
            if (nextOutlined == null)
            {
                ClearOutlinedObject();
            }
            else
            {
                UpdateOutlineObjectSet(new HashSet<GameObject>()
                {
                    nextOutlined
                });
            }
        }
        public void UpdateOutlineObjectSet(ISet<GameObject> nextOutlined)
        {
            var outlineLayer = layerCollection[layerIndex];
            foreach (var removedItem in lastOutlined.Except(nextOutlined))
            {
                outlineLayer.Remove(removedItem);
            }
            foreach (var addedItem in nextOutlined.Except(lastOutlined))
            {
                outlineLayer.Add(addedItem);
            }
            lastOutlined = nextOutlined;
        }

        public void ClearOutlinedObject()
        {
            if (lastOutlined.Count > 0)
            {
                var outlineLayer = layerCollection[layerIndex];
                foreach (var outlineObject in lastOutlined)
                {
                    outlineLayer.Remove(outlineObject);
                }
            }
            lastOutlined.Clear();
        }
    }
}
