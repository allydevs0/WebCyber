using System;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Reptile
{
	public class EnemyNavmeshMovement : MonoBehaviour
	{
		public enum MovementState
		{
			STATIONARY = 0,
			STATIC_LOCATION = 1,
			TRACKING_PLAYER = 2,
			CIRCLE_PLAYER = 3,
			BACK_OFF_PLAYER = 4,
			KNOCK_BACK = 5,
			FLY_BACK = 6,
			DROP_DOWN = 7
		}

		public enum RotationMode
		{
			ROTATE_VELOCITY = 0,
			ROTATE_STATIC_LOCATION = 1,
			ROTATE_VELOCITY_INVERSE = 2,
			ROTATE_TO_PLAYER = 3,
			STATIC = 4
		}

		[SerializeField]
		private NavMeshAgent navMeshAgent;

		[SerializeField]
		private Rigidbody rigidBody;

		[SerializeField]
		private bool ignoreTargetHeight;

		[SerializeField]
		private LayerMask layerMask;

		[SerializeField]
		private float baseOffset;

		[SerializeField]
		private float vaultHandHeight = 1f;

		[SerializeField]
		private float vaultStartDelay;

		[SerializeField]
		private float vaultLandDelay;

		[SerializeField]
		private float escalatorSpeed = 5f;

		private Transform tf;

		private EnemyPositioningInfo positioningInfo;

		private Player player;

		private Transform playerTf;

		private bool activated;

		private MovementState movementState;

		private RotationMode rotationMode;

		private bool rotateInstant;

		private bool rotationOppositeDirection;

		private float movementSpeed;

		private bool canUseOffMeshLinks;

		private Vector3 targetLocation;

		private float targetDistance;

		private float knockbackTimer;

		private float knockbackDuration;

		private float knockbackStartHeight;

		private float knockbackHeight;

		private Vector3 knockbackDirection;

		private Vector3 lastNavMeshPosition;

		private Vector3 dropDownDestination;

		private Vector3 dropDownDir;

		private Vector3 rotationTargetPosition;

		private Vector3 cachedForward;

		private float rotationSpeed = 10f;

		private float minChangeDirInterval;

		private float maxchangeDirInterval;

		private float changeDirTimer;

		private float circlingRadius;

		private float circlingRange;

		private float minRepositionAngle;

		private float maxRepositionAngle;

		private Vector3 circlingOffset;

		private float backOffSpeedPercentage;

		private Vector3 backOffPosition;

		private float targetBaseOffset;

		private float descendAscendTargetSpeed;

		private float descendAscendCurrentSpeed;

		private int defaultAreaMask;

		private bool paused;

		public bool pauseKnockBack;

		private MovementState lastMovementState;

		private float vertSpeed;

		private Vector3 lastPosition = Vector3.zero;

		private Vector3 lastMovement = Vector3.zero;

		private float vaultSpeed = 1.5f;

		private float dropSpeed = 2.2f;

		private bool moveAcrossNavMeshesStarted;

		public bool Landed;

		private float ratio;

		private float drop;

		private float moveAcrossVaultProgress;

		private bool moveAcrossEscalator;

		public MovementState CurrentMovementState => movementState;

		public bool Activated => activated;

		public float CurrentBaseOffset => navMeshAgent.baseOffset;

		public NavMeshAgent Agent => navMeshAgent;

		public float MovementSpeed => movementSpeed;

		public float MoveAcrossVaultProgress => moveAcrossVaultProgress;

		public void Init(BasicEnemy enemy, EnemyPositioningInfo enemyPositioningInfo, Player playerToTarget)
		{
			tf = base.transform;
			positioningInfo = enemyPositioningInfo;
			player = playerToTarget;
			playerTf = player.tf;
			enemy.OnUpdateActive.AddListener(UpdateMovement);
			enemy.OnPause.AddListener(Pause);
			enemy.OnUnPause.AddListener(UnPause);
			defaultAreaMask = navMeshAgent.areaMask;
			Deactivate();
		}

		public void Activate()
		{
			activated = true;
			navMeshAgent.enabled = true;
			navMeshAgent.avoidancePriority += UnityEngine.Random.Range(-5, 5);
			navMeshAgent.baseOffset = baseOffset;
			movementState = MovementState.STATIONARY;
			navMeshAgent.SetDestination(tf.position);
			cachedForward = tf.forward;
		}

		public void Deactivate()
		{
			activated = false;
			StopCoroutine(MoveAcrossDrop());
			StopCoroutine(MoveAcrossVault());
			StopCoroutine(MoveAcrossEscalator());
			if (navMeshAgent.isOnOffMeshLink)
			{
				NavMeshLink navMeshLink = (NavMeshLink)navMeshAgent.navMeshOwner;
				if (navMeshLink != null)
				{
					navMeshLink.costModifier = -1;
				}
			}
			navMeshAgent.enabled = false;
		}

		public void StopMoving()
		{
			targetDistance = 0f;
			targetLocation = tf.position;
			SetMovementSpeed(0f);
			SetState(MovementState.STATIONARY);
		}

		public void WarpToPosition(Vector3 position)
		{
			navMeshAgent.Warp(position);
		}

		public void MoveToStaticTarget(Vector3 newTargetLocation, bool useOffMeshLinks = false)
		{
			canUseOffMeshLinks = useOffMeshLinks;
			targetDistance = 0f;
			targetLocation = newTargetLocation;
			SetState(MovementState.STATIC_LOCATION);
		}

		public void MoveToPlayer(float newTargetOffset, bool useOffMeshLinks = false)
		{
			canUseOffMeshLinks = useOffMeshLinks;
			targetDistance = newTargetOffset;
			SetState(MovementState.TRACKING_PLAYER);
		}

		public void Descend(float speed)
		{
			targetBaseOffset = 0f;
			descendAscendTargetSpeed = 0f - speed;
		}

		public void Ascend(float speed)
		{
			targetBaseOffset = baseOffset;
			descendAscendTargetSpeed = speed;
		}

		public void Ground()
		{
			baseOffset = 0f;
			targetBaseOffset = 0f;
		}

		public void ResumeHover(float offset)
		{
			baseOffset = offset;
		}

		public void LookAtPlayer(bool instant, bool oppositeDirection = false)
		{
			rotationMode = RotationMode.ROTATE_TO_PLAYER;
			rotateInstant = instant;
			rotationOppositeDirection = oppositeDirection;
		}

		public void LookAtPosition(Vector3 newRotationTargetPosition, bool instant)
		{
			rotationTargetPosition = newRotationTargetPosition;
			rotationMode = RotationMode.ROTATE_STATIC_LOCATION;
			rotateInstant = instant;
		}

		public void LookAtMoveDir(bool instant)
		{
			LookAtMoveDir(inverse: false, instant);
		}

		public void LookAtMoveDir(bool inverse, bool instant)
		{
			rotationMode = (inverse ? RotationMode.ROTATE_VELOCITY_INVERSE : RotationMode.ROTATE_VELOCITY);
			rotateInstant = instant;
		}

		public void LookAtCurrentDir(bool instant)
		{
			rotationMode = RotationMode.STATIC;
			cachedForward = tf.forward;
			rotateInstant = instant;
		}

		public void SetMovementSpeed(float speed)
		{
			movementSpeed = speed;
			navMeshAgent.speed = movementSpeed;
		}

		public Vector3 GetVelocity()
		{
			return navMeshAgent.velocity;
		}

		public Vector3 GetLocalMovementDirection()
		{
			return tf.InverseTransformDirection(navMeshAgent.velocity.normalized);
		}

		public Vector3 GetLocalMovementVelocity()
		{
			return tf.InverseTransformVector(navMeshAgent.velocity);
		}

		public float GetFallAngle()
		{
			Vector3 vector = lastMovement;
			float result = 0f;
			if (vector.magnitude > 0f)
			{
				float y = vector.y;
				float magnitude = new Vector3(vector.x, 0f, vector.z).magnitude;
				result = y / magnitude;
			}
			return result;
		}

		public bool IsOnNavMeshLink()
		{
			return navMeshAgent.isOnOffMeshLink;
		}

		public bool IsMovingAlongNavMeshLink()
		{
			return moveAcrossNavMeshesStarted;
		}

		public bool IsEscalator()
		{
			Vector3 vector = navMeshAgent.currentOffMeshLinkData.endPos - navMeshAgent.currentOffMeshLinkData.startPos;
			if (vector.y < 0f)
			{
				vector *= -1f;
			}
			float num = Vector3.Angle(Vector3.up, vector.normalized);
			if (Mathf.Abs(vector.y) > 0.5f)
			{
				return num > 45f;
			}
			return false;
		}

		public bool IsDrop()
		{
			return Mathf.Abs(navMeshAgent.currentOffMeshLinkData.startPos.y - navMeshAgent.currentOffMeshLinkData.endPos.y) > 0.4f;
		}

		public bool IsOnEscalator()
		{
			return moveAcrossEscalator;
		}

		public float GetLinkDuration()
		{
			return (navMeshAgent.currentOffMeshLinkData.endPos - navMeshAgent.currentOffMeshLinkData.startPos).magnitude / ((IsDrop() ? dropSpeed : vaultSpeed) * 2f);
		}

		public void DoKnockBack(Vector3 direction, float distance, float duration, bool forceKnockBack = false)
		{
			DoKnockBack(direction, distance, 0f, duration, lookAtMoveDir: true, forceKnockBack);
		}

		public void DoKnockBack(Vector3 direction, float distance, float height, float duration, bool lookAtMoveDir, bool forceKnockBack = false)
		{
			canUseOffMeshLinks = false;
			direction.y = 0f;
			knockbackDirection = direction;
			knockbackTimer = (knockbackDuration = duration);
			knockbackHeight = navMeshAgent.baseOffset + height;
			knockbackHeight = Mathf.Max(0f, knockbackHeight);
			if (distance / duration > 0f)
			{
				SetMovementSpeed(Mathf.Clamp(distance / duration, 0f, player.boostSpeed * 2f));
			}
			bool flag = false;
			if ((knockbackHeight > 1.5f || movementState == MovementState.FLY_BACK || movementState == MovementState.DROP_DOWN) && !forceKnockBack && !moveAcrossNavMeshesStarted)
			{
				flag = true;
				if (knockbackHeight <= 0f && Physics.Raycast(tf.position, -Vector3.up, -1.5f, layerMask, QueryTriggerInteraction.Ignore))
				{
					flag = false;
				}
			}
			if (flag)
			{
				SetState(MovementState.FLY_BACK);
			}
			else
			{
				SetState(MovementState.KNOCK_BACK);
			}
			LookAtMoveDir(lookAtMoveDir, instant: true);
		}

		public void DoDropDown(Vector3 destination)
		{
			canUseOffMeshLinks = false;
			dropDownDestination = destination;
			dropDownDir = (dropDownDestination - tf.position).normalized;
			SetState(MovementState.DROP_DOWN);
		}

		public void CirclePlayer(float targetMinRepositionInterval, float targetMaxRepositionInterval, float targetCirclingRadius, float targetCirclingRange, float targetMinAngle, float targetMaxAngle, float backOffSpeedPerc = 0.5f, bool useOffMeshLinks = false)
		{
			canUseOffMeshLinks = useOffMeshLinks;
			minChangeDirInterval = targetMinRepositionInterval;
			maxchangeDirInterval = targetMaxRepositionInterval;
			circlingRadius = targetCirclingRadius;
			circlingRange = targetCirclingRange;
			minRepositionAngle = targetMinAngle;
			maxRepositionAngle = targetMaxAngle;
			changeDirTimer = 0f;
			backOffSpeedPercentage = backOffSpeedPerc;
			navMeshAgent.velocity = Vector3.zero;
			SetState(MovementState.CIRCLE_PLAYER);
			LookAtPlayer(instant: false);
		}

		public bool BackOffIfPossible(float desiredTargetRange, float minTargetRange)
		{
			int num = 5;
			NavMeshPath path = new NavMeshPath();
			NavMeshHit hit;
			for (int i = 0; i < num; i++)
			{
				if (NavMesh.SamplePosition(player.tf.position - positioningInfo.FlatDirToPlayer * (Mathf.Lerp(desiredTargetRange, minTargetRange, i / 5) + navMeshAgent.radius), out hit, navMeshAgent.radius, -1) && NavMesh.CalculatePath(tf.position, hit.position, -1, path))
				{
					backOffPosition = hit.position;
					canUseOffMeshLinks = false;
					SetState(MovementState.BACK_OFF_PLAYER);
					return true;
				}
			}
			for (int j = 0; j < num; j++)
			{
				if (NavMesh.SamplePosition(player.tf.position - tf.right * (minTargetRange + navMeshAgent.radius), out hit, navMeshAgent.radius, -1) && NavMesh.CalculatePath(tf.position, hit.position, -1, path))
				{
					backOffPosition = hit.position;
					canUseOffMeshLinks = false;
					SetState(MovementState.BACK_OFF_PLAYER);
					return true;
				}
			}
			for (int k = 0; k < num; k++)
			{
				if (NavMesh.SamplePosition(player.tf.position + tf.right * (minTargetRange + navMeshAgent.radius), out hit, navMeshAgent.radius, -1) && NavMesh.CalculatePath(tf.position, hit.position, -1, path))
				{
					backOffPosition = hit.position;
					canUseOffMeshLinks = false;
					SetState(MovementState.BACK_OFF_PLAYER);
					return true;
				}
			}
			return false;
		}

		public void DoMove(Vector3 movement)
		{
			navMeshAgent.Move(movement);
		}

		public void Pause()
		{
			paused = true;
			if (navMeshAgent.isOnNavMesh && navMeshAgent.isActiveAndEnabled)
			{
				navMeshAgent.isStopped = true;
			}
			navMeshAgent.velocity = Vector3.zero;
		}

		public void UnPause()
		{
			paused = false;
			if (navMeshAgent.isOnNavMesh && navMeshAgent.isActiveAndEnabled)
			{
				navMeshAgent.isStopped = false;
			}
		}

		public void StopObstacleAvoidance()
		{
			navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
		}

		public void StartObstacleAvoidance()
		{
			navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
		}

		public bool RespondingToHit()
		{
			if (movementState != MovementState.DROP_DOWN && movementState != MovementState.FLY_BACK)
			{
				return movementState == MovementState.KNOCK_BACK;
			}
			return true;
		}

		public bool AirBorne()
		{
			if (movementState != MovementState.DROP_DOWN && movementState != MovementState.FLY_BACK)
			{
				return baseOffset > 1f;
			}
			return true;
		}

		public int GetAreaMask()
		{
			return navMeshAgent.areaMask;
		}

		public void SetRotationSpeed(float rotationSpeed)
		{
			this.rotationSpeed = rotationSpeed;
		}

		public void PauseKnockBack()
		{
			pauseKnockBack = true;
		}

		public void ResumeKnockBack()
		{
			pauseKnockBack = false;
		}

		private void SetState(MovementState newState)
		{
			lastMovementState = movementState;
			ExitState(movementState);
			movementState = newState;
			switch (movementState)
			{
			case MovementState.STATIONARY:
				SetAreaMask();
				if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
				{
					navMeshAgent.isStopped = true;
				}
				break;
			case MovementState.STATIC_LOCATION:
				SetAreaMask();
				navMeshAgent.SetDestination(targetLocation);
				break;
			case MovementState.TRACKING_PLAYER:
				SetAreaMask();
				break;
			case MovementState.CIRCLE_PLAYER:
				SetAreaMask();
				circlingOffset = -(positioningInfo.FlatDirToPlayer * 10f);
				changeDirTimer = maxchangeDirInterval;
				circlingOffset = GetPlayerCirclingPosition(circlingRadius, circlingRange, minRepositionAngle, maxRepositionAngle);
				break;
			case MovementState.BACK_OFF_PLAYER:
				SetAreaMask();
				navMeshAgent.SetDestination(backOffPosition);
				break;
			case MovementState.KNOCK_BACK:
				if (!moveAcrossNavMeshesStarted)
				{
					SetAreaMask();
					navMeshAgent.ResetPath();
					navMeshAgent.baseOffset = (knockbackStartHeight = navMeshAgent.baseOffset);
				}
				break;
			case MovementState.FLY_BACK:
				SetAreaMask();
				if (lastMovementState != MovementState.DROP_DOWN && lastMovementState != MovementState.FLY_BACK)
				{
					lastNavMeshPosition = tf.position;
				}
				navMeshAgent.enabled = false;
				break;
			case MovementState.DROP_DOWN:
				SetAreaMask();
				SetMovementSpeed(5f);
				navMeshAgent.enabled = false;
				break;
			}
		}

		public void UpdateMovement()
		{
			if (!activated || paused)
			{
				return;
			}
			switch (movementState)
			{
			case MovementState.STATIC_LOCATION:
				if (navMeshAgent.isOnOffMeshLink)
				{
					if (!moveAcrossNavMeshesStarted)
					{
						StartCoroutine(IsEscalator() ? MoveAcrossEscalator() : (IsDrop() ? MoveAcrossDrop() : MoveAcrossVault()));
						moveAcrossNavMeshesStarted = true;
					}
				}
				else
				{
					navMeshAgent.speed = movementSpeed;
				}
				if (ReachedDestination())
				{
					SetState(MovementState.STATIONARY);
				}
				break;
			case MovementState.TRACKING_PLAYER:
			{
				if (navMeshAgent.isOnOffMeshLink)
				{
					if (!moveAcrossNavMeshesStarted)
					{
						StartCoroutine(IsEscalator() ? MoveAcrossEscalator() : (IsDrop() ? MoveAcrossDrop() : MoveAcrossVault()));
						moveAcrossNavMeshesStarted = true;
					}
				}
				else
				{
					navMeshAgent.speed = movementSpeed;
				}
				bool flag = false;
				if (!ignoreTargetHeight)
				{
					flag = navMeshAgent.SetDestination(playerTf.position - positioningInfo.FlatDirToPlayer * targetDistance);
				}
				if (!flag)
				{
					Vector3 vector9 = playerTf.position - positioningInfo.FlatDirToPlayer * targetDistance;
					vector9.y = tf.position.y;
					if (NavMesh.SamplePosition(vector9, out var hit, 3f, -1))
					{
						navMeshAgent.SetDestination(hit.position);
					}
					else if (NavMesh.SamplePosition(vector9 + Vector3.up * 5f, out hit, 6f, -1))
					{
						navMeshAgent.SetDestination(hit.position);
					}
					else if (NavMesh.SamplePosition(vector9 + Vector3.up * 10f, out hit, 6f, -1))
					{
						navMeshAgent.SetDestination(hit.position);
					}
					else if (NavMesh.SamplePosition(vector9 - Vector3.up * 5f, out hit, 6f, -1))
					{
						navMeshAgent.SetDestination(hit.position);
					}
					else if (NavMesh.SamplePosition(vector9 - Vector3.up * 10f, out hit, 6f, -1))
					{
						navMeshAgent.SetDestination(hit.position);
					}
				}
				break;
			}
			case MovementState.CIRCLE_PLAYER:
				changeDirTimer -= Core.dt;
				if (changeDirTimer <= 0f)
				{
					changeDirTimer = UnityEngine.Random.Range(minChangeDirInterval, maxchangeDirInterval);
					circlingOffset = GetPlayerCirclingPosition(circlingRadius, circlingRange, minRepositionAngle, maxRepositionAngle);
				}
				navMeshAgent.SetDestination(playerTf.position + circlingOffset);
				if (GetLocalMovementDirection().z < 0f)
				{
					navMeshAgent.speed = movementSpeed * backOffSpeedPercentage;
				}
				else if (navMeshAgent.isOnOffMeshLink)
				{
					if (!moveAcrossNavMeshesStarted)
					{
						StartCoroutine(IsEscalator() ? MoveAcrossEscalator() : (IsDrop() ? MoveAcrossDrop() : MoveAcrossVault()));
						moveAcrossNavMeshesStarted = true;
					}
				}
				else
				{
					navMeshAgent.speed = movementSpeed;
				}
				break;
			case MovementState.BACK_OFF_PLAYER:
				navMeshAgent.speed = movementSpeed;
				if (ReachedDestination())
				{
					SetState(MovementState.STATIONARY);
				}
				break;
			case MovementState.KNOCK_BACK:
				knockbackTimer -= Core.dt;
				if (!moveAcrossNavMeshesStarted)
				{
					navMeshAgent.velocity = knockbackDirection * movementSpeed;
					if (knockbackTimer > knockbackDuration / 2f)
					{
						navMeshAgent.baseOffset = Mathf.Lerp(navMeshAgent.baseOffset, Mathf.Max(0f, knockbackHeight), 1f - Mathf.Pow(1f - (knockbackDuration / 2f - (knockbackTimer - knockbackDuration / 2f)) / (knockbackDuration / 2f), 2f));
					}
					else
					{
						navMeshAgent.baseOffset = Mathf.Lerp(Mathf.Max(0f, knockbackHeight), baseOffset, Mathf.Pow((knockbackDuration / 2f - knockbackTimer) / (knockbackDuration / 2f), 2f));
					}
				}
				if (knockbackTimer <= 0f)
				{
					StopMoving();
				}
				break;
			case MovementState.FLY_BACK:
			{
				knockbackTimer -= ((pauseKnockBack && knockbackTimer <= knockbackDuration / 2f) ? 0f : Core.dt);
				lastMovement = tf.position - lastPosition;
				lastPosition = tf.position;
				Vector3 position = base.transform.position;
				Vector3 velocity = player.GetVelocity();
				velocity.y = Mathf.Max(velocity.y, 0f);
				Vector3 vector2 = player.characterVisual.root.position + player.tf.forward * 1.5f - Vector3.up - tf.position;
				position += ((!pauseKnockBack) ? (knockbackDirection.normalized * movementSpeed * Core.dt) : ((vector2.sqrMagnitude * Core.dt > 0f) ? vector2 : (velocity * Core.dt)));
				if (knockbackTimer > knockbackDuration / 2f)
				{
					vertSpeed = Mathf.Clamp(knockbackHeight, -100f, 100f) / (knockbackDuration / 2f) * 2f;
					vertSpeed = Mathf.Lerp(vertSpeed * Core.dt, 0f, 1f - Mathf.Pow(1f - (knockbackDuration / 2f - (knockbackTimer - knockbackDuration / 2f)) / (knockbackDuration / 2f), 2f));
					position.y += vertSpeed;
					if (Physics.Raycast(tf.position, Vector3.up, out var _, 2f, layerMask, QueryTriggerInteraction.Ignore))
					{
						Vector3? vector3 = FindDropDestination(-knockbackDirection);
						if (vector3.HasValue)
						{
							if ((vector3.Value - tf.position).sqrMagnitude < 0.5f)
							{
								base.transform.position = vector3.Value;
								StopMoving();
								return;
							}
							DoDropDown(vector3.Value);
						}
						else
						{
							base.transform.position = lastNavMeshPosition + Vector3.up * 0.2f;
							StopMoving();
						}
						break;
					}
					Vector3 vector4 = position - tf.position;
					if (Physics.Linecast(tf.position, tf.position + vector4.normalized * (vector4.magnitude + navMeshAgent.radius), out var _, layerMask, QueryTriggerInteraction.Ignore))
					{
						Vector3? vector5 = FindDropDestination(-knockbackDirection);
						if (vector5.HasValue)
						{
							if ((vector5.Value - tf.position).sqrMagnitude < 0.5f)
							{
								base.transform.position = vector5.Value;
								StopMoving();
							}
							else
							{
								DoDropDown(vector5.Value);
							}
						}
						else
						{
							base.transform.position = lastNavMeshPosition + Vector3.up * 0.2f;
							StopMoving();
						}
					}
					else
					{
						base.transform.position = position;
					}
					break;
				}
				vertSpeed = Mathf.Lerp(0f, -20f / (knockbackDuration / 2f) * Core.dt, Mathf.Pow((knockbackDuration / 2f - knockbackTimer) / (knockbackDuration / 2f), 2f));
				position.y += (pauseKnockBack ? 0f : vertSpeed);
				if (Physics.Raycast(tf.position + Vector3.up * 0.2f, -Vector3.up, out var _, 0f - vertSpeed + 0.2f, layerMask, QueryTriggerInteraction.Ignore))
				{
					Vector3? vector6 = FindDropDestination(knockbackDirection);
					if (vector6.HasValue)
					{
						base.transform.position = vector6.Value;
						StopMoving();
					}
					else
					{
						base.transform.position = lastNavMeshPosition + Vector3.up * 0.2f;
						StopMoving();
					}
					break;
				}
				Vector3 vector7 = position - tf.position;
				if (Physics.Raycast(tf.position, vector7.normalized, out var _, vector7.magnitude + navMeshAgent.radius, layerMask, QueryTriggerInteraction.Ignore))
				{
					Vector3? vector8 = FindDropDestination(-knockbackDirection);
					if (vector8.HasValue)
					{
						if ((vector8.Value - tf.position).sqrMagnitude < 0.5f)
						{
							base.transform.position = vector8.Value;
							StopMoving();
						}
						else
						{
							DoDropDown(vector8.Value);
						}
					}
					else
					{
						base.transform.position = lastNavMeshPosition + Vector3.up * 0.2f;
						StopMoving();
					}
				}
				else
				{
					base.transform.position = position;
				}
				break;
			}
			case MovementState.DROP_DOWN:
			{
				lastMovement = tf.position - lastPosition;
				lastPosition = tf.position;
				SetMovementSpeed(movementSpeed + 10f * Core.dt);
				Vector3 vector = dropDownDir * movementSpeed * Core.dt;
				if (Physics.Raycast(tf.position, dropDownDir, out var _, movementSpeed * Core.dt, layerMask, QueryTriggerInteraction.Ignore))
				{
					base.transform.position = dropDownDestination;
					StopMoving();
				}
				else
				{
					tf.position += vector;
				}
				break;
			}
			}
			if (targetBaseOffset != navMeshAgent.baseOffset)
			{
				if (descendAscendTargetSpeed > 0f && descendAscendCurrentSpeed < descendAscendTargetSpeed)
				{
					if (targetBaseOffset - navMeshAgent.baseOffset > navMeshAgent.baseOffset)
					{
						descendAscendCurrentSpeed += 70f * Core.dt;
					}
					else
					{
						descendAscendCurrentSpeed -= 70f * Core.dt;
					}
					descendAscendCurrentSpeed = Mathf.Clamp(descendAscendCurrentSpeed, 0f, descendAscendTargetSpeed);
				}
				else if (descendAscendTargetSpeed < 0f && descendAscendCurrentSpeed >= descendAscendTargetSpeed)
				{
					if (baseOffset - navMeshAgent.baseOffset > navMeshAgent.baseOffset)
					{
						descendAscendCurrentSpeed += 70f * Core.dt;
					}
					else
					{
						descendAscendCurrentSpeed -= 70f * Core.dt;
					}
					descendAscendCurrentSpeed = Mathf.Clamp(descendAscendCurrentSpeed, descendAscendTargetSpeed, 0f);
				}
				navMeshAgent.baseOffset += descendAscendCurrentSpeed * Core.dt;
				if (navMeshAgent.baseOffset > targetBaseOffset && targetBaseOffset == baseOffset)
				{
					navMeshAgent.baseOffset = targetBaseOffset;
					targetBaseOffset = baseOffset;
				}
				else if (navMeshAgent.baseOffset < targetBaseOffset && targetBaseOffset < baseOffset)
				{
					navMeshAgent.baseOffset = targetBaseOffset;
					targetBaseOffset = 0f;
				}
			}
			UpdateRotationState();
		}

		private void UpdateRotationState()
		{
			if (navMeshAgent.isOnOffMeshLink)
			{
				return;
			}
			switch (rotationMode)
			{
			case RotationMode.ROTATE_TO_PLAYER:
				tf.rotation = Quaternion.Lerp(tf.rotation, Quaternion.LookRotation(rotationOppositeDirection ? (-positioningInfo.FlatDirToPlayer) : positioningInfo.FlatDirToPlayer, Vector3.up), rotateInstant ? 1f : (1f - Mathf.Exp((0f - rotationSpeed) * Core.dt)));
				break;
			case RotationMode.ROTATE_STATIC_LOCATION:
			{
				Vector3 vector2 = rotationTargetPosition;
				vector2.y = tf.position.y;
				tf.rotation = Quaternion.Lerp(tf.rotation, Quaternion.LookRotation(vector2 - tf.position), rotateInstant ? 1f : (1f - Mathf.Exp((0f - rotationSpeed) * Core.dt)));
				break;
			}
			case RotationMode.ROTATE_VELOCITY:
				tf.rotation = Quaternion.Lerp(tf.rotation, Quaternion.LookRotation((navMeshAgent.velocity.magnitude > 0f) ? navMeshAgent.velocity.normalized : tf.forward), rotateInstant ? 1f : (1f - Mathf.Exp((0f - rotationSpeed) * Core.dt)));
				break;
			case RotationMode.ROTATE_VELOCITY_INVERSE:
			{
				if (navMeshAgent.enabled)
				{
					tf.rotation = Quaternion.Lerp(tf.rotation, Quaternion.LookRotation((navMeshAgent.velocity.magnitude > 0f) ? (-navMeshAgent.velocity.normalized) : tf.forward), rotateInstant ? 1f : (1f - Mathf.Exp((0f - rotationSpeed) * Core.dt)));
					break;
				}
				Vector3 vector = tf.position - lastPosition;
				vector.y = 0f;
				tf.rotation = Quaternion.Lerp(tf.rotation, Quaternion.LookRotation((vector.magnitude > 0f) ? (-vector.normalized) : tf.forward), rotateInstant ? 1f : (1f - Mathf.Exp((0f - rotationSpeed) * Core.dt)));
				break;
			}
			case RotationMode.STATIC:
				tf.forward = cachedForward;
				break;
			}
		}

		private void ExitState(MovementState state)
		{
			switch (state)
			{
			case MovementState.STATIONARY:
				if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
				{
					navMeshAgent.isStopped = false;
				}
				break;
			case MovementState.KNOCK_BACK:
				navMeshAgent.baseOffset = baseOffset;
				knockbackStartHeight = 0f;
				break;
			case MovementState.FLY_BACK:
				navMeshAgent.enabled = true;
				break;
			case MovementState.DROP_DOWN:
				navMeshAgent.enabled = true;
				navMeshAgent.baseOffset = baseOffset;
				break;
			case MovementState.STATIC_LOCATION:
			case MovementState.TRACKING_PLAYER:
			case MovementState.CIRCLE_PLAYER:
			case MovementState.BACK_OFF_PLAYER:
				break;
			}
		}

		private IEnumerator MoveAcrossDrop()
		{
			OffMeshLinkData currentOffMeshLinkData = navMeshAgent.currentOffMeshLinkData;
			navMeshAgent.updateRotation = false;
			Landed = false;
			NavMeshLink link = (NavMeshLink)navMeshAgent.navMeshOwner;
			link.costModifier = 100;
			Vector3 vector = currentOffMeshLinkData.startPos - link.transform.position;
			vector.y = 0f;
			Vector3 vector2 = link.transform.position - currentOffMeshLinkData.endPos;
			vector2.y = 0f;
			ratio = vector.magnitude / (vector2.magnitude * 2f);
			Vector3 startPos = navMeshAgent.transform.position;
			Vector3 endPos = currentOffMeshLinkData.endPos + Vector3.up * navMeshAgent.baseOffset;
			drop = startPos.y - endPos.y;
			float num = Mathf.Sqrt(2f * drop / 10f);
			Vector3 dir = endPos - startPos;
			dir.y = 0f;
			dir = dir.normalized;
			float t = 0f;
			float tStep = 1f / num;
			while (t < 1f)
			{
				if (!paused)
				{
					Vector3 vector3 = Vector3.Lerp(startPos, endPos, t);
					vector3.y += Mathf.Sin(t * MathF.PI) * (drop * ratio);
					tf.rotation = Quaternion.Lerp(tf.rotation, Quaternion.LookRotation(dir), rotateInstant ? 1f : (1f - Mathf.Exp(-10f * Core.dt)));
					tf.position = vector3;
					navMeshAgent.velocity = vector3 - tf.position;
					t += Mathf.Lerp(tStep / 2f, tStep * 2f, t) * Core.dt;
				}
				yield return null;
			}
			base.transform.position = endPos;
			Landed = true;
			yield return new WaitForSeconds(0.5f);
			navMeshAgent.updateRotation = true;
			navMeshAgent.CompleteOffMeshLink();
			moveAcrossNavMeshesStarted = false;
			link.costModifier = -1;
		}

		private IEnumerator MoveAcrossVault()
		{
			OffMeshLinkData currentOffMeshLinkData = navMeshAgent.currentOffMeshLinkData;
			navMeshAgent.updateRotation = false;
			Landed = false;
			NavMeshLink link = (NavMeshLink)navMeshAgent.navMeshOwner;
			link.costModifier = 100;
			Vector3 startPos = navMeshAgent.transform.position;
			Vector3 endPos = currentOffMeshLinkData.endPos + Vector3.up * navMeshAgent.baseOffset;
			Vector3 dir = endPos - startPos;
			_ = dir.magnitude;
			dir = dir.normalized;
			float num = 1.1f;
			float num2 = link.transform.position.y - (startPos.y + endPos.y) / 2f;
			float handOffset = num2 - vaultHandHeight;
			float t = 0f;
			float tStep = 1f / num;
			yield return new WaitForSeconds(vaultStartDelay);
			while (t < 1f)
			{
				if (!paused)
				{
					moveAcrossVaultProgress = t;
					Vector3 position = Vector3.Lerp(startPos, endPos, t);
					position.y += Mathf.Sin(t * MathF.PI) * handOffset;
					tf.rotation = Quaternion.Lerp(tf.rotation, Quaternion.LookRotation(dir), rotateInstant ? 1f : (1f - Mathf.Exp(-10f * Core.dt)));
					tf.position = position;
					t += tStep * Core.dt;
				}
				yield return null;
			}
			Landed = true;
			base.transform.position = endPos;
			yield return new WaitForSeconds(vaultLandDelay);
			navMeshAgent.updateRotation = true;
			navMeshAgent.CompleteOffMeshLink();
			moveAcrossNavMeshesStarted = false;
			link.costModifier = -1;
		}

		private IEnumerator MoveAcrossEscalator()
		{
			moveAcrossEscalator = true;
			OffMeshLinkData data = navMeshAgent.currentOffMeshLinkData;
			navMeshAgent.updateRotation = false;
			NavMeshLink link = (NavMeshLink)navMeshAgent.navMeshOwner;
			link.costModifier = 100;
			Vector3 startPos = navMeshAgent.transform.position;
			Vector3 endPos = data.startPos + Vector3.up * navMeshAgent.baseOffset;
			Vector3 dir = endPos - startPos;
			float magnitude = dir.magnitude;
			dir = dir.normalized;
			float num = magnitude / escalatorSpeed;
			float t = 0f;
			float tStep = 1f / num;
			while (t < 1f)
			{
				if (!paused && movementState != MovementState.KNOCK_BACK)
				{
					Vector3 position = Vector3.Lerp(startPos, endPos, t);
					if (Physics.Raycast(tf.position + Vector3.up, -Vector3.up, out var hitInfo, 5f, layerMask, QueryTriggerInteraction.Ignore))
					{
						position.y = hitInfo.point.y;
					}
					tf.rotation = Quaternion.Lerp(tf.rotation, Quaternion.LookRotation(dir), rotateInstant ? 1f : (1f - Mathf.Exp(-10f * Core.dt)));
					tf.position = position;
					t += tStep * Core.dt;
				}
				yield return null;
			}
			startPos = navMeshAgent.transform.position;
			endPos = data.endPos + Vector3.up * navMeshAgent.baseOffset;
			dir = endPos - startPos;
			float magnitude2 = dir.magnitude;
			dir = dir.normalized;
			num = magnitude2 / escalatorSpeed;
			t = 0f;
			tStep = 1f / num;
			while (t < 1f)
			{
				if (!paused && movementState != MovementState.KNOCK_BACK)
				{
					Vector3 position2 = Vector3.Lerp(startPos, endPos, t);
					if (Physics.Raycast(tf.position + Vector3.up, -Vector3.up, out var hitInfo2, 5f, layerMask, QueryTriggerInteraction.Ignore))
					{
						position2.y = hitInfo2.point.y;
					}
					tf.rotation = Quaternion.Lerp(tf.rotation, Quaternion.LookRotation(dir), rotateInstant ? 1f : (1f - Mathf.Exp(-10f * Core.dt)));
					tf.position = position2;
					t += tStep * Core.dt;
				}
				yield return null;
			}
			base.transform.position = endPos;
			navMeshAgent.updateRotation = true;
			navMeshAgent.CompleteOffMeshLink();
			moveAcrossNavMeshesStarted = false;
			moveAcrossEscalator = false;
			link.costModifier = -1;
		}

		private Vector3? FindDropDestination(Vector3 searchDirection)
		{
			return FindDropDestination(base.transform.position, searchDirection);
		}

		private Vector3? FindDropDestination(Vector3 startPosition, Vector3 searchDirection)
		{
			for (int i = 0; i < 10; i++)
			{
				if (Physics.Raycast(startPosition + searchDirection * 1f * i + Vector3.up * 0.2f, -Vector3.up, out var hitInfo, 100f, layerMask, QueryTriggerInteraction.Ignore))
				{
					Vector3? vector = ValidNavMeshLocation(startPosition, hitInfo.point, navMeshAgent.radius);
					if (vector.HasValue)
					{
						return vector.Value;
					}
				}
			}
			return null;
		}

		private Vector3? ValidNavMeshLocation(Vector3 originPosition, Vector3 samplePosition, float searchRadius)
		{
			if (NavMesh.SamplePosition(samplePosition, out var hit, searchRadius, navMeshAgent.areaMask))
			{
				return hit.position;
			}
			return null;
		}

		private bool ReachedDestination()
		{
			return (navMeshAgent.destination - tf.position).sqrMagnitude <= navMeshAgent.stoppingDistance * navMeshAgent.stoppingDistance;
		}

		private Vector3 GetPlayerCirclingPosition(float ringRadius, float wallRadius, float minAngleOffset, float maxAngleOffset)
		{
			float num = UnityEngine.Random.Range(minAngleOffset, maxAngleOffset);
			num *= (float)((!((double)UnityEngine.Random.value < 0.5)) ? 1 : (-1));
			float num2 = Vector3.Angle(Vector3.forward, -positioningInfo.FlatDirToPlayer) + num;
			num2 *= MathF.PI / 180f;
			return new Vector3(Mathf.Sin(num2), 0f, Mathf.Cos(num2)) * ringRadius;
		}

		private void SetAreaMask()
		{
			if (!canUseOffMeshLinks)
			{
				navMeshAgent.areaMask = defaultAreaMask;
			}
			else
			{
				navMeshAgent.areaMask = defaultAreaMask | (1 << NavMesh.GetAreaFromName("Jump"));
			}
		}

		public bool ValidateCanReachPlayerFromNavmesh()
		{
			NavMeshPath navMeshPath = new NavMeshPath();
			int areaMask = navMeshAgent.areaMask;
			navMeshAgent.areaMask = defaultAreaMask | (1 << NavMesh.GetAreaFromName("Jump"));
			navMeshAgent.CalculatePath(player.lastNavMeshPosition, navMeshPath);
			navMeshAgent.areaMask = areaMask;
			if (navMeshPath == null || navMeshPath.status == NavMeshPathStatus.PathInvalid || Vector3.Distance(navMeshPath.corners[navMeshPath.corners.Length - 1], player.lastNavMeshPosition) > 1f)
			{
				if (!WorldHandler.instance.IsReallyInScreen(tf.position))
				{
					return false;
				}
				if (Physics.Raycast(tf.position + Vector3.up * 1.5f, positioningInfo.DirToPlayer, out var _, positioningInfo.DistToPlayer - 1f, layerMask, QueryTriggerInteraction.Ignore))
				{
					return false;
				}
				return true;
			}
			return true;
		}
	}
}
