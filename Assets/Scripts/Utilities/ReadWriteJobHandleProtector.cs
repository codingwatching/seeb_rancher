using System;
using Unity.Jobs;

namespace Assets.Scripts.Utilities
{
    public class ReadWriteJobHandleProtector : IDisposable
    {
        private JobHandle readers = default;
        public bool isWritable { private set; get; } = true;
        public void RegisterJobHandleForReader(JobHandle handle)
        {
            readers = JobHandle.CombineDependencies(readers, handle);
            isWritable = false;
        }
        public void OpenForEdit()
        {
            readers.Complete();
            isWritable = true;
        }

        public void Dispose()
        {
            readers.Complete();
        }
    }
}
