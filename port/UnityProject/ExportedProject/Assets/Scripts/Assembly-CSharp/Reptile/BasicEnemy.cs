using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Reptile
{
	public class BasicEnemy : MonoBehaviour, IInitializableSceneObject, IPoolable
	{
		public enum BasicEnemyState
		{
			UNACTIVE = 0,
			SPAWNING = 1,
			ACTIVE = 2
		}

		public UnityEvent OnUpdateActive;

		public UnityEvent OnPause;

		public UnityEvent OnUnPause;

		[SerializeField]
		protected int fullHP = 3;

		[SerializeField]
		protected float playerDetectRangeIn = 20f;

		[SerializeField]
		protected float playerDetectRangeOut = 40f;

		[SerializeField]
		protected float shouldBeReplacedDistance = 40f;

		[SerializeField]
		protected EnemyType enemyType;

		[SerializeField]
		protected EnemyPositioningInfo positioningInfo;

		private List<IPoolable> poolables = new List<IPoolable>();

		protected BasicEnemyState basicState;

		protected WorldHandler worldHandlerInstance;

		protected int HP;

		protected Player player;

		internal Battle battle;

		internal Transform tf;

		internal Vector3 velocity;

		private int pause;

		internal float prePauseSpeed;

		internal GraffitiSpotFinisher[] gSpots;

		internal bool receivedAttackPermission;

		internal bool receivedCirclingPermission;

		internal float releaseAttackPermissionTime;

		internal bool requestAttackPermissionRelease;

		internal float releaseCirclingPermissionTime;

		internal bool requestCirclingPermissionRelease;

		public int combatArbiterBasePriority = 1;

		internal int turnsWithoutAttack;

		public int threat = 1;

		internal bool spawnedInEncounter;

		internal bool goingAfterPlayerForCrime;

		protected bool skipSpawnSequence;

		private float checkAutoAimTimer;

		public float spawnableFromStarProgress = 1f;

		protected PoolableState poolableState = PoolableState.Pooled;

		protected bool firstInstantiate = true;

		protected float hitPause;

		protected float basicStateTime;

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

		public BasicEnemyState BasicState => basicState;

		public string PoolableName
		{
			get
			{
				return base.name;
			}
			set
			{
				base.name = value;
			}
		}

		public EnemyType EnemyType => enemyType;

		public Battle Battle => battle;

		public int CurHP => HP;

		protected bool Paused => pause > 0;

		public void InitPoolable(Vector3 position, Quaternion rotation)
		{
			if (poolableState == PoolableState.Pooled)
			{
				base.transform.position = position;
				base.transform.rotation = rotation;
				poolableState = PoolableState.Active;
				base.gameObject.SetActive(value: true);
			}
		}

		public virtual void ReturnToPool()
		{
			base.gameObject.SetActive(value: false);
			poolableState = PoolableState.Pooled;
		}

		public void Awake()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister += OnRegisterToSceneObjectsRegister;
		}

		public virtual void OnDestroy()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister -= OnRegisterToSceneObjectsRegister;
			if (WorldHandler.instance != null)
			{
				WorldHandler.instance.UnsubscribeToCombatArbiter(this);
			}
			worldHandlerInstance = null;
			Core.OnUpdate -= UpdateEnemy;
			Core.OnLateUpdate -= LateUpdateEnemy;
			Core.OnCoreUpdatePaused -= Pause;
			Core.OnCoreUpdateUnPaused -= UnPause;
			if (player != null)
			{
				player.OnRevived -= OnPlayerRevive;
			}
		}

		public void Pause()
		{
			pause++;
			PauseImpl();
		}

		public void UnPause()
		{
			pause--;
			if (!player.IsBusyWithSequence() && hitPause <= 0f && !Paused)
			{
				UnPauseImpl();
			}
		}

		protected virtual void UnPauseImpl()
		{
			OnUnPause.Invoke();
		}

		protected virtual void PauseImpl()
		{
			OnPause.Invoke();
		}

		public virtual void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			sceneObjectsRegister.RegisterSceneObject(this);
		}

		public virtual void InitFromSpawn(Battle setBattle)
		{
			InitFromSpawn(setBattle, setSkipSpawnSequence: false);
		}

		public virtual void InitFromSpawn(Battle setBattle, bool setSkipSpawnSequence)
		{
			if (firstInstantiate)
			{
				Setup();
			}
			Init(setBattle, setSkipSpawnSequence);
		}

		protected virtual void Setup()
		{
			Core.OnUpdate += UpdateEnemy;
			Core.OnLateUpdate += LateUpdateEnemy;
			Core.OnCoreUpdatePaused += Pause;
			Core.OnCoreUpdateUnPaused += UnPause;
			worldHandlerInstance = WorldHandler.instance;
			player = worldHandlerInstance.GetCurrentPlayer();
			player.OnRevived += OnPlayerRevive;
			tf = base.transform;
			positioningInfo.Init(player, this);
			firstInstantiate = false;
		}

		public void InitSceneObject()
		{
			poolableState = PoolableState.Active;
			Setup();
			Init(null, setSkipSpawnSequence: true);
		}

		public virtual void Init(Battle setBattle)
		{
			Init(setBattle, setSkipSpawnSequence: false);
		}

		public virtual void Init(Battle setBattle, bool setSkipSpawnSequence)
		{
			skipSpawnSequence = setSkipSpawnSequence;
			battle = setBattle;
			if (battle != null)
			{
				spawnedInEncounter = true;
			}
			HP = fullHP;
			gSpots = GetComponentsInChildren<GraffitiSpotFinisher>(includeInactive: true);
			worldHandlerInstance.SubscribeToCombatArbiter(this);
			SetBasicState(BasicEnemyState.SPAWNING);
			if (skipSpawnSequence)
			{
				SetBasicState(BasicEnemyState.ACTIVE);
			}
		}

		public virtual void OnPlayerRevive()
		{
			SetBasicState(BasicEnemyState.UNACTIVE);
		}

		public void GrantAttackPermission()
		{
			receivedAttackPermission = true;
			requestAttackPermissionRelease = true;
		}

		public void RetractAttackPermission()
		{
			receivedAttackPermission = false;
		}

		public void GrantCirclingPermission()
		{
			receivedCirclingPermission = true;
			requestCirclingPermissionRelease = true;
		}

		public void RetractCirclingPermission()
		{
			receivedCirclingPermission = false;
		}

		public virtual void ReceiveDamage(int damage)
		{
			HP -= damage;
			if (HP <= 0)
			{
				Die();
			}
		}

		protected virtual void Die()
		{
		}

		public virtual bool IsDead()
		{
			return false;
		}

		public void DoHitPause(float duration, bool enemyOnly = false)
		{
			if (!enemyOnly)
			{
				player.StartHitpause(duration);
			}
			hitPause = duration;
		}

		protected void ReportCrime()
		{
			if (battle == null)
			{
				StartCoroutine("ReportCrimeImpl");
			}
		}

		private IEnumerator ReportCrimeImpl()
		{
			ReportDeath();
			yield return new WaitForSeconds(1f);
			WantedManager.instance.CrimePerpetrated(WantedManager.Crime.OFFICER_DOWN);
		}

		protected void ReportDeath()
		{
			WantedManager.instance.CopDied(enemyType);
		}

		protected void SetBasicState(BasicEnemyState targetState)
		{
			basicState = targetState;
			basicStateTime = 0f;
			switch (basicState)
			{
			case BasicEnemyState.SPAWNING:
				EnterSpawningState();
				break;
			case BasicEnemyState.UNACTIVE:
				EnteringUnactiveState();
				break;
			case BasicEnemyState.ACTIVE:
				EnteringActiveState();
				break;
			}
		}

		protected virtual void EnterSpawningState()
		{
		}

		protected virtual void EnteringActiveState()
		{
		}

		protected virtual void EnteringUnactiveState()
		{
			worldHandlerInstance.UnsubscribeToCombatArbiter(this);
			ReturnToPool();
		}

		public virtual void UpdateEnemy()
		{
			if (hitPause > 0f)
			{
				hitPause -= Core.dt;
			}
			UpdateBasicState();
		}

		private void UpdateBasicState()
		{
			basicStateTime += Core.dt;
			switch (basicState)
			{
			case BasicEnemyState.SPAWNING:
				UpdateSpawning();
				break;
			case BasicEnemyState.ACTIVE:
				UpdateActive();
				break;
			case BasicEnemyState.UNACTIVE:
				UpdateUnactive();
				break;
			}
		}

		public virtual void LateUpdateEnemy()
		{
			LateUpdateBasicState();
		}

		private void LateUpdateBasicState()
		{
			switch (basicState)
			{
			case BasicEnemyState.SPAWNING:
				LateUpdateSpawning();
				break;
			case BasicEnemyState.ACTIVE:
				LateUpdateActive();
				break;
			case BasicEnemyState.UNACTIVE:
				LateUpdateUnactive();
				break;
			}
		}

		protected virtual void UpdateSpawning()
		{
		}

		protected virtual void UpdateActive()
		{
			if (player.IsBusyWithSequence() || hitPause > 0f || Paused)
			{
				PauseImpl();
				return;
			}
			UnPauseImpl();
			OnUpdateActive.Invoke();
			CheckAttackRelease();
		}

		protected virtual void UpdateUnactive()
		{
		}

		protected virtual void LateUpdateSpawning()
		{
		}

		protected virtual void LateUpdateActive()
		{
		}

		protected virtual void LateUpdateUnactive()
		{
		}

		public virtual bool ShouldBeReplaced()
		{
			if (basicState == BasicEnemyState.ACTIVE && !worldHandlerInstance.IsKindaInScreen(tf.position))
			{
				return positioningInfo.SqrDistToPlayer > shouldBeReplacedDistance * shouldBeReplacedDistance;
			}
			return false;
		}

		public virtual bool RequestGoAfterPlayerForCrime()
		{
			goingAfterPlayerForCrime = true;
			return goingAfterPlayerForCrime;
		}

		public virtual void StandDown(bool alsoStandDownFromBattle = false)
		{
			if ((battle == null) | alsoStandDownFromBattle)
			{
				SetBasicState(BasicEnemyState.UNACTIVE);
			}
		}

		protected virtual bool ShouldGoAfterPlayer()
		{
			if (!goingAfterPlayerForCrime)
			{
				return spawnedInEncounter;
			}
			return true;
		}

		public virtual void OnTriggerStay(Collider other)
		{
		}

		public virtual void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("EnemyDeathbox"))
			{
				ReportCrime();
				SetBasicState(BasicEnemyState.UNACTIVE);
			}
		}

		protected virtual void RequestAttack(float attackDuration)
		{
			worldHandlerInstance.RequestAttack(this);
			ScheduleAttackRelease(attackDuration);
		}

		protected virtual void ReleaseAttack()
		{
			releaseAttackPermissionTime = 0f;
			requestAttackPermissionRelease = true;
		}

		protected virtual void ScheduleAttackRelease(float attackDuration)
		{
			releaseAttackPermissionTime = attackDuration;
			requestAttackPermissionRelease = true;
		}

		protected virtual void RequestCircling(float circlingDuration)
		{
			worldHandlerInstance.RequestCirclingPosition(this);
			ScheduleCirclingRelease(circlingDuration);
		}

		protected virtual void ScheduleCirclingRelease(float circlingDuration)
		{
			releaseCirclingPermissionTime = circlingDuration;
			requestCirclingPermissionRelease = true;
		}

		protected virtual void CheckAttackRelease()
		{
			releaseAttackPermissionTime -= Core.dt;
			if (releaseAttackPermissionTime <= 0f && requestAttackPermissionRelease)
			{
				requestAttackPermissionRelease = false;
				worldHandlerInstance.ReleaseAttack(this);
			}
		}

		protected virtual void CheckCirclingRelease(bool shouldReleaseImmediately)
		{
			if (shouldReleaseImmediately)
			{
				releaseCirclingPermissionTime = 0f;
				requestCirclingPermissionRelease = true;
			}
			releaseCirclingPermissionTime -= Core.dt;
			if (releaseCirclingPermissionTime <= 0f && requestCirclingPermissionRelease)
			{
				requestCirclingPermissionRelease = false;
				worldHandlerInstance.ReleaseCirclingPosition(this);
			}
		}

		protected IPoolable GetPoolableFromPoolOrInstantiate(PoolableObjectToName poolableObject)
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

		public void CheckAutoAim()
		{
			if (HP > 0)
			{
				checkAutoAimTimer += Core.dt;
				if (checkAutoAimTimer > 0.1f || player.closestEnemy == null)
				{
					player.DetectEnemyForAutoAim(tf);
					checkAutoAimTimer = 0f;
				}
			}
		}
	}
}
