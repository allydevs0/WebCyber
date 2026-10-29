using System;
using System.Collections;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	[CreateAssetMenu(fileName = "SfxCollection", menuName = "Reptile/Audio/Sfx/Sfx Collection")]
	public class SfxCollection : ScriptableObject
	{
		[Serializable]
		public class RandomAudioClipContainer
		{
			[SerializeField]
			public AudioClipID clipID;

			[SerializeField]
			public int lastRandomClip;

			[SerializeField]
			public AudioClip[] clips;
		}

		public string collectionName = string.Empty;

		[SerializeField]
		public RandomAudioClipContainer[] audioClipContainers;

		private static RandomAudioClipContainer AddNewContainer(RandomAudioClipContainer[] audioClipContainers, int index, AudioClipID clipID)
		{
			RandomAudioClipContainer randomAudioClipContainer = new RandomAudioClipContainer();
			randomAudioClipContainer.clipID = clipID;
			randomAudioClipContainer.lastRandomClip = 0;
			randomAudioClipContainer.clips = null;
			audioClipContainers[index] = randomAudioClipContainer;
			return randomAudioClipContainer;
		}

		public static RandomAudioClipContainer GetAudioClipContainerFromID(ref RandomAudioClipContainer[] audioClipContainers, AudioClipID clipID)
		{
			int num = audioClipContainers.Length;
			int num2 = (int)clipID * 5387 % num;
			RandomAudioClipContainer randomAudioClipContainer = audioClipContainers[num2];
			if (randomAudioClipContainer.clipID != clipID)
			{
				for (int i = 0; i < num; i++)
				{
					int num3 = (num2 + i) % num;
					if (audioClipContainers[num3] == null)
					{
						randomAudioClipContainer = AddNewContainer(audioClipContainers, num3, clipID);
						break;
					}
					if (audioClipContainers[num3].clipID == clipID)
					{
						randomAudioClipContainer = audioClipContainers[num3];
						break;
					}
				}
				if (randomAudioClipContainer.clipID != clipID)
				{
					randomAudioClipContainer = null;
				}
			}
			return randomAudioClipContainer;
		}

		public AudioClip GetAudioClipById(AudioClipID audioClipID)
		{
			if (audioClipContainers.Length == 0)
			{
				return null;
			}
			RandomAudioClipContainer audioClipContainerFromID = GetAudioClipContainerFromID(ref audioClipContainers, audioClipID);
			AudioClip result = null;
			if (audioClipContainerFromID != null && audioClipContainerFromID.clips != null)
			{
				result = audioClipContainerFromID.clips[0];
			}
			return result;
		}

		public AudioClip GetRandomAudioClipById(AudioClipID audioClipID)
		{
			if (audioClipContainers.Length == 0)
			{
				return null;
			}
			RandomAudioClipContainer audioClipContainerFromID = GetAudioClipContainerFromID(ref audioClipContainers, audioClipID);
			AudioClip result = null;
			if (audioClipContainerFromID != null && audioClipContainerFromID.clipID == audioClipID && audioClipContainerFromID.clips != null)
			{
				int num = audioClipContainerFromID.clips.Length;
				int num2 = UnityEngine.Random.Range(0, num);
				if (num2 == audioClipContainerFromID.lastRandomClip)
				{
					num2 = (num2 + 1) % num;
				}
				audioClipContainerFromID.lastRandomClip = num2;
				result = audioClipContainerFromID.clips[num2];
			}
			return result;
		}

		public AudioClip GetAudioClipByIndex(AudioClipID audioClipID, int index)
		{
			if (audioClipContainers.Length == 0)
			{
				return null;
			}
			RandomAudioClipContainer audioClipContainerFromID = GetAudioClipContainerFromID(ref audioClipContainers, audioClipID);
			AudioClip result = null;
			if (audioClipContainerFromID != null && audioClipContainerFromID.clips != null && index < audioClipContainerFromID.clips.Length)
			{
				result = audioClipContainerFromID.clips[index];
			}
			return result;
		}

		public void LoadAllAudioClips(Assets assets)
		{
			for (int i = 0; i < audioClipContainers.Length; i++)
			{
				AudioClip[] clips = audioClipContainers[i].clips;
				for (int j = 0; j < clips.Length; j++)
				{
					assets.LoadAudioClip(clips[j]);
				}
			}
		}

		public IEnumerator LoadAllAudioClipsASync(Assets assets)
		{
			for (int containerIndex = 0; containerIndex < audioClipContainers.Length; containerIndex++)
			{
				AudioClip[] audioClips = audioClipContainers[containerIndex].clips;
				for (int clipIndex = 0; clipIndex < audioClips.Length; clipIndex++)
				{
					yield return assets.LoadAudioClipASync(audioClips[clipIndex]);
				}
			}
		}

		public void UnloadAllAudioClips()
		{
			for (int i = 0; i < audioClipContainers.Length; i++)
			{
				AudioClip[] clips = audioClipContainers[i].clips;
				for (int j = 0; j < clips.Length; j++)
				{
					clips[j].UnloadAudioData();
				}
			}
		}
	}
}
