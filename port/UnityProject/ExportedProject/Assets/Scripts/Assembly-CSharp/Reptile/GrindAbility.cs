using UnityEngine;

namespace Reptile
{
	public class GrindAbility : Ability
	{
		private string[] trickNames;

		private string[] startTrickNames;

		private string[] trickingStartTrickNames = new string[7] { "Slide", "Slide", "Slide", "Slide", "boostTrick", "boostTrick", "boostTrick" };

		private string[] trickingTrickNames = new string[4] { "", "", "", "" };

		private string[] skateboardStartTrickNames = new string[6] { "Backside Overcrook", "Frontside 50-50", "Frontside Boardslide", "Darkslide", "Darkslide", "Darkslide" };

		private string[] skateboardTrickNames = new string[6] { "Shove-it Backside Overcrook", "360 Flip Frontside Tailslide", "360 Frontside Boardslide", "Darkslide", "Darkslide", "Darkslide" };

		private string[] bmxStartTrickNames = new string[6] { "Toothpick Grind", "Double Peg Grind", "Crank Arm Grind", "Backyard Grind", "Backyard Grind", "Backyard Grind" };

		private string[] bmxTrickNames = new string[6] { "Toothpick", "Whiplash Ice Pick", "240 Crank Arm Grind", "Backyard Grind", "Backyard Grind", "Backyard Grind" };

		private string[] inlineStartTrickNames = new string[6] { "Makio Grind", "Soul Grind", "Unity Grind", "Topside Pornstar", "Topside Pornstar", "Topside Pornstar" };

		private string[] inlineTrickNames = new string[6] { "Makio Grind", "Frontside Grind", "Unity Grind", "Topside Pornstar", "Topside Pornstar", "Topside Pornstar" };

		private string inlineSwitchMovestyleTrickName = "360 Frontside Grind";

		private string skateboardSwitchMovestyleTrickName = "Caveman";

		private string bmxSwitchMovestyleTrickName = "Whiplash Double Peg Grind";

		public float speed;

		public float speedTarget;

		public float showBoostpackEffectAfterSpeed = 14.8f;

		public float grindRotSpeed = 5f;

		public float cornerBoost = 7.3f;

		public float grindDeccAboveNormal = 2.9f;

		public float grindDeccBelowNormal = 10f;

		public float grindAcc = 8f;

		public float cooldown;

		public bool braking;

		public GrindLine grindLine;

		public Vector3 preGrindDir;

		public GrindNode nextNode;

		public GrindPath lastPath;

		public float posOnLine;

		public Vector2 grindTilt;

		public Vector2 grindTiltBuffer;

		public float grindTiltBufferTimer;

		public float grindTiltAnimMultiplyer = 1f;

		public Vector3 normal;

		public bool softCornerBoost;

		private float timeSinceLastNode;

		private EnemyGrindLines grindingOnEnemy;

		public float trickTimer;

		private float reTrickMargin = 0.205f;

		private bool reTrickFail;

		private bool trickBuffered;

		private float trickStandardDuration;

		private float trickingTrickDuration = 0.666f;

		private float skateboardTrickDuration = 0.666f;

		private float bmxTrickDuration = 0.712f;

		private int curTrick = 1;

		private bool curTrickFirst;

		private bool curTrickBoost;

		private float curTrickDuration;

		private bool inRetour;

		private int jumpHash;

		private int switchToEquippedMovestyleGrindHash;

		private int[] grindTrickHashes;

		private int[] grindTrickHoldHashes;

		private int[] grindBoostTrickHashes;

		private int[] grindBoostTrickHoldHashes;

		private int grindBoostHash;

		private int grindDirectionHash;

		private bool justFollowNodes => p.smoothRotation;

