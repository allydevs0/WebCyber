using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class GraffitiLoader : ASceneAssetLoader
	{
		public const string GRAFFITI_ART_BUNDLE_NAME = "graffiti";

		private const string GRAFFITI_ART_INFO_NAME = "graffitiartinfo";

		private GraffitiArtInfo graffitiArtInfo;

		public GraffitiArtInfo GraffitiArtInfo => graffitiArtInfo;

		public GraffitiLoader(Assets assets)
			: base(assets)
		{
		}

		public override void LoadAssets(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadGraffiti)
			{
				LoadGraffitiArtInfo();
			}
		}

		public override IEnumerator LoadAssetsASync(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadGraffiti)
			{
				yield return LoadGraffitiArtInfoAsync(assets);
			}
		}

		public override void LoadAssetsEditor(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadGraffiti)
			{
				LoadGraffitiArtInfoForEditor();
			}
		}

		public override void UnloadAssets(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadGraffiti)
			{
				graffitiArtInfo = null;
			}
		}

		public void LoadGraffitiArtInfo()
		{
			graffitiArtInfo = assets.LoadAssetFromBundle<GraffitiArtInfo>("graffiti", "graffitiartinfo");
		}

		private IEnumerator LoadGraffitiArtInfoAsync(Assets assets)
		{
			AssetBundleRequest graffitiArtInfoRequest = assets.LoadAssetFromBundleASync<GraffitiArtInfo>("graffiti", "graffitiartinfo");
			yield return graffitiArtInfoRequest;
			graffitiArtInfo = graffitiArtInfoRequest.asset as GraffitiArtInfo;
		}

		private void LoadGraffitiArtInfoForEditor()
		{
		}
	}
}
