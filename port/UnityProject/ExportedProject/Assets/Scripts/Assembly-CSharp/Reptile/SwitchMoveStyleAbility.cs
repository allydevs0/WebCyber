using System;
using UnityEngine;

namespace Reptile
{
	public class SwitchMoveStyleAbility : Ability
	{
		private bool stopDecided;

		private bool wantToStop;

		private MoveStyle switchingFromMovestyle;

		private float changePropAfterDuration;

		private float duration = 0.75f;

		private float durationStanding = 1f;

		private bool propChanged;

		private bool bufferedTrick;

		private bool bufferedSlide;

		private bool bufferedJump;

		private bool isMoving;

		private float cancelBeforeEnd = 0.3f;

		private int stopRunHash = Animator.StringToHash("stopRun");

		private int[] switchFromEquippedMovestyleSkateboardHashes = new int[2]
		{
			Animator.StringToHash("switchFromEquippedMovestyleSkateboardStanding"),
			Animator.StringToHash("switchFromEquippedMovestyleSkateboardMoving")
		};

		private int[] switchFromEquippedMovestyleBMXHashes = new int[2]
		{
			Animator.StringToHash("switchFromEquippedMovestyleBMXStanding"),
			Animator.StringToHash("switchFromEquippedMovestyleBMXMoving")
		};

		private int[] switchFromEquippedMovestyleSkatesHashes = new int[2]
		{
			Animator.StringToHash("switchFromEquippedMovestyleSkatesStanding"),
			Animator.StringToHash("switchFromEquippedMovestyleSkatesMoving")
		};

		private int[] switchToEquippedMovestyleHashes = new int[2]
		{
			Animator.StringToHash("switchToEquippedMovestyleStanding"),
			Animator.StringToHash("switchToEquippedMovestyleMoving")
		};

		public SwitchMoveStyleAbility(Player player)
			: base(player)
		{
			canUseSpraycan = false;
		}

		public override void OnStartAbility()
		{
			switchingFromMovestyle = p.moveStyle;
			p.SwitchToEquippedMovestyle(!p.usingEquippedMovestyle, doAirTrick: false, changeProp: false, showEffect: false);
			if (p.IsComboing())
			{
				p.DoComboTimeOut(0.125f);
			}
			isMoving = p.GetForwardSpeed() > p.minMoveSpeed;
			int num = Convert.ToInt32(isMoving);
			if (switchingFromMovestyle == MoveStyle.SKATEBOARD || switchingFromMovestyle == MoveStyle.SPECIAL_SKATEBOARD)
			{
				changePropAfterDuration = 0.5f;
				if (!isMoving)
				{
					changePropAfterDuration = 1.2f;
				}
				p.PlayAnim(switchFromEquippedMovestyleSkateboardHashes[num], forceOverwrite: true, instant: false, 0f);
			}
			else if (switchingFromMovestyle == MoveStyle.BMX)
			{
				changePropAfterDuration = 0.65f;
				if (!isMoving)
				{
					changePropAfterDuration = 0.8f;
				}
				p.PlayAnim(switchFromEquippedMovestyleBMXHashes[num], forceOverwrite: true, instant: false, 0f);
			}
			else if (switchingFromMovestyle == MoveStyle.INLINE)
			{
				changePropAfterDuration = 0.3f;
				if (!isMoving)
				{
					changePropAfterDuration = 0.5f;
				}
				p.PlayAnim(switchFromEquippedMovestyleSkatesHashes[num], forceOverwrite: true, instant: false, 0f);
			}
			else
			{
				changePropAfterDuration = 0f;
				p.PlayAnim(switchToEquippedMovestyleHashes[num], forceOverwrite: true, instant: false, 0f);
			}
			targetSpeed = (isMoving ? p.GetForwardSpeed() : 0f);
			rotateSpeed = 3f;
			acc = 35f;
			decc = p.stats.groundDecc / 2.5f;
			bufferedTrick = (bufferedJump = (bufferedSlide = (propChanged = false)));
		}

		public override void FixedUpdateAbility()
		{
			p.OrientVisualToSurfaceInAbility();
			if (isMoving)
			{
				durationStanding = 0f;
			}
			if (!propChanged && p.abilityTimer > changePropAfterDuration)
			{
				MoveStyle moveStyle = p.moveStyle;
				if (moveStyle == MoveStyle.ON_FOOT)
				{
					moveStyle = switchingFromMovestyle;
				}
				switch (moveStyle)
				{
				case MoveStyle.BMX:
					p.AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_1_BMX, AudioClipID.TailWhip, p.playerOneShotAudioSource);
					break;
				case MoveStyle.INLINE:
					p.AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_3_Inline, AudioClipID.land, p.playerOneShotAudioSource);
					break;
				}
				if (moveStyle == MoveStyle.SKATEBOARD)
				{
					p.AudioManager.PlaySfxGameplay(SfxCollectionID.MoveStyle_2_SkateBoard, AudioClipID.groundTrick2, p.playerOneShotAudioSource);
				}
				p.characterVisual.SetMoveStyleVisualProps(p, p.moveStyle);
				p.characterVisual.DoMoveStyleEquipEffect(p.moveStyleEquipped);
				propChanged = true;
			}
			if (p.abilityTimer > duration - 0.5f)
			{
				if (p.AnyTrickInput())
				{
					bufferedTrick = true;
				}
				if (p.slideButtonHeld)
				{
					bufferedSlide = true;
				}
				if (p.jumpButtonNew)
				{
					bufferedJump = true;
				}
			}
			if (p.vertAbility.CheckActivation())
			{
				return;
			}
			if (!p.IsGrounded())
			{
				if (p.abilityTimer > 0.5f)
				{
					p.StopCurrentAbility();
				}
				return;
			}
			if (p.abilityTimer > duration + durationStanding)
			{
				p.StopCurrentAbility();
				return;
			}
			if (isMoving && p.abilityTimer > duration - 0.25f)
			{
				if (!stopDecided)
				{
					wantToStop = p.moveInput.magnitude < 0.5f;
					stopDecided = true;
				}
				if (!wantToStop)
				{
					targetSpeed = p.maxMoveSpeed;
					rotateSpeed = p.stats.rotSpeedAtMaxSpeed;
				}
				else
				{
					targetSpeed = 0f;
					p.PlayAnim(stopRunHash);
				}
			}
			if (p.abilityTimer > duration - cancelBeforeEnd)
			{
				if (bufferedJump)
				{
					p.StopCurrentAbility();
					p.Jump();
				}
				else if (bufferedSlide && !p.slideAbility.locked && p.slideButtonHeld)
				{
					p.ActivateAbility(p.slideAbility);
				}
			}
		}

		public override void OnStopAbility()
		{
			durationStanding = 1f;
			p.characterVisual.SetMoveStyleVisualProps(p, p.moveStyle);
			base.OnStopAbility();
		}

		public override bool CheckActivation()
		{
			if (p.switchStyleButtonNew && !p.switchToEquippedMovestyleLocked && p.IsGrounded() && !p.inWalkZone)
			{
				p.ActivateAbility(this);
				return true;
			}
			return false;
		}
	}
}
