using System.IO;
using UnityEngine;

namespace Reptile
{
	public class Bundle
	{
		private enum State
		{
			UNLOADED = 0,
			LOADING = 1,
			LOADED = 2
		}

		private string filePath = string.Empty;

		private string name = string.Empty;

		private AssetBundle assetBundle;

		private State currentState;

		public bool IsLoading => currentState == State.LOADING;

		public bool IsLoaded => currentState == State.LOADED;

		public string Name => name;

		public string FilePath => filePath;

		public AssetBundle AssetBundle => assetBundle;

		public Bundle(string assetBundleFilePath)
		{
			filePath = assetBundleFilePath;
			name = Path.GetFileNameWithoutExtension(assetBundleFilePath);
			currentState = State.UNLOADED;
		}

		public void InitializeLoad()
		{
			currentState = State.LOADING;
		}

		public void ResetLoadState()
		{
			currentState = State.UNLOADED;
		}

		public void SetAssetBundle(AssetBundle assetBundle)
		{
			this.assetBundle = assetBundle;
			currentState = State.LOADED;
		}

		public void Unload()
		{
			currentState = State.UNLOADED;
			assetBundle.Unload(unloadAllLoadedObjects: true);
			assetBundle = null;
		}
	}
}
