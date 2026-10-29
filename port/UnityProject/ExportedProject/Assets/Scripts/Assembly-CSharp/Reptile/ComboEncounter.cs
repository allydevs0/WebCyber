using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Reptile
{
	public class ComboEncounter : Encounter
	{
		private float retryTimer;

		private bool retryFade;

		private Collider startTrigger;

		private List<Collider> checkpointTriggers = new List<Collider>();

		private List<Collider> failTriggers = new List<Collider>();

		private float playerInStartTriggerTimer;

		private bool playerInStartTrigger;

		private List<Collider> checkpointTriggersHit = new List<Collider>();

		public GameObject trackSetup;

		private Transform playerSpawner;

		public int scoreRequired;

		public int cornersRequired;

		public int tricksRequired;

		public bool instantRetry;

		public override void InitSceneObject()
		{
			startTrigger = trackSetup.transform.FindRecursive("startTrigger").GetComponent<Collider>();
			Transform[] array = trackSetup.transform.FindChilden("checkpointTrigger");
			foreach (Transform transform in array)
			{
				checkpointTriggers.Add(transform.GetComponent<Collider>());
			}
			array = trackSetup.transform.FindChilden("failTrigger");
			foreach (Transform transform2 in array)
			{
				failTriggers.Add(transform2.GetComponent<Collider>());
			}
			playerSpawner = trackSetup.transform.FindRecursive("spawner");
			playerSpawner.gameObject.SetActive(value: false);
			trackSetup.SetActive(value: false);
			base.InitSceneObject();
		}

		public override void StartMainEvent()
		{
			trackSetup.SetActive(value: true);
			ReadyPlayer();
		}

		public override void UpdateMainEvent()
		{
			if (instantRetry && retryTimer > 0f)
			{
				retryTimer -= Core.dt;
				if (retryTimer < 0.25f && !retryFade)
				{
					Core.Instance.UIManager.effects.FadeInAndOutBlack(0.2f, 0.1f, 0.2f, UpdateType.Normal);
					retryFade = true;
				}
				if (retryTimer < 0f)
				{
					ReadyPlayer();
				}
			}
			else if (!player.IsBusyWithSequence())
			{
				if (checkpointTriggers.Count == checkpointTriggersHit.Count && player.lastCornered >= cornersRequired && player.lastScore >= (float)scoreRequired && player.tricksInLastCombo >= tricksRequired)
				{
					SetEncounterState(EncounterState.MAIN_EVENT_SUCCES_DECAY);
				}
				else if (playerInStartTriggerTimer <= 0f && !player.IsComboing() && player.IsGrounded())
				{
					FailEncounterStraightToRetry();
				}
			}
			if (playerInStartTriggerTimer > 0f)
			{
				playerInStartTriggerTimer -= Core.dt;
			}
			base.UpdateMainEvent();
		}

		public override void JostleStreetlifeSignal()
		{
			if (mustDodgeStreetlife)
			{
				FailEncounterStraightToRetry();
			}
		}

		public void FailEncounterStraightToRetry()
		{
			if (!instantRetry)
			{
				SetEncounterState(EncounterState.MAIN_EVENT_FAILED_DECAY);
			}
			else if (state == EncounterState.MAIN_EVENT && retryTimer <= 0f)
			{
				MonoBehaviour.print("fail combo encounter");
				retryTimer = 1.1f;
				retryFade = false;
			}
		}

		public override void TriggerDetectPlayer(Collider trigger, Player player)
		{
			if (startTrigger == trigger)
			{
				playerInStartTriggerTimer = 1f;
			}
			if (checkpointTriggers.Contains(trigger) && !checkpointTriggersHit.Contains(trigger))
			{
				checkpointTriggersHit.Add(trigger);
			}
			if (failTriggers.Contains(trigger))
			{
				FailEncounterStraightToRetry();
			}
		}

		public void ReadyPlayer()
		{
			Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			currentPlayer.cam.ResetCameraPositionRotation();
			WorldHandler.instance.PlaceCurrentPlayerAt(playerSpawner);
			PlayerSpawner component = playerSpawner.GetComponent<PlayerSpawner>();
			if (component != null)
			{
				component.SetReached();
			}
			checkpointTriggersHit.Clear();
			currentPlayer.ClearMultipliersDone();
			currentPlayer.lastScore = 0f;
			currentPlayer.tricksInLastCombo = 0;
			currentPlayer.lastCornered = 0;
			currentPlayer.SwitchToEquippedMovestyle(set: false, doAirTrick: false, changeProp: true, showEffect: false);
			currentPlayer.ActivateAbility(currentPlayer.switchMoveStyleAbility);
			playerInStartTriggerTimer = 1f;
		}
	}
}
