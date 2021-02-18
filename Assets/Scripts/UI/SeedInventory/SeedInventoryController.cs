using Dman.SceneSaveSystem;
using Dman.Utilities;
using UnityEngine;

namespace Assets.Scripts.UI.SeedInventory
{
    public class SeedInventoryController : MonoBehaviour
    {
        public int defaultSeedBuckets;

        public GameObject seedGridLayoutParent;
        
        public static SeedInventoryController Instance;

        [HideInInspector]
        
        private void Awake()
        {
            Instance = this;
        }
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}