using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Reptile
{
	public class Assets : MonoBehaviour
	{
		public const string ASSETBUNDLES_DIR_NAME = "Assets";

		protected AssetBundleLibrary assetBundleLibrary;

		private int assetBundlesLoaded;

		private float assetBundleLoadingProgressMultiplier;

		private int assetLoadersLoaded;

		private int assetLoadersToLoad;

		private Dictionary<string, Bundle> availableBundles;

		private List<ISceneAssetLoader> assetLoaders;

		private MusicAssetsLoader musicAssetLoader;

		private AssetsToLoadDataLibrary assetsToLoadDataLibrary;

		private AssetBundleCreateRequest currentAssetBundleCreateRequest;

		private AssetBundleCollection currentAssetBundleCollection;

		private ISceneAssetLoader currentSceneAssetLoader;

		public virtual void Init(AssetBundleLibrary assetBundleLibrary, AssetsToLoadDataLibrary assetsToLoadDataLibrary)
		{
			availableBundles = new Dictionary<string, Bundle>();
			assetLoaders = new List<ISceneAssetLoader>();
			musicAssetLoader = new MusicAssetsLoader();
			this.assetBundleLibrary = assetBundleLibrary;
			this.assetsToLoadDataLibrary = assetsToLoadDataLibrary;
			CreateLoadersForPlatform();
			GenerateBundlesForPlatform();
		}

		public void ResetLoading()
		{
			currentAssetBundleCreateRequest = null;
			currentAssetBundleCollection = null;
			currentSceneAssetLoader = null;
			assetBundlesLoaded = 0;
			assetBundleLoadingProgressMultiplier = 0f;
			assetLoadersLoaded = 0;
			assetLoadersToLoad = 0;
		}

		public virtual void OnDestroy()
		{
			availableBundles.Clear();
			availableBundles = null;
			assetLoaders.Clear();
			assetLoaders = null;
			musicAssetLoader = null;
			assetBundleLibrary = null;
			assetsToLoadDataLibrary = null;
			currentAssetBundleCollection = null;
			currentAssetBundleCreateRequest = null;
			currentSceneAssetLoader = null;
		}

		public virtual float GetLoadingProgress()
		{
			float num = GetAssetBundleLoadingProgress() * 0.5f;
			float num2 = GetAssetLoaderLoadingProgress() * 0.5f;
			return num + num2;
		}

		private float GetAssetBundleLoadingProgress()
		{
			if (currentAssetBundleCollection == null || currentAssetBundleCollection.assetBundleNames == null || currentAssetBundleCreateRequest == null)
			{
				return 0f;
			}
			if (currentAssetBundleCollection.assetBundleNames.Length == 0)
			{
				return 1f;
			}
			return Mathf.Min(currentAssetBundleCreateRequest.progress * assetBundleLoadingProgressMultiplier + assetBundleLoadingProgressMultiplier * (float)assetBundlesLoaded, 1f);
		}

		private float GetAssetLoaderLoadingProgress()
		{
			if (currentSceneAssetLoader == null)
			{
				return 0f;
			}
			if (assetLoadersToLoad == 0)
			{
				return 1f;
			}
			return Mathf.Min(assetLoadersLoaded / assetLoadersToLoad, 1f);
		}

		private void CreateLoadersForPlatform()
		{
			AddSceneAssetLoader(new GraffitiLoader(this));
			AddSceneAssetLoader(new CharacterLoader(this));
			AddSceneAssetLoader(new SfxCollectionLoader(this));
			AddSceneAssetLoader(new MoveStyleLoader(this));
			AddSceneAssetLoader(new GamePrefabLoader(this));
		}

		protected void AddSceneAssetLoader(ISceneAssetLoader assetLoader)
		{
			assetLoaders.Add(assetLoader);
		}

		protected virtual void GenerateBundlesForPlatform()
		{
			availableBundles = new Dictionary<string, Bundle>();
			foreach (string item in GetAssetBundlePathsFromDisk(Path.Combine(Application.streamingAssetsPath, "Assets")))
			{
				Bundle bundle = new Bundle(item);
				availableBundles.Add(bundle.Name, bundle);
			}
		}

		public virtual void LoadAssetBundleByName(string bundleName)
		{
			Bundle value = null;
			if (availableBundles.TryGetValue(bundleName, out value))
			{
				LoadBundle(value);
			}
		}

		public virtual void LoadBundle(Bundle bundleToLoad)
		{
			AssetBundle assetBundle = AssetBundle.LoadFromFile(bundleToLoad.FilePath);
			bundleToLoad.SetAssetBundle(assetBundle);
		}

		public virtual IEnumerator LoadAssetBundleByNameASync(string assetBundleName)
		{
			Bundle value = null;
			if (availableBundles.TryGetValue(assetBundleName, out value))
			{
				yield return LoadBundleASync(value);
			}
		}

		public virtual IEnumerator LoadBundleASync(Bundle bundleToLoad)
		{
			bundleToLoad.InitializeLoad();
			currentAssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(bundleToLoad.FilePath);
			yield return currentAssetBundleCreateRequest;
			if (currentAssetBundleCreateRequest.assetBundle == null)
			{
				bundleToLoad.ResetLoadState();
			}
			else
			{
				bundleToLoad.SetAssetBundle(currentAssetBundleCreateRequest.assetBundle);
			}
		}

		public virtual void LoadAssetBundlesForScene(string sceneName)
		{
			LoadAssetBundleCollectionForScene(sceneName);
		}

		private void LoadAssetBundleCollectionForScene(string sceneName)
		{
			AssetBundleCollection bundleCollectionForScene = GetBundleCollectionForScene(sceneName);
			LoadAssetBundlesFromCollection(bundleCollectionForScene);
		}

		private AssetBundleCollection GetBundleCollectionForScene(string sceneName)
		{
			return GetBundleCollectionByName(sceneName);
		}

		private AssetBundleCollection GetBundleCollectionByName(string name)
		{
			return assetBundleLibrary.GetCollectionByName(name);
		}

		public virtual void LoadAssetBundlesFromCollection(AssetBundleCollection assetBundleCollection)
		{
			for (int i = 0; i < assetBundleCollection.assetBundleNames.Length; i++)
			{
				LoadAssetBundleByName(assetBundleCollection.assetBundleNames[i]);
			}
		}

		public virtual IEnumerator LoadAssetBundlesForSceneASync(string sceneName)
		{
			yield return LoadAssetBundleCollectionForSceneASync(sceneName);
		}

		private IEnumerator LoadAssetBundleCollectionForSceneASync(string sceneName)
		{
			SetupLoadingAssetBundleCollection(sceneName);
			yield return LoadAssetBundlesFromCollectionASync(currentAssetBundleCollection);
		}

		private void SetupLoadingAssetBundleCollection(string sceneName)
		{
			currentAssetBundleCollection = GetBundleCollectionForScene(sceneName);
			if (currentAssetBundleCollection.assetBundleNames.Length != 0)
			{
				assetBundleLoadingProgressMultiplier = 1f / (float)currentAssetBundleCollection.assetBundleNames.Length;
			}
		}

		public virtual IEnumerator LoadAssetBundlesFromCollectionASync(AssetBundleCollection assetBundleCollection)
		{
			for (int i = 0; i < assetBundleCollection.assetBundleNames.Length; i++)
			{
				assetBundlesLoaded = i;
				yield return LoadAssetBundleByNameASync(assetBundleCollection.assetBundleNames[i]);
			}
		}

		public virtual void UnloadAssetBundleByName(string assetBundleName)
		{
			Bundle value = null;
			if (availableBundles.TryGetValue(assetBundleName, out value))
			{
				UnloadBundle(value);
			}
		}

		public virtual void UnloadBundle(Bundle bundleToUnload)
		{
			if (bundleToUnload.IsLoaded)
			{
				bundleToUnload.Unload();
			}
		}

		public virtual void UnloadAssetBundlesForScene(string sceneName)
		{
			UnloadAssetBundleCollectionForScene(sceneName);
		}

		private void UnloadAssetBundleCollectionForScene(string sceneName)
		{
			AssetBundleCollection bundleCollectionForScene = GetBundleCollectionForScene(sceneName);
			UnloadAssetBundleCollection(bundleCollectionForScene);
		}

		public virtual void UnloadAssetBundleCollection(AssetBundleCollection assetBundleCollection)
		{
			for (int i = 0; i < assetBundleCollection.assetBundleNames.Length; i++)
			{
				UnloadAssetBundleByName(assetBundleCollection.assetBundleNames[i]);
			}
		}

		public T LoadAssetFromBundle<T>(string bundleName, string assetName) where T : Object
		{
			Bundle value = null;
			if (availableBundles.TryGetValue(bundleName, out value) && value.IsLoaded)
			{
				return value.AssetBundle.LoadAsset<T>(assetName);
			}
			throw new NoBundleFoundException(bundleName);
		}

		public AssetBundleRequest LoadAssetFromBundleASync<T>(string bundleName, string assetName) where T : Object
		{
			Bundle value = null;
			if (availableBundles.TryGetValue(bundleName, out value) && value.IsLoaded)
			{
				return value.AssetBundle.LoadAssetAsync<T>(assetName);
			}
			throw new NoBundleFoundException(bundleName);
		}

		public void LoadMusicAssetFromDisk(MusicPlayerData data)
		{
			StartCoroutine(musicAssetLoader.LoadMusicAssetFromDiskRoutine(data, data.AudioType));
		}

		public void LoadMusicAsset(MusicPlayerData data)
		{
			StartCoroutine(musicAssetLoader.LoadMusicAssetFromClipASync(data));
		}

		public void LoadAudioClip(AudioClip audioClip)
		{
			musicAssetLoader.LoadAudioClipAsset(audioClip);
		}

		public IEnumerator LoadAudioClipASync(AudioClip audioClip)
		{
			if (audioClip.loadState == AudioDataLoadState.Unloaded)
			{
				yield return musicAssetLoader.LoadAudioClipAssetASync(audioClip);
			}
		}

		public T GetAssetsLoader<T>() where T : ISceneAssetLoader
		{
			foreach (ISceneAssetLoader assetLoader in assetLoaders)
			{
				if (assetLoader is T)
				{
					return (T)assetLoader;
				}
			}
			return default;
		}

		public virtual void LoadAssetLoadersForScene(string sceneName)
		{
			AssetsToLoadData assetsToLoadDataForScene = GetAssetsToLoadDataForScene(sceneName);
			List<ISceneAssetLoader> list = RetrieveAllAssetLoaders();
			assetLoadersToLoad = list.Count;
			foreach (ISceneAssetLoader item in list)
			{
				item.LoadAssets(assetsToLoadDataForScene);
				assetLoadersLoaded++;
			}
		}

		protected AssetsToLoadData GetAssetsToLoadDataForScene(string sceneName)
		{
			return assetsToLoadDataLibrary.GetAssetsToLoadDataForScene(sceneName);
		}

		protected List<ISceneAssetLoader> RetrieveAllAssetLoaders()
		{
			return assetLoaders;
		}

		public virtual IEnumerator LoadAssetLoadersForSceneASync(string sceneName)
		{
			AssetsToLoadData assetsToLoadData = GetAssetsToLoadDataForScene(sceneName);
			List<ISceneAssetLoader> list = RetrieveAllAssetLoaders();
			assetLoadersToLoad = list.Count;
			foreach (ISceneAssetLoader item in list)
			{
				yield return item.LoadAssetsASync(assetsToLoadData);
				assetLoadersLoaded++;
			}
		}

		public virtual void UnloadAssetLoadersForScene(string sceneName)
		{
			AssetsToLoadData assetsToLoadDataForScene = GetAssetsToLoadDataForScene(sceneName);
			foreach (ISceneAssetLoader item in RetrieveAllAssetLoaders())
			{
				item.UnloadAssets(assetsToLoadDataForScene);
			}
		}

		protected virtual HashSet<string> GetAssetBundlePathsFromDisk(string pathToBundles)
		{
			return GetAssetBundlePathsFromDiskStandAlone(pathToBundles);
		}

		private HashSet<string> GetAssetBundlePathsFromDiskStandAlone(string pathToBundles)
		{
			HashSet<string> hashSet = new HashSet<string>();
			FileInfo[] files = new DirectoryInfo(pathToBundles).GetFiles("*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				hashSet.Add(files[i].FullName);
			}
			return hashSet;
		}
	}
}
