using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Gameplay.SaveBlocking
{
    [AlwaysUpdateSystem]
    public class SavePossibleCheckSystem : SystemBase
    {
        public bool CanSave
        {
            get
            {
                canSaveDep.Complete();
                return canSave[0];
            }
        }

        private NativeArray<bool> canSave;
        private JobHandle canSaveDep = default;

        private EntityQuery saveBlockers;

        protected override void OnCreate()
        {
            base.OnCreate();
            canSave = new NativeArray<bool>(1, Allocator.Persistent);

            saveBlockers = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(BlockSaveFlagComponent) },
                Options = EntityQueryOptions.Default
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            canSaveDep.Complete();
            canSave.Dispose();
        }

        protected override void OnUpdate()
        {
            canSave[0] = saveBlockers.IsEmpty;
        }
    }
}