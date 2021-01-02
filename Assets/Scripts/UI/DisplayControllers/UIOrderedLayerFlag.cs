using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.DisplayControllers
{
    [RequireComponent(typeof(RectTransform))]
    public class UIOrderedLayerFlag : MonoBehaviour
    {
        private static IDictionary<int, UIOrderedLayerFlag> _orderLayer;
        public static IDictionary<int, UIOrderedLayerFlag> OrderingLayers
        {
            get
            {
                if (_orderLayer == null)
                {
                    _orderLayer = new Dictionary<int, UIOrderedLayerFlag>();
                }
                return _orderLayer;
            }
        }

        public int orderLayer;

        private void Awake()
        {
            OrderingLayers[orderLayer] = this;
        }

        private void OnDestroy()
        {
            if (OrderingLayers[orderLayer] == this)
            {
                OrderingLayers.Remove(orderLayer);
            }
        }
    }
}