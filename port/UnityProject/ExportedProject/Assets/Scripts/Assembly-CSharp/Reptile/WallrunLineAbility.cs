using UnityEngine;

namespace Reptile
{
	public class WallrunLineAbility : Ability
	{
		private string trickName;

		public float lowest = 0.05f;

		public float highest = 1.5f;

		public WallrunLine wallrunLine;

		public Vector3 wallrunFaceNormal;

		public Vector3 dirToNextNode;

		private float journey;

		private float nodeToNodeLength;

		public Side animSide;

		private float scoreTimer;

		public float cooldownTimer = 0.5f;

		private Transform prevNode;

		private Transform nextNode;

		private Vector3 wallrunPos;

		private float wallrunHeight;

		private float maxHeight;

		private float minHeight;

		private float minDurationBeforeJump = 0.06f;

		private bool betweenLines;

		public float rotSpeed = 10f;

		public float wallRunUpDownDecc = 5f;

		public float wallRunUpDownMaxSpeed = 3f;

		public float wallRunUpDownSpeed;

		public float wallRunMoveSpeed = 13.3f;

		private float wallrunDecc = 3.7f;

		private float speed;

		private float lastSpeed;

		private int fallHash;

		private int jumpHash;

		private int wallRunRightHash;

		private int wallRunLeftHash;

		public WallrunLineAbility(Player player)
			: base(player)
		{
		}

		public override void Init()
		{
			normalMovement = false;
			normalRotation = false;
			customGravity = 0f;
			treatPlayerAsSortaGrounded = true;
			fallHash = Animator.StringToHash("fall");
			jumpHash = Animator.StringToHash("jump");
			wallRunRightHash = Animator.StringToHash("wallRunRight");
			wallRunLeftHash = Animator.StringToHash("wallRunLeft");
			customCameraMode = new CameraModeWallrun(this);
		}

