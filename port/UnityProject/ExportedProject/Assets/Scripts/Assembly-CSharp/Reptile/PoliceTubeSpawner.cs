using UnityEngine;

namespace Reptile
{
	public class PoliceTubeSpawner : BasicEnemySpawner
	{
		public enum TubeState
		{
			CONCEALED = 0,
			DOWN = 1,
			UP = 2,
			UPTORETURN = 3
		}

		[SerializeField]
		private Animator animator;

		[SerializeField]
		private Transform spawnTransform;

		[SerializeField]
		private Transform mainTube;

		[SerializeField]
		private Transform tubeTop;

		[SerializeField]
		private Transform posA;

		[SerializeField]
		private Transform posB;

		[SerializeField]
		private Transform posC;

		[SerializeField]
		private Transform posD;

		[SerializeField]
		private AudioSource audioSource;

		private TubeState state = TubeState.DOWN;

		private float stateTime;

		private bool readyToMoveUp;

		private bool claimedForReturn;

		private Quaternion mainTubeOriginalRot;

		private Quaternion tubeTopOriginalRot;

		private WantedManager wantedManager;

		private Transform returnTransform;

		public TubeState State => state;

		public float TubeStateTime => stateTime;

		public bool ClaimedForReturn => claimedForReturn;

		public override void InitSceneObject()
		{
			base.InitSceneObject();
			wantedManager = WantedManager.instance;
		}

		private void OnEnable()
		{
			mainTubeOriginalRot = mainTube.rotation;
			tubeTopOriginalRot = tubeTop.rotation;
		}