		public GrindAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalMovement = false;
			normalRotation = false;
			rotateWithPlatform = false;
			customGravity = 0f;
			canStartGrind = false;
			canStartWallrun = true;
			treatPlayerAsSortaGrounded = true;
			if (p.isAI)
			{
				curTrick = Random.Range(0, 3);
			}
			jumpHash = Animator.StringToHash("jump");
			switchToEquippedMovestyleGrindHash = Animator.StringToHash("switchToEquippedMovestyleGrind");
			grindBoostHash = Animator.StringToHash("grindBoost");
			grindDirectionHash = Animator.StringToHash("grindDirection");
			int num = 3;
			grindTrickHashes = new int[num];
			grindTrickHoldHashes = new int[num];
			for (int i = 0; i < num; i++)
			{
				string text = i.ToString();
				grindTrickHashes[i] = Animator.StringToHash("grindTrick" + text);
				grindTrickHoldHashes[i] = Animator.StringToHash("grindTrick" + text + "Hold");
			}
			grindBoostTrickHashes = new int[num];
			grindBoostTrickHoldHashes = new int[num];
			for (int j = 0; j < num; j++)
			{
				string text2 = j.ToString();
				grindBoostTrickHashes[j] = Animator.StringToHash("grindBoostTrick" + text2);
				grindBoostTrickHoldHashes[j] = Animator.StringToHash("grindBoostTrick" + text2 + "Hold");
			}
			customCameraMode = new CameraModeGrind(this);
		}

		public bool CanSetToLine(GrindLine line)
		{
			if (!p.CanStartGrind() && (!line.isPole || p.ability != p.vertAbility || p.IsBusyWithSequence()))
			{
				return false;
			}
			bool flag = Mathf.Abs(Vector3.Dot(p.tf.up, Vector3.up)) < 0.25f || (line.isPole && p.ability != p.flipOutJumpAbility);
			if (cooldown > 0f && line.Path == lastPath && !flag)
			{
				return false;
			}
			Vector3 position = p.tf.position;
			Vector3 velocity = p.GetVelocity();
			if (p.ability == p.wallrunAbility)
			{
				velocity = p.wallrunAbility.GetVelocity();
			}
			Vector3 vector = Vector3.ProjectOnPlane(velocity, Vector3.up);
			Vector3 vector2 = vector.normalized;
			if (vector2 == Vector3.zero)
			{
				vector2 = p.dir;
			}
			if (line.GetRelativePosOnLine(position, vector2) >= 1f)
			{
				return false;
			}
			Vector3 vector3 = line.SnapPosToLine(position);
			Vector3 normalAtPos = line.GetNormalAtPos(vector3);
			if (velocity.y > 0f && vector != Vector3.zero && (vector3 - position).magnitude > 0.1f && Vector3.Dot((vector3 - position).normalized, velocity.normalized) < 0.01f)
			{
				return false;
			}
			if (p.IsGrounded() && vector3.y - position.y > 0.25f)
			{
				return false;
			}
			if (vector3.y < position.y + 1f || (vector3.y > position.y + 1f && Vector3.Dot(normalAtPos, Vector3.down) > 0f))
			{
				return true;
			}
			return false;
		}

		public void PassiveUpdate()
		{
			if (cooldown > 0f)
			{
				cooldown -= Core.dt;
			}
		}

		public Vector3 GetPlayerDirForLine(GrindLine setGrindLine)
		{
			Vector3 vector = Vector3.ProjectOnPlane(p.GetVelocity(), Vector3.up);
			if (vector == Vector3.zero)
			{
				vector = p.dir;
			}
			vector.y = 0f;
			vector = vector.normalized;
			vector.y = 0.1f;
			if (setGrindLine.isPole)
			{
				vector = Vector3.up;
			}
			return vector;
		}

