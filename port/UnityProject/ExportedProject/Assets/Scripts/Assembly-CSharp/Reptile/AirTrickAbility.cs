using UnityEngine;

namespace Reptile
{
	public class AirTrickAbility : Ability
	{
		public enum TrickType
		{
			NORMAL_TRICK = 0,
			VERT_TRICK = 1,
			BOOST_TRICK = 2
		}

		private TrickType trickType;

		private string[] trickingTrickNames = new string[6] { "Bullet Spin", "Backflip Grab", "Cheat 720", "Shuriken", "Shuriken", "Shuriken" };

		private string[] skateboardTrickNames = new string[6] { "Backside 360 Varial", "Backflip Indy", "Method Grab", "1080", "1080", "1080" };

		private string[] bmxTrickNames = new string[6] { "Tailwhip 360", "No Hand Backflip", "Superman Seat Grab Indian", "720 Double Backflip", "720 Double Backflip", "720 Double Backflip" };

		private string[] inlineTrickNames = new string[6] { "Cork 720", "Method Grab", "Abstract 360", "1080 California Roll", "1080 California Roll", "1080 California Roll" };

		private bool hitEnemy;

		private bool bufferSwitchStyle;

		public float duration;

		public float trickingTrickDuration = 0.90000004f;

		public float skateboardTrickDuration = 0.95000005f;

		public float bmxTrickDuration = 0.95000005f;

		public float inlineTrickDuration = 0.95000005f;

		public int curTrick = 1;

		internal int[] airTrickHashes;

		internal int[] airBoostTrickHashes;

		public AirTrickAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			allowNormalJump = true;
			keepJumping = true;
			int num = 3;
			airTrickHashes = new int[num];
			for (int i = 0; i < num; i++)
			{
				string text = i.ToString();
				airTrickHashes[i] = Animator.StringToHash("airTrick" + text);
			}
			airBoostTrickHashes = new int[num];
			for (int j = 0; j < num; j++)
			{
				string text2 = j.ToString();
				airBoostTrickHashes[j] = Animator.StringToHash("airBoostTrick" + text2);
			}
		}

		public override void OnStartAbility()
		{
			hitEnemy = false;
			p.SetDustEmission(0);
			curTrick = p.InputToTrickNumber();
			bufferSwitchStyle = false;
			targetSpeed = -1f;
			rotateSpeed = -1f;
			decc = -1f;
			trickType = TrickType.NORMAL_TRICK;
			duration = trickingTrickDuration;
			if (p.moveStyle == MoveStyle.SKATEBOARD || p.moveStyle == MoveStyle.SPECIAL_SKATEBOARD)
			{
				duration = skateboardTrickDuration;
			}
			else if (p.moveStyle == MoveStyle.BMX)
			{
				duration = bmxTrickDuration;
			}
			else if (p.moveStyle == MoveStyle.INLINE)
			{
				duration = inlineTrickDuration;
			}
			if (p.CheckBoostTrick())
			{
				SetupBoostTrick();
			}
			else
			{
				p.PlayAnim(airTrickHashes[curTrick], forceOverwrite: true, instant: false, 0f);
			}
			p.hitboxLeftLeg.SetActive(value: false);
			p.hitboxRightLeg.SetActive(value: false);
			p.hitboxUpperBody.SetActive(value: false);
		}

		public void SetupBoostTrick()
		{
			p.PlayAnim(airBoostTrickHashes[curTrick], forceOverwrite: true, instant: false, 0f);
			p.PlayVoice(AudioClipID.VoiceBoostTrick, VoicePriority.BOOST_TRICK);
			p.ringParticles.Emit(1);
			trickType = TrickType.BOOST_TRICK;
			duration *= 1.5f;
			if (!p.isAI)
			{
				p.SetForwardSpeed(p.boostSpeed);
			}
			p.AddBoostCharge(0f - p.boostTrickCost);
		}

