using Dman.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityFx.Outline;

namespace Assets.Scripts.Buildings
{
    public class OutlineHoveredObject: MonoBehaviour
    {
        private GameObject lastOutlined;
        public OutlineLayerCollection outlineLayers;
        public RaycastGroup hoverRaycaster;

        private void Update()
        {
            var outlinedGameObject = GetHoveredGameObjectIfOutlineable();

            var outlineLayer = outlineLayers[0];

            if (lastOutlined != null && outlinedGameObject != lastOutlined)
            {
                if (outlineLayer.Contains(lastOutlined))
                {
                    Debug.Log("removing " + lastOutlined.name);
                    outlineLayer.Remove(lastOutlined);
                }
                lastOutlined = null;
            }

            if (outlinedGameObject != null && outlinedGameObject != lastOutlined)
            {
                Debug.Log("outlining " + outlinedGameObject.name);
                outlineLayer.Add(outlinedGameObject);
                lastOutlined = outlinedGameObject;
            }
        }

        private GameObject GetHoveredGameObjectIfOutlineable()
        {
            var mouseOvered = hoverRaycaster.CurrentlyHitObject;
            var hoveredGameObject = mouseOvered.HasValue ? mouseOvered.Value.collider.gameObject : null;
            var outlineReciever = hoveredGameObject?.GetComponentInParent<IHoverOutlineReciever>();
            return outlineReciever?.GetOutlinedObject();
        }
    }
}