		public void SetToLine(GrindLine setGrindLine)
		{
			timeSinceLastNode = 0f;
			speedTarget = p.stats.grindSpeed;
			bool switchToEquippedMovestyle = false;
			if (!p.usingEquippedMovestyle)
			{
				switchToEquippedMovestyle = true;
				p.SwitchToEquippedMovestyle(set: true);
			}
			speed = Mathf.Max(p.boosting ? p.boostSpeed : p.stats.grindSpeed, p.GetForwardSpeed());
			startTrickNames = trickingStartTrickNames;
			trickNames = trickingTrickNames;
			trickStandardDuration = trickingTrickDuration;
			if (p.moveStyle == MoveStyle.SKATEBOARD || p.moveStyle == MoveStyle.SPECIAL_SKATEBOARD)
			{
				startTrickNames = skateboardStartTrickNames;
				trickNames = skateboardTrickNames;
				trickStandardDuration = skateboardTrickDuration;
			}
			else if (p.moveStyle == MoveStyle.INLINE)
			{
				startTrickNames = inlineStartTrickNames;
				trickNames = inlineTrickNames;
				trickStandardDuration = trickingTrickDuration;
			}
			else if (p.moveStyle == MoveStyle.BMX)
			{
				startTrickNames = bmxStartTrickNames;
				trickNames = bmxTrickNames;
				trickStandardDuration = bmxTrickDuration;
			}
			Vector3 position = p.tf.position;
			p.motor.HaveCollision(have: false);
			p.ForceUnground();
			Vector3 playerDirForLine = GetPlayerDirForLine(setGrindLine);
			nextNode = setGrindLine.GetNextNode(playerDirForLine);
			p.SetVelocity(Vector3.zero);
			p.ResetVisualRot();
			Vector3 vector = setGrindLine.SnapPosToLine(position);
			p.motor.SetPositionTeleport(vector);
			grindLine = setGrindLine;
			lastPath = grindLine.Path;
			posOnLine = (justFollowNodes ? grindLine.GetRelativePosOnLine(vector, nextNode) : grindLine.GetRelativePosOnLine(vector, playerDirForLine));
			if ((bool)grindLine.attachedToEnemyGrindLine)
			{
				grindingOnEnemy = grindLine.attachedToEnemyGrindLine;
				grindingOnEnemy.BeingGrindedByPlayer = true;
			}
			AudioManager audioManager = p.AudioManager;
			audioManager.PlaySfxGameplay(setGrindLine.sfxCollection, AudioClipID.railStart, p.playerOneShotAudioSource);
			audioManager.PlaySfxGameplayLooping(setGrindLine.sfxCollection, AudioClipID.grindLoop, p.playerGrindLoopAudioSource);
			p.anim.SetFloat(grindBoostHash, 0f);
			grindTilt = (grindTiltBuffer = Vector2.zero);
			grindTiltBufferTimer = 0f;
			softCornerBoost = false;
			p.ActivateAbility(this);
			normal = grindLine.GetNormalAtPos(vector);
			if (grindLine.isPole)
			{
				normal = Vector3.ProjectOnPlane(p.dir * -1f, Vector3.up).normalized;
			}
			Vector3 forward = (preGrindDir = setGrindLine.GetLineDir(playerDirForLine));
			p.SetRotHard(Quaternion.LookRotation(forward, normal));
			StartGrindTrick(first: true, switchToEquippedMovestyle);
		}

		public Vector3 GetGrindMovementAsVelocity()
		{
			if (!nextNode)
			{
				return Vector3.zero;
			}
			return (nextNode - p.tf.position).normalized * speed;
		}

