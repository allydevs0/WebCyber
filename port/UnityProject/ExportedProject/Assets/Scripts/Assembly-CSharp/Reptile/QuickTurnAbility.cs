using UnityEngine;

namespace Reptile
{
	public class QuickTurnAbility : Ability
	{
		private float speed;

		private float accel;

		private int runHash = Animator.StringToHash("run");

		private int turnRunHash = Animator.StringToHash("turnRun");

		public QuickTurnAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			allowNormalJump = true;
			normalMovement = false;
			normalRotation = false;
			accel = 35f;
		}

		public override void OnStartAbility()
		{
			p.PlayAnim(turnRunHash);
			customVelocity = p.GetVelocity() * -1f;
			speed = 0f - p.GetForwardSpeed();
		}

		public override void FixedUpdateAbility()
		{
			speed = Mathf.Clamp(speed + accel * Core.dt, speed, p.maxMoveSpeed);
			customVelocity = speed * p.dir;
			if (p.abilityTimer > 0.15f)
			{
				p.PlayAnim(runHash);
			}
			if (!p.IsGrounded())
			{
				p.StopCurrentAbility();
			}
			else if (p.moveInput.sqrMagnitude == 0f && p.abilityTimer > 0.25f)
			{
				p.StopCurrentAbility();
			}
			else if (speed >= p.maxMoveSpeed)
			{
				p.StopCurrentAbility();
			}
		}

		public override void OnJump()
		{
			p.StopCurrentAbility();
		}
	}
}
