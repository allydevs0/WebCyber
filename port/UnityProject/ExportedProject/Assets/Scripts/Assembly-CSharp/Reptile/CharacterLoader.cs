using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class CharacterLoader : ASceneAssetLoader
	{
		public const string CHARACTER_BUNDLE_NAME = "characters";

		private const string CHARACTER_MATERIAL_AFFIX = "Mat";

		private Material[,] loadedCharacterMaterials;

		private GameObject[] loadedCharacterFbxAssets;

		public Material[,] LoadedCharacterMaterials => loadedCharacterMaterials;

		public CharacterLoader(Assets assets)
			: base(assets)
		{
			loadedCharacterMaterials = new Material[26, 4];
			loadedCharacterFbxAssets = new GameObject[26];
		}

		public override void LoadAssets(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadCharacters)
			{
				LoadCharacterAssets();
			}
		}

		public override IEnumerator LoadAssetsASync(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadCharacters)
			{
				yield return LoadCharacterAssetsASync();
			}
		}

		public override void LoadAssetsEditor(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadCharacters)
			{
				LoadCharactersAssetsForEditor();
			}
		}

		public override void UnloadAssets(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadCharacters)
			{
				UnloadCharactersAssets();
			}
		}

		private void LoadCharacterAssets()
		{
			for (int i = 0; i < 26; i++)
			{
				LoadCharacterAssetsFor((Characters)i);
			}
		}

		private void LoadCharacterAssetsFor(Characters characterToLoad)
		{
			LoadFBXForCharacter(characterToLoad);
			for (int i = 0; i < 4; i++)
			{
				LoadMaterialForCharacter(characterToLoad, i);
			}
		}

		private void LoadFBXForCharacter(Characters characterToLoad)
		{
			GameObject fbxAsset = assets.LoadAssetFromBundle<GameObject>("characters", characterToLoad.ToString());
			AddCharacterFBX(characterToLoad, fbxAsset);
		}

		private void LoadMaterialForCharacter(Characters characterToLoad, int outfitIndex)
		{
			Material materialAsset = assets.LoadAssetFromBundle<Material>("characters", characterToLoad.ToString() + "Mat" + outfitIndex);
			AddCharacterMaterial(characterToLoad, outfitIndex, materialAsset);
		}

		private IEnumerator LoadCharacterAssetsASync()
		{
			for (int i = 0; i < 26; i++)
			{
				yield return LoadCharacterAssetsForASync((Characters)i);
			}
		}

		private IEnumerator LoadCharacterAssetsForASync(Characters characterToLoad)
		{
			yield return LoadFBXForCharacterASync(characterToLoad);
			for (int i = 0; i < 4; i++)
			{
				yield return LoadMaterialsForCharacterASync(characterToLoad, i);
			}
		}

		private IEnumerator LoadFBXForCharacterASync(Characters characterToLoad)
		{
			AssetBundleRequest characterFbxAssetRequest = assets.LoadAssetFromBundleASync<GameObject>("characters", characterToLoad.ToString());
			yield return characterFbxAssetRequest;
			AddCharacterFBX(characterToLoad, characterFbxAssetRequest.asset as GameObject);
		}

		private IEnumerator LoadMaterialsForCharacterASync(Characters characterToLoad, int outfitIndex)
		{
			AssetBundleRequest characterMaterialRequest = assets.LoadAssetFromBundleASync<Material>("characters", characterToLoad.ToString() + "Mat" + outfitIndex);
			yield return characterMaterialRequest;
			AddCharacterMaterial(characterToLoad, outfitIndex, characterMaterialRequest.asset as Material);
		}

		private void AddCharacterFBX(Characters character, GameObject fbxAsset)
		{
			loadedCharacterFbxAssets[(int)character] = fbxAsset;
		}

		private void AddCharacterMaterial(Characters character, int outfitIndex, Material materialAsset)
		{
			loadedCharacterMaterials[(int)character, outfitIndex] = materialAsset;
		}

		private void LoadCharactersAssetsForEditor()
		{
			for (int i = 0; i < 26; i++)
			{
				LoadCharacterAssetsForEditor((Characters)i);
			}
		}

		private void LoadCharacterAssetsForEditor(Characters characterToLoad)
		{
		}

		private void UnloadCharactersAssets()
		{
			for (int i = 0; i < 26; i++)
			{
				UnloadCharacterAssetsFor((Characters)i);
			}
		}

		private void UnloadCharacterAssetsFor(Characters characterToUnload)
		{
			RemoveCharacterFBX(characterToUnload);
			for (int i = 0; i < 4; i++)
			{
				RemoveCharacterMaterial(characterToUnload, i);
			}
		}

		private void RemoveCharacterFBX(Characters character)
		{
			loadedCharacterFbxAssets[(int)character] = null;
		}

		private void RemoveCharacterMaterial(Characters character, int outfitIndex)
		{
			loadedCharacterMaterials[(int)character, outfitIndex] = null;
		}

		public Material GetCharacterMaterial(Characters character, int outfitIndex)
		{
			if (loadedCharacterMaterials[(int)character, outfitIndex] == null)
			{
				throw new NoOutfitException(character, outfitIndex);
			}
			return loadedCharacterMaterials[(int)character, outfitIndex];
		}

		public GameObject GetCharacterFbx(Characters character)
		{
			if (loadedCharacterFbxAssets[(int)character] == null)
			{
				throw new NoModelException(character);
			}
			return loadedCharacterFbxAssets[(int)character];
		}
	}
}