		private void SetState(TubeState newState)
		{
			TubeState tubeState = state;
			state = newState;
			stateTime = 0f;
			switch (state)
			{
			case TubeState.CONCEALED:
				animator.Play("policeTubeClose");
				readyToMoveUp = false;
				break;
			case TubeState.DOWN:
				readyToMoveUp = false;
				if (tubeState == TubeState.UP || tubeState == TubeState.UPTORETURN)
				{
					animator.Play("policeTubeMoveDown");
					Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.PoliceTubeMoveDown, audioSource, 0.05f);
				}
				if (tubeState == TubeState.CONCEALED)
				{
					animator.Play("policeTubeOpen");
				}
				break;
			case TubeState.UP:
			{
				animator.Play("policeTubeMoveUp");
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.CombatSfx, AudioClipID.PoliceTubeMoveUp, audioSource, 0.05f);
				float sqrMagnitude = (worldhandlerInstance.GetCurrentPlayer().tf.position - posA.position).sqrMagnitude;
				float sqrMagnitude2 = (worldhandlerInstance.GetCurrentPlayer().tf.position - posB.position).sqrMagnitude;
				float sqrMagnitude3 = (worldhandlerInstance.GetCurrentPlayer().tf.position - posC.position).sqrMagnitude;
				float sqrMagnitude4 = (worldhandlerInstance.GetCurrentPlayer().tf.position - posD.position).sqrMagnitude;
				int num2 = -1;
				switch ((!(sqrMagnitude <= sqrMagnitude2) || !(sqrMagnitude <= sqrMagnitude3) || !(sqrMagnitude <= sqrMagnitude4)) ? ((sqrMagnitude2 <= sqrMagnitude && sqrMagnitude2 <= sqrMagnitude3 && sqrMagnitude2 <= sqrMagnitude4) ? 1 : ((!(sqrMagnitude3 <= sqrMagnitude) || !(sqrMagnitude3 <= sqrMagnitude2) || !(sqrMagnitude3 <= sqrMagnitude4)) ? 3 : 2)) : 0)
				{
				case 0:
					mainTube.rotation = mainTubeOriginalRot;
					tubeTop.rotation = tubeTopOriginalRot;
					break;
				case 1:
					mainTube.rotation = mainTubeOriginalRot;
					mainTube.Rotate(0f, 90f, 0f);
					tubeTop.rotation = tubeTopOriginalRot;
					break;
				case 2:
					mainTube.rotation = mainTubeOriginalRot;
					mainTube.Rotate(0f, 180f, 0f);
					tubeTop.rotation = tubeTopOriginalRot;
					break;
				case 3:
					mainTube.rotation = mainTubeOriginalRot;
					mainTube.Rotate(0f, -90f, 0f);
					tubeTop.rotation = tubeTopOriginalRot;
					break;
				}
				spawnedEnemy.transform.parent = spawnTransform;
				spawnedEnemy.transform.localPosition = Vector3.zero;
				spawnedEnemy.transform.localRotation = Quaternion.identity;
				break;
			}
			case TubeState.UPTORETURN:
			{
				int num = -1;
				switch ((!(returnTransform == posA)) ? ((returnTransform == posB) ? 1 : ((!(returnTransform == posC)) ? 3 : 2)) : 0)
				{
				case 0:
					mainTube.rotation = mainTubeOriginalRot;
					tubeTop.rotation = tubeTopOriginalRot;
					break;
				case 1:
					mainTube.rotation = mainTubeOriginalRot;
					mainTube.Rotate(0f, 90f, 0f);
					tubeTop.rotation = tubeTopOriginalRot;
					break;
				case 2:
					mainTube.rotation = mainTubeOriginalRot;
					mainTube.Rotate(0f, 180f, 0f);
					tubeTop.rotation = tubeTopOriginalRot;
					break;
				case 3:
					mainTube.rotation = mainTubeOriginalRot;
					mainTube.Rotate(0f, -90f, 0f);
					tubeTop.rotation = tubeTopOriginalRot;
					break;
				}
				animator.Play("policeTubeMoveUp");
				break;
			}
			}
		}

		protected override void UpdateSpawner()
		{
			base.UpdateSpawner();
			if (!wantedManager.Wanted && worldhandlerInstance.subscribedEnemies.Count == 0 && state == TubeState.CONCEALED)
			{
				return;
			}
			stateTime += Core.dt;
			switch (state)
			{
			case TubeState.CONCEALED:
				if (spawnTimeOutTimer <= 0f && (spawnedEnemy == null || spawnedEnemy.BasicState == BasicEnemy.BasicEnemyState.UNACTIVE) && WantedManager.instance.Wanted)
				{
					SetState(TubeState.DOWN);
				}
				break;
			case TubeState.DOWN:
				if (stateTime > 2f)
				{
					if ((spawnedEnemy != null && spawnedEnemy.BasicState != BasicEnemy.BasicEnemyState.UNACTIVE) || spawnTimeOutTimer > 0f || !WantedManager.instance.Wanted)
					{
						SetState(TubeState.CONCEALED);
					}
					else
					{
						readyToMoveUp = true;
					}
				}
				break;
			case TubeState.UP:
				if (stateTime > 2f && spawnedEnemy != null && spawnedEnemy.transform.parent == spawnTransform)
				{
					spawnedEnemy.transform.parent = null;
				}
				if (stateTime > 3f)
				{
					SetState(TubeState.DOWN);
				}
				break;
			case TubeState.UPTORETURN:
				break;
			}
		}

		public override bool CanSpawnEnemy(EnemyType targetEnemyType, Vector3 playerPosition)
		{
			if (!readyToMoveUp && !claimedForReturn)
			{
				return false;
			}
			return BaseCanSpawnEnemy(targetEnemyType, playerPosition);
		}

		public override bool SpawnEnemy(EnemyType targetEnemyType, Vector3 playerPosition, bool forceSpawn = false)
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
						poolableFromPoolOrInstantiate.InitPoolable(spawnTransform.position, spawnTransform.rotation);
						spawnedEnemy = poolableFromPoolOrInstantiate as BasicEnemy;
						spawnedEnemy.InitFromSpawn(null);
						spawnTimeOutTimer = spawnTimeOutDuration;
						SetState(TubeState.UP);
						return true;
					}
				}
			}
			return false;
		}

		public Vector3 ClaimForReturn(Vector3 position)
		{
			claimedForReturn = true;
			float sqrMagnitude = (position - posA.position).sqrMagnitude;
			float sqrMagnitude2 = (position - posB.position).sqrMagnitude;
			float sqrMagnitude3 = (position - posC.position).sqrMagnitude;
			float sqrMagnitude4 = (position - posD.position).sqrMagnitude;
			int num = -1;
			switch ((!(sqrMagnitude <= sqrMagnitude2) || !(sqrMagnitude <= sqrMagnitude3) || !(sqrMagnitude <= sqrMagnitude4)) ? ((sqrMagnitude2 <= sqrMagnitude && sqrMagnitude2 <= sqrMagnitude3 && sqrMagnitude2 <= sqrMagnitude4) ? 1 : ((!(sqrMagnitude3 <= sqrMagnitude) || !(sqrMagnitude3 <= sqrMagnitude2) || !(sqrMagnitude3 <= sqrMagnitude4)) ? 3 : 2)) : 0)
			{
			case 0:
				returnTransform = posA;
				return posA.position;
			case 1:
				returnTransform = posB;
				return posB.position;
			case 2:
				returnTransform = posC;
				return posC.position;
			case 3:
				returnTransform = posD;
				return posD.position;
			default:
				return posA.position;
			}
		}

		public void ComeUpForReturn()
		{
			if (state != TubeState.UPTORETURN)
			{
				SetState(TubeState.UPTORETURN);
			}
		}

		public void ReadyToGoDown()
		{
			if (state == TubeState.UPTORETURN)
			{
				SetState(TubeState.DOWN);
			}
		}

		public void UnclaimForReturn()
		{
			claimedForReturn = false;
			if (state == TubeState.UPTORETURN)
			{
				SetState(TubeState.DOWN);
			}
		}

		public Transform GetSpawnTransform()
		{
			return spawnTransform;
		}
	}
}
