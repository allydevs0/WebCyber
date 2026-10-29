using UnityEngine;

namespace Reptile
{
	public class HitBounceAbility : Ability
	{
		public enum BounceType
		{
			PLAIN_BOUNCE = 0,
			FAIL_BOUNCE = 1,
			TURRET_BOUNCE_FROM_AIR_GRAF = 2,
			TURRET_BOUNCE_FROM_GROUND_GRAF = 3,
			TURRET_BOUNCE_FROM_AIR = 4,
			TURRET_BOUNCE_FROM_GROUND = 5,
			SNAKEBOSS_BOUNCE = 6,
			TURRET_CLAW_BOUNCE = 7,
			COPTER_BOUNCE_FROM_BELOW = 8,
			COPTER_BOUNCE_FROM_FRONT = 9
		}

		public enum State
		{
			BOUNCE_IN = 0,
			BOUNCE_OUT = 1
		}

		private bool didTrick;

		private string trickName;

		private string trickingTrickName = "Webster";

		private string skateboardTrickName = "Boneless Frontflip";

		private string inlineTrickName = "Flatspin Safety Grab";

		private string BMXTrickName = "Barrel 90";

		public BounceType bounceType;

		public float hitpauseDuration = 0.17f;

		public State state;

		private int hitBounceHash = Animator.StringToHash("hitBounce");

		public HitBounceAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalMovement = true;
			decc = p.stats.airDecc;
			customGravity = 0f;
			treatPlayerAsSortaGrounded = true;
		}

		public void FreeformBounce(BounceType bounceType = BounceType.PLAIN_BOUNCE)
		{
			Trigger(null, null, Vector3.zero, bounceType);
		}

		public void TurretBounce(GameObject ob, Collider collider, BounceType bounceType = BounceType.PLAIN_BOUNCE)
		{
			Trigger(ob, collider, Vector3.zero, bounceType);
		}

		public void FailBounce(GameObject ob, Vector3 wallAxis)
		{
			Trigger(ob, null, wallAxis, BounceType.FAIL_BOUNCE);
		}

		public bool Trigger(Vector3 hitObjectPosition, BoxCollider hitCollider, BounceType bounceType = BounceType.PLAIN_BOUNCE)
		{
			bool result = false;
			Vector3 position = p.transform.position;
			Vector3 normalized = (hitObjectPosition + hitCollider.center - position).normalized;
			if (hitCollider.Raycast(new Ray(position, normalized), out var hitInfo, 5f))
			{
				Vector3 normal = hitInfo.normal;
				if (normal != hitInfo.transform.up && normal != -hitInfo.transform.up)
				{
					Vector3 vector = hitInfo.normal * 0.7f;
					vector.y = 0f;
					Vector3 vector2 = hitInfo.point + vector;
					int layerMask = 8389649;
					Vector3 vector3 = vector2 + p.motor.GetCapsule().radius * Vector3.up + Vector3.up * 0.01f;
					Vector3 end = vector3 + Vector3.up * (p.motor.GetCapsule().height - p.motor.GetCapsule().radius * 2f);
					if (!Physics.CheckCapsule(vector3, end, p.motor.GetCapsule().radius, layerMask))
					{
						p.transform.position = vector2;
						p.SetRotHard(Quaternion.LookRotation(-normal));
						p.ActivateAbility(this);
						this.bounceType = bounceType;
						result = true;
					}
				}
			}
			return result;
		}

