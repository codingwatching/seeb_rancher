using Dman.ReactiveVariables;
using Dman.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.MainMenu
{
    public class NewGame : MonoBehaviour
    {
        public IntVariable waveVariable;
        public SceneReference targetScene;


        public void LoadNewGame()
        {
            Debug.Log($"loading scene {targetScene.scenePath}");
            StartCoroutine(NewGameRoutine());
        }
        private IEnumerator NewGameRoutine()
        {
            //waveVariable.SetValue(-1);

            DontDestroyOnLoad(gameObject);
            var sceneIndex = SceneUtility.GetBuildIndexByScenePath(targetScene.scenePath);
            var loadingScene = SceneManager.LoadScene(sceneIndex, new LoadSceneParameters(LoadSceneMode.Single));
            yield return new WaitUntil(() => loadingScene.isLoaded);
            yield return new WaitForEndOfFrame();

            //waveVariable.SetValue(0);
            Destroy(gameObject);
        }
    }
}