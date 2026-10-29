using System;
using UnityEngine;

namespace Reptile
{
	public class AnimationEventRelay : MonoBehaviour
	{
		protected Player player;

		public void Init()
		{
			player = base.transform.parent.parent.GetComponentInParent<Player>();
		}

		public void PlaySound(AnimationEvent animationEvent)
		{
			if (!(player == null))
			{
				AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), animationEvent.stringParameter, ignoreCase: true);
				int index = Mathf.Max(0, animationEvent.intParameter - 1);
				player.AudioManager.PlaySfxGameplayByindex(SfxCollectionID.GenericMovementSfx, audioClipId, player.playerOneShotAudioSource, index);
			}
		}

		public void PlaySoundMoveStyle(AnimationEvent animationEvent)
		{
			if (!(player == null))
			{
				AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), animationEvent.stringParameter, ignoreCase: true);
				int index = Mathf.Max(0, animationEvent.intParameter - 1);
				player.AudioManager.PlaySfxGameplayByindex(player.moveStyle, audioClipId, player.playerOneShotAudioSource, index);
			}
		}

		public void PlaySoundRandomPitch(AnimationEvent animationEvent)
		{
			if (!(player == null))
			{
				AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), animationEvent.stringParameter, ignoreCase: true);
				int index = Mathf.Max(0, animationEvent.intParameter - 1);
				player.AudioManager.PlaySfxGameplayByindex(SfxCollectionID.GenericMovementSfx, audioClipId, player.playerOneShotAudioSource, index, 0.05f);
			}
		}

		public void PlayRandomSound(string soundName)
		{
			if (!(player == null))
			{
				AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true);
				player.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, audioClipId, player.playerOneShotAudioSource);
			}
		}

		public void PlayRandomSoundMoveStyle(string soundName)
		{
			if (!(player == null))
			{
				AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true);
				player.AudioManager.PlaySfxGameplay(player.moveStyle, audioClipId, player.playerOneShotAudioSource);
			}
		}

		public void PlayRandomSoundRandomPitch(string soundName)
		{
			if (!(player == null))
			{
				AudioClipID audioClipId = (AudioClipID)Enum.Parse(typeof(AudioClipID), soundName, ignoreCase: true);
				player.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, audioClipId, player.playerOneShotAudioSource, 0.05f);
			}
		}

		public void PlayFootstepSound()
		{
			if (!(player == null))
			{
				player.AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_0_Default, AudioClipID.footstep, player.playerOneShotAudioSource);
			}
		}

		public void PlayPedalLeftSound()
		{
			if (!(player == null) && player.IsGrounded())
			{
				player.AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_1_BMX, AudioClipID.pedalLeft, player.playerOneShotAudioSource);
			}
		}

		public void PlayPedalRightSound()
		{
			if (!(player == null) && player.IsGrounded())
			{
				player.AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_1_BMX, AudioClipID.pedalRight, player.playerOneShotAudioSource);
			}
		}

		public void PlayPedalLeftShortSound()
		{
			if (!(player == null) && player.IsGrounded())
			{
				player.AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_1_BMX, AudioClipID.PedalLeftShort, player.playerOneShotAudioSource);
			}
		}

		public void PlayPedalRightShortSound()
		{
			if (!(player == null) && player.IsGrounded())
			{
				player.AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_1_BMX, AudioClipID.PedalRightShort, player.playerOneShotAudioSource);
			}
		}

		public void PlayStrideLeftSound()
		{
			if (!(player == null))
			{
				player.AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_3_Inline, AudioClipID.strideLeft, player.playerOneShotAudioSource);
			}
		}

		public void PlayStrideRightSound()
		{
			if (!(player == null))
			{
				player.AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_3_Inline, AudioClipID.strideRight, player.playerOneShotAudioSource);
			}
		}

		public void ActivateLeftLegCollider()
		{
			if (!(player == null))
			{
				player.hitboxLeftLeg.SetActive(value: true);
			}
		}

		public void DeactivateLeftLegCollider()
		{
			if (!(player == null))
			{
				player.hitboxLeftLeg.SetActive(value: false);
			}
		}

		public void ActivateRightLegCollider()
		{
			if (!(player == null))
			{
				player.hitboxRightLeg.SetActive(value: true);
			}
		}

		public void DeactivateRightLegCollider()
		{
			if (!(player == null))
			{
				player.hitboxRightLeg.SetActive(value: false);
			}
		}

		public void ActivateUpperBodyCollider()
		{
			if (!(player == null))
			{
				player.hitboxUpperBody.SetActive(value: true);
			}
		}

		public void DeactivateUpperBodyCollider()
		{
			if (!(player == null))
			{
				player.hitboxUpperBody.SetActive(value: false);
			}
		}
	}
}