		public bool Trigger(GameObject ob, Collider collider, Vector3 wallAxis, BounceType bounceType = BounceType.PLAIN_BOUNCE)
		{
			bool result = false;
			if (ob != null)
			{
				Vector3 vector = p.transform.position + Vector3.up * 1.1f;
				Vector3 forward = p.transform.forward;
				forward.y = 0f;
				if (collider == null)
				{
					collider = ob.GetComponent<Collider>();
				}
				Vector3 vector2 = ((!collider.Raycast(new Ray(vector, forward), out var hitInfo, 5f)) ? collider.ClosestPointOnBounds(vector) : hitInfo.point);
				int layerMask = 8389649;
				Vector3 vector3 = vector2 - Vector3.up * 0.5f;
				Vector3 vector4 = vector3 + p.motor.GetCapsule().radius * Vector3.up + Vector3.up * 0.01f;
				Vector3 end = vector4 + Vector3.up * (p.motor.GetCapsule().height - p.motor.GetCapsule().radius * 2f);
				if (bounceType == BounceType.TURRET_BOUNCE_FROM_AIR || bounceType == BounceType.TURRET_BOUNCE_FROM_AIR_GRAF || bounceType == BounceType.TURRET_BOUNCE_FROM_GROUND || bounceType == BounceType.TURRET_BOUNCE_FROM_GROUND_GRAF || !Physics.CheckCapsule(vector4, end, p.motor.GetCapsule().radius, layerMask))
				{
					Vector3 vector5 = vector3 - vector;
					vector5.y = 0f;
					Vector3 vector6 = ob.transform.position - vector2;
					vector6.y = 0f;
					if (wallAxis != Vector3.zero)
					{
						vector6 = Vector3.ProjectOnPlane(vector6, wallAxis);
					}
					vector6.Normalize();
					vector5 -= vector6 * 0.7f;
					p.transform.position += vector5;
					p.SetRotHard(Quaternion.LookRotation(vector6));
					result = true;
					p.ActivateAbility(this);
					this.bounceType = bounceType;
				}
			}
			else
			{
				result = true;
				p.ActivateAbility(this);
				this.bounceType = bounceType;
			}
			return result;
		}

		public void Trigger(Vector3 targetPos, float radius, BounceType bounceType = BounceType.PLAIN_BOUNCE)
		{
			Vector3 normalized = (p.transform.position - targetPos).normalized;
			Trigger(targetPos, radius, normalized, bounceType);
		}

		public bool Trigger(Vector3 targetPos, float radius, Vector3 normal, BounceType bounceType = BounceType.PLAIN_BOUNCE)
		{
			bool result = false;
			int layerMask = 8389649;
			Vector3 vector = targetPos + p.motor.GetCapsule().radius * Vector3.up + Vector3.up * 0.01f;
			Vector3 end = vector + Vector3.up * (p.motor.GetCapsule().height - p.motor.GetCapsule().radius * 2f);
			if (!Physics.CheckCapsule(vector, end, p.motor.GetCapsule().radius, layerMask))
			{
				Vector3 vector2 = targetPos + normal * (radius + 0.7f) - p.transform.position;
				vector2.y = 0f;
				p.transform.position += vector2;
				p.SetRotHard(Quaternion.LookRotation(-normal));
				p.ActivateAbility(this);
				this.bounceType = bounceType;
				result = true;
			}
			return result;
		}

		public override void OnStartAbility()
		{
			acc = 0f;
			targetSpeed = 0f;
			normalRotation = false;
			p.RegainAirMobility();
			bounceType = BounceType.PLAIN_BOUNCE;
			didTrick = false;
			SetState(State.BOUNCE_IN);
			trickName = trickingTrickName;
			if (p.moveStyle == MoveStyle.SKATEBOARD)
			{
				trickName = skateboardTrickName;
			}
			else if (p.moveStyle == MoveStyle.INLINE)
			{
				trickName = inlineTrickName;
			}
			else if (p.moveStyle == MoveStyle.BMX)
			{
				trickName = BMXTrickName;
			}
		}

		public void SetState(State setState)
		{
			state = setState;
			switch (state)
			{
			case State.BOUNCE_IN:
				p.SetVelocity(Vector3.zero);
				p.PlayAnim(hitBounceHash);
				break;
			case State.BOUNCE_OUT:
				BounceOut();
				if (bounceType == BounceType.PLAIN_BOUNCE || bounceType == BounceType.FAIL_BOUNCE)
				{
					p.SetVelocity(p.dir * -2.75f + Vector3.up * 6.5f);
				}
				else if (bounceType == BounceType.TURRET_BOUNCE_FROM_AIR_GRAF)
				{
					p.SetVelocity(Vector3.up * 11.35f);
				}
				else if (bounceType == BounceType.TURRET_BOUNCE_FROM_GROUND_GRAF)
				{
					p.SetVelocity(Vector3.up * 12.35f);
				}
				else if (bounceType == BounceType.TURRET_BOUNCE_FROM_AIR)
				{
					p.SetVelocity(Vector3.up * 11.85f);
				}
				else if (bounceType == BounceType.TURRET_BOUNCE_FROM_GROUND)
				{
					p.SetVelocity(Vector3.up * 12.85f);
				}
				else if (bounceType == BounceType.SNAKEBOSS_BOUNCE)
				{
					p.SetVelocity(p.dir * p.maxMoveSpeed * 0.5f);
				}
				else if (bounceType == BounceType.TURRET_CLAW_BOUNCE)
				{
					p.SetVelocity(p.dir * p.maxMoveSpeed * 0.4f + Vector3.up * 6.5f);
				}
				else if (bounceType == BounceType.COPTER_BOUNCE_FROM_BELOW)
				{
					p.SetVelocity(Vector3.up * 10f);
				}
				else if (bounceType == BounceType.COPTER_BOUNCE_FROM_FRONT)
				{
					p.SetVelocity(Vector3.up * 3.5f);
				}
				break;
			}
		}

