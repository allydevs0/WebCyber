using System.Collections;
using System.IO;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reptile
{
	public class Bootstrap : MonoBehaviour
	{
		[SerializeField]
		private GameObject steamManagerGameObject;

		[SerializeField]
		private GameObject gogGalaxyManagerGameObject;

		[SerializeField]
		private SceneLoader sceneLoader;

		[SerializeField]
		private SceneLibrary sceneLibrary;

		[SerializeField]
		private AssetBundleLibrary assetBundleLibrary;

		[SerializeField]
		private AssetsToLoadDataLibrary assetsToLoadDataLibrary;

		[SerializeField]
		private GameVersion gameVersion;

		private Assets assets;

		public void LaunchGameIntoStage(Stage stage)
		{
			StartCoroutine(SetupGameToStage(stage));
		}

		public void LaunchGame()
		{
			StartCoroutine(SetupGame());
		}

		private void Awake()
		{
			InitializeSteam();
			InitializeGogGalaxy();
			InitializeDoTween();
			InitializeAssets();
			LaunchGame();
		}

		private void InitializeSteam()
		{
			steamManagerGameObject.GetComponent<SteamManager>().Initialize();
		}

		private void InitializeGogGalaxy()
		{
			Object.Destroy(gogGalaxyManagerGameObject);
		}

		private void InitializeDoTween()
		{
			DOTween.Init(false, true, LogBehaviour.Default);
			DOTween.SetTweensCapacity(200, 50);
		}

		private void InitializeAssets()
		{
			assets = base.gameObject.AddComponent<Assets>();
			assets.Init(assetBundleLibrary, assetsToLoadDataLibrary);
		}

		private IEnumerator SetupGame()
		{
			string introSceneName = Path.GetFileNameWithoutExtension(sceneLibrary.introScenePath);
			string coreSceneName = Path.GetFileNameWithoutExtension(sceneLibrary.coreScenePath);
			yield return LoadSceneAssets(introSceneName);
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(introSceneName));
			yield return LoadSceneAssets(coreSceneName);
			Core.Instance.StartCore(assets, gameVersion);
			Core.Instance.StartGame();
		}

		private IEnumerator SetupGameToStage(Stage stage)
		{
			string coreSceneName = Path.GetFileNameWithoutExtension(sceneLibrary.coreScenePath);
			yield return LoadSceneAssets(coreSceneName);
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(coreSceneName));
			Core.Instance.StartCore(assets, gameVersion);
			Core.Instance.StartGameToStage(stage);
		}

		private IEnumerator LoadSceneAssets(string sceneName)
		{
			yield return assets.LoadAssetBundlesForSceneASync(sceneName);
			yield return LoadScene(sceneName);
		}

		private IEnumerator LoadScene(string sceneName)
		{
			yield return sceneLoader.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
		}
	}
}
