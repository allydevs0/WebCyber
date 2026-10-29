using UnityEngine;

namespace Reptile
{
	public class SpecialAirAbility : Ability
	{
		private bool hitEnemy;

		private float jumpSpeed = 11.5f;

		public float duration = 1.4f;

		private bool didTrick;

		private int jumpTrick1Hash = Animator.StringToHash("jumpTrick1");

		public SpecialAirAbility(Player player)
			: base(player)
		{
		}

		public override void OnStartAbility()
		{
			hitEnemy = false;
			p.jumpedThisFrame = true;
			p.isJumping = true;
			p.maintainSpeedJump = false;
			p.jumpConsumed = true;
			p.jumpRequested = false;
			p.jumpedThisFrame = true;
			p.timeSinceLastJump = 0f;
			p.ForceUnground();
			Vector3 velocity = p.GetVelocity();
			velocity.y = jumpSpeed;
			p.SetVelocity(velocity);
			p.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.jump_special, p.playerOneShotAudioSource);
			p.PlayVoice(AudioClipID.VoiceJump);
			p.radialHitbox.SetActive(value: true);
			p.PlayAnim(jumpTrick1Hash);
			p.DoHighJumpEffects(p.motor.groundNormalVisual * -1f);
			didTrick = false;
		}

		public override void FixedUpdateAbility()
		{
			if (p.abilityTimer > duration * 0.035f && p.radialHitbox.activeSelf)
			{
				p.radialHitbox.SetActive(value: false);
			}
			if (!didTrick && p.abilityTimer > duration * 0.25f)
			{
				p.DoTrick(Player.TrickType.SPECIAL_AIR, "Corkscrew");
				didTrick = true;
			}
			if (!p.slideAbility.CheckActivation() && !p.ledgeClimbAbility.CheckActivation())
			{
				if (p.abilityTimer < 0.4f)
				{
					p.radialHitbox.SetActive(value: true);
				}
				else
				{
					p.radialHitbox.SetActive(value: false);
				}
				if (p.IsGrounded() || p.abilityTimer > duration)
				{
					p.StopCurrentAbility();
				}
				else if (p.abilityTimer > duration - (hitEnemy ? 1.2f : 0.35f) && (p.airDashAbility.CheckActivation() || p.airTrickAbility.CheckActivation()))
				{
					return;
				}
				p.SetVisualRotLocal0();
			}
		}

		public void HitEnemy()
		{
			hitEnemy = true;
		}

		public override void OnStopAbility()
		{
			p.radialHitbox.SetActive(value: false);
		}
	}
}
