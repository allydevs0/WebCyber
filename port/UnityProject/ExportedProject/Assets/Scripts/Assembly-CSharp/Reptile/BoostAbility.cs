using UnityEngine;

namespace Reptile
{
	public class BoostAbility : Ability
	{
		public enum State
		{
			START_BOOST = 0,
			BOOST = 1,
			BRAKE = 2
		}

		private bool equippedMovestyleWasUsed;

		private float stateTimer;

		private float uncuffTimer;

		private float uncuffInterval = 1f;

		public bool haveAirStartBoost;

		private bool inAir;

		private int boostBrakeHash = Animator.StringToHash("boostBrake");

		private int fallHash = Animator.StringToHash("fall");

		private int jumpHash = Animator.StringToHash("jump");

		private int boostRunHash = Animator.StringToHash("boostRun");

		private int startBoostHash = Animator.StringToHash("startBoost");

		private State state;

		public BoostAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			allowNormalJump = true;
		}

		public override void OnStartAbility()
		{
			haveAirStartBoost = false;
			equippedMovestyleWasUsed = p.usingEquippedMovestyle;
			if (p.moveStyleEquipped != MoveStyle.INLINE)
			{
				p.SwitchToEquippedMovestyle(set: false, doAirTrick: false, changeProp: true, showEffect: false);
			}
			SetState(State.START_BOOST);
		}

		public void StartFromJumpGrindOrWallrun()
		{
			haveAirStartBoost = true;
			SetState(State.BOOST);
			LeaveGround();
			p.PlayAnim(jumpHash);
		}

		public void StartFromRunOffGrindOrWallrun()
		{
			haveAirStartBoost = true;
			SetState(State.BOOST);
			LeaveGround();
		}

