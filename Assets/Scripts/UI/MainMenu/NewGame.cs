using Dman.ReactiveVariables;
using Dman.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI.MainMenu
{
    public class NewGame : MonoBehaviour
    {
        public IntVariable phaseVariable;
        public SceneReference targetScene;

        public void LoadNewGame()
        {
            Debug.Log($"loading scene {targetScene.scenePath}");
            StartCoroutine(NewGameRoutine());
        }
        private IEnumerator NewGameRoutine()
        {
            phaseVariable.SetValue(-1);
            DontDestroyOnLoad(gameObject);
            var sceneIndex = SceneUtility.GetBuildIndexByScenePath(targetScene.scenePath);
            var loadingScene = SceneManager.LoadScene(sceneIndex, new LoadSceneParameters(LoadSceneMode.Single));
            yield return new WaitUntil(() => loadingScene.isLoaded);
            yield return new WaitForEndOfFrame();
            phaseVariable.SetValue(0);
            Destroy(gameObject);
        }
    }
}