		public override void FixedUpdateAbility()
		{
			if (inRetour)
			{
				if (p.GetAnimTime() < 1f)
				{
					return;
				}
				EndRetour();
			}
			int curAnim = p.curAnim;
			bool flag = false;
			int num = 3;
			for (int i = 0; i < num; i++)
			{
				flag |= curAnim == grindTrickHoldHashes[i] || curAnim == grindBoostTrickHoldHashes[i];
			}
			timeSinceLastNode += Core.dt;
			p.SetDustEmission(0);
			UpdateSpeed();
			UpdateTricks();
			Vector3 position = p.tf.position;
			Vector3 vector = p.tf.forward;
			Vector3 right = p.tf.right;
			if (!justFollowNodes)
			{
				nextNode = this.grindLine.GetNextNode(vector);
			}
			position = this.grindLine.SnapPosToLine(position);
			position += (nextNode - position).normalized * speed * Core.dt;
			posOnLine = (justFollowNodes ? this.grindLine.GetRelativePosOnLine(position, nextNode) : this.grindLine.GetRelativePosOnLine(position, vector));
			float num2 = (justFollowNodes ? this.grindLine.GetAbsoluteLinePos(position, nextNode) : this.grindLine.GetAbsoluteLinePos(position, vector));
			if (posOnLine >= 1f)
			{
				timeSinceLastNode = 0f;
				float num3 = posOnLine * this.grindLine.Length() - this.grindLine.Length();
				if (nextNode.IsEndpoint)
				{
					if (nextNode.retour)
					{
						StartRetour();
						return;
					}
					if (this.grindLine.isPole)
					{
						if (Mathf.Abs(Vector3.Dot(vector, Vector3.up)) > 0f)
						{
							p.handplantAbility.SetToPole(nextNode.position);
						}
						else
						{
							JumpOut(flipOut: true);
						}
						return;
					}
					if (Mathf.Abs(Vector3.Dot(p.tf.up, Vector3.up)) < 0.35f || this.grindLine.alwaysFlipBack)
					{
						JumpOut(flipOut: true);
						return;
					}
					p.motor.SetPositionTeleport(position + Vector3.up * p.motor.GetCapsule().height * Mathf.Clamp(p.tf.up.y, -1f, 0f));
					normal = Vector3.up;
					Vector3 vector2 = p.FlattenRotationHard();
					p.SetVelocity(vector2 * speed);
					if (p.boosting)
					{
						p.ActivateAbility(p.boostAbility);
						p.boostAbility.StartFromRunOffGrindOrWallrun();
					}
					else
					{
						p.StopCurrentAbility();
						p.FlattenRotationHard();
					}
					return;
				}
				GrindLine grindLine = (justFollowNodes ? this.grindLine.GetNextLine(vector, nextNode) : this.grindLine.GetNextLine(vector));
				Vector3 normalized = (grindLine.GetOtherNode(nextNode) - nextNode).normalized;
				RewardTilting(right, normalized);
				vector = normalized;
				position = nextNode.position + num3 * normalized;
				if (this.grindLine.sfxCollection != grindLine.sfxCollection && grindLine.sfxCollection != SfxCollectionID.NONE)
				{
					p.AudioManager.PlaySfxGameplayLooping(grindLine.sfxCollection, AudioClipID.grindLoop, p.playerGrindLoopAudioSource);
				}
				this.grindLine = grindLine;
				nextNode = (justFollowNodes ? this.grindLine.GetOtherNode(nextNode) : this.grindLine.GetNextNode(normalized));
				posOnLine = (justFollowNodes ? this.grindLine.GetRelativePosOnLine(position, nextNode) : this.grindLine.GetRelativePosOnLine(position, normalized));
				num2 = (justFollowNodes ? this.grindLine.GetAbsoluteLinePos(position, nextNode) : this.grindLine.GetAbsoluteLinePos(position, vector));
				normal = this.grindLine.GetNormalAtPos(position);
				p.SetRotation(normalized, normal);
			}
			UpdateBoostpack();
			UpdateTilting();
			if (!this.grindLine.isPole)
			{
				normal = this.grindLine.GetNormalAtPos(position);
			}
			float num4 = this.grindLine.Length();
			float num5 = Mathf.Min(num4 * 0.5f, 1.5f);
			Vector3 forward = Vector3.forward;
			if (!nextNode.IsEndpoint && num2 >= num4 - num5)
			{
				Vector3 normalized2 = ((justFollowNodes ? this.grindLine.GetNextLine(vector, nextNode) : this.grindLine.GetNextLine(vector)).GetOtherNode(nextNode) - nextNode).normalized;
				Vector3 normalized3 = (nextNode - position).normalized;
				Vector3 vector3 = Vector3.Slerp(normalized3, normalized2, 0.5f);
				forward = ((!(num2 >= num4)) ? Vector3.Slerp(normalized3, vector3, 1f - (num4 - num2) / num5) : vector3);
				preGrindDir = normalized3;
				if (!p.smoothRotation)
				{
					p.SetRotation(forward.normalized, normal);
				}
			}
			else if (preGrindDir != Vector3.zero && num2 <= num5)
			{
				Vector3 normalized4 = (nextNode - position).normalized;
				Vector3 vector4 = Vector3.Slerp(normalized4, preGrindDir, 0.5f);
				forward = ((!(num2 <= 0f)) ? Vector3.Slerp(vector4, normalized4, num2 / num5) : vector4);
				if (!p.smoothRotation)
				{
					p.SetRotation(forward.normalized, normal);
				}
			}
			else if (p.smoothRotation)
			{
				forward = (preGrindDir = nextNode - position);
			}
			else
			{
				p.SetRotHard(Quaternion.LookRotation((nextNode - position).normalized, normal));
			}
			if (p.smoothRotation)
			{
				Quaternion rotation = Quaternion.LookRotation(forward, normal);
				p.SetRotation(rotation);
			}
			customVelocity = (position - p.tf.position) * 60f;
			p.lastElevationForSlideBoost = position.y;
			if (p.jumpButtonNew && (p.isAI || trickTimer < reTrickMargin) && p.abilityTimer > 0.1f)
			{
				float num6 = Vector3.Dot(p.tf.up, Vector3.up);
				JumpOut(this.grindLine.isPole || (num6 > -0.25f && num6 < 0.35f));
			}
			else
			{
				p.SetVisualRotLocalZ(grindTilt.x * -30f);
			}
		}