		public void SetState(State setState)
		{
			stateTimer = 0f;
			state = setState;
			switch (state)
			{
			case State.START_BOOST:
				p.ResetLean();
				inAir = false;
				p.characterVisual.SetBoostpackEffect(BoostpackEffectMode.OFF);
				p.boosting = false;
				p.hitbox.SetActive(value: false);
				p.PlayAnim(startBoostHash);
				targetSpeed = 0f;
				rotateSpeed = 3f;
				decc = 10f;
				break;
			case State.BOOST:
				p.ringParticles.Emit(1);
				p.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.boostStart, p.playerOneShotAudioSource);
				p.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.GenericMovementSfx, AudioClipID.ultraBoostLoop, p.playerUltraBoostLoopAudioSource);
				p.characterVisual.SetBoostpackEffect(BoostpackEffectMode.BOOSTING);
				p.boosting = true;
				p.hitbox.SetActive(value: true);
				p.PlayAnim(boostRunHash);
				targetSpeed = p.boostSpeed;
				p.SetSpeedFlat(p.boostSpeed);
				rotateSpeed = 0.9f;
				p.TryToRemoveCuff();
				uncuffTimer = 0f;
				break;
			case State.BRAKE:
				if (p.moveStyleEquipped != MoveStyle.INLINE)
				{
					p.SwitchToEquippedMovestyle(equippedMovestyleWasUsed, doAirTrick: false, changeProp: true, showEffect: false);
				}
				p.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.boostStop, p.playerOneShotAudioSource);
				p.AudioManager.StopLoopingSfx(p.playerUltraBoostLoopAudioSource);
				p.characterVisual.SetBoostpackEffect(BoostpackEffectMode.OFF);
				p.boosting = false;
				p.PlayAnim(boostBrakeHash);
				rotateSpeed = 0.5f;
				targetSpeed = 0f;
				decc = 13f;
				p.hitbox.SetActive(value: false);
				break;
			}
		}

		public override void FixedUpdateAbility()
		{
			p.OrientVisualToSurfaceInAbility();
			stateTimer += Core.dt;
			if (p.slideAbility.CheckActivation())
			{
				p.SetSpeedFlat(Mathf.Min(p.slideAbility.slopeSlideSpeed, p.GetForwardSpeed()));
			}
			else
			{
				if (p.groundTrickAbility.CheckActivation() || p.airDashAbility.CheckActivation() || p.vertAbility.CheckActivation() || p.airTrickAbility.CheckActivation())
				{
					return;
				}
				if (state == State.START_BOOST)
				{
					if ((double)p.abilityTimer > 0.25)
					{
						SetState(State.BOOST);
					}
				}
				else if (state == State.BOOST)
				{
					if (!p.isAI)
					{
						p.StartScreenShake(ScreenShakeType.ULTRALIGHT, 0.3f, continuous: true);
						Core.Instance.GameInput.SetVibrationOnCurrentController(0.19f, 0.19f, 0.1f);
					}
					p.UseBoostChargeForBoost();
					TryToRemoveCuffs();
					if (!p.HasBoostCharge() || (!p.boostButtonHeld && (double)p.abilityTimer > 0.5))
					{
						if (p.IsGrounded())
						{
							SetState(State.BRAKE);
						}
						else
						{
							p.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.boostStop, p.playerOneShotAudioSource);
							p.StopCurrentAbility();
						}
					}
					else if (inAir)
					{
						p.characterVisual.SetBoostpackEffect(BoostpackEffectMode.BOOSTING, Mathf.Max(Mathf.Lerp(-1f, 1.5f, (p.GetForwardSpeed() - p.maxMoveSpeed) / (p.boostSpeed - p.maxMoveSpeed)), 0.62f));
						if (p.GetVelocity().y <= 0f && p.curAnim == jumpHash)
						{
							p.PlayAnim(fallHash);
						}
						if (p.IsGrounded())
						{
							SetState(State.BOOST);
							inAir = false;
						}
					}
					else if (!p.IsGrounded())
					{
						LeaveGround();
					}
				}
				else if (state == State.BRAKE && (p.curAnim != boostBrakeHash || (p.moveInput != Vector3.zero && stateTimer > 0.5f) || !p.IsGrounded()))
				{
					p.StopCurrentAbility();
				}
				p.ledgeClimbAbility.CheckActivation();
			}
		}

		private void TryToRemoveCuffs()
		{
			uncuffTimer += Core.dt;
			if (uncuffTimer > uncuffInterval)
			{
				p.TryToRemoveCuff();
				uncuffTimer = 0f;
			}
		}

		public override void RecheckLoopingSoundInAbilityUse()
		{
			if (state == State.BOOST)
			{
				p.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.GenericMovementSfx, AudioClipID.ultraBoostLoop, p.playerUltraBoostLoopAudioSource);
			}
		}

		public override bool CheckActivation()
		{
			if (p.boostButtonHeld && p.HasBoostCharge() && !locked && (haveAirStartBoost || p.IsGrounded()))
			{
				p.ActivateAbility(this);
				return true;
			}
			return false;
		}

		private void SetDust(float size)
		{
			p.SetDustEmission((size > 0f) ? 100 : 0);
			p.SetDustSize(Mathf.Max(size, 0f));
		}

		public override void OnJump()
		{
			LeaveGround();
		}

		public void LeaveGround()
		{
			targetSpeed = p.maxMoveSpeed;
			rotateSpeed = 1f;
			decc = p.stats.airDecc / 1.5f;
			inAir = true;
		}

		public override void OnStopAbility()
		{
			if (equippedMovestyleWasUsed && p.moveStyleEquipped != MoveStyle.INLINE)
			{
				p.SwitchToEquippedMovestyle(set: true, doAirTrick: false, changeProp: true, showEffect: false);
			}
			p.boosting = false;
			p.hitbox.SetActive(value: false);
			p.AudioManager.StopLoopingSfx(p.playerUltraBoostLoopAudioSource);
			p.characterVisual.SetBoostpackEffect(BoostpackEffectMode.OFF);
			SetDust(0f);
		}
	}
}
