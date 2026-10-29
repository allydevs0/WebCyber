using System.Collections;

namespace Reptile
{
	public class SfxCollectionLoader : ASceneAssetLoader
	{
		public SfxCollectionLoader(Assets assets)
			: base(assets)
		{
		}

		public override void LoadAssets(AssetsToLoadData assetsToLoadData)
		{
			LoadSfxCollections(assetsToLoadData);
		}

		public override IEnumerator LoadAssetsASync(AssetsToLoadData assetsToLoadData)
		{
			yield return LoadSfxCollectionsASync(assetsToLoadData);
		}

		public override void LoadAssetsEditor(AssetsToLoadData assetsToLoadData)
		{
			LoadSfxCollectionsEditor();
		}

		public override void UnloadAssets(AssetsToLoadData assetsToLoadData)
		{
			UnloadSfxCollections(assetsToLoadData);
		}

		private void LoadSfxCollections(AssetsToLoadData assetsToLoadData)
		{
			SfxCollection[] sfxCollectionsToLoad = assetsToLoadData.sfxCollectionsToLoad;
			for (int i = 0; i < sfxCollectionsToLoad.Length; i++)
			{
				if (sfxCollectionsToLoad[i] == null)
				{
					ShowNoSfxCollectionAssignedError();
				}
				else
				{
					sfxCollectionsToLoad[i].LoadAllAudioClips(assets);
				}
			}
		}

		private IEnumerator LoadSfxCollectionsASync(AssetsToLoadData assetsToLoadData)
		{
			SfxCollection[] sfxCollectionsToLoad = assetsToLoadData.sfxCollectionsToLoad;
			for (int i = 0; i < sfxCollectionsToLoad.Length; i++)
			{
				if (sfxCollectionsToLoad[i] == null)
				{
					ShowNoSfxCollectionAssignedError();
				}
				else
				{
					yield return sfxCollectionsToLoad[i].LoadAllAudioClipsASync(assets);
				}
			}
		}

		private void UnloadSfxCollections(AssetsToLoadData assetsToLoadData)
		{
			SfxCollection[] sfxCollectionsToLoad = assetsToLoadData.sfxCollectionsToLoad;
			for (int i = 0; i < sfxCollectionsToLoad.Length; i++)
			{
				if (sfxCollectionsToLoad[i] == null)
				{
					ShowNoSfxCollectionAssignedError();
				}
				else
				{
					sfxCollectionsToLoad[i].UnloadAllAudioClips();
				}
			}
		}

		private void ShowNoSfxCollectionAssignedError()
		{
		}

		private void LoadSfxCollectionsEditor()
		{
		}
	}
}
