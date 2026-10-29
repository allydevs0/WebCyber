using System;
using UnityEngine;

namespace Reptile
{
	public class SnakeBossAnimationEventRelay : MonoBehaviour
	{
		private AudioManager audioManager;

		private SnakeBossNEW snakeBoss;

		public void Init(SnakeBossNEW snake)
		{
			audioManager = Core.Instance.AudioManager;
			snakeBoss = snake;
		}

		public void PlaySound(AnimationEvent animationEvent)
		{
			AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), animationEvent.stringParameter, ignoreCase: true);
			int index = Mathf.Max(0, animationEvent.intParameter - 1);
			audioManager.PlaySfxGameplayByindex(SfxCollectionID.CombatSfx, audioClipId, snakeBoss.GetAudioSourceForAnimationEvents(), index);
		}

		public void PlaySoundRandomPitch(AnimationEvent animationEvent)
		{
			AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), animationEvent.stringParameter, ignoreCase: true);
			int index = Mathf.Max(0, animationEvent.intParameter - 1);
			audioManager.PlaySfxGameplayByindex(SfxCollectionID.CombatSfx, audioClipId, snakeBoss.GetAudioSourceForAnimationEvents(), index, 0.05f);
		}

		public void PlayRandomSound(string soundName)
		{
			AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true);
			audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, audioClipId, snakeBoss.GetAudioSourceForAnimationEvents());
		}

		public void PlayRandomSoundRandomPitch(string soundName)
		{
			AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true);
			audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, audioClipId, snakeBoss.GetAudioSourceForAnimationEvents(), 0.05f);
		}
	}
}
