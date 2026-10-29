using UnityEngine;

namespace Reptile
{
	public class VertAbility : Ability
	{
		private float visualPosOffsetHorizontal = -0.6f;

		private float trickDuration;

		private bool trickScoreBuffered;

		private float trickTimer;

		private AirTrickAbility.TrickType trickType;

		public Vector3 jumpOffPoint;

		public bool readyToLaunch;

		public Vector3 readyToLaunchNormal;

		private Vector3 planeRight;

		private Vector3 planeNormal;

		private Quaternion? visualRotation;

		public VertAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalRotation = false;
			normalMovement = true;
			acc = 0f;
			decc = 0f;
			targetSpeed = 0f;
			treatPlayerAsSortaGrounded = true;
			customCameraMode = new CameraModeVert(this);
		}

		public override void OnStartAbility()
		{
			p.hitbox.SetActive(value: true);
			canStartGrind = false;
			readyToLaunch = false;
			trickDuration = p.airTrickAbility.trickingTrickDuration;
			if (p.moveStyle == MoveStyle.SKATEBOARD || p.moveStyle == MoveStyle.SPECIAL_SKATEBOARD)
			{
				trickDuration = p.airTrickAbility.skateboardTrickDuration;
			}
			else if (p.moveStyle == MoveStyle.BMX)
			{
				trickDuration = p.airTrickAbility.bmxTrickDuration;
			}
			else if (p.moveStyle == MoveStyle.INLINE)
			{
				trickDuration = p.airTrickAbility.inlineTrickDuration;
			}
			p.PlayAnim(Animator.StringToHash("jump"));
			p.ForceUnground(resetVisual: false);
			p.SetRotHard(planeNormal);
			Vector3 vector = Vector3.ProjectOnPlane(p.reallyMovedVelocity, planeNormal).normalized;
			float num = Vector3.Angle(Vector3.up, vector);
			if (num > p.vertMaxSideJumpAngle)
			{
				vector = Quaternion.AngleAxis(num - p.vertMaxSideJumpAngle, Vector3.Cross(vector, Vector3.up)) * vector;
			}
			vector = (vector + planeNormal * p.vertJumpAway).normalized;
			vector *= p.vertTopJumpSpeed;
			p.SetVelocity(vector);
			Vector3 positionTeleport = jumpOffPoint + Vector3.ProjectOnPlane(p.tf.position - jumpOffPoint, planeNormal);
			positionTeleport.y = Mathf.Max(positionTeleport.y, jumpOffPoint.y);
			p.motor.SetPositionTeleport(positionTeleport);
			p.SetVisualRot(Quaternion.LookRotation(vector, planeNormal), hard: true);
			p.visualTf.position = Vector3.Lerp(p.visualTf.position, p.tf.position + planeNormal * visualPosOffsetHorizontal + Vector3.up, 1f - Mathf.Exp(-18f * Core.dt));
			visualRotation = null;
		}

		public override void FixedUpdateAbility()
		{
			if (trickTimer > 0f)
			{
				trickTimer -= Core.dt;
				if (trickScoreBuffered && trickTimer < trickDuration - 0.25f)
				{
					p.airTrickAbility.Trick(trickType);
					trickScoreBuffered = false;
				}
			}
			if (trickTimer < 0.3f && p.AnyTrickInput())
			{
				StartTrick();
			}
			bool flag = p.GetVelocity().y <= 0f;
			if (flag)
			{
				canStartGrind = true;
			}
			if (p.OnAnyGround())
			{
				if (p.motor.groundCollider.gameObject.layer != 23)
				{
					p.StopCurrentAbility();
				}
				return;
			}
			if (p.smoothRotation)
			{
				Quaternion quaternion = Quaternion.LookRotation(planeNormal, p.GetVelocity()) * Quaternion.Euler(-90f, 90f, 90f);
				if (!visualRotation.HasValue)
				{
					visualRotation = quaternion;
				}
				else
				{
					visualRotation = Quaternion.Lerp(visualRotation.Value, quaternion, 1f - Mathf.Exp(-5f * Core.dt));
				}
				p.SetVisualRot(visualRotation.Value);
			}
			else
			{
				p.visualTf.rotation = Quaternion.Lerp(p.visualTf.rotation, Quaternion.LookRotation(Vector3.down, planeNormal), 1f - Mathf.Exp(-1.3f * Core.dt));
			}
			p.visualTf.position = Vector3.Lerp(p.visualTf.position, p.tf.position + planeNormal * visualPosOffsetHorizontal + Vector3.up, 1f - Mathf.Exp(-2f * Core.dt));
			if (p.abilityTimer > 0.1f)
			{
				Vector3 dirIfNoSteer = (flag ? planeNormal : (-planeNormal));
				p.airDashAbility.CheckActivation(dirIfNoSteer);
			}
		}

		private void StartTrick()
		{
			AirTrickAbility airTrickAbility = p.airTrickAbility;
			airTrickAbility.curTrick = p.InputToTrickNumber();
			if (p.CheckBoostTrick())
			{
				p.PlayVoice(AudioClipID.VoiceBoostTrick, VoicePriority.BOOST_TRICK);
				p.PlayAnim(airTrickAbility.airBoostTrickHashes[airTrickAbility.curTrick]);
				trickScoreBuffered = true;
				trickTimer = trickDuration * 1.5f;
				p.AddBoostCharge(0f - p.boostTrickCost);
				trickType = AirTrickAbility.TrickType.BOOST_TRICK;
			}
			else
			{
				p.PlayAnim(airTrickAbility.airTrickHashes[airTrickAbility.curTrick]);
				trickScoreBuffered = true;
				trickTimer = trickDuration;
				trickType = AirTrickAbility.TrickType.VERT_TRICK;
			}
		}

		public void Trigger(Vector3 jumpOffPoint)
		{
			planeRight = Vector3.Cross(Vector3.up, readyToLaunchNormal) * -1f;
			planeNormal = Vector3.Cross(planeRight, Vector3.up) * -1f;
			Trigger(jumpOffPoint, planeNormal);
		}

		public void Trigger(Vector3 jumpOffPoint, Vector3 planeNormal)
		{
			this.jumpOffPoint = jumpOffPoint;
			this.planeNormal = planeNormal;
			p.ActivateAbility(this);
		}

		public override void OnStopAbility()
		{
			p.hitbox.SetActive(value: false);
			p.visualTf.localPosition = Vector3.zero;
			normalMovement = true;
		}
	}
}
