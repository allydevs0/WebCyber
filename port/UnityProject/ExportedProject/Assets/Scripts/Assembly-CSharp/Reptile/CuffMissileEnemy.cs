using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class CuffMissileEnemy : MonoBehaviour, IPoolable
	{
		public enum CuffMissileState
		{
			ACTIVE = 0,
			RETURNING = 1,
			ATTACHED = 2,
			DEPLOYED = 3,
			UNGUIDED = 4
		}

		private bool completedInternalOneTimeInit;

		private float totalDist;

		private float deployTimer;

		private float gravitySpeed;

		private float newTargetOffsetTimer;

		private int currentLastIndex;

		private Transform attachedTransform;

		public static int totalCuffs;

		private bool partOfTotalCuffs;

		[SerializeField]
		private GameObject boostEffect;

		internal CuffMissileLauncher owner;

		private Transform chainOriginTf;

		private Player player;

		private Rigidbody rBody;

		private float targetOffsetFactor;

		private Vector3 targetOffset;

		private Vector3 prePos;

		private bool hadSelectedReflectTarget;

		private Transform reflectTarget;

		private Vector3 targetPos;

		private Quaternion targetRotation;

		private Vector3 outOfControlDeviate;

		private float rotSpeed;

		private float maxActiveSpeed = 15f;

		private float speed;

		private float activeRotateSpeed = 140f;

		private float returnSpeed = 60f;

		private float returnRotateSpeed = 560f;

		private float deployedSpeed = 20f;

		private float deployedRotateSpeed = 140f;

		private Vector3 dropVelocity;

		private float deploymentDistance = 3f;

		private float deployDuration = 0.5f;

		public GameObject linePrefab;

		private LineRenderer line;

		private float stateTimer;

		private float targetOffsetInterval;

		private float linePointInterval = 0.4f;

		private float linePointDist;

		private Vector3[] chainPointArray = new Vector3[200];

		private Collider hitbox;

		public GameObject canisterDeploySmokePrefab;

		public AudioSource audioSource;

		private CuffMissileState state;

		protected PoolableState poolableState = PoolableState.Pooled;

		private PoolableObjectToName canisterDeploySmoke;

		private string poolableName;

		private List<IPoolable> poolables = new List<IPoolable>();

		public CuffMissileState MissileState => state;

		public PoolableState State
		{
			get
			{
				return poolableState;
			}
			set
			{
				poolableState = value;
			}
		}

		public string PoolableName
		{
			get
			{
				return poolableName;
			}
			set
			{
				poolableName = value;
			}
		}

		public void InitPoolable(Vector3 position, Quaternion rotation)
		{
			base.gameObject.SetActive(value: true);
			base.transform.position = position;
			base.transform.rotation = rotation;
			poolableState = PoolableState.Active;
		}

		public void Init(CuffMissileLauncher setOwner, Transform chainOrigin)
		{
			if (!completedInternalOneTimeInit)
			{
				InternalOneTimeInit();
			}
			totalCuffs++;
			partOfTotalCuffs = true;
			owner = setOwner;
			player = WorldHandler.instance.GetCurrentPlayer();
			chainOriginTf = chainOrigin;
			ResetState();
			Core.OnLateUpdate += UpdateChain;
		}

		private void InternalOneTimeInit()
		{
			rBody = GetComponent<Rigidbody>();
			line = Object.Instantiate(linePrefab).GetComponent<LineRenderer>();
			canisterDeploySmoke = new PoolableObjectToName(canisterDeploySmokePrefab, canisterDeploySmokePrefab.name);
			hitbox = GetComponent<BoxCollider>();
			Core.OnUpdate += UpdateMissileEnemy;
			completedInternalOneTimeInit = true;
		}

		private void ResetState()
		{
			chainPointArray[0] = chainOriginTf.position;
			currentLastIndex = 0;
			prePos = base.transform.position;
			SetNewTargetOffset();
			SetState(CuffMissileState.ACTIVE);
		}

		public virtual void ReturnToPool()
		{
			base.gameObject.SetActive(value: false);
			poolableState = PoolableState.Pooled;
		}

		private void UpdateMissileEnemy()
		{
			if (poolableState == PoolableState.Pooled)
			{
				return;
			}
			if (owner == null)
			{
				Delete();
				return;
			}
			stateTimer += Core.dt;
			newTargetOffsetTimer += Core.dt;
			Vector3 normalized = (targetPos - base.transform.position).normalized;
			prePos = base.transform.position;
			switch (state)
			{
			case CuffMissileState.ACTIVE:
				rotSpeed = Mathf.Min(rotSpeed + 200f * Core.dt, activeRotateSpeed);
				targetPos = player.capsuleCenter;
				targetOffsetFactor = Mathf.Clamp01(targetOffsetFactor - Core.dt * 0.25f);
				if (newTargetOffsetTimer > targetOffsetInterval)
				{
					SetNewTargetOffset();
				}
				MoveToTarget();
				if (Vector3.Distance(base.transform.position, targetPos) < deploymentDistance && Vector3.Dot(normalized, base.transform.forward) > 0.2f)
				{
					deployTimer += Core.dt;
					if (deployTimer > deployDuration)
					{
						SetState(CuffMissileState.DEPLOYED);
					}
				}
				else
				{
					deployTimer = 0f;
				}
				if (stateTimer > 3f)
				{
					SetState(CuffMissileState.UNGUIDED);
				}
				break;
			case CuffMissileState.UNGUIDED:
				rotSpeed = Mathf.Lerp(rotSpeed, activeRotateSpeed / 2f, stateTimer);
				targetPos = player.capsuleCenter;
				targetOffsetFactor = Mathf.Clamp01(targetOffsetFactor - Core.dt * 0.25f);
				if (newTargetOffsetTimer > targetOffsetInterval)
				{
					SetNewTargetOffset();
				}
				MoveToTarget();
				if (Vector3.Distance(base.transform.position, targetPos) < deploymentDistance && Vector3.Dot(normalized, base.transform.forward) > 0.2f)
				{
					deployTimer += Core.dt;
					if (deployTimer > deployDuration)
					{
						SetState(CuffMissileState.DEPLOYED);
					}
				}
				else
				{
					deployTimer = 0f;
				}
				if (stateTimer > 2f)
				{
					SetState(CuffMissileState.RETURNING);
				}
				break;
			case CuffMissileState.RETURNING:
				if (stateTimer > 0.05f)
				{
					rotSpeed = Mathf.Lerp(returnRotateSpeed, returnRotateSpeed * 2f, stateTimer - 0.05f);
					targetPos = chainOriginTf.position;
					speed += 100f * Core.dt;
					if (speed > returnSpeed)
					{
						speed = returnSpeed;
					}
					MoveToTarget();
				}
				if (stateTimer > 2f)
				{
					Delete();
				}
				break;
			case CuffMissileState.DEPLOYED:
				if (stateTimer < 0.05f)
				{
					dropVelocity = Vector3.zero;
				}
				else if (dropVelocity == Vector3.zero)
				{
					dropVelocity = base.transform.forward * deployedSpeed;
				}
				else
				{
					Drop();
				}
				if (stateTimer > 2f)
				{
					SetState(CuffMissileState.RETURNING);
				}
				targetPos = player.capsuleCenter;
				break;
			case CuffMissileState.ATTACHED:
				if (attachedTransform != null)
				{
					base.transform.position = attachedTransform.position;
				}
				if (attachedTransform == null)
				{
					SetState(CuffMissileState.RETURNING);
					base.transform.GetChild(0).gameObject.SetActive(value: false);
				}
				break;
			}
		}

		private void UpdateChain()
		{
			if (base.transform == null)
			{
				return;
			}
			float magnitude = (base.transform.position - prePos).magnitude;
			linePointDist += magnitude;
			if (linePointDist > linePointInterval)
			{
				linePointDist = 0f;
				if (currentLastIndex == chainPointArray.Length - 1)
				{
					RemovePosition(straighten: true);
				}
				AddPositionAtEnd(base.transform.position);
			}
			Vector3 vector = base.transform.position - chainOriginTf.position;
			float magnitude2 = vector.magnitude;
			Vector3 normalized = vector.normalized;
			float num = magnitude2 / (float)(currentLastIndex + 1);
			if (num < linePointInterval && currentLastIndex > 0)
			{
				RemovePosition(straighten: false);
				num = magnitude2 / (float)(currentLastIndex + 1);
			}
			float t = 0.01f;
			if (state == CuffMissileState.ATTACHED)
			{
				t = 0.1f;
			}
			if (state == CuffMissileState.RETURNING || currentLastIndex >= chainPointArray.Length - 1)
			{
				t = 0.3f;
			}
			totalDist = 0f;
			for (int i = 0; i < currentLastIndex; i++)
			{
				Vector3 b = chainOriginTf.position + num * (float)i * normalized;
				chainPointArray[i] = Vector3.Lerp(chainPointArray[i], b, t);
				if (i > 0)
				{
					totalDist += Vector3.Distance(chainPointArray[i], chainPointArray[i - 1]);
				}
			}
			chainPointArray[currentLastIndex] = base.transform.position;
			line.positionCount = currentLastIndex + 1;
			chainPointArray[0] = chainOriginTf.position;
			for (int j = 0; j < currentLastIndex + 1; j++)
			{
				line.SetPosition(j, chainPointArray[j]);
			}
			UpdateUVs();
		}

		private void AddPositionAtEnd(Vector3 position)
		{
			chainPointArray[currentLastIndex + 1] = position;
			currentLastIndex++;
		}

		private void RemovePosition(bool straighten)
		{
			int num = Mathf.Clamp((currentLastIndex + 1) / 2, 1, currentLastIndex);
			if (straighten)
			{
				if (num > 0)
				{
					chainPointArray[num - 1] = Vector3.Lerp(chainPointArray[num - 1], chainPointArray[num], 0.5f);
				}
				if (num < currentLastIndex - 1)
				{
					chainPointArray[num + 1] = Vector3.Lerp(chainPointArray[num + 1], chainPointArray[num], 0.5f);
				}
			}
			for (int i = num; i < currentLastIndex; i++)
			{
				chainPointArray[i - 1] = chainPointArray[i];
			}
			currentLastIndex--;
		}

		private void UpdateUVs()
		{
			line.material.SetTextureOffset("_MainTex", new Vector2(0f - totalDist * line.material.GetTextureScale("_MainTex").x, 0f));
		}

		private void SetNewTargetOffset()
		{
			targetOffsetFactor = 1f;
			targetOffset = Random.insideUnitSphere * 2.7f;
			if (targetOffset.y < 0f)
			{
				targetOffset *= -1f;
			}
			targetOffset.y *= 0.75f;
			newTargetOffsetTimer = 0f;
			targetOffsetInterval = Random.Range(1.2f, 3f);
		}

		public void Drop()
		{
			if (dropVelocity.sqrMagnitude > 1f)
			{
				dropVelocity -= dropVelocity.normalized * 3f * Core.dt;
			}
			gravitySpeed = Mathf.Min(gravitySpeed + 6f * Core.dt, 10f);
			dropVelocity += Vector3.down * gravitySpeed * Core.dt;
			base.transform.position += dropVelocity * Core.dt;
			targetRotation = Quaternion.LookRotation(dropVelocity.normalized);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRotation, rotSpeed * Core.dt);
		}

		public void MoveToTarget()
		{
			base.transform.position += base.transform.forward * speed * Core.dt;
			float num = Mathf.Clamp01(player.GetPracticalVelocity().sqrMagnitude / (player.maxMoveSpeed * player.maxMoveSpeed));
			Vector3 normalized = (targetPos + targetOffset * num * targetOffsetFactor - base.transform.position).normalized;
			targetRotation = Quaternion.LookRotation(normalized);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRotation, rotSpeed * Core.dt);
		}

		public void SetState(CuffMissileState setState)
		{
			stateTimer = 0f;
			deployTimer = 0f;
			targetOffset = Vector3.zero;
			state = setState;
			newTargetOffsetTimer = 0f;
			if (setState != CuffMissileState.ACTIVE && setState != CuffMissileState.DEPLOYED && setState != CuffMissileState.UNGUIDED && owner.activeMissiles.Contains(base.transform))
			{
				owner.activeMissiles.Remove(base.transform);
			}
			switch (state)
			{
			case CuffMissileState.ACTIVE:
				base.transform.parent = null;
				speed = maxActiveSpeed;
				rotSpeed = 0f;
				boostEffect.SetActive(value: true);
				hitbox.enabled = true;
				if ((bool)audioSource)
				{
					Core.Instance.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.cuffMissile, audioSource);
				}
				break;
			case CuffMissileState.UNGUIDED:
				base.transform.parent = null;
				speed = maxActiveSpeed;
				rotSpeed = 0f;
				boostEffect.SetActive(value: true);
				hitbox.enabled = true;
				if ((bool)audioSource)
				{
					Core.Instance.AudioManager.PlaySfxGameplayLooping(SfxCollectionID.CombatSfx, AudioClipID.cuffMissile, audioSource);
				}
				break;
			case CuffMissileState.RETURNING:
				base.transform.parent = null;
				base.transform.rotation = Quaternion.LookRotation((chainOriginTf.position - base.transform.position).normalized);
				rotSpeed = 0f;
				if ((bool)boostEffect)
				{
					boostEffect.SetActive(value: false);
				}
				stateTimer = 0f;
				hitbox.enabled = true;
				DeployCanister();
				if ((bool)audioSource)
				{
					Core.Instance.AudioManager.StopLoopingSfx(audioSource);
				}
				break;
			case CuffMissileState.ATTACHED:
				base.transform.GetChild(0).gameObject.SetActive(value: false);
				if ((bool)audioSource)
				{
					Core.Instance.AudioManager.StopLoopingSfx(audioSource);
					Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.cuffClose, audioSource);
				}
				boostEffect.SetActive(value: false);
				hitbox.enabled = false;
				break;
			case CuffMissileState.DEPLOYED:
				speed = 0f;
				gravitySpeed = 0f;
				rotSpeed = deployedRotateSpeed;
				stateTimer = 0f;
				base.transform.parent = null;
				boostEffect.SetActive(value: false);
				hitbox.enabled = true;
				DeployCanister();
				if ((bool)audioSource)
				{
					Core.Instance.AudioManager.StopLoopingSfx(audioSource);
					Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.deployCanister, audioSource);
				}
				break;
			}
		}

		private void DeployCanister()
		{
			GetPoolableFromPoolOrInstantiate(canisterDeploySmoke).InitPoolable(base.transform.position - base.transform.forward * 0.2f, Quaternion.LookRotation(base.transform.forward));
		}

		public void OnTriggerStay(Collider other)
		{
			if (owner == null)
			{
				return;
			}
			if (other.gameObject.layer == 9)
			{
				Player componentInParent = other.gameObject.GetComponentInParent<Player>();
				if (componentInParent != null && !componentInParent.isAI && state != CuffMissileState.RETURNING)
				{
					Attach(componentInParent);
				}
			}
			else if (other.transform.IsChildOf(owner.transform))
			{
				if (state == CuffMissileState.RETURNING)
				{
					Delete();
				}
			}
			else
			{
				if (other.gameObject.layer == 18)
				{
					return;
				}
				if (other.gameObject.layer == 17)
				{
					if (state != CuffMissileState.RETURNING)
					{
						SetState(CuffMissileState.RETURNING);
					}
				}
				else if ((other.gameObject.layer == 0 || other.gameObject.layer == 21 || other.gameObject.layer == 13 || GroundDetection.LayerIsNonStable(other.gameObject.layer)) && state != CuffMissileState.RETURNING)
				{
					SetState(CuffMissileState.RETURNING);
				}
			}
		}

		public void Delete()
		{
			line.positionCount = 0;
			if ((bool)owner && owner.activeMissiles.Contains(base.transform))
			{
				owner.activeMissiles.Remove(base.transform);
			}
			if (partOfTotalCuffs)
			{
				totalCuffs--;
				partOfTotalCuffs = false;
			}
			Core.OnLateUpdate -= UpdateChain;
			ReturnToPool();
		}

		public void Attach(Player setAttachedToPlayer)
		{
			if (state != CuffMissileState.DEPLOYED)
			{
				SetState(CuffMissileState.DEPLOYED);
			}
			attachedTransform = setAttachedToPlayer.AttachCuff(this);
			if (attachedTransform != null)
			{
				SetState(CuffMissileState.ATTACHED);
			}
			else
			{
				Delete();
			}
		}

		private IPoolable GetPoolableFromPoolOrInstantiate(PoolableObjectToName poolableObject)
		{
			IPoolable poolable = null;
			for (int i = 0; i < poolables.Count; i++)
			{
				if (poolables[i].PoolableName == poolableObject.name && poolables[i].State == PoolableState.Pooled)
				{
					return poolables[i];
				}
			}
			if (poolable == null)
			{
				poolable = Object.Instantiate(poolableObject.obj).GetComponent<IPoolable>();
				poolable.PoolableName = poolableObject.name;
				poolables.Add(poolable);
			}
			return poolable;
		}

		public void OnDestroy()
		{
			if (partOfTotalCuffs)
			{
				totalCuffs--;
				partOfTotalCuffs = false;
			}
			Core.OnLateUpdate -= UpdateChain;
			Core.OnUpdate -= UpdateMissileEnemy;
		}
	}
}
