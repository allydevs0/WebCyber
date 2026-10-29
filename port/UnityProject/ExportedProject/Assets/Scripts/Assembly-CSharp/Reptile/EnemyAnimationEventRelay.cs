using System;
using UnityEngine;

namespace Reptile
{
	public class EnemyAnimationEventRelay : MonoBehaviour
	{
		[SerializeField]
		public AudioSource oneShotAudioSource;

		private AudioManager audioManager;

		public void Init()
		{
			audioManager = Core.Instance.AudioManager;
		}

		public void PlaySound(AnimationEvent animationEvent)
		{
			AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), animationEvent.stringParameter, ignoreCase: true);
			int index = Mathf.Max(0, animationEvent.intParameter - 1);
			audioManager.PlaySfxGameplayByindex(SfxCollectionID.CombatSfx, audioClipId, oneShotAudioSource, index);
		}

		public void PlaySoundRandomPitch(AnimationEvent animationEvent)
		{
			AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), animationEvent.stringParameter, ignoreCase: true);
			int index = Mathf.Max(0, animationEvent.intParameter - 1);
			audioManager.PlaySfxGameplayByindex(SfxCollectionID.CombatSfx, audioClipId, oneShotAudioSource, index, 0.05f);
		}

		public void PlayRandomSound(string soundName)
		{
			AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true);
			audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, audioClipId, oneShotAudioSource);
		}

		public void PlayRandomSoundRandomPitch(string soundName)
		{
			AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true);
			audioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, audioClipId, oneShotAudioSource, 0.05f);
		}
	}
}
