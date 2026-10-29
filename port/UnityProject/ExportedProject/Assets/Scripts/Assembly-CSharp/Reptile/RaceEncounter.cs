using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class RaceEncounter : Encounter
	{
		private bool setGraffitiOnCompletedEncounter;

		private List<Collider> checkpointTriggersHit = new List<Collider>();

		public bool mustHitAllCheckpoints;

		public GraffitiSpotFinisher graffitiSpot;

		public int defaultBoostChargesForAI;

		public bool playerStartOnMovestyle = true;

		public Racer[] racers;

		public override void InitSceneObject()
		{
			graffitiSpot.encounter = this;
			graffitiSpot.gameObject.SetActive(value: false);
			base.InitSceneObject();
		}

		public override void StartMainEvent()
		{
			checkpointTriggersHit.Clear();
			base.StartMainEvent();
		}

		public override void UpdateMainEvent()
		{
			base.UpdateMainEvent();
		}

		public override void UpdateGameplayEvent()
		{
			base.UpdateGameplayEvent();
			if (state == EncounterState.CLOSED && graffitiSpot.raceEncounterGraffitiThatShouldRemainVisibleAfterwards && !setGraffitiOnCompletedEncounter && CompletedEncounter())
			{
				if (!graffitiSpot.ClaimedByPlayableCrew())
				{
					graffitiSpot.SetDefaultData(null);
					graffitiSpot.SetToPlayerGraffiti();
				}
				setGraffitiOnCompletedEncounter = true;
			}
		}

		public override void EnterEncounterState(EncounterState setState)
		{
			if (setState == EncounterState.INTRO)
			{
				graffitiSpot.StartCoroutine(graffitiSpot.WaitForGraffitiProgressBeforeResettinAndOpening());
				player.SwitchToEquippedMovestyle(playerStartOnMovestyle, doAirTrick: false, changeProp: true, showEffect: false);
				EnterEncounterStateContestants();
			}
			if (setState == EncounterState.MAIN_EVENT)
			{
				for (int i = 0; i < racers.Length; i++)
				{
					racers[i].player.AI.StartFirstPath();
					racers[i].player.userInputEnabled = true;
				}
			}
			if (setState == EncounterState.CLOSED)
			{
				DestroyEnemyRacers();
			}
			base.EnterEncounterState(setState);
		}

		private void EnterEncounterStateContestants()
		{
			for (int i = 0; i < racers.Length; i++)
			{
				racers[i].player = WorldHandler.instance.SetupAIPlayerAt(racers[i].spawner, racers[i].character, PlayerType.RACER_AI, racers[i].outfit, racers[i].moveStyle, racers[i].crew);
				racers[i].player.userInputEnabled = false;
				racers[i].player.SwitchToEquippedMovestyle(racers[i].moveStyle == MoveStyle.ON_FOOT, doAirTrick: false, changeProp: true, showEffect: false);
				racers[i].player.ActivateAbility(racers[i].player.switchMoveStyleAbility);
				PlayerAIPath pathFromPathID = WorldHandler.instance.GetPathFromPathID(racers[i].pathID);
				if (pathFromPathID != null)
				{
					racers[i].player.AI.SetPath(pathFromPathID);
				}
				else
				{
					racers[i].player.AI.SetPath(racers[i].path);
				}
				racers[i].player.AddBoostCharge((float)defaultBoostChargesForAI * Pickup.boostChargeValue);
			}
		}

		public override void ResetEncounter()
		{
			graffitiSpot.ClearPaint();
			graffitiSpot.SetState(GraffitiState.NONE);
			graffitiSpot.gameObject.SetActive(value: false);
		}

		public override void ActivateEncounter()
		{
			currentCheckpoint = -1;
			DestroyEnemyRacers();
			graffitiSpot.ClearPaint();
			graffitiSpot.gameObject.SetActive(value: true);
			base.ActivateEncounter();
		}

		public override void TriggerDetectPlayer(Collider trigger, Player player)
		{
			if (state == EncounterState.OPEN)
			{
				ActivateEncounter();
			}
		}

		public void DestroyEnemyRacers()
		{
			for (int i = 0; i < racers.Length; i++)
			{
				if (racers[i].player != null)
				{
					WorldHandler.instance.RemovePlayer(racers[i].player);
					Object.Destroy(racers[i].player.gameObject);
				}
			}
		}

		public override void CheckpointReached(Collider trigger)
		{
			if (checkpoints != null && checkpoints.Length != 0)
			{
				if (!checkpointTriggersHit.Contains(trigger))
				{
					checkpointTriggersHit.Add(trigger);
				}
				base.CheckpointReached(trigger);
			}
		}

		public override void GraffitiDoneSignal(bool win)
		{
			if (win && (!mustHitAllCheckpoints || checkpoints == null || checkpointTriggersHit.Count == checkpoints.Length))
			{
				SetEncounterState(EncounterState.MAIN_EVENT_SUCCES_DECAY);
			}
			else
			{
				SetEncounterState(EncounterState.MAIN_EVENT_FAILED_DECAY);
			}
		}

		public override void ReadFromData()
		{
			base.ReadFromData();
		}

		public override void CheatComplete()
		{
			WorldHandler.instance.PlaceCurrentPlayerAt(graffitiSpot.transform.position + graffitiSpot.transform.forward * 3f, Quaternion.LookRotation(graffitiSpot.transform.forward * -1f));
		}
	}
}
