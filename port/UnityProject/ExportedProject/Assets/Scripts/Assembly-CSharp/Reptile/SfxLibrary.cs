using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	[CreateAssetMenu(fileName = "SfxLibrary", menuName = "Reptile/Audio/Sfx/Sfx Library")]
	public class SfxLibrary : ScriptableObject
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		internal struct SfxCollectionIDComparer : IEqualityComparer<SfxCollectionID>
		{
			public bool Equals(SfxCollectionID x, SfxCollectionID y)
			{
				return x == y;
			}

			public int GetHashCode(SfxCollectionID obj)
			{
				return (int)obj;
			}
		}

		[SerializeField]
		public SfxCollectionDictionary sfxCollectionDictionary = new SfxCollectionDictionary();

		public Dictionary<SfxCollectionID, SfxCollection> sfxCollectionIDDictionary;

		public void Init()
		{
			GenerateEnumDictionary();
		}

		private void GenerateEnumDictionary()
		{
			sfxCollectionIDDictionary = new Dictionary<SfxCollectionID, SfxCollection>(default(SfxCollectionIDComparer));
			int num = 0;
			foreach (KeyValuePair<string, SfxCollection> item in sfxCollectionDictionary)
			{
				if (!Enum.TryParse<SfxCollectionID>(item.Key, out var result))
				{
					result = (SfxCollectionID)(67 + num);
					num++;
				}
				sfxCollectionIDDictionary.Add(result, item.Value);
				if (item.Value is ICollectionGenerator)
				{
					(item.Value as ICollectionGenerator).GenerateEnumDictionary();
				}
			}
		}

		public SfxCollection GetSfxCollection(string sfxCollectionName)
		{
			foreach (KeyValuePair<string, SfxCollection> item in sfxCollectionDictionary)
			{
				if (!(item.Value == null) && item.Value.collectionName.Equals(sfxCollectionName))
				{
					return item.Value;
				}
			}
			return null;
		}

		public SfxCollection GetSfxCollectionById(SfxCollectionID sfxCollectionId)
		{
			SfxCollection value = null;
			sfxCollectionIDDictionary.TryGetValue(sfxCollectionId, out value);
			return value;
		}

		public AudioClip GetAudioClipFromCollection(SfxClip sfxClip)
		{
			return GetAudioClipFromCollection(sfxClip.collectionId, sfxClip.audioClipId);
		}

		public AudioClip GetAudioClipFromCollection(SfxCollectionID sfxCollectionId, AudioClipID audioClipId)
		{
			if (audioClipId == AudioClipID.NONE)
			{
				return null;
			}
			SfxCollection sfxCollectionById = GetSfxCollectionById(sfxCollectionId);
			if (sfxCollectionById == null)
			{
				return null;
			}
			return sfxCollectionById.GetAudioClipById(audioClipId);
		}

		internal AudioClip GetRandomAudioClipFromCollection(SfxCollectionID sfxCollectionId, AudioClipID audioClipId)
		{
			if (audioClipId == AudioClipID.NONE)
			{
				return null;
			}
			SfxCollection sfxCollectionById = GetSfxCollectionById(sfxCollectionId);
			if (sfxCollectionById == null)
			{
				return null;
			}
			return sfxCollectionById.GetRandomAudioClipById(audioClipId);
		}

		internal AudioClip GetAudioClipByIndexFromCollection(SfxCollectionID sfxCollectionId, AudioClipID audioClipId, int index)
		{
			if (audioClipId == AudioClipID.NONE)
			{
				return null;
			}
			SfxCollection sfxCollectionById = GetSfxCollectionById(sfxCollectionId);
			if (sfxCollectionById == null)
			{
				return null;
			}
			return sfxCollectionById.GetAudioClipByIndex(audioClipId, index);
		}
	}
}
