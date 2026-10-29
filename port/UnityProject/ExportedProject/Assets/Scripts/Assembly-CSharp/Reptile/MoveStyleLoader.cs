using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class MoveStyleLoader : ASceneAssetLoader
	{
		public const string MOVESTYLE_BUNDLE_NAME = "characters";

		public const string MOVESTYLE_AFFIX = "Tex";

		private Texture[,] loadedMoveStyleTextures;

		public MoveStyleLoader(Assets assets)
			: base(assets)
		{
			loadedMoveStyleTextures = new Texture[5, 10];
		}

		public Texture GetSkinTexture(MoveStyle moveStyle, int skinIndex)
		{
			return loadedMoveStyleTextures[(int)moveStyle, skinIndex];
		}

		public static Material[] GetMoveStyleMaterials(Player player, MoveStyle moveStyle)
		{
			Material[] result = null;
			PlayerMoveStyleProps moveStyleProps = player.characterVisual.moveStyleProps;
			switch (moveStyle)
			{
			case MoveStyle.SKATEBOARD:
				result = moveStyleProps.skateboard.GetComponent<Renderer>().sharedMaterials;
				break;
			case MoveStyle.BMX:
				result = moveStyleProps.bmxWheelF.GetComponent<Renderer>().sharedMaterials;
				break;
			case MoveStyle.INLINE:
				result = moveStyleProps.skateL.GetComponent<Renderer>().sharedMaterials;
				break;
			}
			return result;
		}

		private void LoadMoveStyleAssets()
		{
			for (int i = 0; i < 5; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					Assets obj = assets;
					MoveStyle moveStyle = (MoveStyle)i;
					Texture texture = obj.LoadAssetFromBundle<Texture>("characters", moveStyle.ToString() + "Tex" + j);
					loadedMoveStyleTextures[i, j] = texture;
				}
			}
		}

		private IEnumerator LoadMoveStyleAssetsAsync()
		{
			for (int moveStyleIndex = 0; moveStyleIndex < 5; moveStyleIndex++)
			{
				for (int skinIndex = 0; skinIndex < 10; skinIndex++)
				{
					Assets obj = assets;
					MoveStyle moveStyle = (MoveStyle)moveStyleIndex;
					AssetBundleRequest loadRequest = obj.LoadAssetFromBundleASync<Texture>("characters", moveStyle.ToString() + "Tex" + skinIndex);
					yield return loadRequest;
					loadedMoveStyleTextures[moveStyleIndex, skinIndex] = loadRequest.asset as Texture;
				}
				yield return null;
			}
		}

		private void UnloadMoveStyleAssets()
		{
			for (int i = 0; i < 5; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					loadedMoveStyleTextures[i, j] = null;
				}
			}
		}

		public override void LoadAssets(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadCharacters)
			{
				LoadMoveStyleAssets();
			}
		}

		public override IEnumerator LoadAssetsASync(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadCharacters)
			{
				yield return LoadMoveStyleAssetsAsync();
			}
		}

		public override void LoadAssetsEditor(AssetsToLoadData assetsToLoadData)
		{
			LoadMoveStyleAssets();
		}

		public override void UnloadAssets(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadCharacters)
			{
				UnloadMoveStyleAssets();
			}
		}
	}
}
