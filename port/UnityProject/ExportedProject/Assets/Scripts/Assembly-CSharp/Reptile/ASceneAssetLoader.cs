using System.Collections;

namespace Reptile
{
	public abstract class ASceneAssetLoader : ISceneAssetLoader
	{
		protected Assets assets;

		public ASceneAssetLoader(Assets assets)
		{
			this.assets = assets;
		}

		public abstract void LoadAssets(AssetsToLoadData assetsToLoadData);

		public abstract IEnumerator LoadAssetsASync(AssetsToLoadData assetsToLoadData);

		public abstract void LoadAssetsEditor(AssetsToLoadData assetsToLoadData);

		public abstract void UnloadAssets(AssetsToLoadData assetsToLoadData);
	}
}
