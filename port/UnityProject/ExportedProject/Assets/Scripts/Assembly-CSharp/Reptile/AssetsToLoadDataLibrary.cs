using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "AssetsToLoadDataLibrary", menuName = "Reptile/AssetLoading/AssetLoadingData/Assets load data library", order = 0)]
	public class AssetsToLoadDataLibrary : ScriptableObject
	{
		public AssetsToLoadData[] assetsLoadingForSceneLibrary;

		public AssetsToLoadData GetAssetsToLoadDataForScene(string sceneName)
		{
			for (int i = 0; i < assetsLoadingForSceneLibrary.Length; i++)
			{
				if (assetsLoadingForSceneLibrary[i] != null && assetsLoadingForSceneLibrary[i].forSceneWithName.Equals(sceneName))
				{
					return assetsLoadingForSceneLibrary[i];
				}
			}
			return null;
		}
	}
}
