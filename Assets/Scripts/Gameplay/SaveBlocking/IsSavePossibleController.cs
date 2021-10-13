using Dman.ReactiveVariables;
using UI.Manipulators;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Gameplay.SaveBlocking
{
    /// <summary>
    /// determines whether or not the game state can be saved
    /// </summary>
    public class IsSavePossibleController : MonoBehaviour
    {
        public BooleanVariable canSave;

        private SavePossibleCheckSystem checkSystem;

        private void Start()
        {
            checkSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SavePossibleCheckSystem>();
        }

        private void Update()
        {
            var newCanSave = checkSystem.CanSave;
            if(newCanSave != canSave.CurrentValue)
            {
                canSave.SetValue(newCanSave);
            }
        }
    }
}