		public void BounceOut()
		{
			p.StartScreenShake(ScreenShakeType.LIGHT, 0.2f);
			p.ringParticles.Emit(1);
			if (p.IsGrounded())
			{
				p.jumpedThisFrame = true;
				p.isJumping = true;
				p.maintainSpeedJump = false;
				p.jumpConsumed = true;
				p.jumpRequested = false;
				p.jumpedThisFrame = true;
				p.timeSinceLastJump = 0f;
				p.ForceUnground();
			}
		}

		public override void FixedUpdateAbility()
		{
			p.SetVisualRotLocal0();
			switch (state)
			{
			case State.BOUNCE_IN:
				if (p.abilityTimer > hitpauseDuration)
				{
					SetState(State.BOUNCE_OUT);
				}
				break;
			case State.BOUNCE_OUT:
				if (p.IsGrounded())
				{
					p.StopCurrentAbility();
					break;
				}
				if (p.abilityTimer > 0.25f && !didTrick)
				{
					p.DoTrick(Player.TrickType.HIT_BOUNCE, trickName);
					didTrick = true;
				}
				if (bounceType == BounceType.TURRET_BOUNCE_FROM_AIR_GRAF || bounceType == BounceType.TURRET_BOUNCE_FROM_GROUND_GRAF || bounceType == BounceType.TURRET_BOUNCE_FROM_AIR || bounceType == BounceType.TURRET_BOUNCE_FROM_GROUND || bounceType == BounceType.COPTER_BOUNCE_FROM_BELOW || bounceType == BounceType.COPTER_BOUNCE_FROM_FRONT)
				{
					if (bounceType == BounceType.TURRET_BOUNCE_FROM_AIR || (bounceType == BounceType.TURRET_BOUNCE_FROM_GROUND && p.abilityTimer > 0.3f))
					{
						acc = p.stats.airAcc + 10f;
						normalRotation = true;
						targetSpeed = p.stats.walkSpeed + 1f;
					}
					if ((!(p.abilityTimer > 0.5f) || (!p.airDashAbility.CheckActivation() && !p.airTrickAbility.CheckActivation())) && p.abilityTimer > 1f)
					{
						p.StopCurrentAbility();
					}
				}
				else if (bounceType == BounceType.PLAIN_BOUNCE)
				{
					if (p.abilityTimer > 0.6f)
					{
						acc = p.stats.airAcc;
						normalRotation = true;
						if (p.airDashAbility.CheckActivation() || p.airTrickAbility.CheckActivation())
						{
							break;
						}
					}
					if (p.abilityTimer > 0.8f)
					{
						p.StopCurrentAbility();
					}
				}
				else if (bounceType == BounceType.FAIL_BOUNCE)
				{
					if (p.abilityTimer > 1.2f)
					{
						p.StopCurrentAbility();
					}
				}
				else
				{
					if (bounceType != BounceType.SNAKEBOSS_BOUNCE)
					{
						break;
					}
					if (p.abilityTimer > 0.6f)
					{
						acc = p.stats.airAcc;
						normalRotation = true;
						if (p.airDashAbility.CheckActivation() || p.airTrickAbility.CheckActivation())
						{
							break;
						}
					}
					else if (bounceType == BounceType.TURRET_CLAW_BOUNCE && p.abilityTimer > 1.2f)
					{
						p.StopCurrentAbility();
					}
					if (p.abilityTimer > 1.2f)
					{
						p.StopCurrentAbility();
					}
				}
				break;
			}
		}

		public override void OnStopAbility()
		{
			base.OnStopAbility();
			p.hitbox.SetActive(value: false);
			p.hitboxLeftLeg.SetActive(value: false);
			p.hitboxRightLeg.SetActive(value: false);
			p.hitboxUpperBody.SetActive(value: false);
		}
	}
}
