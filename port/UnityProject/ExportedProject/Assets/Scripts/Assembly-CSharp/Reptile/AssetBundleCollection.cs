using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "AssetBundleCollection", menuName = "Reptile/AssetLoading/AssetBundles/Asset Bundle Collection", order = 1)]
	public class AssetBundleCollection : ScriptableObject
	{
		[Tooltip("The name of the assetbundle collection, which is similar to the name of the scene this collection should be associated with")]
		public string assetBundleCollectionName = string.Empty;

		[Tooltip("The assetbundle names for this collection, which are going to be loaded when the scene is loaded.")]
		public string[] assetBundleNames = new string[0];
	}
}
