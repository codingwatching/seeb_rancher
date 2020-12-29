using System;

namespace Assets.Scripts.Utilities.SaveSystem.Objects
{
    [Serializable]
    public class SaveData
    {
        public string uniqueSaveDataId;
        public object savedSerializableObject;
        public string[] saveDataIDDependencies;
    }
}
