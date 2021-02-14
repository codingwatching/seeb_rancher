using UnityEngine;

namespace Assets.Scripts.Buildings
{
    public class HoverOutlineReciever : MonoBehaviour, IHoverOutlineReciever
    {
        public bool DoesOutline = true;

        public GameObject GetOutlinedObject()
        {
            return DoesOutline ? gameObject : null;
        }
    }

    public interface IHoverOutlineReciever
    {
        GameObject GetOutlinedObject();
    }
}
