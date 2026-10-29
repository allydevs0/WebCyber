using System;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	public struct SfxClip
	{
		[SerializeField]
		[HideInInspector]
		public SfxCollectionID collectionId;

		[SerializeField]
		[HideInInspector]
		public AudioClipID audioClipId;

		public static implicit operator SfxClip(SfxCollectionID sfxCollectionID)
		{
			SfxClip result = default;
			result.collectionId = sfxCollectionID;
			result.audioClipId = AudioClipID.NONE;
			return result;
		}
	}
}
