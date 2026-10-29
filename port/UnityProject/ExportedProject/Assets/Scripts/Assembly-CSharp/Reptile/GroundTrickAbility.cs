using UnityEngine;

namespace Reptile
{
	public class GroundTrickAbility : Ability
	{
		private bool boostTrick;

		private string[] trickingTrickNames = new string[6] { "Hook kick", "Cartwheel", "Back Sweep", "Coin Drop", "Coin Drop", "Coin Drop" };

		private string[] skateboardTrickNames = new string[6] { "Kickflip", "Backside 360 Powerspin", "360 Flip", "Handstand Flip", "Handstand Flip", "Handstand Flip" };

		private string[] inlineSkatesTrickNames = new string[6] { "Unity Spin Cess Slide", "Duckwalk Fakie", "Back Closed Tree", "Bio", "Bio", "Bio" };

		private string[] bmxTrickNames = new string[6] { "Cyclone", "Knoll Spin", "Stubble duck", "Around The World", "Around The World", "Around The World" };

		private HitDir[] trickingHitDir = new HitDir[6]
		{
			HitDir.left,
			HitDir.down,
			HitDir.left,
			HitDir.left,
			HitDir.left,
			HitDir.left
		};

		private bool stopDecided;

		private bool wantToStop;

		public float defaultSpeed;

		public int curTrick = 1;

		public HitDir curTrickDir = HitDir.forward;

		private int trickCount;

		private bool hitEnemy;

		private float duration;

		private float cancelBeforeEnd;

		private bool reTrickFail;

		private float reTrickMargin = 0.325f;

		private const float inlineTrickDuration = 0.75f;

		private const float trickingTrickDuration = 0.75f;

		private const float skateboardTrickDuration = 0.8431f;

		private const float bmxTrickDuration = 0.862f;

		private int stopRunHash = Animator.StringToHash("stopRun");

		private int[] groundTrickHashes;

		private int[] groundBoostTrickHashes;

		private const int SPECIAL_KICK_TRICK_ID = 1;

		public HitDir CurTrickDir => curTrickDir;

		public bool IsCurrentTrickSpecialKick => curTrick == 1;

		public GroundTrickAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			int num = 3;
			groundTrickHashes = new int[num];
			for (int i = 0; i < num; i++)
			{
				string text = i.ToString();
				groundTrickHashes[i] = Animator.StringToHash("groundTrick" + text);
			}
			groundBoostTrickHashes = new int[num];
			for (int j = 0; j < num; j++)
			{
				string text2 = j.ToString();
				groundBoostTrickHashes[j] = Animator.StringToHash("groundBoostTrick" + text2);
			}
		}

		public override void OnStartAbility()
		{
			hitEnemy = false;
			duration = 0.75f;
			if (p.moveStyle == MoveStyle.SKATEBOARD || p.moveStyle == MoveStyle.SPECIAL_SKATEBOARD)
			{
				duration = 0.8431f;
			}
			else if (p.moveStyle == MoveStyle.BMX)
			{
				duration = 0.862f;
			}
			else if (p.moveStyle == MoveStyle.INLINE)
			{
				duration = 0.75f;
			}
			defaultSpeed = p.maxMoveSpeed * 0.62f;
			curTrick = p.InputToTrickNumber();
			curTrickDir = trickingHitDir[curTrick];
			targetSpeed = defaultSpeed;
			rotateSpeed = 3f;
			acc = 35f;
			decc = p.stats.groundDecc / 2.5f;
			trickCount = 1;
			if (p.GetForwardSpeed() < defaultSpeed)
			{
				p.SetForwardSpeed(defaultSpeed);
			}
			wantToStop = (stopDecided = (reTrickFail = false));
			if (!p.IsComboing())
			{
				p.RefillComboTimeOut();
			}
			boostTrick = false;
			if (!p.isAI && p.CheckBoostTrick())
			{
				DoBoostTrick();
			}
			else
			{
				p.PlayAnim(groundTrickHashes[curTrick], forceOverwrite: true, instant: false, 0f);
			}
			p.hitboxLeftLeg.SetActive(value: false);
			p.hitboxRightLeg.SetActive(value: false);
			p.hitboxUpperBody.SetActive(value: false);
			if (!p.isAI)
			{
				p.StartClosestCheckEnemyForAutoAim();
			}
		}

		public void DoBoostTrick()
		{
			p.PlayAnim(groundBoostTrickHashes[curTrick], forceOverwrite: true, instant: false, 0f);
			boostTrick = true;
			p.ringParticles.Emit(1);
			duration *= 1.8f;
			if (!p.isAI)
			{
				p.SetForwardSpeed(p.boostSpeed);
			}
			p.DoTrick(Player.TrickType.GROUND_BOOST, GetTrickName(curTrick, boostTrick: true), curTrick);
			p.PlayVoice(AudioClipID.VoiceBoostTrick, VoicePriority.BOOST_TRICK);
			p.AddBoostCharge(0f - p.boostTrickCost);
		}

