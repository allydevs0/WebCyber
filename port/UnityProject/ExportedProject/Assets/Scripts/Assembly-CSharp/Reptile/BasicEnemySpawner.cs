using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class BasicEnemySpawner : MonoBehaviour, IInitializableSceneObject
	{
		[SerializeField]
		protected GameObject[] spawnableEnemies;

		[SerializeField]
		protected PoolableInstantiator poolableInstantiator;

		[SerializeField]
		protected bool spawnOutOfSight;

		[SerializeField]
		protected float spawnTimeOutDuration;

		[SerializeField]
		protected float spawnRange;

		[SerializeField]
		protected float maxVerticalSpawnRange;

		protected List<PoolableObjectToName> enemies = new List<PoolableObjectToName>();

		protected BasicEnemy spawnedEnemy;

		protected Vector3 position;

		protected WorldHandler worldhandlerInstance;

		protected Player player;

		protected Transform playerTf;

		protected float spawnTimeOutTimer;

		protected EnemyType[] spawnableEnemyTypes;

		protected List<IPoolable> poolables = new List<IPoolable>();

		public Transform tf;

		public EnemyType[] SpawnableEnemyTypes => spawnableEnemyTypes;

		public BasicEnemy SpawnedEnemy => spawnedEnemy;

		public void Awake()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister += OnRegisterToSceneObjectsRegister;
			tf = base.transform;
		}

		public void OnDestroy()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister -= OnRegisterToSceneObjectsRegister;
			Core.OnUpdate -= UpdateSpawner;
			Core.OnCoreUpdatePaused -= OnCoreUpdatePaused;
			Core.OnCoreUpdateUnPaused -= OnCoreUpdateUnPaused;
			if (player != null)
			{
				player.OnRevived -= OnPlayerRevive;
			}
		}

		public void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			sceneObjectsRegister.RegisterSceneObject(this);
		}

		public virtual void InitSceneObject()
		{
			Core.OnUpdate += UpdateSpawner;
			Core.OnCoreUpdatePaused += OnCoreUpdatePaused;
			Core.OnCoreUpdateUnPaused += OnCoreUpdateUnPaused;
			worldhandlerInstance = WorldHandler.instance;
			player = worldhandlerInstance.GetCurrentPlayer();
			player.OnRevived += OnPlayerRevive;
			playerTf = player.tf;
			position = base.transform.position;
			for (int i = 0; i < spawnableEnemies.Length; i++)
			{
				enemies.Add(new PoolableObjectToName(spawnableEnemies[i], spawnableEnemies[i].name));
			}
			spawnableEnemyTypes = new EnemyType[spawnableEnemies.Length];
			for (int j = 0; j < spawnableEnemies.Length; j++)
			{
				spawnableEnemyTypes[j] = spawnableEnemies[j].GetComponent<BasicEnemy>().EnemyType;
			}
		}

		protected virtual void UpdateSpawner()
		{
			if (spawnTimeOutTimer > 0f && (spawnedEnemy == null || spawnedEnemy.BasicState == BasicEnemy.BasicEnemyState.UNACTIVE))
			{
				spawnTimeOutTimer -= Core.dt;
			}
		}

		protected virtual void OnCoreUpdatePaused()
		{
		}

		protected virtual void OnCoreUpdateUnPaused()
		{
		}

		public void OnPlayerRevive()
		{
			spawnedEnemy = null;
			spawnTimeOutTimer = 0f;
		}

		public virtual bool CanSpawnEnemy(EnemyType targetEnemyType, Vector3 playerPosition)
		{
			return BaseCanSpawnEnemy(targetEnemyType, playerPosition);
		}

		protected bool BaseCanSpawnEnemy(EnemyType targetEnemyType, Vector3 playerPosition)
		{
			if (spawnTimeOutTimer > 0f)
			{
				return false;
			}
			Vector3 vector = playerPosition - position;
			if (vector.sqrMagnitude < spawnRange * 0.25f * (spawnRange * 0.25f))
			{
				return false;
			}
			if (vector.sqrMagnitude > spawnRange * spawnRange)
			{
				return false;
			}
			if (Mathf.Abs(vector.y) > maxVerticalSpawnRange)
			{
				return false;
			}
			if (spawnOutOfSight && worldhandlerInstance.IsReallyInScreen(position))
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < spawnableEnemyTypes.Length; i++)
			{
				if (spawnableEnemyTypes[i] == targetEnemyType)
				{
					flag = true;
					if (spawnedEnemy == null || spawnedEnemy.BasicState == BasicEnemy.BasicEnemyState.UNACTIVE)
					{
						flag2 = true;
					}
				}
			}
			return flag & flag2;
		}

		public virtual bool SpawnEnemy(EnemyType targetEnemyType, Vector3 playerPosition, bool forceSpawn = false)
		{
			if (CanSpawnEnemy(targetEnemyType, playerPosition) | forceSpawn)
			{
				for (int i = 0; i < spawnableEnemies.Length; i++)
				{
					if (spawnableEnemies[i].GetComponent<BasicEnemy>().EnemyType != targetEnemyType)
					{
						continue;
					}
					PoolableObjectToName poolableObjectToName = null;
					for (int j = 0; j < enemies.Count; j++)
					{
						if (enemies[j].name == spawnableEnemies[i].name)
						{
							poolableObjectToName = enemies[j];
							break;
						}
					}
					if (poolableObjectToName != null)
					{
						IPoolable poolableFromPoolOrInstantiate = poolableInstantiator.GetPoolableFromPoolOrInstantiate(poolableObjectToName);
						poolableFromPoolOrInstantiate.InitPoolable(base.transform.position, base.transform.rotation);
						spawnedEnemy = poolableFromPoolOrInstantiate as BasicEnemy;
						spawnedEnemy.InitFromSpawn(null);
						spawnTimeOutTimer = spawnTimeOutDuration;
						return true;
					}
				}
			}
			return false;
		}
	}
}
