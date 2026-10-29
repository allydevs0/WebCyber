using UnityEngine;

namespace Reptile
{
	public class LedgeClimbAbility : Ability
	{
		public enum State
		{
			ONLY_ALIGN_POS = 0,
			PREP = 1,
			VAULT_MOVE = 2,
			VAULT_STAND = 3
		}

		private Collider groundCollider;

		private float climbableHeightFromPos = 1.1f;

		private Vector3 targetLedgeClimbPoint;

		private Vector3 setVel;

		private int landRunHash = Animator.StringToHash("landRun");

		private int vaultSlideHash = Animator.StringToHash("vaultSlide");

		public State state;

		public LedgeClimbAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			rotateSpeed = 3f;
			treatPlayerAsSortaGrounded = true;
		}

		public override void OnStartAbility()
		{
			p.skipLanding = true;
			allowNormalJump = false;
			p.motor.SetPositionTeleport(targetLedgeClimbPoint);
			if (targetLedgeClimbPoint.y > p.tf.position.y + 0.3f)
			{
				SetState(State.PREP);
			}
			else
			{
				SetState(State.ONLY_ALIGN_POS);
			}
		}

		public override bool CheckActivation()
		{
			if (p.IsGrounded() || locked)
			{
				return false;
			}
			bool num = p.moveInput != Vector3.zero && Vector3.Dot(p.moveInput.normalized, p.dir) > 0.5f;
			bool flag = Vector3.ProjectOnPlane(p.GetVelocity(), Vector3.up).magnitude > 0.4f;
			if (!num && !flag)
			{
				return false;
			}
			int num2 = 1;
			float height = p.motor.GetCapsule().height;
			Vector3 origin = p.tf.position + p.motor.GetCapsule().radius * p.dir * 1.3f + climbableHeightFromPos * Vector3.up;
			float maxDistance = climbableHeightFromPos;
			if (Physics.Raycast(origin, Vector3.down, out var hitInfo, maxDistance, num2))
			{
				targetLedgeClimbPoint = hitInfo.point;
				_ = p.tf.position;
				_ = hitInfo.distance;
				Vector3 vector = targetLedgeClimbPoint + p.motor.GetCapsule().radius * Vector3.up + Vector3.up * 0.01f;
				Vector3 end = vector + Vector3.up * (height - p.motor.GetCapsule().radius * 2f);
				int layerMask = num2 | 0x410 | 0x800000 | 0x200000 | 0x10;
				if (!Physics.CheckCapsule(vector, end, p.motor.GetCapsule().radius, layerMask) && Vector3.Angle(hitInfo.normal, Vector3.up) < p.motor.groundDetection.groundLimit)
				{
					bool flag2 = Physics.Raycast(p.tf.position + Vector3.up * 0.001f, Vector3.down, out var hitInfo2, height + 0.001f, num2);
					if (p.ability == p.airDashAbility || !flag2 || targetLedgeClimbPoint.y - hitInfo2.point.y > height)
					{
						if (!p.isAI)
						{
							groundCollider = hitInfo.collider;
							p.cam.IgnoredColliders.Add(groundCollider);
						}
						p.ActivateAbility(this);
						return true;
					}
				}
			}
			return false;
		}

		public override void FixedUpdateAbility()
		{
			if (state == State.ONLY_ALIGN_POS)
			{
				if (p.abilityTimer > 0.15f)
				{
					p.skipLanding = true;
					p.StopCurrentAbility();
				}
			}
			else if (state == State.PREP)
			{
				if (p.abilityTimer > 0.28f)
				{
					if (p.forwardInput < 0.3f)
					{
						SetState(State.VAULT_STAND);
					}
					else
					{
						SetState(State.VAULT_MOVE);
					}
				}
			}
			else if (state == State.VAULT_MOVE)
			{
				if (p.moveStyle == MoveStyle.ON_FOOT)
				{
					p.PlayAnim(Animator.StringToHash("run"));
				}
			}
			else
			{
				_ = state;
				_ = 3;
			}
			if (state != State.PREP && !p.IsGrounded())
			{
				p.StopCurrentAbility();
				return;
			}
			if (p.moveInput != Vector3.zero && p.abilityTimer > 0.5f)
			{
				p.StopCurrentAbility();
			}
			else if (p.abilityTimer > 0.4f && p.slideButtonHeld && !p.slideAbility.locked)
			{
				p.ActivateAbility(p.slideAbility);
				return;
			}
			if (p.curAnim != vaultSlideHash)
			{
				p.StopCurrentAbility();
			}
			p.SetVisualRotLocal0();
		}

		public void SetState(State setState)
		{
			state = setState;
			switch (state)
			{
			case State.ONLY_ALIGN_POS:
			{
				p.PlayAnim(landRunHash);
				Vector3 preVelocity = p.preVelocity;
				preVelocity.y = -1f;
				p.SetVelocity(preVelocity);
				break;
			}
			case State.PREP:
			{
				p.PlayAnim(vaultSlideHash);
				targetSpeed = 1.5f;
				Vector3 velocity2 = p.dir * targetSpeed;
				velocity2.y = -1f;
				p.SetVelocity(velocity2);
				break;
			}
			case State.VAULT_MOVE:
			{
				RemoveIgnoredCollider();
				allowNormalJump = true;
				targetSpeed = p.maxMoveSpeed;
				Vector3 velocity = p.dir * targetSpeed;
				velocity.y = -1f;
				p.SetVelocity(velocity);
				break;
			}
			case State.VAULT_STAND:
				RemoveIgnoredCollider();
				allowNormalJump = true;
				targetSpeed = 0f;
				break;
			}
		}

		public override void OnJump()
		{
			p.StopCurrentAbility();
		}

		public override void OnStopAbility()
		{
			RemoveIgnoredCollider();
			base.OnStopAbility();
		}

		private void RemoveIgnoredCollider()
		{
			if (!p.isAI)
			{
				p.cam.IgnoredColliders.Remove(groundCollider);
			}
		}
	}
}
