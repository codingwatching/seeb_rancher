using Dman.SceneSaveSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Gameplay.SaveBlocking
{
    public class ECSCleanupController : MonoBehaviour
    {
        private void Awake()
        {
            SaveSystemHooks.Instance.PreLoad += CleanECS;
        }

        private void CleanECS()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            manager.DestroyEntity(manager.UniversalQuery);
        }
    }
}