		public void Trick(TrickType type)
		{
			string text = "";
			int num = curTrick + ((type == TrickType.BOOST_TRICK) ? 3 : 0);
			text = ((p.moveStyle == MoveStyle.ON_FOOT) ? trickingTrickNames[num] : ((p.moveStyle == MoveStyle.BMX) ? bmxTrickNames[num] : ((p.moveStyle != MoveStyle.INLINE) ? skateboardTrickNames[num] : inlineTrickNames[num])));
			switch (type)
			{
			case TrickType.VERT_TRICK:
				p.DoTrick(Player.TrickType.AIR_VERT, text, curTrick);
				break;
			case TrickType.NORMAL_TRICK:
				p.DoTrick(Player.TrickType.AIR, text, curTrick);
				break;
			case TrickType.BOOST_TRICK:
				p.DoTrick(Player.TrickType.AIR_BOOST, text, curTrick);
				break;
			}
		}

		public void HitEnemy()
		{
			hitEnemy = true;
			if (p.usingEquippedMovestyle)
			{
				p.SwitchToEquippedMovestyle(set: false);
			}
			float num = (targetSpeed = p.maxMoveSpeed * 0.35f);
			if (p.GetForwardSpeed() > num)
			{
				p.SetForwardSpeed(num);
			}
			Vector3 velocity = p.GetVelocity();
			velocity.y = 5f;
			p.SetVelocity(velocity);
		}

		public override void FixedUpdateAbility()
		{
			p.SetVisualRotLocal0();
			if (!p.didAbilityTrick)
			{
				if (p.abilityTimer < 0.1f && trickType != TrickType.BOOST_TRICK && p.CheckBoostTrick())
				{
					SetupBoostTrick();
				}
				if (p.abilityTimer > duration * 0.6f)
				{
					Trick(trickType);
				}
			}
			if (trickType == TrickType.BOOST_TRICK)
			{
				if (p.abilityTimer < 0.15f)
				{
					p.characterVisual.SetBoostpackEffect(BoostpackEffectMode.BOOSTING);
				}
				else
				{
					p.characterVisual.SetBoostpackEffect(BoostpackEffectMode.OFF);
				}
				if (p.abilityTimer < 0.35f && !p.isAI)
				{
					Core.Instance.GameInput.SetVibrationOnCurrentController(0.19f, 0.19f, 0.1f);
				}
			}
			if (!p.switchToEquippedMovestyleLocked && p.switchStyleButtonNew)
			{
				bufferSwitchStyle = true;
			}
			if (p.slideAbility.CheckActivation() || p.airDashAbility.CheckActivation() || p.ledgeClimbAbility.CheckActivation())
			{
				return;
			}
			if (p.abilityTimer > 0.15f && p.abilityTimer < 0.4f && p.moveStyle != MoveStyle.ON_FOOT)
			{
				p.airialHitbox.SetActive(value: true);
			}
			else if (p.moveStyle != MoveStyle.ON_FOOT)
			{
				p.airialHitbox.SetActive(value: false);
			}
			if (p.IsGrounded() || p.abilityTimer > duration)
			{
				p.StopCurrentAbility();
			}
			else if (p.abilityTimer > duration - 0.3f)
			{
				if (bufferSwitchStyle)
				{
					p.SwitchToEquippedMovestyle(!p.usingEquippedMovestyle, doAirTrick: true);
				}
				else
				{
					CheckActivation();
				}
			}
		}

		public override bool CheckActivation()
		{
			if (p.AnyTrickInput() && !p.IsGrounded() && !locked)
			{
				p.ActivateAbility(this);
				return true;
			}
			return false;
		}

		public override void OnStopAbility()
		{
			p.airialHitbox.SetActive(value: false);
			p.hitbox.SetActive(value: false);
			p.hitboxLeftLeg.SetActive(value: false);
			p.hitboxRightLeg.SetActive(value: false);
			p.hitboxUpperBody.SetActive(value: false);
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