		private void UpdateSpeed()
		{
			if (speed < speedTarget)
			{
				speed = Mathf.Min(speedTarget, speed + grindAcc * Core.dt);
			}
			else if (speed > speedTarget)
			{
				speed = Mathf.Max(speedTarget, speed - ((speed >= p.stats.grindSpeed) ? grindDeccAboveNormal : grindDeccBelowNormal) * Core.dt);
			}
			if (p.boosting)
			{
				speed = (speedTarget = p.boostSpeed);
			}
			else if (softCornerBoost)
			{
				speedTarget = p.boostSpeed;
				if (timeSinceLastNode > 1f)
				{
					softCornerBoost = false;
				}
			}
			else if (p.AirBraking() && !p.isAI)
			{
				braking = true;
				speedTarget = p.stats.grindSpeed * 0.35f;
			}
			else
			{
				braking = false;
				speedTarget = p.stats.grindSpeed;
			}
		}

		public void UpdateBoostpack()
		{
			if (p.boostButtonHeld && p.HasBoostCharge() && !p.boostAbility.locked)
			{
				if (trickTimer <= 0.325f)
				{
					p.characterVisual.SetBoostpackEffect(BoostpackEffectMode.BOOSTING);
				}
				else
				{
					p.characterVisual.SetBoostpackEffect(BoostpackEffectMode.OFF);
				}
				if (!p.boosting)
				{
					p.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.boostStart, p.playerOneShotAudioSource);
					p.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.GenericMovementSfx, AudioClipID.ultraBoostLoop, p.playerUltraBoostLoopAudioSource);
				}
				if (!p.isAI)
				{
					p.StartScreenShake(ScreenShakeType.ULTRALIGHT, 0.3f, continuous: true);
					Core.Instance.GameInput.SetVibrationOnCurrentController(0.19f, 0.19f, 0.1f);
				}
				p.boosting = true;
				p.UseBoostChargeForBoost();
			}
			else
			{
				if (p.boosting)
				{
					p.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.boostStop, p.playerOneShotAudioSource);
				}
				p.boosting = false;
				p.AudioManager.StopLoopingSfx(p.playerUltraBoostLoopAudioSource);
			}
			if (!p.boosting)
			{
				if (trickTimer <= 0.325f && !braking)
				{
					p.SetBoostpackAndFrictionEffects((!(speed > showBoostpackEffectAfterSpeed) && !softCornerBoost) ? BoostpackEffectMode.ON : BoostpackEffectMode.GRIND_CORNERING, FrictionEffectMode.BLUE);
				}
				else
				{
					p.SetBoostpackAndFrictionEffects(BoostpackEffectMode.OFF, FrictionEffectMode.BLUE);
				}
			}
		}

		public void UpdateTricks()
		{
			if (trickBuffered)
			{
				if (trickTimer <= 0f)
				{
					StartGrindTrick();
				}
			}
			else if (p.AnyTrickInput() && !braking)
			{
				if (trickTimer > reTrickMargin)
				{
					reTrickFail = true;
				}
				else if (!reTrickFail)
				{
					StartGrindTrick();
				}
				else if (trickTimer <= 0f)
				{
					StartGrindTrick();
				}
			}
			if (trickTimer > 0f)
			{
				trickTimer -= Core.dt;
				if (curTrickBoost)
				{
					if (trickTimer > curTrickDuration - 0.35f && !p.isAI)
					{
						Core.Instance.GameInput.SetVibrationOnCurrentController(0.19f, 0.19f, 0.1f);
					}
				}
				else if (!p.isAI && trickTimer > curTrickDuration - 0.1f)
				{
					if (p.CheckBoostTrick())
					{
						DoBoostTrick();
					}
				}
				else if (!p.didAbilityTrick)
				{
					p.DoTrick(Player.TrickType.GRIND, trickNames[curTrick], curTrick);
				}
				if (trickTimer < 0f)
				{
					p.hitbox.SetActive(value: false);
				}
			}
			int curAnim = p.curAnim;
			bool flag = false;
			int num = 3;
			for (int i = 0; i < num; i++)
			{
				flag |= curAnim == grindTrickHashes[i];
			}
			if (flag)
			{
				p.targetMovement = Player.MovementType.NONE;
			}
		}

		private void StartGrindTrick(bool first = false, bool switchToEquippedMovestyle = false)
		{
			curTrickFirst = first;
			curTrickBoost = false;
			p.didAbilityTrick = false;
			if (switchToEquippedMovestyle)
			{
				string trickName = skateboardSwitchMovestyleTrickName;
				if (p.moveStyle == MoveStyle.INLINE)
				{
					trickName = inlineSwitchMovestyleTrickName;
				}
				else if (p.moveStyle == MoveStyle.BMX)
				{
					trickName = bmxSwitchMovestyleTrickName;
				}
				p.DoTrick(Player.TrickType.GRIND_SWITCH_MOVESTYLE, trickName);
				trickTimer = (curTrickDuration = trickStandardDuration);
				p.PlayAnim(switchToEquippedMovestyleGrindHash);
				curTrick = 1;
				return;
			}
			curTrick = p.InputToTrickNumber();
			p.TempSetSpeedAnim(1f);
			if (p.CheckBoostTrick())
			{
				DoBoostTrick();
			}
			else if (first)
			{
				curTrick = 1;
				p.DoTrick(Player.TrickType.GRIND_START, startTrickNames[curTrick], curTrick);
				p.PlayAnim(Animator.StringToHash("grindTrick3"));
			}
			else
			{
				p.hitbox.SetActive(value: true);
				trickTimer = (curTrickDuration = trickStandardDuration);
				p.PlayAnim(grindTrickHashes[curTrick], forceOverwrite: true, instant: false, 0f);
			}
			trickBuffered = (reTrickFail = false);
		}

		private void DoBoostTrick()
		{
			curTrickBoost = true;
			p.ringParticles.Emit(1);
			trickTimer = (curTrickDuration = trickStandardDuration * 1.8f);
			if (!p.isAI)
			{
				speed = p.boostSpeed;
				speedTarget = p.stats.grindSpeed;
			}
			p.AddBoostCharge(0f - p.boostTrickCost);
			p.DoTrick(Player.TrickType.GRIND_BOOST, startTrickNames[3 + curTrick], curTrick);
			p.PlayVoice(AudioClipID.VoiceBoostTrick, VoicePriority.BOOST_TRICK);
			p.PlayAnim(grindBoostTrickHashes[curTrick], forceOverwrite: true, instant: false, 0f);
		}

		public void UpdateTilting()
		{
			float num = p.moveInputPlain.x;
			if (trickTimer > 0.15f)
			{
				num = 0f;
			}
			float maxDelta = 0.18f;
			if (Vector3.Dot(p.tf.up, Vector3.down) > 0.5f)
			{
				num *= -1f;
			}
			if (num > 0f)
			{
				grindTilt.x = Utility.Lerp(grindTilt.x, num, 0.4f, maxDelta);
			}
			else if (num < 0f)
			{
				grindTilt.x = Utility.Lerp(grindTilt.x, num, 0.4f, maxDelta);
			}
			else if (grindTilt.x > 0f)
			{
				grindTilt.x = Utility.Lerp(grindTilt.x, 0f, 0.3f, maxDelta);
			}
			else if (grindTilt.x < 0f)
			{
				grindTilt.x = Utility.Lerp(grindTilt.x, 0f, 0.3f, maxDelta);
			}
			if (Mathf.Abs(grindTilt.x) > 0.25f)
			{
				grindTiltBuffer.x = grindTilt.x;
				grindTiltBufferTimer = 0.1f;
			}
			if (grindTiltBufferTimer > 0f)
			{
				grindTiltBufferTimer -= Core.dt;
				if (grindTiltBufferTimer <= 0f)
				{
					grindTiltBuffer.x = 0f;
				}
			}
			p.anim.SetFloat(grindDirectionHash, grindTilt.x * grindTiltAnimMultiplyer);
		}

		private void RewardTilting(Vector3 rightDir, Vector3 nextLineDir)
		{
			if (!grindLine.cornerBoost)
			{
				return;
			}
			float num = Vector3.Dot(rightDir, Vector3.ProjectOnPlane(nextLineDir, Vector3.up).normalized);
			Side side = Side.NONE;
			if (grindTiltBuffer.x < -0.25f)
			{
				side = Side.LEFT;
			}
			else if (grindTiltBuffer.x > 0.25f)
			{
				side = Side.RIGHT;
			}
			Side side2 = Side.NONE;
			if (num < -0.02f)
			{
				side2 = Side.LEFT;
			}
			else if (num > 0.02f)
			{
				side2 = Side.RIGHT;
			}
			if (side2 != Side.NONE)
			{
				softCornerBoost = false;
			}
			bool flag = Mathf.Abs(num) > 0.3f;
			if (side != Side.NONE && side == side2)
			{
				if (flag && lastPath.hardCornerBoostsAllowed)
				{
					p.StartScreenShake(ScreenShakeType.LIGHT, 0.2f);
					p.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.singleBoost, p.playerOneShotAudioSource);
					p.ringParticles.Emit(1);
					speed = p.stats.grindSpeed + cornerBoost;
					p.HardCornerGrindLine(nextNode);
				}
				else if (lastPath.softCornerBoostsAllowed)
				{
					softCornerBoost = true;
					p.DoTrick(Player.TrickType.SOFT_CORNER, "Corner");
				}
			}
		}

		public override void UpdateAbilityJumpStatus()
		{
			if (!p.jumpedThisFrame)
			{
				p.jumpConsumed = false;
			}
			p.timeSinceLastAbleToJump = 0f;
			p.timeSinceGrinding = 0f;
		}

		private void JumpOut(bool flipOut)
		{
			p.AudioManager.PlayVoice(ref p.currentVoicePriority, p.character, AudioClipID.VoiceJump, p.playerGameplayVoicesAudioSource);
			p.timeSinceLastJump = 0f;
			p.isJumping = true;
			p.jumpConsumed = true;
			p.jumpRequested = false;
			p.jumpedThisFrame = true;
			p.maintainSpeedJump = false;
			p.lastElevationForSlideBoost = float.PositiveInfinity;
			Vector3 up = p.tf.up;
			p.DoJumpEffects(up);
			p.tf.position += Vector3.up * p.motor.GetCapsule().height * Mathf.Clamp(p.tf.up.y, -1f, 0f);
			float num = 0f;
			float num2 = 0f;
			if (flipOut)
			{
				Vector3 normalized = Vector3.ProjectOnPlane(up, Vector3.up).normalized;
				p.SetRotHard(Quaternion.LookRotation(normalized));
				num = p.jumpSpeed * 0.35f;
				num2 = p.maxMoveSpeed * 0.57f;
				p.SetVelocity(num * Vector3.up + normalized * num2);
				p.ActivateAbility(p.flipOutJumpAbility);
			}
			else
			{
				p.PlayAnim(jumpHash);
				Vector3 vector = p.FlattenRotationHard();
				float num3 = 1f + Mathf.Clamp01(p.dir.y) * 0.5f;
				if (!lastPath.upwardsGrindJumpAllowed || !grindLine.upwardsGrindJump)
				{
					num3 = 1f;
				}
				float num4 = p.jumpSpeed + p.bonusJumpSpeedGrind;
				num = ((Vector3.Dot(up, Vector3.up) > -0.1f) ? (num4 * num3) : ((0f - p.jumpSpeed) * 0.5f));
				num2 = Mathf.Min(speed, p.boostSpeed);
				if (p.boosting)
				{
					p.ActivateAbility(p.boostAbility);
					p.boostAbility.StartFromJumpGrindOrWallrun();
				}
				else if (p.slideButtonHeld)
				{
					num *= p.abilityShorthopFactor;
					p.StopCurrentAbility();
					p.maintainSpeedJump = true;
				}
				else
				{
					p.StopCurrentAbility();
				}
				p.SetVelocity(num * Vector3.up + vector * num2);
			}
			p.ForceUnground();
		}

		public override void OnStartAbility()
		{
			if (p.boosting)
			{
				p.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.GenericMovementSfx, AudioClipID.ultraBoostLoop, p.playerUltraBoostLoopAudioSource);
			}
			base.OnStartAbility();
		}

		public override void RecheckLoopingSoundInAbilityUse()
		{
			if (p.boosting)
			{
				p.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.GenericMovementSfx, AudioClipID.ultraBoostLoop, p.playerUltraBoostLoopAudioSource);
			}
			if (grindLine != null && grindLine.sfxCollection != SfxCollectionID.NONE)
			{
				p.AudioManager.PlaySfxGameplayLooping(grindLine.sfxCollection, AudioClipID.grindLoop, p.playerGrindLoopAudioSource);
			}
		}

		public override void OnStopAbility()
		{
			p.RegainAirMobility();
			if ((bool)grindingOnEnemy)
			{
				grindingOnEnemy.BeingGrindedByPlayer = false;
			}
			p.boosting = false;
			cooldown = 0.2f;
			p.motor.HaveCollision(have: true);
			p.ResetVisualRot();
			grindTiltAnimMultiplyer = 1f;
			p.hitbox.SetActive(value: false);
			AudioManager audioManager = p.AudioManager;
			audioManager.StopLoopingSfx(p.playerUltraBoostLoopAudioSource);
			audioManager.StopLoopingSfx(p.playerGrindLoopAudioSource);
			if (grindLine.sfxCollection != SfxCollectionID.NONE)
			{
				audioManager.PlaySfxGameplay(grindLine.sfxCollection, AudioClipID.railStop, p.playerOneShotAudioSource);
			}
			base.OnStopAbility();
			nextNode = null;
		}

		private void StartRetour()
		{
			inRetour = true;
			customVelocity = Vector3.zero;
			p.SetVisualRotLocal0();
			p.PlayAnim(Animator.StringToHash("grindRetour"));
		}

		private void EndRetour()
		{
			inRetour = false;
			timeSinceLastNode = 0f;
			posOnLine = 0f;
			preGrindDir = Vector3.zero;
			grindTilt = Vector2.zero;
			softCornerBoost = false;
			nextNode = grindLine.GetOtherNode(nextNode);
			Vector3 forward = (preGrindDir = grindLine.GetLineDir(nextNode));
			p.SetRotHard(Quaternion.LookRotation(forward, normal));
			p.PlayAnim(Animator.StringToHash("grindRetourEnd"));
		}
	}
}
