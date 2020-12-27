namespace Assets.WorldObjects.SaveObjects.SaveManager
{
    public class SaveContext
    {
        private static SaveContext _instance;
        public static SaveContext instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SaveContext();
                }
                return _instance;
            }
            set => _instance = value;
        }

        public string saveName = "defaultSaveName";
    }
}
