using System;
using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class HandplantAbility : Ability
	{
		internal float screwSpinTimer;

		private float screwFastSpinTimer;

		private int screwFastPressCount;

		private float screwAcc = 2f;

		private float screwDecc = 1f;

		private float maxScrewSpeed = 1.5f;

		internal bool screwPoleMode;

		private SkateboardScrewPole screwPole;

		public float cooldownTimer;

		private string inlineTrickName = "Invert";

		private string bmxTrickName = "Can Can Footplant";

		private string skateboardTrickName = "Andrecht Invert";

		private string skateboardTrickNameScrewPole = "Natas Spin";

		private int poleFreezeHash = Animator.StringToHash("poleFreeze");

		private int screwSpinHash = Animator.StringToHash("screwSpin");

		public HandplantAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalMovement = false;
			normalRotation = false;
			rotateSpeed = p.stats.rotSpeedAtMaxSpeed;
			customGravity = 0f;
			canStartGrind = false;
			canStartWallrun = false;
			treatPlayerAsSortaGrounded = true;
			canUseSpraycan = false;
			customCameraMode = new CameraModeHandplant(this);
		}

		public void SetToPole(Vector3 polePoint, SkateboardScrewPole setScrewPole)
		{
			p.ActivateAbility(this);
			p.motor.HaveCollision(have: false);
			p.SetVelocity(Vector3.zero);
			p.SetRotation(p.tf.up * -1f, Vector3.up);
			p.motor.SetPositionTeleport(polePoint + Vector3.up * 0.03f);
			p.StartCoroutine(WaitUntilFrameIsDone(polePoint));
			screwPole = setScrewPole;
			screwPole.reset = false;
			screwPoleMode = true;
			screwSpinTimer = 0f;
			customVelocity = Vector3.zero;
			p.SetRotation(Quaternion.identity);
			p.RegainAirMobility();
			p.PlayAnim(poleFreezeHash);
			p.PlayAnim(screwSpinHash);
			p.AudioManager.PlaySfxGameplay(p.moveStyle, AudioClipID.handplant, p.playerOneShotAudioSource);
			if (p.moveStyle == MoveStyle.BMX)
			{
				p.DoTrick(Player.TrickType.HANDPLANT, bmxTrickName);
			}
			else if (p.moveStyle == MoveStyle.SKATEBOARD)
			{
				p.DoTrick(Player.TrickType.HANDPLANT, skateboardTrickNameScrewPole);
			}
			else
			{
				p.DoTrick(Player.TrickType.HANDPLANT, inlineTrickName);
			}
		}

		public void SetToPole(Vector3 polePoint)
		{
			p.ActivateAbility(this);
			p.motor.HaveCollision(have: false);
			p.SetVelocity(Vector3.zero);
			p.SetRotation(p.tf.up * -1f, Vector3.up);
			p.motor.SetPositionTeleport(polePoint + Vector3.up * 0.03f);
			p.StartCoroutine(WaitUntilFrameIsDone(polePoint));
			screwPoleMode = false;
			customVelocity = Vector3.zero;
			p.RegainAirMobility();
			p.AudioManager.PlaySfxGameplay(p.moveStyle, AudioClipID.handplant, p.playerOneShotAudioSource);
			p.PlayAnim(poleFreezeHash);
			if (p.moveStyle == MoveStyle.BMX)
			{
				p.DoTrick(Player.TrickType.HANDPLANT, bmxTrickName);
			}
			else if (p.moveStyle == MoveStyle.SKATEBOARD)
			{
				p.DoTrick(Player.TrickType.HANDPLANT, skateboardTrickName);
			}
			else
			{
				p.DoTrick(Player.TrickType.HANDPLANT, inlineTrickName);
			}
		}

		public void PassiveUpdate()
		{
			if (cooldownTimer > 0f)
			{
				cooldownTimer -= Core.dt;
			}
		}

		public IEnumerator WaitUntilFrameIsDone(Vector3 point)
		{
			yield return new WaitForEndOfFrame();
			p.motor.SetPositionTeleport(point + Vector3.up * 0.03f);
		}

		public override void FixedUpdateAbility()
		{
			if (p.abilityTimer < 0.35f)
			{
				p.hitbox.SetActive(value: true);
			}
			else
			{
				p.hitbox.SetActive(value: false);
			}
			if (screwPoleMode)
			{
				float num = 0f;
				if (p.transform.position.y + Mathf.Epsilon < screwPole.maxPoint.position.y)
				{
					int num2 = 6;
					float num3 = 1f;
					if (p.AnyTrickInput())
					{
						if ((double)screwSpinTimer < 0.25)
						{
							screwSpinTimer = 0.5f;
						}
						if (screwFastSpinTimer > 0f)
						{
							screwFastPressCount++;
							num = ((screwFastPressCount > num2) ? num3 : 0f);
						}
						else
						{
							screwFastPressCount = 0;
						}
						screwFastSpinTimer = 0.25f;
					}
					else if (screwFastSpinTimer > 0f)
					{
						screwFastSpinTimer -= Core.dt;
						num = Mathf.Lerp(0f, num3, (float)Convert.ToInt32(screwFastPressCount > num2) * screwFastSpinTimer * 4f);
					}
				}
				float num4 = 0f;
				if (screwSpinTimer > 0f)
				{
					screwSpinTimer -= Core.dt;
					num4 = Mathf.Min(customVelocity.y + screwAcc * Core.dt, maxScrewSpeed + num);
				}
				else
				{
					num4 = Mathf.Max(customVelocity.y - screwDecc * Core.dt, 0f);
				}
				if (p.transform.position.y + Mathf.Epsilon < screwPole.maxPoint.position.y)
				{
					customVelocity.y = num4;
					screwPole.Move(num4);
					p.rotLean = Mathf.Min(num4, screwSpinTimer * 2f);
				}
				else
				{
					if (customVelocity.y != 0f)
					{
						screwPole.StopPlayingSound();
						p.AudioManager.PlaySfxGameplay(p.moveStyle, AudioClipID.ScrewPolePlant, p.playerOneShotAudioSource);
					}
					customVelocity.y = 0f;
					p.rotLean = screwSpinTimer * 2f;
				}
			}
			if (!(p.abilityTimer > 0.12f) || (!p.jumpButtonNew && !p.isAI))
			{
				return;
			}
			float num5 = p.maxMoveSpeed;
			if (screwPoleMode)
			{
				num5 = p.maxMoveSpeed / 2f;
			}
			Vector3 dirToWaypoint;
			if (p.isAI)
			{
				p.AI.HandplantJump();
				if (p.AI.nextWaypoint != null)
				{
					dirToWaypoint = p.AI.dirToWaypoint;
					dirToWaypoint.y = 0f;
					dirToWaypoint = dirToWaypoint.normalized;
					p.SetRotHard(Vector3.ProjectOnPlane(dirToWaypoint, Vector3.up));
					dirToWaypoint *= num5;
				}
				else
				{
					dirToWaypoint = p.dir * num5;
				}
			}
			else
			{
				dirToWaypoint = p.cam.realTf.forward;
				dirToWaypoint.y = 0f;
				dirToWaypoint = dirToWaypoint.normalized;
				p.SetRotHard(Vector3.ProjectOnPlane(dirToWaypoint, Vector3.up));
				dirToWaypoint *= num5;
			}
			p.AudioManager.PlayVoice(ref p.currentVoicePriority, p.character, AudioClipID.VoiceJump, p.playerGameplayVoicesAudioSource);
			dirToWaypoint.y = p.jumpSpeed + p.bonusJumpSpeedHandplantJump;
			p.SetVelocity(dirToWaypoint);
			p.CreateHighJumpDustEffect(Vector3.up);
			p.PlayAnim(Animator.StringToHash("jump"));
			p.StopCurrentAbility();
		}

		public override void OnStopAbility()
		{
			if ((bool)screwPole)
			{
				screwPole.reset = true;
			}
			cooldownTimer = 0.5f;
			p.motor.HaveCollision(have: true);
			p.hitbox.SetActive(value: false);
		}
	}
}
