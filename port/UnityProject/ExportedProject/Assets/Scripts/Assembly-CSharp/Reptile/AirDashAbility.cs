using UnityEngine;

namespace Reptile
{
	public class AirDashAbility : Ability
	{
		private float duration = 0.35f;

		public bool haveAirDash;

		private float airDashStartSpeed = 9f;

		public float airDashSpeed;

		private float airDashInitialUpSpeed = 4f;

		private Vector3? dirIfNoSteer;

		private int airDashHash = Animator.StringToHash("airDash");

		public AirDashAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			decc = p.stats.groundDecc;
			allowNormalJump = false;
		}

		public override void OnStartAbility()
		{
			if (p.moveStyle == MoveStyle.SPECIAL_SKATEBOARD)
			{
				p.ActivateAbility(p.airTrickAbility);
				Vector3 velocity = p.GetVelocity();
				velocity.y = 5f;
				p.SetVelocity(velocity);
				return;
			}
			p.ringParticles.Emit(1);
			airDashSpeed = airDashStartSpeed;
			Vector3 vector = p.moveInput;
			if (vector.sqrMagnitude == 0f)
			{
				vector = (dirIfNoSteer.HasValue ? dirIfNoSteer.Value : p.dir);
			}
			vector = Vector3.ProjectOnPlane(vector, Vector3.up).normalized;
			if (p.smoothRotation)
			{
				p.SetRotHard(vector);
			}
			else
			{
				p.SetRotation(vector);
			}
			p.OrientVisualInstantReset();
			if (p.GetFlatVelocity().magnitude > airDashStartSpeed)
			{
				float num = Vector3.Dot(vector, p.GetFlatVelocity().normalized);
				if (num < 0f)
				{
					num = 0f;
				}
				airDashSpeed = airDashStartSpeed + (p.GetFlatVelocity().magnitude - airDashStartSpeed) * num;
			}
			p.SetVelocity(new Vector3(vector.x * airDashSpeed, Mathf.Max(airDashInitialUpSpeed, p.GetVelocity().y), vector.z * airDashSpeed));
			targetSpeed = airDashSpeed;
			haveAirDash = false;
			p.PlayAnim(airDashHash, forceOverwrite: true);
			Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.airdash, p.playerOneShotAudioSource);
			p.wallrunAbility.cooldownTimer = 0f;
			p.wallrunAbility.wallrunLine = null;
		}

		public override void FixedUpdateAbility()
		{
			p.airialHitbox.SetActive(value: true);
			p.SetBoostpackAndFrictionEffects(BoostpackEffectMode.AIRDASH);
			if (p.IsGrounded() || p.abilityTimer > duration)
			{
				p.StopCurrentAbility();
				return;
			}
			p.ledgeClimbAbility.CheckActivation();
			p.SetVisualRotLocal0();
		}

		public bool CheckActivation(Vector3 _dirIfNoSteer)
		{
			dirIfNoSteer = _dirIfNoSteer;
			bool result = CheckActivation();
			dirIfNoSteer = null;
			return result;
		}

		public override bool CheckActivation()
		{
			if (p.jumpButtonNew && !p.IsGrounded() && haveAirDash && (!p.jumpRequested || !p.JumpIsAllowed()) && !locked)
			{
				p.ActivateAbility(this);
				return true;
			}
			return false;
		}

		public override void OnStopAbility()
		{
			p.airialHitbox.SetActive(value: false);
			if (p.smoothRotation)
			{
				p.SetVisualRotXZ(Quaternion.FromToRotation(Vector3.up, p.motor.groundNormalVisual));
				return;
			}
			p.visualTf.up = p.motor.groundNormalVisual;
			p.visualTf.Rotate(0f, p.motor.transform.localEulerAngles.y, 0f, Space.Self);
		}
	}
}