		public string GetTrickName(int num, bool boostTrick)
		{
			num = (boostTrick ? (num + 3) : num);
			if (p.moveStyle == MoveStyle.ON_FOOT)
			{
				return trickingTrickNames[num];
			}
			if (p.moveStyle == MoveStyle.BMX)
			{
				return bmxTrickNames[num];
			}
			if (p.moveStyle == MoveStyle.INLINE)
			{
				return inlineSkatesTrickNames[num];
			}
			return skateboardTrickNames[num];
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
		}

		public override void FixedUpdateAbility()
		{
			p.OrientVisualToSurfaceInAbility();
			if (p.IsComboing())
			{
				p.DoComboTimeOut(Core.dt / 4f);
			}
			if (p.vertAbility.CheckActivation())
			{
				return;
			}
			if (boostTrick)
			{
				if (p.abilityTimer < 0.35f && !p.isAI)
				{
					Core.Instance.GameInput.SetVibrationOnCurrentController(0.19f, 0.19f, 0.1f);
				}
			}
			else if (!p.isAI && p.abilityTimer < 0.1f)
			{
				if (p.CheckBoostTrick())
				{
					DoBoostTrick();
				}
			}
			else if (!p.didAbilityTrick)
			{
				p.DoTrick(Player.TrickType.GROUND, GetTrickName(curTrick, boostTrick: false), curTrick);
			}
			if (!p.IsGrounded())
			{
				if (p.abilityTimer > 0.5f)
				{
					p.StopCurrentAbility();
				}
			}
			else if (p.abilityTimer > duration)
			{
				p.StopCurrentAbility();
			}
			else
			{
				if (p.abilityTimer < duration - reTrickMargin - (hitEnemy ? 0.06f : 0f))
				{
					if (p.AnyTrickInput())
					{
						reTrickFail = true;
					}
				}
				else
				{
					if ((!reTrickFail || hitEnemy) && p.AnyTrickInput())
					{
						DoGroundTrick();
						return;
					}
					if (p.slideButtonHeld && !p.slideAbility.locked)
					{
						p.ActivateAbility(p.slideAbility);
					}
					if (p.jumpButtonNew)
					{
						if (!p.specialAirAbility.locked && !p.onLauncher && p.moveStyle == MoveStyle.ON_FOOT)
						{
							p.ActivateAbility(p.specialAirAbility);
						}
						else
						{
							p.Jump();
							p.ActivateAbility(p.airTrickAbility);
						}
						p.DoComboTimeOut(0.125f);
						return;
					}
				}
				if (p.abilityTimer > 0.15f && p.abilityTimer < 0.38f && p.moveStyle != MoveStyle.ON_FOOT)
				{
					p.hitbox.SetActive(value: true);
				}
				else if (p.moveStyle != MoveStyle.ON_FOOT)
				{
					p.hitbox.SetActive(value: false);
				}
				if (p.abilityTimer > duration - 0.25f)
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
			}
			if (!p.isAI && p.closestEnemy != null && p.abilityTimer < 0.4f)
			{
				Vector3 normalized = (p.closestEnemy.position - p.tf.position).normalized;
				normalized.y = 0f;
				normalized.Normalize();
				customTargetRot = (p.moveInput + normalized * 1.9f).normalized;
				rotateSpeed = 6f;
			}
			else
			{
				if (rotateSpeed == 6f)
				{
					rotateSpeed = -1f;
				}
				customTargetRot = Vector3.zero;
			}
			if (p.usingEquippedMovestyle && p.inWalkZone)
			{
				p.StopCurrentAbility();
			}
		}

		public void DoGroundTrick()
		{
			int num = trickCount;
			p.ActivateAbility(this);
			trickCount = num + 1;
			if (trickCount > 3)
			{
				trickCount = 3;
			}
		}

		public override bool CheckActivation()
		{
			if (p.AnyTrickInput() && p.IsGrounded() && !locked)
			{
				p.ActivateAbility(this);
				return true;
			}
			return false;
		}

		public override void OnStopAbility()
		{
			base.OnStopAbility();
			p.hitbox.SetActive(value: false);
			p.hitboxLeftLeg.SetActive(value: false);
			p.hitboxRightLeg.SetActive(value: false);
			p.hitboxUpperBody.SetActive(value: false);
			if (!p.isAI)
			{
				p.autoAimCheck.SetActive(value: false);
			}
		}
	}
}
