using UnityEngine;

namespace Reptile
{
	public class SlideAbility : Ability
	{
		public enum SlideState
		{
			ROLL = 0,
			NORMAL = 1,
			SLOPE = 2,
			SUPER = 3,
			GET_UP = 4
		}

		private float stateTimer;

		private float stopBySpeedTimer;

		private bool stopDecided;

		private bool wantToStop;

		private string trickName;

		private bool slideEffect;

		private bool boostEffectOn;

		private float baseSpeed;

		internal float slopeSlideSpeed = 14f;

		private float normalDecc = 1.6f;

		private float stopDecc = 16f;

		private float normalAcc = 22.5f;

		private float superAcc = 100f;

		private float momentumSlideDecc = 0.8f;

		public float stopAtSpeed = 5.8f;

		public float superSpeed = 14.2f;

		private float singleBoostCooldown;

		public SlideState slideState;

		private bool crouched;

		private float normalCapsuleHeight;

		private float normalCapsuleCenter;

		private float slopeDot;

		private float superBuildUp;

		private float maxBuildUp = 0.35f;

		private float slideAnimDir;

		private float uncuffTimer;

		private float uncuffInterval = 1f;

		private int stopRunHash = Animator.StringToHash("stopRun");

		private int rollHash = Animator.StringToHash("roll");

		private int slideToRunHash = Animator.StringToHash("slideToRun");

		private int slideDirectionHash = Animator.StringToHash("slideDirection");

		public bool IsPerformingManual
		{
			get
			{
				if (p.moveStyle != MoveStyle.SKATEBOARD && p.moveStyle != MoveStyle.BMX)
				{
					return p.moveStyle == MoveStyle.INLINE;
				}
				return true;
			}
		}

		public SlideAbility(Player player)
			: base(player)
		{
		}

		public override void OnStartAbility()
		{
			setSpeedOnHittingBreakable = (baseSpeed = p.maxMoveSpeed);
			singleBoostCooldown = 0f;
			normalCapsuleHeight = p.motor.GetCapsule().height;
			normalCapsuleCenter = p.motor.GetCapsule().center.y;
			autoAirTrickFromLauncher = false;
			wantToStop = (stopDecided = false);
			if (p.moveStyle == MoveStyle.ON_FOOT)
			{
				trickName = "Slide";
				Crouch(set: true);
			}
			else if (p.moveStyle == MoveStyle.SKATEBOARD)
			{
				trickName = "Manual";
			}
			else if (p.moveStyle == MoveStyle.INLINE)
			{
				trickName = "Cess Slide";
			}
			else if (p.moveStyle == MoveStyle.BMX)
			{
				trickName = "Manual";
			}
			SetSlideState(SlideState.ROLL);
			if (p.lastElevationForSlideBoost > p.tf.position.y + 0.5f)
			{
				SingleBoost(p.boostSpeed - 1f);
			}
		}

