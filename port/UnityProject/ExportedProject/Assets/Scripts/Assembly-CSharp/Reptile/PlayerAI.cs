using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class PlayerAI : MonoBehaviour
	{
		public enum PlayerAIState
		{
			WAIT_TO_START = 0,
			WAIT_FOR_PLAYER = 1,
			NORMAL_MOVE = 2,
			JUMPING = 3
		}

		private static int count;

		private bool startedWithMovestyle;

		private float teleportToNextWaypointCheatTimer;

		private float targetModifier = float.MaxValue;

		private float modifierTimer;

		private UserInputHandler.InputBuffer input;

		public PlayerAIPath path;

		public PlayerAIWaypoint comingFromWaypoint;

		public PlayerAIWaypoint nextWaypoint;

		public bool activateNPC;

		public NPC linkedNPC;

		private Player humanPlayer;

		private float age;

		internal Player self;

		private float distToWaypointSqrd;

		private Vector3 diffToWaypoint;

		internal Vector3 dirToWaypoint;

		private float dotToWaypoint;

		private bool onGround;

		private float waitTimer;

		private float waypointFailTimer;

		private Vector3 vel;

		internal Collider detectedObstacle;

		private bool preparingToJump;

		private bool isHookedUpToNPCDialogue;

		private NPC.Dialogue hookedUpToNPCDialogue;

		private Vector3 alignToNPCStartPos;

		private Quaternion alignToNPCStartRot;

		private float alignToNPCTimer;

		private float alignToNPCWaitTimer;

		internal bool startsTalkingToPlayer;

		private float letOtherAIGoFirstTimer;

		internal int AINumber = -1;

		private bool standby;

		private PlayerAIState state;

		public void Init()
		{
			AINumber = count;
			count++;
			humanPlayer = WorldHandler.instance.GetCurrentPlayer();
			self = GetComponent<Player>();
			self.obstacleDetection.SetActive(value: true);
		}

		public void SetHasMoveStyle(bool hasMoveStyle)
		{
			startedWithMovestyle = hasMoveStyle;
		}

		public void StartFirstPath()
		{
			nextWaypoint = path.firstWaypoint;
			state = PlayerAIState.NORMAL_MOVE;
		}

		public void SetPath(PlayerAIPath p, bool setWaypointAlso = true, bool teleport = false)
		{
			path = p;
			if (setWaypointAlso)
			{
				nextWaypoint = path.firstWaypoint;
				if (teleport)
				{
					self.motor.SetPositionTeleport(nextWaypoint.position);
				}
			}
		}

		public UserInputHandler.InputBuffer UpdatePlayerAI()
		{
			age += Core.dt;
			input = default;
			if (nextWaypoint == null || path == null)
			{
				if (activateNPC)
				{
					AlignWithNPC();
				}
				return input;
			}
			if (!standby && state != PlayerAIState.WAIT_TO_START && !path.ChunkIsActive())
			{
				PlayerAIPath playerAIPath = FindNextActiveAIPath(path);
				if (playerAIPath != null)
				{
					self.StopCurrentAbility();
					SetPath(playerAIPath, setWaypointAlso: true, teleport: true);
				}
				else
				{
					standby = true;
					self.ActivateAbility(self.standbyAbility);
				}
			}
			if (standby)
			{
				if (!path.ChunkIsActive())
				{
					return input;
				}
				standby = false;
				self.StopCurrentAbility();
				if (comingFromWaypoint != null && nextWaypoint != null)
				{
					TeleportToWaypoint(comingFromWaypoint, nextWaypoint);
				}
			}
			SetInfo();
			Rubberbanding();
			switch (state)
			{
			case PlayerAIState.WAIT_FOR_PLAYER:
				UpdateWaitForPlayer();
				break;
			case PlayerAIState.NORMAL_MOVE:
				UpdateNormalMove();
				CheckNextTarget();
				break;
			case PlayerAIState.JUMPING:
				UpdateJumping();
				CheckNextTarget();
				break;
			}
			detectedObstacle = null;
			return input;
		}

		public void HandplantJump()
		{
			if (comingFromWaypoint.IsJumper())
			{
				state = PlayerAIState.JUMPING;
			}
		}

		public void AlignWithNPC()
		{
			if (alignToNPCTimer == 0f)
			{
				alignToNPCStartPos = self.tf.position;
				alignToNPCStartRot = self.tf.rotation;
				self.stats.rotSpeedAtMaxSpeed = 0f;
				self.stats.groundAcc = (self.stats.airAcc = 0f);
				self.SetVelocity(Vector3.zero);
			}
			Transform transform = linkedNPC.transform;
			alignToNPCTimer += (((self.tf.position - transform.position).magnitude > 0.2f) ? Core.dt : (Core.dt * 0.5f));
			float num = alignToNPCTimer * self.stats.walkSpeed / (alignToNPCStartPos - transform.position).magnitude;
			Vector3 pos = Vector3.Lerp(alignToNPCStartPos, transform.position, num);
			Quaternion rot = Quaternion.Lerp(alignToNPCStartRot, transform.rotation, num);
			self.SetPosAndRotHard(pos, rot);
			if (num >= 1f)
			{
				alignToNPCWaitTimer += Core.dt;
				if (alignToNPCWaitTimer > 0.2f)
				{
					linkedNPC.SetDialogueLevel(0);
					activateNPC = false;
					Object.Destroy(self.gameObject);
					WorldHandler.instance.RemovePlayer(self);
				}
			}
			else
			{
				input.walkButtonHeld = true;
				input.moveAxisX = transform.forward.x;
				input.moveAxisY = transform.forward.z;
			}
		}

		public void UpdateNormalMove()
		{
			if (startsTalkingToPlayer)
			{
				return;
			}
			if ((bool)comingFromWaypoint)
			{
				if (!self.IsGrinding())
				{
					waypointFailTimer += Core.dt;
					if (waypointFailTimer > 18f)
					{
						waypointFailTimer = 0f;
						StartOnPath(path, teleport: true);
					}
				}
				if (comingFromWaypoint.wait > waitTimer)
				{
					waitTimer += Core.dt;
					return;
				}
				if (comingFromWaypoint.hasTriggerPlayerNeedsToPass)
				{
					return;
				}
				if (comingFromWaypoint.boost)
				{
					if (!input.boostButtonHeld)
					{
						input.boostButtonNew = true;
					}
					input.boostButtonHeld = true;
				}
				if (comingFromWaypoint.walk)
				{
					input.walkButtonHeld = true;
				}
				if (comingFromWaypoint.unequipMovestyle && startedWithMovestyle)
				{
					if (self.usingEquippedMovestyle)
					{
						input.switchStyleButtonNew = true;
					}
				}
				else if (!self.usingEquippedMovestyle && startedWithMovestyle)
				{
					input.switchStyleButtonNew = true;
				}
				if (comingFromWaypoint.slide)
				{
					if (!input.slideButtonHeld)
					{
						input.slideButtonNew = true;
					}
					input.slideButtonHeld = true;
				}
				if (comingFromWaypoint.trick && (!comingFromWaypoint.slide || !self.IsGrounded()))
				{
					input.trick1ButtonNew = true;
				}
				if (comingFromWaypoint.grindTiltCorner != 0f && self.IsGrinding())
				{
					input.moveAxisX = comingFromWaypoint.grindTiltCorner;
				}
				if (comingFromWaypoint.autoTeleport)
				{
					TeleportToWaypoint(comingFromWaypoint, nextWaypoint);
				}
			}
			for (int i = 0; i < WorldHandler.instance.SceneObjectsRegister.players.Count; i++)
			{
				Player player = WorldHandler.instance.SceneObjectsRegister.players[i];
				if (player.isAI && player.AI.AINumber > AINumber && (player.tf.position - self.tf.position).magnitude < 1.6f)
				{
					letOtherAIGoFirstTimer = 3f;
				}
			}
			if (!self.IsGrinding())
			{
				input.moveAxisX = dirToWaypoint.x;
				input.moveAxisY = dirToWaypoint.z;
			}
			if (letOtherAIGoFirstTimer > 0f)
			{
				self.runSpeedModifier = -3f;
				letOtherAIGoFirstTimer -= Core.dt;
			}
			JumpOverObstacles();
			if (!self.CanJump() && diffToWaypoint.y > 1f)
			{
				input.jumpButtonHeld = true;
				if (self.airDashAbility.haveAirDash && vel.y < 0f)
				{
					input.jumpButtonNew = true;
				}
			}
		}

		public void UpdateJumping()
		{
			Vector3 vector = comingFromWaypoint.jumpPosition - self.tf.position;
			Vector3 vector2 = Vector3.ProjectOnPlane(vector, Vector3.up);
			Vector3 normalized = vector2.normalized;
			float num = Vector3.Dot(self.dir, normalized);
			bool flag = self.CanJump();
			bool flag2 = num < -0.3f || vector.magnitude < 0.5f * comingFromWaypoint.jumpPointScale;
			if (preparingToJump)
			{
				input.moveAxisX = normalized.x;
				input.moveAxisY = normalized.z;
				if (self.CanJump() && (num > 0.8f || self.IsGrinding() || self.ability == self.wallrunAbility))
				{
					preparingToJump = false;
					input.jumpButtonHeld = true;
					input.jumpButtonNew = true;
				}
				return;
			}
			if (vector.y > 0f || vector2.magnitude > 7.5f)
			{
				input.jumpButtonHeld = true;
			}
			if (comingFromWaypoint.jumpPointBehavoir == PlayerAIWaypoint.JumpPointBehavoir.TRICK_AT_JUMP_POINT)
			{
				if (flag2)
				{
					input.trick1ButtonNew = true;
				}
			}
			else if (!self.CanJump() && self.preVelocity.y > self.jumpSpeed / 4f && vel.y < self.jumpSpeed / 4f)
			{
				input.trick1ButtonNew = true;
			}
			if (flag2 | flag)
			{
				if (flag2 && !flag && comingFromWaypoint.jumpPointBehavoir == PlayerAIWaypoint.JumpPointBehavoir.AIRDASH_AT_JUMP_POINT && self.airDashAbility.haveAirDash)
				{
					input.moveAxisX = dirToWaypoint.normalized.x;
					input.moveAxisY = dirToWaypoint.normalized.z;
					input.jumpButtonNew = true;
				}
				state = PlayerAIState.NORMAL_MOVE;
			}
			else
			{
				input.moveAxisX = vector.normalized.x;
				input.moveAxisY = vector.normalized.z;
				if (comingFromWaypoint.jumpPointBehavoir == PlayerAIWaypoint.JumpPointBehavoir.REACH_WITH_AIRDASH_IF_FAR && vel.y < 0f && vector2.magnitude > 6f && self.airDashAbility.haveAirDash)
				{
					input.jumpButtonNew = true;
				}
			}
		}

		public void Rubberbanding()
		{
			if (state == PlayerAIState.JUMPING || !self.IsGrounded())
			{
				self.runSpeedModifier = 0f;
				return;
			}
			if (self.playerType == PlayerType.TEMP_RACER_AI)
			{
				self.runSpeedModifier = 0f;
				return;
			}
			PlayerAIWaypoint closestWaypoint = GetClosestWaypoint(path, humanPlayer.tf.position);
			PlayerAIWaypoint playerAIWaypoint = comingFromWaypoint;
			if (playerAIWaypoint == null || distToWaypointSqrd < (comingFromWaypoint.position - self.tf.position).sqrMagnitude)
			{
				playerAIWaypoint = nextWaypoint;
			}
			float magnitude = (self.tf.position - humanPlayer.tf.position).magnitude;
			modifierTimer += Core.dt;
			int siblingIndex = closestWaypoint.transform.GetSiblingIndex();
			if (playerAIWaypoint.transform.GetSiblingIndex() > siblingIndex && magnitude > 10f)
			{
				if (targetModifier != -2.2f)
				{
					modifierTimer = 0f;
				}
				targetModifier = -2.2f;
			}
			else if (playerAIWaypoint.transform.GetSiblingIndex() < siblingIndex && magnitude > 6f)
			{
				if (magnitude > 12f)
				{
					teleportToNextWaypointCheatTimer += Core.dt;
					if (teleportToNextWaypointCheatTimer > 0.5f)
					{
						int num = playerAIWaypoint.transform.GetSiblingIndex() + 1;
						if (path.transform.childCount > num && num < siblingIndex)
						{
							PlayerAIWaypoint component = path.transform.GetChild(num).GetComponent<PlayerAIWaypoint>();
							if (!component.dontCheatTeleportTo && !WorldHandler.instance.IsReallyInScreen(component.position + Vector3.up) && !WorldHandler.instance.IsReallyInScreen(self.tf.position + Vector3.up))
							{
								TeleportToWaypoint(playerAIWaypoint, component);
							}
						}
						teleportToNextWaypointCheatTimer = 0f;
					}
				}
				if (targetModifier != 3.9f)
				{
					modifierTimer = 0f;
				}
				targetModifier = 3.9f;
			}
			else
			{
				if (targetModifier != 0f)
				{
					modifierTimer = 0f;
				}
				targetModifier = 0f;
			}
			self.runSpeedModifier = Mathf.Lerp(self.runSpeedModifier, targetModifier, modifierTimer * 0.1f);
		}

		private void TeleportToWaypoint(PlayerAIWaypoint from, PlayerAIWaypoint to)
		{
			Vector3 normalized = (to.position - from.transform.position).normalized;
			normalized.y = 0f;
			self.StopCurrentAbility();
			SetNextWaypoint(to);
			self.SetPosAndRotHard(to.position, Quaternion.LookRotation(normalized));
		}

		private void CheckNextTarget()
		{
			if (!(distToWaypointSqrd < Mathf.Pow(nextWaypoint.transform.localScale.x, 2f)) && (!nextWaypoint.skipIfCantReach || !(nextWaypoint.position.y > self.tf.position.y + 7f)) && (self.ability != self.wallrunAbility || !((FlatPos(nextWaypoint.position) - FlatPos(self.tf.position)).magnitude < nextWaypoint.transform.localScale.x * 2f)))
			{
				return;
			}
			if (nextWaypoint == path.lastWaypoint)
			{
				if (path.nextPath != null)
				{
					if (!path.nextPath.ChunkIsActive())
					{
						state = PlayerAIState.WAIT_FOR_PLAYER;
						return;
					}
					SetPath(path.nextPath);
					state = PlayerAIState.NORMAL_MOVE;
				}
				else
				{
					SetNextWaypoint(null);
				}
			}
			else
			{
				SetNextWaypoint(path.GetNextWaypoint(nextWaypoint));
			}
		}

		public void SkipToNextTarget()
		{
			SetNextWaypoint(path.GetNextWaypoint(nextWaypoint));
		}

		private Vector3 FlatPos(Vector3 pos)
		{
			return new Vector3(pos.x, 0f, pos.z);
		}

		public PlayerAIPath FindNextActiveAIPath(PlayerAIPath nextPath)
		{
			if (path.Equals(nextPath))
			{
				return null;
			}
			if (path.nextPath == null)
			{
				return null;
			}
			if (path.nextPath.ChunkIsActive())
			{
				return path.nextPath;
			}
			return FindNextActiveAIPath(path.nextPath);
		}

		public void UpdateWaitForPlayer()
		{
			if (path.nextPath != null && path.nextPath.ChunkIsActive())
			{
				state = PlayerAIState.NORMAL_MOVE;
			}
		}

		public void SetNextWaypointClean(PlayerAIWaypoint waypoint)
		{
			waypointFailTimer = 0f;
			waitTimer = 0f;
			comingFromWaypoint = nextWaypoint;
			nextWaypoint = waypoint;
			if (!(nextWaypoint == null))
			{
				preparingToJump = false;
				state = PlayerAIState.NORMAL_MOVE;
			}
		}

		private void SetNextWaypoint(PlayerAIWaypoint waypoint)
		{
			waypointFailTimer = 0f;
			waitTimer = 0f;
			comingFromWaypoint = nextWaypoint;
			nextWaypoint = waypoint;
			if (!(nextWaypoint == null))
			{
				if (comingFromWaypoint.IsJumper())
				{
					preparingToJump = true;
					state = PlayerAIState.JUMPING;
				}
				else
				{
					state = PlayerAIState.NORMAL_MOVE;
				}
			}
		}

		private void JumpOverObstacles()
		{
			if (!nextWaypoint.ignoreObstructions && (bool)detectedObstacle && dotToWaypoint > 0.35f && self.pseudoGraffitiAbility.cooldown <= 0f)
			{
				if (onGround && self.motor.groundCollider.gameObject.layer != 23)
				{
					input.jumpButtonNew = true;
					input.jumpButtonHeld = true;
				}
				else
				{
					input.jumpButtonHeld = true;
				}
			}
		}

		private void SetInfo()
		{
			diffToWaypoint = nextWaypoint.position - self.tf.position;
			dirToWaypoint = diffToWaypoint.normalized;
			distToWaypointSqrd = diffToWaypoint.sqrMagnitude;
			vel = self.GetPracticalVelocity();
			dotToWaypoint = Vector3.Dot(vel.normalized, dirToWaypoint);
			onGround = self.IsGrounded();
		}

		private bool AbilityIs(Ability a)
		{
			return self.ability == a;
		}

		public void StartClosestTargetOnClosestPath()
		{
			PlayerAIPath playerAIPath = null;
			float num = float.MaxValue;
			foreach (PlayerAIPath item in new List<PlayerAIPath>(Object.FindObjectsOfType<PlayerAIPath>(includeInactive: true)))
			{
				Vector3 position = item.firstWaypoint.position;
				if ((self.tf.position - position).sqrMagnitude < num)
				{
					playerAIPath = item;
					num = (self.tf.position - position).sqrMagnitude;
				}
			}
			SetPath(playerAIPath, setWaypointAlso: false);
			nextWaypoint = GetClosestWaypoint(path, self.tf.position);
			Vector3 normalized = Vector3.ProjectOnPlane(playerAIPath.waypoints[0].position - self.tf.position, Vector3.up).normalized;
			self.SetRotation(normalized);
			state = PlayerAIState.NORMAL_MOVE;
		}

		public void StartClosestTargetOnAnyPath()
		{
			List<PlayerAIPath> list = new List<PlayerAIPath>(Object.FindObjectsOfType<PlayerAIPath>(includeInactive: true));
			PlayerAIPath playerAIPath = null;
			float num = float.MaxValue;
			foreach (PlayerAIPath item in list)
			{
				for (int i = 0; i < item.waypoints.Count; i++)
				{
					if ((self.tf.position - item.waypoints[i].position).sqrMagnitude < num)
					{
						playerAIPath = item;
						nextWaypoint = item.waypoints[i];
						num = (self.tf.position - item.waypoints[i].position).sqrMagnitude;
					}
				}
			}
			SetPath(playerAIPath, setWaypointAlso: false);
			Vector3 normalized = Vector3.ProjectOnPlane(playerAIPath.waypoints[0].position - self.tf.position, Vector3.up).normalized;
			self.SetRotation(normalized);
			state = PlayerAIState.NORMAL_MOVE;
		}

		public void StartOnPath(PlayerAIPath path, bool teleport = false)
		{
			SetPath(path, setWaypointAlso: true, teleport);
			Vector3 normalized = Vector3.ProjectOnPlane(path.waypoints[0].position - self.tf.position, Vector3.up).normalized;
			self.SetRotation(normalized);
			state = PlayerAIState.NORMAL_MOVE;
		}

		public PlayerAIWaypoint GetClosestWaypoint(PlayerAIPath onPath, Vector3 pos)
		{
			PlayerAIWaypoint result = null;
			float num = float.MaxValue;
			for (int i = 0; i < onPath.waypoints.Count; i++)
			{
				if ((pos - onPath.waypoints[i].position).sqrMagnitude < num)
				{
					result = onPath.waypoints[i];
					num = (pos - onPath.waypoints[i].position).sqrMagnitude;
				}
			}
			return result;
		}

		public void SetHookedUpToNPCDialogue(NPC.Dialogue dialogue)
		{
			hookedUpToNPCDialogue = dialogue;
			isHookedUpToNPCDialogue = true;
			activateNPC = true;
			self.transform.GetChild(0).Find("NPCTrigger").gameObject.SetActive(value: true);
		}

		private void OnDrawGizmos()
		{
			if ((bool)nextWaypoint)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawSphere(nextWaypoint.transform.position, nextWaypoint.transform.localScale.x + 0.1f);
			}
		}
	}
}
