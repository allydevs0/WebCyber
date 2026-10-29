using System;
using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "AssetBundleLibrary", menuName = "Reptile/AssetLoading/AssetBundles/Asset Bundle Library", order = 0)]
	public class AssetBundleLibrary : ScriptableObject
	{
		public AssetBundleCollection[] collections;

		[SerializeField]
		private AssetBundleCollection[] demoCollections;

		[SerializeField]
		private AssetBundleCollection[] fullGameCollections;

		public AssetBundleCollection GetCollectionByName(string collectionName)
		{
			for (int i = 0; i < collections.Length; i++)
			{
				if (collections[i].assetBundleCollectionName.Equals(collectionName))
				{
					return collections[i];
				}
			}
			throw new Exception("AssetBundleLibrary.GetCollectionByName(): Could not find assetbundle collection with name: " + collectionName + ", add it to the asset bundle library asset");
		}
	}
}
