using Assets.Scripts.Utilities.SaveSystem.Objects;

namespace Assets.Scripts.Utilities.SaveSystem.Components
{
    public interface ISaveableData
    {
        public string UniqueSaveIdentifier { get; }
        object GetSaveObject();
        void SetupFromSaveObject(object save);
        ISaveableData[] GetDependencies();
    }
}
