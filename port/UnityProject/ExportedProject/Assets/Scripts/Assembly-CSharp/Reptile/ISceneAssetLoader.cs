using System.Collections;

namespace Reptile
{
	public interface ISceneAssetLoader
	{
		void LoadAssets(AssetsToLoadData assetsToLoadData);

		IEnumerator LoadAssetsASync(AssetsToLoadData assetsToLoadData);

		void LoadAssetsEditor(AssetsToLoadData assetsToLoadData);

		void UnloadAssets(AssetsToLoadData assetsToLoadData);
	}
}
