using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class EnemyHitResponse : MonoBehaviour
	{
		public enum HitType
		{
			PLAYER_GROUNDTRICK = 0,
			PLAYER_BOOST = 1,
			PLAYER_SPECIALAIR = 2,
			PLAYER_AIRTRICK = 3,
			PLAYER_SLIDE = 4,
			EXPLOSION = 5,
			SPRAY = 6,
			NONE = 7
		}

		public enum HitResponseState
		{
			NONE = 0,
			KNOCKBACK = 1,
			KNOCKDOWN = 2,
			LYINGDOWN = 3,
			GETUP = 4
		}

		[SerializeField]
		private bool stayDownOnDeath = true;

		[SerializeField]
		private bool canBeSprayed = true;

		[SerializeField]
		private float airTrickHitDuration = 1.5f;

		[SerializeField]
		private float airTrickHitHeight = 3f;

		[SerializeField]
		private int airTrickHitDamage = 1;

		[SerializeField]
		private float specialAirHitDuration = 2f;

		[SerializeField]
		private float specialAirHitHeight = 6f;

		[SerializeField]
		private int specialAirHitDamage = 1;

		[SerializeField]
		private float sprayedHitDuration = 3f;

		[SerializeField]
		private float deathHitDuration = 0.5f;

		[SerializeField]
		private float deathHitHeight = 1f;

		[SerializeField]
		private float deathHitDist = 5f;

		[SerializeField]
		private float getUpDuration = 1.5f;

		[SerializeField]
		private AudioSource audioSource;

		protected const SfxCollectionID collectionID = SfxCollectionID.CombatSfx;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		protected AudioClipID getHitClipID = AudioClipID.NONE;

		[SerializeField]
		[AudioClipFilter("collectionID")]
		protected AudioClipID bodyDropClipID = AudioClipID.NONE;

		private BasicEnemy enemy;

		private Player player;

		private EnemyPositioningInfo positioningInfo;

		private EnemyNavmeshMovement navmeshMovement;

		private EnemyAnimations animations;

		private bool activated;

		private bool hitCooldownFinished = true;

		private bool hitToRespondTo;

		private bool hitIsStale;

		private bool hasBeenSprayed;

		private Collider lastHitCollider;

		private HitType lastHitType = HitType.NONE;

		private Vector3 incomingHitPosition = Vector3.zero;

		private Vector3 incomingHitDirection = Vector3.zero;

		private float incomingHitDist;

		private float incomingHitDuration;

		private float incomingHitHeight;

		private HitType incomingHitType = HitType.NONE;

		private HitDir incomingHitDir = HitDir.forward;

		private int incomingHitDamage;

		private Vector3 currentHitPosition = Vector3.zero;

		private Vector3 currentHitDirection = Vector3.zero;

		private float currentHitDist;

		private float currentHitDuration;

		private float currentHitHeight;

		private HitType currentHitType = HitType.NONE;

		private HitDir currentHitDir = HitDir.forward;

		private int currentHitDamage;

		private List<HitType> hitHistory = new List<HitType>();

		private int getHitAHash = Animator.StringToHash("getHitA");

		private int getHitBHash = Animator.StringToHash("getHitB");

		private int getHitSprayHash = Animator.StringToHash("getHitSpray");

		private int getHitFlyBackHash = Animator.StringToHash("getHitFlyBack");

		private int getHitJuggleHash = Animator.StringToHash("getHitJuggle");

		private int getHitLyingDownHash = Animator.StringToHash("getHitLyingDown");

		private int landHash = Animator.StringToHash("land");

		private int lieDownHash = Animator.StringToHash("lieDown");

		private int fallAngleHash = Animator.StringToHash("fallAngle");

		private int getUpHash = Animator.StringToHash("getUp");

		private HitResponseState responseState;

		private float stateTime;

		private bool lyingDown;

		private bool singleStayDown;

		private int lastAirTrick = -1;

		private int lastGroundTrick = -1;

		public HitResponseState State => responseState;

		public HitType IncomingHitType => incomingHitType;

		public bool HitToRespondTo => hitToRespondTo;

		public void Init(BasicEnemy targetEnemy, Player currentPlayer, EnemyPositioningInfo enemyPositioningInfo, EnemyNavmeshMovement movement, EnemyAnimations animations)
		{
			enemy = targetEnemy;
			player = currentPlayer;
			positioningInfo = enemyPositioningInfo;
			navmeshMovement = movement;
			this.animations = animations;
			activated = true;
			enemy.OnUpdateActive.AddListener(UpdateHitResponse);
		}

		public void Activate()
		{
			activated = true;
			hitCooldownFinished = true;
			lastHitCollider = null;
			hitToRespondTo = false;
			hitIsStale = false;
			lyingDown = false;
			hasBeenSprayed = false;
		}

		public void Deactivate()
		{
			activated = false;
		}

		public bool RespondToHit()
		{
			bool result = false;
			if ((responseState == HitResponseState.NONE || hitCooldownFinished) && hitToRespondTo)
			{
				enemy.ReceiveDamage(incomingHitDamage);
				if (lastHitType != HitType.EXPLOSION)
				{
					if (lastHitType == HitType.PLAYER_GROUNDTRICK || lastHitType == HitType.PLAYER_AIRTRICK)
					{
						player.DoHitEffectPosition(incomingHitPosition);
					}
					else if (IncomingHitType != HitType.SPRAY)
					{
						player.DoHitEffect(base.transform);
					}
				}
				currentHitDamage = incomingHitDamage;
				currentHitDir = incomingHitDir;
				currentHitDirection = incomingHitDirection;
				currentHitDist = incomingHitDist;
				currentHitDuration = incomingHitDuration;
				currentHitHeight = incomingHitHeight;
				currentHitPosition = incomingHitPosition;
				currentHitType = incomingHitType;
				if (currentHitType == HitType.PLAYER_AIRTRICK)
				{
					player.airTrickAbility.HitEnemy();
				}
				if (((!navmeshMovement.AirBorne() && (currentHitType == HitType.PLAYER_GROUNDTRICK || currentHitType == HitType.SPRAY)) || (currentHitType == HitType.PLAYER_SLIDE && lyingDown) || (currentHitType == HitType.PLAYER_AIRTRICK && !navmeshMovement.AirBorne())) && (enemy.CurHP > 0 || currentHitType == HitType.SPRAY))
				{
					SetState(HitResponseState.KNOCKBACK);
					if (currentHitType == HitType.SPRAY)
					{
						hasBeenSprayed = true;
					}
				}
				else if (currentHitType != HitType.SPRAY)
				{
					SetState(HitResponseState.KNOCKDOWN);
				}
				result = true;
			}
			hitToRespondTo = false;
			hitIsStale = false;
			return result;
		}

		public void CancelHit()
		{
			hitToRespondTo = false;
			hitIsStale = false;
		}

		public void SetStayDownOnDeathThisTime(bool value)
		{
			singleStayDown = value;
		}

		private void SetState(HitResponseState targetState)
		{
			ExitState(responseState);
			responseState = targetState;
			stateTime = 0f;
			switch (responseState)
			{
			case HitResponseState.NONE:
				hitCooldownFinished = true;
				lyingDown = false;
				break;
			case HitResponseState.KNOCKBACK:
				hitCooldownFinished = false;
				enemy.DoHitPause(0.06f);
				if (lyingDown)
				{
					animations.PlayAnim(getHitLyingDownHash, restart: true);
					currentHitHeight = 0f;
				}
				else if (currentHitType == HitType.SPRAY)
				{
					animations.PlayAnim(getHitSprayHash, restart: true);
				}
				else
				{
					switch (incomingHitDir)
					{
					case HitDir.right:
					case HitDir.up:
					case HitDir.down:
					case HitDir.forward:
						animations.PlayAnim(getHitBHash, restart: true);
						break;
					case HitDir.left:
						animations.PlayAnim(getHitAHash, restart: true);
						break;
					}
					if (audioSource != null && getHitClipID != AudioClipID.NONE)
					{
						Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, getHitClipID, audioSource, 0.05f);
					}
				}
				navmeshMovement.DoKnockBack(currentHitDirection, currentHitDist, currentHitDuration);
				break;
			case HitResponseState.KNOCKDOWN:
				lyingDown = false;
				hitCooldownFinished = false;
				enemy.DoHitPause(0.06f);
				if (navmeshMovement.AirBorne())
				{
					animations.PlayAnim(getHitJuggleHash, restart: true);
				}
				else
				{
					animations.PlayAnim(getHitFlyBackHash, restart: true);
				}
				if (audioSource != null && getHitClipID != AudioClipID.NONE)
				{
					Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, getHitClipID, audioSource, 0.05f);
				}
				navmeshMovement.DoKnockBack(currentHitDirection, (enemy.CurHP > 0) ? currentHitDist : Mathf.Max(currentHitDist, deathHitDist), (enemy.CurHP > 0) ? currentHitHeight : Mathf.Max(currentHitHeight, deathHitHeight), (enemy.CurHP > 0) ? currentHitDuration : Mathf.Max(currentHitDuration, deathHitDuration), lookAtMoveDir: true);
				currentHitDuration = ((enemy.CurHP > 0) ? currentHitDuration : Mathf.Max(currentHitDuration, deathHitDuration + 0.25f));
				break;
			case HitResponseState.LYINGDOWN:
				hitCooldownFinished = true;
				navmeshMovement.DoKnockBack(-base.transform.forward, 1.5f, 0.5f);
				navmeshMovement.StopObstacleAvoidance();
				if (!lyingDown)
				{
					animations.PlayAnim(landHash);
				}
				else
				{
					animations.PlayAnim(lieDownHash);
				}
				if (audioSource != null && bodyDropClipID != AudioClipID.NONE)
				{
					Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, bodyDropClipID, audioSource, 0.05f);
				}
				break;
			case HitResponseState.GETUP:
				navmeshMovement.StartObstacleAvoidance();
				hitCooldownFinished = true;
				animations.PlayAnim(getUpHash);
				break;
			}
		}

		public void UpdateHitResponse()
		{
			if (hitIsStale)
			{
				CancelHit();
			}
			if (hitToRespondTo && !hitIsStale)
			{
				hitIsStale = true;
			}
			switch (responseState)
			{
			case HitResponseState.KNOCKBACK:
				if (stateTime > 0.35f)
				{
					hitCooldownFinished = true;
				}
				if (stateTime > currentHitDuration)
				{
					if (lyingDown && stateTime > currentHitDuration + 0.5f)
					{
						SetState(HitResponseState.GETUP);
						return;
					}
					if (!lyingDown)
					{
						SetState(HitResponseState.NONE);
					}
				}
				break;
			case HitResponseState.KNOCKDOWN:
				FallAnimation();
				if (stateTime > 1f)
				{
					hitCooldownFinished = true;
				}
				if (stateTime > currentHitDuration || !navmeshMovement.RespondingToHit())
				{
					SetState(HitResponseState.LYINGDOWN);
				}
				break;
			case HitResponseState.LYINGDOWN:
				if (stateTime > 0.2f)
				{
					lyingDown = true;
				}
				if (stateTime > 0.5f && ((!stayDownOnDeath && !singleStayDown) || enemy.CurHP > 0))
				{
					SetState(HitResponseState.GETUP);
				}
				break;
			case HitResponseState.GETUP:
				if (stateTime > 0.3f)
				{
					lyingDown = false;
				}
				if (stateTime > getUpDuration)
				{
					SetState(HitResponseState.NONE);
				}
				break;
			}
			stateTime += Core.dt;
		}

		private void ExitState(HitResponseState exitingState)
		{
			if ((uint)(exitingState - 1) <= 1u)
			{
				navmeshMovement.ResumeKnockBack();
				hitCooldownFinished = true;
			}
		}

		private void FallAnimation()
		{
			animations.SetFloatParameter(fallAngleHash, navmeshMovement.GetFallAngle());
		}

		public void ManualDamage(HitType type, Vector3 dir, float dist, float dur, float height, int damage)
		{
			incomingHitDirection = dir;
			incomingHitDist = dist;
			incomingHitHeight = height;
			incomingHitDuration = dur;
			incomingHitDamage = damage;
			incomingHitType = type;
			hitToRespondTo = true;
			hitIsStale = false;
		}

		public void OnTriggerStay(Collider other)
		{
			if (!activated || lyingDown)
			{
				return;
			}
			if (other.gameObject.layer == 18)
			{
				if (other.gameObject.GetComponentInParent<Player>() != player)
				{
					return;
				}
				lastHitCollider = other;
				float f = Vector3.Dot(Vector3.Cross(player.tf.forward, -positioningInfo.DirToPlayer.normalized), Vector3.up);
				Vector3 vector = enemy.tf.position - player.tf.position;
				vector.y = 0f;
				Vector3 practicalVelocity = player.GetPracticalVelocity();
				practicalVelocity.y = 0f;
				incomingHitDirection = Vector3.Lerp(practicalVelocity.normalized, vector.normalized, Mathf.Abs(f) * 0.25f);
				incomingHitDist = 0f;
				incomingHitPosition = other.transform.position - other.transform.right * 0.5f;
				incomingHitType = HitType.NONE;
				incomingHitDamage = 1;
				if (player.ability == player.groundTrickAbility || player.ability == player.grindAbility || ((player.ability == player.airTrickAbility || player.ability == player.airDashAbility) && !navmeshMovement.AirBorne()))
				{
					if (player.ability == player.groundTrickAbility)
					{
						incomingHitDir = player.groundTrickAbility.CurTrickDir;
						player.groundTrickAbility.HitEnemy();
					}
					incomingHitDuration = ((navmeshMovement.CurrentMovementState == EnemyNavmeshMovement.MovementState.FLY_BACK || navmeshMovement.CurrentMovementState == EnemyNavmeshMovement.MovementState.DROP_DOWN) ? 1f : 0.6f);
					incomingHitDist = player.GetPracticalVelocity().magnitude * incomingHitDuration;
					incomingHitHeight = 0f;
					incomingHitType = HitType.PLAYER_GROUNDTRICK;
					incomingHitDamage = 1;
				}
				if ((player.ability == player.airTrickAbility || player.ability == player.airDashAbility) && navmeshMovement.AirBorne())
				{
					incomingHitDuration = airTrickHitDuration;
					incomingHitHeight = positioningInfo.DiffToPlayer.y + airTrickHitHeight;
					incomingHitDist = player.maxMoveSpeed * 1f * incomingHitDuration - Mathf.Max(Vector3.Dot(-positioningInfo.DiffToPlayer, player.forward) - 1f, 0f);
					incomingHitType = HitType.PLAYER_AIRTRICK;
					incomingHitDamage = airTrickHitDamage;
				}
				else if (player.ability == player.specialAirAbility)
				{
					incomingHitType = HitType.PLAYER_SPECIALAIR;
					player.specialAirAbility.HitEnemy();
					incomingHitDuration = specialAirHitDuration;
					incomingHitHeight = specialAirHitHeight;
					incomingHitDamage = specialAirHitDamage;
					incomingHitDist = Vector3.ProjectOnPlane(player.GetPracticalVelocity(), Vector3.up).magnitude * incomingHitDuration + Mathf.Clamp(1f - vector.magnitude, 0f, float.MaxValue);
				}
				else if (player.boosting)
				{
					incomingHitType = HitType.PLAYER_BOOST;
					incomingHitHeight = 3.5f;
					incomingHitDuration = 0.9f;
					incomingHitDist = player.GetPracticalVelocity().magnitude * 1f * incomingHitDuration + Mathf.Clamp(1f - vector.magnitude, 0f, float.MaxValue);
					incomingHitDamage = 3;
				}
				else if (player.ability == player.slideAbility && !navmeshMovement.AirBorne())
				{
					incomingHitType = HitType.PLAYER_SLIDE;
					incomingHitDamage = 1;
					incomingHitHeight = ((responseState != HitResponseState.KNOCKDOWN || lyingDown) ? 0.5f : 0f);
					incomingHitDuration = 0.4f;
					incomingHitDist = player.GetPracticalVelocity().magnitude * 1.3f * incomingHitDuration + Mathf.Clamp(1f - vector.magnitude, 0f, float.MaxValue);
					incomingHitDamage = 1;
				}
				float num = Mathf.Abs(Mathf.Clamp(vector.magnitude - 2f, -100f, 0f));
				incomingHitDist += num;
				if (currentHitType != incomingHitType || (currentHitType == HitType.PLAYER_AIRTRICK && player.airTrickAbility.curTrick != lastAirTrick) || hitCooldownFinished)
				{
					hitToRespondTo = true;
					hitIsStale = false;
					if (currentHitType == HitType.PLAYER_AIRTRICK)
					{
						lastAirTrick = player.airTrickAbility.curTrick;
					}
					if (currentHitType == HitType.PLAYER_GROUNDTRICK)
					{
						lastGroundTrick = player.groundTrickAbility.curTrick;
					}
					hitHistory.Add(incomingHitType);
				}
			}
			else if (other.gameObject.layer == 20)
			{
				BasicEnemy component = other.transform.parent.GetComponent<BasicEnemy>();
				if (component != null && component.EnemyType == EnemyType.TANKWALKER)
				{
					lastHitCollider = other;
					Vector3 vector2 = enemy.tf.position - other.transform.position;
					vector2.y = 0f;
					incomingHitDirection = vector2;
					incomingHitDist = 5f;
					incomingHitHeight = 3f;
					incomingHitDuration = 2f;
					incomingHitDamage = 1;
					incomingHitType = HitType.EXPLOSION;
					hitToRespondTo = true;
					hitIsStale = false;
				}
			}
			else if (other.gameObject.layer == 30)
			{
				if (canBeSprayed && !hasBeenSprayed && player.sprayHitbox == other.gameObject && (currentHitType != HitType.SPRAY || hitCooldownFinished))
				{
					lastHitCollider = other;
					Vector3 vector3 = enemy.tf.position - other.transform.position;
					vector3.y = 0f;
					incomingHitDirection = vector3;
					incomingHitDist = 0.001f;
					incomingHitHeight = 0f;
					incomingHitDuration = sprayedHitDuration;
					incomingHitDamage = 0;
					incomingHitType = HitType.SPRAY;
					hitToRespondTo = true;
					hitIsStale = false;
				}
				else if (player.autoAimCheck == other.gameObject)
				{
					enemy.CheckAutoAim();
				}
			}
		}
	}
}
