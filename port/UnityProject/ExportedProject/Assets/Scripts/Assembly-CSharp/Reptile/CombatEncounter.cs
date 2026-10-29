using System;
using UnityEngine;

namespace Reptile
{
	public class CombatEncounter : Encounter
	{
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
		private Battle battle;

		[SerializeField]
		private bool setWantedToMaxDuringBattle = true;

		[SerializeField]
		private bool instantOutro;

		public bool autoWinAfterFiveMinutes;

		public override void InitSceneObject()
		{
			base.InitSceneObject();
			if (instantOutro)
			{
				outroDelay = 0f;
			}
		}

		public override void ActivateEncounter()
		{
			foreach (Transform item in base.transform)
			{
				if (item.gameObject.layer == 19)
				{
					item.gameObject.SetActive(value: false);
				}
			}
			base.ActivateEncounter();
		}

		public override void EnterEncounterState(EncounterState setState)
		{
			if (setState == EncounterState.OUTRO_FAIL || setState == EncounterState.OUTRO_SUCCES)
			{
				battle.DestroyAllEnemies();
			}
			base.EnterEncounterState(setState);
		}

		public override void TriggerDetectPlayer(Collider trigger, Player player)
		{
			if (state == EncounterState.OPEN)
			{
				ActivateEncounter();
			}
		}

		public override void ResetEncounter()
		{
			foreach (Transform item in base.transform)
			{
				if (item.gameObject.layer == 19)
				{
					item.gameObject.SetActive(value: true);
				}
			}
			base.ResetEncounter();
		}

		public override void StartMainEvent()
		{
			Core.Instance.AudioManager.StartCustomTrack(customMusicTrack, ref customMusicTrackInstance);
			base.StartMainEvent();
			if (battle != null)
			{
				if (setWantedToMaxDuringBattle)
				{
					WorldHandler.instance.GetCurrentPlayer().ShowWantedUI(shouldShow: true);
					WorldHandler.instance.GetCurrentPlayer().ChangeWantedStars(Mathf.FloorToInt(Core.Instance.SaveManager.CurrentSaveSlot.maxWantedStars));
				}
				battle.ActivateBattle();
				battle.OnCompleted.AddListener(BattleComplete);
				battle.OnFailed.AddListener(BattleFail);
				battle.ClearEnemiesOnComplete = false;
			}
			else
			{
				BattleComplete();
			}
		}

		public override void UpdateMainEvent()
		{
			base.UpdateMainEvent();
			if (autoWinAfterFiveMinutes && stateTimer > 300f)
			{
				CheatComplete();
			}
		}

		public void BattleFail()
		{
			SetEncounterState(EncounterState.MAIN_EVENT_FAILED_DECAY);
		}

		public void BattleComplete()
		{
			WorldHandler.instance.GetCurrentPlayer().ShowWantedUI(shouldShow: false);
			SetEncounterState(EncounterState.MAIN_EVENT_SUCCES_DECAY);
		}

		public override void CheatComplete()
		{
			battle.DestroyAllEnemies();
		}
	}
}
