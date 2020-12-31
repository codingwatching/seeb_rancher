using Assets.Scripts.Utilities.SaveSystem;
using UnityEngine;

namespace Assets.Scripts.Utilities.ScriptableObjectRegistries
{
    public class RegistryRegistry : MonoBehaviour
    {
        public UniqueObjectRegistry[] registries;

        public static RegistryRegistry Instance;

        private void Awake()
        {
            Instance = this;
            SaveSystemHooks.Instance.PreLoad += OnPreLoad;
        }
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            SaveSystemHooks.Instance.PreLoad -= OnPreLoad;
        }

        private void OnPreLoad()
        {
            // TODO: do something to make sure the registries are up to date? might not even be necessary at all
        }

        public static UniqueObjectRegistryWithAccess<T> GetObjectRegistry<T>() where T : IDableObject
        {
            if (Instance == null)
            {
                throw new System.Exception("the registry registry is not initialized!!");
            }
            foreach (var registry in Instance.registries)
            {
                if (registry is UniqueObjectRegistryWithAccess<T> typedRegistry)
                {
                    return typedRegistry;
                }
            }
            return null;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}