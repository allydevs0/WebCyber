using System;
using UnityEngine;

namespace Reptile
{
	public class NonPlayerAnimationEventRelay : MonoBehaviour
	{
		private AudioSource audioSource;

		private AudioManager audioManager;

		public void Init()
		{
			audioSource = base.gameObject.AddComponent<AudioSource>();
			audioSource.spatialBlend = 1f;
			audioManager = Core.Instance.AudioManager;
		}

		public void PlaySound(string soundName)
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true), audioSource);
		}

		public void PlaySoundMoveStyle(string soundName)
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true), audioSource);
		}

		public void PlaySoundRandomPitch(string soundName)
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true), audioSource, 0.05f);
		}

		public void PlayRandomSound(string soundName)
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true), audioSource);
		}

		public void PlayRandomSoundMoveStyle(string soundName)
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true), audioSource);
		}

		public void PlayRandomSoundRandomPitch(string soundName)
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true), audioSource, 0.05f);
		}

		public void PlayFootstepSound()
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_0_Default, AudioClipID.footstep, audioSource, 0.05f);
		}

		public void PlayPedalLeftSound()
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_1_BMX, AudioClipID.pedalLeft, audioSource, 0.05f);
		}

		public void PlayPedalRightSound()
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_1_BMX, AudioClipID.pedalRight, audioSource, 0.05f);
		}

		public void PlayPedalLeftShortSound()
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_1_BMX, AudioClipID.pedalLeft, audioSource, 0.05f);
		}

		public void PlayPedalRightShortSound()
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_1_BMX, AudioClipID.PedalRightShort, audioSource, 0.05f);
		}

		public void PlayStrideLeftSound()
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_3_Inline, AudioClipID.strideLeft, audioSource, 0.05f);
		}

		public void PlayStrideRightSound()
		{
			audioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_3_Inline, AudioClipID.strideRight, audioSource, 0.05f);
		}
	}
}
