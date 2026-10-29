using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Reptile
{
	public class Battle : MonoBehaviour, IInitializableSceneObject
	{
		public enum BattleState
		{
			INACTIVE = 0,
			INTRO = 1,
			MAIN = 2,
			FAIL = 3,
			SUCCESS = 4,
			OUTRO_FAIL = 5,
			OUTRO_SUCCESS = 6
		}

		[Serializable]
		public class EnemySpawnSet
		{
			public bool requiredForCompletion = true;

			public Transform spawnHolder;

			public BasicEnemy enemyType;
		}

		public class SpawnedEnemy
		{
			public EnemySpawnSet spawnSet;

			public bool requiredForCompletion;

			public BasicEnemy enemy;
		}

		public enum SpawnCondition
		{
			AT_START = 0,
			ON_SPAWNSET_DEFEATED = 1
		}

		[SerializeField]
		private EnemySpawnSet[] enemySpawnSets;

		[SerializeField]
		private int maxAttackers = 2;

		[SerializeField]
		private int maxThreat = 8;

		[SerializeField]
		private Transform[] addSpawnerGroups;

		[SerializeField]
		private PlayableDirector introSequence;

		[SerializeField]
		private PlayableDirector outroSuccesSequence;

		public bool ClearEnemiesOnComplete = true;

		public Transform startSpawner;

		public Transform completeSpawner;

		public UnityEvent OnCompleted;

		public UnityEvent OnFailed;

		[Header("the moment the intro starts")]
		public UnityEvent OnIntro;

		[Header("when the intro is over, the moment main event is started")]
		public UnityEvent OnStart;

		[Header("the moment the outro starts, both win and lose")]
		public UnityEvent OnOutro;

		private List<SpawnedEnemy> spawnedEnemies = new List<SpawnedEnemy>();

		private BattleState currentState;

		private float stateTimer;

		public int MaxAttackers => maxAttackers;

		public int MaxThreat => maxThreat;

		public void Awake()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister += OnRegisterToSceneObjectsRegister;
		}

		public virtual void OnDestroy()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister -= OnRegisterToSceneObjectsRegister;
			Core.OnUpdate -= UpdateBattle;
		}

		public virtual void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			sceneObjectsRegister.RegisterSceneObject(this);
		}

		public void InitSceneObject()
		{
			Core.OnUpdate += UpdateBattle;
		}

		public void ActivateBattle()
		{
			foreach (Transform item in base.transform)
			{
				if (item.gameObject.layer == 19)
				{
					item.gameObject.SetActive(value: false);
				}
			}
			SetState(BattleState.INTRO);
		}

		private void SetState(BattleState state)
		{
			currentState = state;
			stateTimer = 0f;
			switch (currentState)
			{
			case BattleState.INTRO:
				WorldHandler.instance.SetBattleValues(maxAttackers, maxThreat);
				if (introSequence != null)
				{
					SequenceHandler.instance.StartEnteringSequence(introSequence, null, setHidePlayer: true, setInterruptPlayer: true, instantly: false, setPausePlayer: true, !(WorldHandler.instance.currentEncounter != null) || WorldHandler.instance.currentEncounter.allowPhone, null, skippable: true, lowerVolumeDuringSequence: true, this);
				}
				else
				{
					SetState(BattleState.MAIN);
				}
				break;
			case BattleState.MAIN:
				OnStart.Invoke();
				StartBattle();
				ActivateSpawnGroups();
				break;
			case BattleState.OUTRO_FAIL:
				WorldHandler.instance.ResetbattleValues();
				if (ClearEnemiesOnComplete)
				{
					DestroyAllEnemies();
				}
				DeactivateSpawnGroups();
				SetState(BattleState.INACTIVE);
				break;
			case BattleState.OUTRO_SUCCESS:
				WorldHandler.instance.ResetbattleValues();
				if (ClearEnemiesOnComplete)
				{
					DestroyAllEnemies();
				}
				DeactivateSpawnGroups();
				if (outroSuccesSequence != null)
				{
					SequenceHandler.instance.StartEnteringSequence(outroSuccesSequence, null, setHidePlayer: true, setInterruptPlayer: true, instantly: false, setPausePlayer: true, WorldHandler.instance.currentEncounter.allowPhone, null, skippable: true, lowerVolumeDuringSequence: true, this);
					break;
				}
				Complete();
				SetState(BattleState.INACTIVE);
				break;
			case BattleState.FAIL:
			case BattleState.SUCCESS:
				break;
			}
		}

		public void UpdateBattle()
		{
			stateTimer += Core.dt;
			switch (currentState)
			{
			case BattleState.MAIN:
			{
				int num = 0;
				for (int i = 0; i < spawnedEnemies.Count; i++)
				{
					if (spawnedEnemies[i].enemy != null && !spawnedEnemies[i].enemy.IsDead() && spawnedEnemies[i].enemy.BasicState != BasicEnemy.BasicEnemyState.UNACTIVE && spawnedEnemies[i].requiredForCompletion)
					{
						num++;
					}
				}
				if (num == 0)
				{
					SetState(BattleState.SUCCESS);
				}
				else if (WorldHandler.instance.GetCurrentPlayer().ability == WorldHandler.instance.GetCurrentPlayer().dieAbility)
				{
					SetState(BattleState.FAIL);
				}
				break;
			}
			case BattleState.FAIL:
				if (stateTimer > 0f)
				{
					SetState(BattleState.OUTRO_FAIL);
				}
				break;
			case BattleState.SUCCESS:
				if (stateTimer > 0f)
				{
					SetState(BattleState.OUTRO_SUCCESS);
				}
				break;
			case BattleState.INACTIVE:
			case BattleState.INTRO:
			case BattleState.OUTRO_FAIL:
			case BattleState.OUTRO_SUCCESS:
				break;
			}
		}

		private void ActivateSpawnGroups()
		{
			for (int i = 0; i < addSpawnerGroups.Length; i++)
			{
				if (addSpawnerGroups[i] != null)
				{
					addSpawnerGroups[i].gameObject.SetActive(value: true);
				}
			}
		}

		private void DeactivateSpawnGroups()
		{
			for (int i = 0; i < addSpawnerGroups.Length; i++)
			{
				if (addSpawnerGroups[i] != null)
				{
					addSpawnerGroups[i].gameObject.SetActive(value: false);
				}
			}
		}

		private void Complete()
		{
			SpawnOnComplete();
			OnCompleted.Invoke();
		}

		public void DestroyAllEnemies()
		{
			for (int i = 0; i < spawnedEnemies.Count; i++)
			{
				if (spawnedEnemies[i].enemy != null)
				{
					WorldHandler.instance.UnsubscribeToCombatArbiter(spawnedEnemies[i].enemy);
					spawnedEnemies[i].enemy.StandDown(alsoStandDownFromBattle: true);
					UnityEngine.Object.Destroy(spawnedEnemies[i].enemy.gameObject);
				}
			}
			spawnedEnemies.Clear();
		}

		public void TriggerDetectPlayer(Collider trigger, Player player)
		{
			if (ClearEnemiesOnComplete)
			{
				DestroyAllEnemies();
			}
			ActivateBattle();
		}

		public void ResetEncounter()
		{
			foreach (Transform item in base.transform)
			{
				if (item.gameObject.layer == 19)
				{
					item.gameObject.SetActive(value: true);
				}
			}
		}

		private void StartBattle()
		{
			SpawnAtStart();
			for (int i = 0; i < enemySpawnSets.Length; i++)
			{
				foreach (Transform item in enemySpawnSets[i].spawnHolder)
				{
					SpawnedEnemy spawnedEnemy = new SpawnedEnemy();
					spawnedEnemy.requiredForCompletion = enemySpawnSets[i].requiredForCompletion;
					spawnedEnemy.spawnSet = enemySpawnSets[i];
					spawnedEnemy.enemy = UnityEngine.Object.Instantiate(enemySpawnSets[i].enemyType, item.position, item.rotation).GetComponent<BasicEnemy>();
					spawnedEnemies.Add(spawnedEnemy);
					spawnedEnemy.enemy.InitFromSpawn(this, setSkipSpawnSequence: true);
					spawnedEnemy.enemy.tf.parent = base.transform;
				}
			}
		}

		public void EnterSequence(PlayableDirector sequence)
		{
			if (sequence == introSequence)
			{
				OnIntro.Invoke();
			}
			else if (sequence == outroSuccesSequence)
			{
				OnOutro.Invoke();
			}
		}

		public void ExitSequence(PlayableDirector sequence)
		{
			if (sequence == introSequence)
			{
				SetState(BattleState.MAIN);
				SequenceHandler.instance.LetPlayerExitSequence();
			}
			else if (sequence == outroSuccesSequence)
			{
				SetState(BattleState.INACTIVE);
				SequenceHandler.instance.LetPlayerExitSequence();
				Complete();
			}
		}

		public void SpawnAtStart()
		{
			if ((bool)startSpawner)
			{
				WorldHandler.instance.PlaceCurrentPlayerAt(startSpawner);
			}
		}

		public void SpawnOnComplete()
		{
			if ((bool)completeSpawner)
			{
				WorldHandler.instance.PlaceCurrentPlayerAt(completeSpawner);
			}
		}

		public void AddEnemy(BasicEnemy enemy, bool requiredForCompletion)
		{
			SpawnedEnemy spawnedEnemy = new SpawnedEnemy();
			spawnedEnemy.requiredForCompletion = requiredForCompletion;
			spawnedEnemy.enemy = enemy;
			spawnedEnemies.Add(spawnedEnemy);
		}

		public void SpawnAddsFromGroup(int group, bool requiredForCompletion, int maxAdds = int.MaxValue)
		{
			List<EnemyType> list = new List<EnemyType>();
			BasicEnemySpawner[] componentsInChildren = addSpawnerGroups[group].GetComponentsInChildren<BasicEnemySpawner>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!list.Contains(componentsInChildren[i].SpawnableEnemyTypes[0]))
				{
					list.Add(componentsInChildren[i].SpawnableEnemyTypes[0]);
				}
			}
			int num = 0;
			for (int j = 0; j < spawnedEnemies.Count; j++)
			{
				if (spawnedEnemies[j].enemy != null && !spawnedEnemies[j].enemy.IsDead() && list.Contains(spawnedEnemies[j].enemy.EnemyType))
				{
					num++;
				}
			}
			Vector3 position = WorldHandler.instance.GetCurrentPlayer().transform.position;
			for (int k = 0; k < Mathf.Min(maxAdds - num, componentsInChildren.Length); k++)
			{
				if (componentsInChildren[k].SpawnEnemy(componentsInChildren[k].SpawnableEnemyTypes[0], position, forceSpawn: true))
				{
					componentsInChildren[k].SpawnedEnemy.battle = this;
					AddEnemy(componentsInChildren[k].SpawnedEnemy, requiredForCompletion);
				}
			}
		}

		public void ClearAddsFromGroup(int group)
		{
			BasicEnemySpawner[] componentsInChildren = addSpawnerGroups[group].GetComponentsInChildren<BasicEnemySpawner>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].SpawnedEnemy != null)
				{
					componentsInChildren[i].SpawnedEnemy.StandDown(alsoStandDownFromBattle: true);
					UnityEngine.Object.Destroy(componentsInChildren[i].SpawnedEnemy.gameObject);
				}
			}
		}

		public int SpawnedEmeiesOfType(EnemyType targetType)
		{
			int num = 0;
			if (spawnedEnemies.Count > 0)
			{
				for (int i = 0; i < spawnedEnemies.Count; i++)
				{
					if (spawnedEnemies[i].enemy != null && spawnedEnemies[i].enemy.EnemyType == targetType && !spawnedEnemies[i].enemy.IsDead() && spawnedEnemies[i].enemy.BasicState != BasicEnemy.BasicEnemyState.UNACTIVE)
					{
						num++;
					}
				}
			}
			return num;
		}

		public EnemyType EnemyTypeOfSpawnGroup(int spawnGroup)
		{
			return addSpawnerGroups[spawnGroup].GetComponentsInChildren<BasicEnemySpawner>()[0].SpawnableEnemyTypes[0];
		}
	}
}
