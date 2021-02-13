using Assets.Scripts.DataModels;
using Dman.ReactiveVariables;
using UnityEngine;

namespace Assets.Scripts.UI.SeedInventory
{
    // TODO: some way to return the dragging seeds to their source inventory slot
    [RequireComponent(typeof(SeedBucketDisplay))]
    public class DraggingSeeds : MonoBehaviour
    {
        private void Update()
        {
            var mousePos = Input.mousePosition;
            transform.position = mousePos;
        }
    }
}
