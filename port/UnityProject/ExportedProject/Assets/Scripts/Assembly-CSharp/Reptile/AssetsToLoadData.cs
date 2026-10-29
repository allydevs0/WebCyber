using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "AssetsToLoadData", menuName = "Reptile/AssetLoading/AssetLoadingData/Assets load data", order = 1)]
	public class AssetsToLoadData : ScriptableObject
	{
		public string forSceneWithName = string.Empty;

		public SfxCollection[] sfxCollectionsToLoad;

		public bool loadCharacters;

		public bool loadGraffiti;

		public bool loadGameplayPrefabs;
	}
}