		public void SetSlideState(SlideState setState)
		{
			superBuildUp = 0f;
			stateTimer = 0f;
			slideState = setState;
			stopBySpeedTimer = 0f;
			switch (slideState)
			{
			case SlideState.ROLL:
				if (!p.IsComboing())
				{
					p.RefillComboTimeOut();
				}
				slideEffect = false;
				boostEffectOn = false;
				p.PlayAnim(rollHash);
				rotateSpeed = 0.3f;
				targetSpeed = baseSpeed;
				p.SetSpeedFlat(Mathf.Max(baseSpeed, p.GetForwardSpeed()));
				if (p.comboTimeOutTimer <= 0f)
				{
					p.LandCombo();
				}
				p.hitbox.SetActive(value: false);
				p.hitboxLeftLeg.SetActive(value: false);
				p.hitboxRightLeg.SetActive(value: false);
				break;
			case SlideState.NORMAL:
				slideEffect = true;
				boostEffectOn = false;
				rotateSpeed = 2f;
				targetSpeed = 2.5f;
				if (p.moveStyle != MoveStyle.ON_FOOT)
				{
					p.hitbox.SetActive(value: true);
				}
				else
				{
					p.hitboxLeftLeg.SetActive(value: true);
					p.hitboxRightLeg.SetActive(value: true);
				}
				p.AudioManager.PlaySfxGameplayLooping(p.moveStyle, AudioClipID.slideLoop, p.playerMovementLoopAudioSource);
				p.AudioManager.StopLoopingSfx(p.playerSlideAudioSource);
				break;
			case SlideState.SLOPE:
				slideEffect = true;
				boostEffectOn = true;
				if (p.moveStyle != MoveStyle.ON_FOOT)
				{
					p.hitbox.SetActive(value: true);
				}
				else
				{
					p.hitboxLeftLeg.SetActive(value: true);
					p.hitboxRightLeg.SetActive(value: true);
				}
				p.TryToRemoveCuff();
				rotateSpeed = 2f;
				uncuffTimer = 0f;
				p.AudioManager.PlaySfxGameplayLooping(p.moveStyle, AudioClipID.slideLoop, p.playerMovementLoopAudioSource);
				p.AudioManager.StopLoopingSfx(p.playerSlideAudioSource);
				break;
			case SlideState.SUPER:
				slideEffect = true;
				boostEffectOn = true;
				if (p.moveStyle != MoveStyle.ON_FOOT)
				{
					p.hitbox.SetActive(value: true);
				}
				else
				{
					p.hitboxLeftLeg.SetActive(value: true);
					p.hitboxRightLeg.SetActive(value: true);
				}
				rotateSpeed = 2f;
				p.TryToRemoveCuff();
				if (p.moveStyle == MoveStyle.ON_FOOT || p.moveStyle == MoveStyle.INLINE)
				{
					p.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.GenericMovementSfx, AudioClipID.slideSuperLoop, p.playerMovementLoopAudioSource);
				}
				else if (p.moveStyle == MoveStyle.SKATEBOARD || p.moveStyle == MoveStyle.BMX)
				{
					p.AudioManager.PlaySfxGameplayLooping(p.moveStyle, AudioClipID.slideLoop, p.playerMovementLoopAudioSource);
				}
				uncuffTimer = 0f;
				break;
			case SlideState.GET_UP:
				Crouch(set: false);
				p.PlayAnim(slideToRunHash);
				rotateSpeed = 2f;
				slideEffect = false;
				boostEffectOn = false;
				p.AudioManager.StopLoopingSfx(p.playerMovementLoopAudioSource);
				p.AudioManager.StopLoopingSfx(p.playerSlideAudioSource);
				p.hitbox.SetActive(value: false);
				p.hitboxLeftLeg.SetActive(value: false);
				p.hitboxRightLeg.SetActive(value: false);
				break;
			}
		}

