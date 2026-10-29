using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[Serializable]
	public class VoiceSfxTimelineClip : AudioPlayableAsset
	{
		public static SfxCollectionID sfxCollection = SfxCollectionID.VoiceMetalHead;

		[AudioClipFilter("sfxCollection")]
		[SerializeField]
		public AudioClipID voiceClip;

		[SerializeField]
		public bool randomAudioClip;

		internal static AudioClip GetAudioClip(AudioClipID audioClipID, bool randomAudioClip)
		{
			SfxCollectionID characterVoiceSfxCollection = sfxCollection;
			AudioManager audioManager = Core.Instance.AudioManager;
			Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			if (currentPlayer != null)
			{
				characterVoiceSfxCollection = audioManager.GetCharacterVoiceSfxCollection(currentPlayer.character);
			}
			if (randomAudioClip)
			{
				return audioManager.GetRandomAudioClipFromID(characterVoiceSfxCollection, audioClipID);
			}
			return audioManager.GetAudioClipFromID(characterVoiceSfxCollection, audioClipID);
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
			if (Core.Instance.AudioManager != null)
			{
				base.clip = GetAudioClip(voiceClip, randomAudioClip);
			}
			if (base.clip == null)
			{
				return Playable.Null;
			}
			return base.CreatePlayable(graph, go);
		}
	}
}