		public override void OnStartAbility()
		{
			p.motor.HaveCollision(have: false);
			p.SetVelocity(Vector3.zero);
			scoreTimer = 0f;
			wallRunUpDownSpeed = 0f;
			if (Vector3.Dot(p.tf.right, wallrunFaceNormal) >= 0f)
			{
				animSide = Side.LEFT;
			}
			else
			{
				animSide = Side.RIGHT;
			}
			if (animSide == Side.RIGHT)
			{
				p.PlayAnim(wallRunRightHash);
			}
			else
			{
				p.PlayAnim(wallRunLeftHash);
			}
			if (p.moveStyle == MoveStyle.ON_FOOT)
			{
				trickName = "Wallrun";
			}
			else
			{
				trickName = "Wallride";
			}
			bool flag = p.UseColliderInCombo(wallrunLine.GetComponent<Collider>());
			p.DoTrick(Player.TrickType.WALLRUN, trickName + (flag ? " (New!)" : ""));
			betweenLines = false;
			SetNewNodes(wallrunLine.node0, wallrunLine.node1, wallrunFaceNormal, lineTransition: false);
			SetHeightLimits(wallrunLine);
			speed = Mathf.Max(p.GetForwardSpeed(), p.boosting ? p.boostSpeed : wallRunMoveSpeed);
			if (p.boosting)
			{
				if (!p.isAI)
				{
					Core.Instance.GameInput.SetVibrationOnCurrentController(0.19f, 0.19f, 0.1f);
				}
				p.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.GenericMovementSfx, AudioClipID.ultraBoostLoop, p.playerUltraBoostLoopAudioSource);
			}
			p.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.wallrunStart, p.playerOneShotAudioSource);
			p.AudioManager.PlaySfxGameplayLooping(p.moveStyle, AudioClipID.wallrunLoop, p.playerMovementLoopAudioSource);
			if (journey > 1f)
			{
				AtEndOfWallrunLine();
			}
			base.OnStartAbility();
		}

		public void SetNewNodes(Transform node0, Transform node1, Vector3 wallNormal, bool lineTransition)
		{
			wallrunFaceNormal = wallNormal;
			Vector3 vector = Vector3.Cross(wallrunFaceNormal, Vector3.up);
			Vector3 rhs = ((Vector3.Dot(vector, p.dir) > 0f) ? vector : (-vector));
			if (Vector3.Dot(node1.position - node0.position, rhs) > 0f)
			{
				prevNode = node0;
				nextNode = node1;
			}
			else
			{
				prevNode = node1;
				nextNode = node0;
			}
			if (!lineTransition)
			{
				wallrunPos = ProjectOnLine(p.tf.position);
				wallrunHeight = p.tf.position.y - wallrunPos.y;
			}
			dirToNextNode = (nextNode.position - wallrunPos).normalized;
			journey = GetRelativePosOnLine(wallrunPos);
			nodeToNodeLength = (nextNode.position - prevNode.position).magnitude;
		}

		public override void FixedUpdateAbility()
		{
			UpdateBoostpack();
			scoreTimer += Core.dt;
			if (scoreTimer > 0.7f)
			{
				scoreTimer = 0f;
				p.DoTrick(Player.TrickType.WALLRUN, trickName);
			}
			if (speed > wallRunMoveSpeed)
			{
				speed = Mathf.Max(wallRunMoveSpeed, speed - wallrunDecc * Core.dt);
			}
			if (p.boosting)
			{
				speed = p.boostSpeed;
			}
			journey += speed / nodeToNodeLength * Core.dt;
			wallrunPos = Vector3.LerpUnclamped(prevNode.position, nextNode.position, journey);
			dirToNextNode = (nextNode.position - prevNode.position).normalized;
			wallrunFaceNormal = wallrunLine.transform.forward;
			float num = (0f - p.motor.GetCapsule().height) * 1f;
			if (wallrunHeight > num)
			{
				if (wallRunUpDownSpeed > 0f)
				{
					wallRunUpDownSpeed = Mathf.Max(wallRunUpDownSpeed - wallRunUpDownDecc * Core.dt, 0f);
				}
				else if (wallRunUpDownSpeed < 0f)
				{
					wallRunUpDownSpeed = Mathf.Min(wallRunUpDownSpeed + wallRunUpDownDecc * Core.dt, 0f);
				}
			}
			else
			{
				wallRunUpDownSpeed = wallRunUpDownMaxSpeed;
			}
			wallrunHeight += wallRunUpDownSpeed * Core.dt;
			Vector3 vector = wallrunPos + wallrunHeight * Vector3.up + wallrunFaceNormal * p.motor.GetCapsule().radius;
			customVelocity = (vector - p.tf.position) / Core.dt;
			lastSpeed = customVelocity.magnitude;
			p.lastElevationForSlideBoost = vector.y;
			Vector3 normalized = Vector3.RotateTowards(p.dir, dirToNextNode, rotSpeed * Core.dt, 1000f).normalized;
			if (p.smoothRotation)
			{
				p.SetRotation(normalized);
			}
			else
			{
				p.SetRotHard(normalized);
			}
			if (p.jumpButtonNew && (p.abilityTimer > minDurationBeforeJump || p.isAI))
			{
				Jump();
			}
			else if (journey > 1f)
			{
				AtEndOfWallrunLine();
			}
			p.SetVisualRotLocal0();
		}

		private void AtEndOfWallrunLine()
		{
			WallrunLine wallrunLine;
			Transform node;
			Transform node2;
			if (betweenLines)
			{
				wallrunLine = this.wallrunLine;
				node = wallrunLine.node0;
				node2 = wallrunLine.node1;
				betweenLines = false;
			}
			else
			{
				wallrunLine = ((nextNode == this.wallrunLine.node0) ? this.wallrunLine.attachedWallrunLine0 : this.wallrunLine.attachedWallrunLine1);
				if (wallrunLine == null)
				{
					Vector3 normalized = (nextNode.position - prevNode.position).normalized;
					RunOff(normalized);
					return;
				}
				Vector3 position = p.tf.position;
				Vector3 position2 = wallrunLine.node0.position;
				Vector3 onNormal = wallrunLine.node1.position - position2;
				Vector3 vector = position2 + Vector3.Project(position - position2, onNormal);
				float num = ((!(vector == position2)) ? ((vector - position2).magnitude / onNormal.magnitude) : 0f);
				if (num > 0f && num < 1f)
				{
					node = wallrunLine.node0;
					node2 = wallrunLine.node1;
					betweenLines = false;
				}
				else
				{
					node = nextNode;
					node2 = wallrunLine.NearestNode(position);
					betweenLines = true;
				}
			}
			if (wallrunLine != this.wallrunLine)
			{
				p.UseColliderInCombo(wallrunLine.GetComponent<Collider>());
				p.DoTrick(Player.TrickType.WALLRUN, trickName);
			}
			SetHeightLimits(wallrunLine);
			Vector3 wallNormal = (wallrunLine.transform.forward + this.wallrunLine.transform.forward) / 2f;
			SetNewNodes(node, node2, wallNormal, lineTransition: true);
			this.wallrunLine = wallrunLine;
		}

		private void SetHeightLimits(WallrunLine newLine)
		{
			float num = newLine.GetComponent<BoxCollider>().size.y * newLine.transform.localScale.y;
			num *= 0.5f;
			maxHeight = num - 1.5f;
			minHeight = 0f - num - 1.5f;
			wallrunHeight = Mathf.Clamp(wallrunHeight, minHeight, maxHeight);
		}

		public void UpdateBoostpack()
		{
			if (p.boostButtonHeld && p.HasBoostCharge() && !p.boostAbility.locked)
			{
				p.characterVisual.SetBoostpackEffect(BoostpackEffectMode.BOOSTING);
				if (!p.boosting)
				{
					p.AudioManager.PlaySfxGameplay(SfxCollectionID.GenericMovementSfx, AudioClipID.boostStart, p.playerOneShotAudioSource);
					p.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.GenericMovementSfx, AudioClipID.ultraBoostLoop, p.playerUltraBoostLoopAudioSource);
				}
				p.boosting = true;
				p.UseBoostChargeForBoost();
				if (!p.isAI)
				{
					p.StartScreenShake(ScreenShakeType.ULTRALIGHT, 0.3f, continuous: true);
					Core.Instance.GameInput.SetVibrationOnCurrentController(0.19f, 0.19f, 0.1f);
				}
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
				p.SetBoostpackAndFrictionEffects(BoostpackEffectMode.ON);
			}
		}

		public void PassiveUpdate()
		{
			if (p.ability != p.wallrunAbility)
			{
				cooldownTimer -= Core.dt;
				if (p.IsGrounded())
				{
					cooldownTimer = 0f;
				}
			}
		}

		public bool CanStart()
		{
			return cooldownTimer <= 0f;
		}

		public void Jump()
		{
			p.AudioManager.PlayVoice(ref p.currentVoicePriority, p.character, AudioClipID.VoiceJump, p.playerGameplayVoicesAudioSource);
			p.AudioManager.PlaySfxGameplay(p.moveStyle, AudioClipID.jump, p.playerOneShotAudioSource);
			p.PlayAnim(jumpHash);
			Vector3 vector = Vector3.ProjectOnPlane(dirToNextNode, Vector3.up);
			float num = p.jumpSpeed + p.bonusJumpSpeedWallrun;
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
				p.maintainSpeedJump = false;
				p.StopCurrentAbility();
			}
			Vector3 velocity = vector * lastSpeed + num * Vector3.up + wallrunFaceNormal * 1f;
			p.jumpedThisFrame = true;
			p.timeSinceLastJump = 0f;
			p.isJumping = true;
			p.jumpConsumed = true;
			p.SetVelocity(velocity);
			p.SetRotation(velocity.normalized);
			p.lastElevationForSlideBoost = float.PositiveInfinity;
			p.FlattenRotationHard();
		}

		public void RunOff(Vector3 direction)
		{
			Vector3 velocity = direction * lastSpeed + wallrunFaceNormal * 1f;
			p.SetVelocity(velocity);
			p.SetRotHard(Quaternion.LookRotation(velocity.normalized));
			p.FlattenRotationHard();
			if (p.boosting)
			{
				p.ActivateAbility(p.boostAbility);
				p.boostAbility.StartFromRunOffGrindOrWallrun();
			}
			else
			{
				p.StopCurrentAbility();
				p.PlayAnim(fallHash);
			}
		}

		public Vector3 GetVelocity()
		{
			return p.motor.velocity + p.motor.platformVelocity;
		}

		public Vector3 ProjectOnLine(Vector3 pos)
		{
			return prevNode.position + Vector3.Project(pos - prevNode.position, nextNode.position - prevNode.position);
		}

		public float GetRelativePosOnLine(Vector3 pos)
		{
			Vector3 posOnLine;
			return GetRelativePosOnLine(pos, out posOnLine);
		}

		public float GetRelativePosOnLine(Vector3 pos, out Vector3 posOnLine)
		{
			posOnLine = ProjectOnLine(pos);
			Vector3 rhs = nextNode.position - prevNode.position;
			float num = ((!(posOnLine == prevNode.position)) ? ((posOnLine - prevNode.position).magnitude / rhs.magnitude) : 0f);
			if (Vector3.Dot(posOnLine - prevNode.position, rhs) < 0f)
			{
				num *= -1f;
			}
			return num;
		}

		public override void UpdateAbilityJumpStatus()
		{
			if (!p.jumpedThisFrame)
			{
				p.jumpConsumed = false;
			}
			p.timeSinceLastAbleToJump = 0f;
			p.timeSinceWallrunning = 0f;
		}

		public override void RecheckLoopingSoundInAbilityUse()
		{
			if (p.boosting)
			{
				p.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.GenericMovementSfx, AudioClipID.ultraBoostLoop, p.playerUltraBoostLoopAudioSource);
			}
			p.AudioManager.PlaySfxGameplayLooping(p.moveStyle, AudioClipID.wallrunLoop, p.playerMovementLoopAudioSource);
		}

		public override void OnStopAbility()
		{
			p.RegainAirMobility();
			p.boosting = false;
			cooldownTimer = 0.3f;
			p.motor.HaveCollision(have: true);
			p.SetDustEmission(0);
			p.SetDustSize(1f);
			p.ResetVisualRot();
			p.AudioManager.StopLoopingSfx(p.playerUltraBoostLoopAudioSource);
			p.AudioManager.StopLoopingSfx(p.playerMovementLoopAudioSource);
			base.OnStopAbility();
		}
	}
}
