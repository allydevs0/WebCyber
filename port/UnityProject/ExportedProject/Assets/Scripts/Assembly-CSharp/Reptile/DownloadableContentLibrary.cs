using System;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	[CreateAssetMenu(fileName = "DownloadableContentLibrary", menuName = "Reptile/DLC/Downloadable content library")]
	public class DownloadableContentLibrary : ScriptableObject
	{
		[SerializeField]
		private DownloadableContentItem[] downloadableContentItems;

		public int DownloadableContentCount => downloadableContentItems.Length;

		public DownloadableContentItem GetDownloadableContentItem(int index)
		{
			if (index < 0 || index >= downloadableContentItems.Length)
			{
				return null;
			}
			return downloadableContentItems[index];
		}
	}
}