		public override void FixedUpdateAbility()
		{
			stateTimer += Core.dt;
			bool flag = false;
			if (singleBoostCooldown > 0f)
			{
				singleBoostCooldown -= Core.dt;
			}
			Vector3 moveInput = p.moveInput;
			bool flag2 = RoomToUncrouch();
			if (!flag2)
			{
				targetSpeed = Mathf.Max(targetSpeed, 6.5f);
			}
			allowNormalJump = slideState != SlideState.ROLL && (!crouched | flag2);
			slopeDot = Vector3.Dot(Vector3.ProjectOnPlane(p.motor.groundNormal, Vector3.up).normalized, p.dir);
			switch (slideState)
			{
			case SlideState.ROLL:
				p.DoTrick(Player.TrickType.SLIDE, trickName);
				SetDust(0f);
				acc = normalAcc;
				decc = 0f;
				if (p.curAnim != rollHash || stateTimer > 0.15f)
				{
					if (GoingDown())
					{
						SetSlideState(SlideState.SLOPE);
					}
					else
					{
						SetSlideState(SlideState.NORMAL);
					}
				}
				break;
			case SlideState.NORMAL:
			{
				p.DoTrick(Player.TrickType.SLIDE, trickName);
				float num2 = Mathf.Min(p.GetForwardSpeed() / p.boostSpeed, 0.95f);
				float num3 = 1.3f;
				if (p.moveStyle == MoveStyle.BMX || p.moveStyle == MoveStyle.SKATEBOARD)
				{
					num3 = 0.7f;
				}
				p.DoComboTimeOut(Core.dt * (1f - num2) * num3);
				decc = Mathf.Lerp(p.stats.slideDeccLowSpeed, p.stats.slideDeccHighSpeed, num2);
				if (GoingUp() && p.moveStyle == MoveStyle.ON_FOOT)
				{
					decc = p.stats.groundDecc;
				}
				if (p.IsGrounded())
				{
					SetDust(0.5f);
				}
				else
				{
					SetDust(0f);
				}
				if (p.inSpeedZone && p.moveStyle == MoveStyle.ON_FOOT)
				{
					SetSlideState(SlideState.SUPER);
				}
				else if (GoingDown())
				{
					SetSlideState(SlideState.SLOPE);
				}
				if (p.moveStyle == MoveStyle.INLINE)
				{
					slideEffect = true;
				}
				break;
			}
			case SlideState.SLOPE:
				p.DoTrick(Player.TrickType.SLIDE, trickName);
				p.RefillComboTimeOut();
				if (p.IsGrounded())
				{
					SetDust(0.5f);
				}
				else
				{
					SetDust(0f);
				}
				targetSpeed = slopeSlideSpeed;
				acc = normalAcc;
				decc = momentumSlideDecc;
				CheckSingleBoost();
				TryToRemoveCuffs();
				if (p.inSpeedZone && p.moveStyle == MoveStyle.ON_FOOT)
				{
					SetSlideState(SlideState.SUPER);
				}
				else if (!GoingDown())
				{
					SetSlideState(SlideState.NORMAL);
				}
				break;
			case SlideState.SUPER:
				targetSpeed = superSpeed;
				acc = superAcc;
				decc = momentumSlideDecc;
				if (p.IsGrounded())
				{
					SetDust(1f);
				}
				else
				{
					SetDust(0f);
				}
				if (p.inSpeedZone)
				{
					superBuildUp = Mathf.Clamp(superBuildUp + Core.dt, 0f, maxBuildUp);
					p.DoTrick(Player.TrickType.SLIDE, trickName);
				}
				else
				{
					superBuildUp = Mathf.Clamp(superBuildUp - Core.dt, 0f, float.MaxValue);
				}
				CheckSingleBoost();
				TryToRemoveCuffs();
				if (superBuildUp > 0f)
				{
					p.RefillComboTimeOut();
				}
				else
				{
					SetSlideState(SlideState.NORMAL);
				}
				break;
			case SlideState.GET_UP:
			{
				SetDust(0f);
				targetSpeed = 0f;
				decc = normalDecc;
				float num = 0.5f;
				if (stateTimer > num - 0.25f)
				{
					stopDecided = true;
					wantToStop = moveInput.magnitude < 0.5f;
					if (stateTimer > num)
					{
						flag = true;
					}
				}
				if (stopDecided)
				{
					if (wantToStop)
					{
						decc = stopDecc;
						p.PlayAnim(stopRunHash);
					}
					else
					{
						targetSpeed = p.maxMoveSpeed;
						rotateSpeed = p.stats.rotSpeedAtMaxSpeed;
					}
				}
				break;
			}
			}
			if (slideState != SlideState.ROLL && slideState != SlideState.GET_UP)
			{
				bool num4 = !p.slideButtonHeld && p.abilityTimer > 0.7f;
				if (p.GetForwardSpeed() < stopAtSpeed)
				{
					stopBySpeedTimer += Core.dt;
				}
				if ((num4 || stopBySpeedTimer > 0.5f) & flag2)
				{
					SetSlideState(SlideState.GET_UP);
				}
			}
			DoAnim();
			BoostpackEffectMode boostpack = BoostpackEffectMode.OFF;
			if (boostEffectOn)
			{
				boostpack = BoostpackEffectMode.ON;
			}
			FrictionEffectMode friction = FrictionEffectMode.OFF;
			if (slideEffect && p.moveStyle == MoveStyle.ON_FOOT)
			{
				friction = FrictionEffectMode.ON;
			}
			if (slideEffect && p.moveStyle == MoveStyle.INLINE)
			{
				friction = FrictionEffectMode.ON;
			}
			p.SetBoostpackAndFrictionEffects(boostpack, friction);
			if (p.vertAbility.CheckActivation())
			{
				return;
			}
			if (!p.IsGrounded())
			{
				flag = true;
			}
			if (flag & flag2)
			{
				p.StopCurrentAbility();
			}
			else if ((slideState != SlideState.ROLL) & flag2)
			{
				if (p.switchStyleButtonNew && !p.switchToEquippedMovestyleLocked)
				{
					if (p.IsGrounded())
					{
						p.ActivateAbility(p.switchMoveStyleAbility);
					}
					else
					{
						p.SwitchToEquippedMovestyle(!p.usingEquippedMovestyle, doAirTrick: true);
					}
				}
				else
				{
					p.groundTrickAbility.CheckActivation();
				}
			}
			if (p.usingEquippedMovestyle && p.inWalkZone && GoingUp())
			{
				p.StopCurrentAbility();
			}
		}

		private bool RoomToUncrouch()
		{
			if (p.moveStyle != MoveStyle.ON_FOOT || !crouched)
			{
				return true;
			}
			float height = p.motor.GetCapsule().height;
			Vector3 origin = p.motor.GetCapsule().transform.position + height * Vector3.up;
			int layerMask = 8389649;
			RaycastHit hitInfo;
			return !Physics.Raycast(origin, Vector3.up, out hitInfo, normalCapsuleHeight - height, layerMask);
		}

		private bool GoingDown()
		{
			if (slopeDot > 0.5f)
			{
				return p.motor.groundAngle > 1f;
			}
			return false;
		}

		private bool GoingUp()
		{
			if (slopeDot < -0.5f)
			{
				return p.motor.groundAngle > 1f;
			}
			return false;
		}

