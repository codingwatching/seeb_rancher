namespace Assets.Scripts.Utilities.SaveObjects
{
    public interface ISaveable<T>
    {
        T GetSaveObject();
        void SetupFromSaveObject(T save);
    }
}
