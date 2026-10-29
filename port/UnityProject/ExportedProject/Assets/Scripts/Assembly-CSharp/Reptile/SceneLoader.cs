using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reptile
{
	public class SceneLoader : MonoBehaviour
	{
		public IEnumerator LoadScene(string sceneName, LoadSceneMode loadSceneMode)
		{
			yield return LoadSceneRoutine(sceneName, loadSceneMode);
		}

		public IEnumerator LoadStage(Stage stage, LoadSceneMode loadSceneMode)
		{
			yield return LoadScene(stage.ToString(), loadSceneMode);
		}

		public AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode)
		{
			return LoadSceneAsyncRoutine(sceneName, loadSceneMode);
		}

		public AsyncOperation LoadStageASync(Stage stage, LoadSceneMode loadSceneMode)
		{
			return LoadSceneAsync(stage.ToString(), loadSceneMode);
		}

		private IEnumerator LoadSceneRoutine(string sceneName, LoadSceneMode loadSceneMode)
		{
			SceneManager.LoadScene(sceneName, loadSceneMode);
			while (!SceneManager.GetSceneByName(sceneName).isLoaded || UIPopup.waitForPopupToFinish || BaseModule.waitForSaveAndLogout)
			{
				yield return null;
			}
		}

		private AsyncOperation LoadSceneAsyncRoutine(string sceneName, LoadSceneMode loadSceneMode)
		{
			return SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
		}

		public AsyncOperation UnloadScene(string sceneName)
		{
			return SceneManager.UnloadSceneAsync(sceneName);
		}

		public void SetSceneActive(Scene scene)
		{
			SceneManager.SetActiveScene(scene);
		}
	}
}