		private void CheckSingleBoost()
		{
			if (p.boostButtonNew && singleBoostCooldown <= 0f && p.HasBoostCharge(p.maxBoostCharge / 4f))
			{
				SingleBoost(p.boostSpeed);
				p.AddBoostCharge(0f - p.maxBoostCharge / 4f);
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

		private void SingleBoost(float setSpeed)
		{
			singleBoostCooldown = 0.5f;
			p.ringParticles.Emit(1);
			p.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.slideDropdownBoost, p.playerOneShotAudioSource);
			targetSpeed = baseSpeed;
			p.SetSpeedFlat(Mathf.Max(setSpeed, p.GetForwardSpeed()));
		}

		public void ComboTimeout()
		{
			if (p.ability == this && slideState == SlideState.NORMAL && RoomToUncrouch())
			{
				SetSlideState(SlideState.GET_UP);
			}
		}

		public void DoAnim()
		{
			Vector3 vector = p.moveInputPlain;
			if (vector.x > 0.2f)
			{
				slideAnimDir = Mathf.Lerp(slideAnimDir, vector.x + 0.25f, 1f - Mathf.Exp(-10f * Core.dt));
			}
			if (vector.x < -0.2f)
			{
				slideAnimDir = Mathf.Lerp(slideAnimDir, vector.x - 0.25f, 1f - Mathf.Exp(-10f * Core.dt));
			}
			if (Mathf.Abs(vector.x) < 0.2f)
			{
				slideAnimDir = Mathf.Lerp(slideAnimDir, vector.x / 2f, 1f - Mathf.Exp(-5f * Core.dt));
			}
			p.anim.SetFloat(slideDirectionHash, slideAnimDir);
			p.OrientVisualToSurfaceInAbility();
		}

		public override void OnJump()
		{
			if (superBuildUp > 0f)
			{
				p.ActivateAbility(p.airTrickAbility);
			}
			else if ((bool)p.onLauncher)
			{
				p.ActivateAbility(p.airTrickAbility);
			}
			p.DoComboTimeOut(0.125f);
		}

		public override void OnStopAbility()
		{
			p.ResetVisualRot();
			p.SetDustEmission(0);
			p.SetDustSize(1f);
			p.hitbox.SetActive(value: false);
			p.hitboxLeftLeg.SetActive(value: false);
			p.hitboxRightLeg.SetActive(value: false);
			p.hitboxUpperBody.SetActive(value: false);
			p.AudioManager.StopLoopingSfx(p.playerMovementLoopAudioSource);
			p.AudioManager.StopLoopingSfx(p.playerSlideAudioSource);
			Crouch(set: false);
			base.OnStopAbility();
		}

		private void Crouch(bool set)
		{
			crouched = set;
			if (crouched)
			{
				float radius = p.motor.GetCapsule().radius;
				p.motor.GetCapsule().center = Vector3.up * radius;
				p.motor.GetCapsule().height = radius * 2f;
				if (!p.isAI)
				{
					p.cam.postureOffsetTarget = p.motor.GetCapsule().height - normalCapsuleHeight + 0.2f;
				}
			}
			else
			{
				p.motor.GetCapsule().center = Vector3.up * normalCapsuleCenter;
				if (!p.isAI)
				{
					p.cam.postureOffsetTarget = 0f;
				}
				p.motor.GetCapsule().height = normalCapsuleHeight;
			}
		}

		public override void RecheckLoopingSoundInAbilityUse()
		{
			switch (slideState)
			{
			case SlideState.NORMAL:
				p.AudioManager.PlaySfxGameplayLooping(p.moveStyle, AudioClipID.slideLoop, p.playerMovementLoopAudioSource);
				break;
			case SlideState.SLOPE:
				p.AudioManager.PlaySfxGameplayLooping(p.moveStyle, AudioClipID.slideLoop, p.playerMovementLoopAudioSource);
				break;
			case SlideState.SUPER:
				if (p.moveStyle == MoveStyle.ON_FOOT || p.moveStyle == MoveStyle.INLINE)
				{
					p.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.GenericMovementSfx, AudioClipID.slideSuperLoop, p.playerMovementLoopAudioSource);
				}
				else if (p.moveStyle == MoveStyle.SKATEBOARD || p.moveStyle == MoveStyle.BMX)
				{
					p.AudioManager.PlaySfxGameplayLooping(p.moveStyle, AudioClipID.slideLoop, p.playerMovementLoopAudioSource);
				}
				break;
			}
		}

		public override bool CheckActivation()
		{
			if (p.slideButtonNew && p.IsGrounded() && !locked)
			{
				p.ActivateAbility(this);
				return true;
			}
			return false;
		}

		private void SetDust(float size)
		{
			if (p.moveStyle == MoveStyle.ON_FOOT)
			{
				p.SetDustEmission((size > 0f) ? 100 : 0);
				p.SetDustSize(size);
			}
		}
	}
}
