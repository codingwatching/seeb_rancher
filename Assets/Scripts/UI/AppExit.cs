using UnityEngine;

namespace Assets.Scripts.UI
{
    public class AppExit : MonoBehaviour
    {
        public void ExitApp()
        {
            Debug.Log("Exiting application");
            Application.Quit();
        }
    }
}