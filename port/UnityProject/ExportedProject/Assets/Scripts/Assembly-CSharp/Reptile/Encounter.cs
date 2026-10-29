using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Reptile
{
	public class Encounter : GameplayEvent
	{
		public enum EncounterState
		{
			CLOSED = 0,
			OPEN_STARTUP = 1,
			OPEN = 2,
			INTRO = 3,
			MAIN_EVENT = 4,
			MAIN_EVENT_SUCCES_DECAY = 5,
			MAIN_EVENT_FAILED_DECAY = 6,
			OUTRO_SUCCES = 7,
			OUTRO_FAIL = 8
		}

		[Serializable]
		public class EncounterCheckpoint
		{
			public Collider trigger;

			public Transform spawnLocation;
		}

		[Serializable]
		public class Racer
		{
			public Characters character;

			public Crew crew;

			public int outfit;

			public MoveStyle moveStyle;

			public Transform spawner;

			public PlayerAIPath path;

			public int pathID = -1;

			[HideInInspector]
			public Player player;
		}

		internal EncounterState state;

		private SequenceHandler handler;

		internal Player player;

		internal float stateTimer;

		protected float outroDelay = 2f;

		private bool shownOpening;

		private bool currentlyActive;

		private bool win;

		public bool allowPhone = true;

		public bool stopWantedOnStart = true;

		public GameObject[] makeUnavailableDuringEncounter;

		public bool restartImmediatelyOnFail;

		public bool instantIntro;

		public PlayableDirector introSequence;

		public PlayableDirector outroSuccesSequence;

		[Header("Has to be different than the succes outro! Will break otherwise")]
		public PlayableDirector outroFailSequence;

		public bool turnOffPlayerDuringSequences;

		public bool lowerVolumeDuringSequence;

		public Transform startSpawner;

		public Transform completeSpawner;

		public Transform failSpawner;

		public UnityEvent OnCompleted;

		public UnityEvent OnFailed;

		[Header("the moment the intro starts")]
		public UnityEvent OnIntro;

		[Header("when the intro is over, the moment main event is started")]
		public UnityEvent OnStart;

		[Header("the moment the outro starts, both win and lose")]
		public UnityEvent OnOutro;

		public Material uniqueSkybox;

		private Material originalSkybox;

		private StageManager stageManager;

		public bool mustDodgeStreetlife;

		public bool doNotRestartStageMusicAtEnd;

		public Teleport deathZoneTeleporter;

		[SerializeField]
		public EncounterCheckpoint[] checkpoints;

		internal int currentCheckpoint = -1;

		public GameObject stuffToTurnOnDuringEncounter;

		public bool assumeActivatingEncounterFromReadDataMeansWeFailed;

		public override void InitSceneObject()
		{
			base.InitSceneObject();
			if (introSequence != null)
			{
				introSequence.gameObject.SetActive(value: false);
			}
			if (outroSuccesSequence != null)
			{
				outroSuccesSequence.gameObject.SetActive(value: false);
			}
			if (openingSequence != null)
			{
				openingSequence.gameObject.SetActive(value: false);
			}
			player = WorldHandler.instance.GetCurrentPlayer();
			handler = SequenceHandler.instance;
			stageManager = Core.Instance.BaseModule.StageManager;
			if ((bool)stuffToTurnOnDuringEncounter)
			{
				stuffToTurnOnDuringEncounter.SetActive(value: false);
			}
			if (currentlyActive)
			{
				ActivateEncounter();
			}
			else
			{
				SetEncounterState(EncounterState.CLOSED);
			}
		}

		public virtual void SetEncounterState(EncounterState setState)
		{
			stateTimer = 0f;
			state = setState;
			switch (state)
			{
			case EncounterState.CLOSED:
				EnterEncounterState(EncounterState.CLOSED);
				if (WorldHandler.instance.currentEncounter == this)
				{
					WorldHandler.instance.MakeStuffAvailableAgainCauseEncounterOver();
				}
				if (player.phone != null)
				{
					player.phone.AllowPhone(isAllowed: false, completelyTurnOff: true);
					player.phone.AllowPhone(isAllowed: true, completelyTurnOff: true);
				}
				break;
			case EncounterState.OPEN_STARTUP:
				EnterEncounterState(EncounterState.OPEN_STARTUP);
				break;
			case EncounterState.OPEN:
				if (openOn == OpenOn.REQUIRED_REP && requirement != 0)
				{
					WorldHandler.instance.ResetRequiredREP();
				}
				requirement = 0;
				if (openingSequence != null && !shownOpening)
				{
					handler.StartEnteringSequence(openingSequence, this, turnOffPlayerDuringSequences, setInterruptPlayer: true, instantly: false, setPausePlayer: false, setAllowPhoneOnAfterSequence: true, null, skippable: true, lowerVolumeDuringSequence);
					break;
				}
				EnterEncounterState(EncounterState.OPEN);
				shownOpening = true;
				break;
			case EncounterState.INTRO:
				WorldHandler.instance.StartEncounter(this);
				currentlyActive = true;
				ChangeSkybox(toEncounter: true);
				player.phone.AllowPhone(isAllowed: false, completelyTurnOff: true);
				if (introSequence != null && currentCheckpoint == -1)
				{
					handler.StartEnteringSequence(introSequence, this, turnOffPlayerDuringSequences, setInterruptPlayer: true, instantIntro, setPausePlayer: false, allowPhone, null, skippable: true, lowerVolumeDuringSequence);
				}
				else
				{
					player.phone.AllowPhone(allowPhone, completelyTurnOff: true);
					EnterEncounterState(EncounterState.INTRO);
					OnIntro.Invoke();
					SetEncounterState(EncounterState.MAIN_EVENT);
					SequenceHandler.instance.LetPlayerExitSequence();
				}
				WriteToData();
				break;
			case EncounterState.MAIN_EVENT:
				EnterEncounterState(EncounterState.MAIN_EVENT);
				OnStart.Invoke();
				StartMainEvent();
				break;
			case EncounterState.MAIN_EVENT_SUCCES_DECAY:
				win = true;
				EnterEncounterState(EncounterState.MAIN_EVENT_SUCCES_DECAY);
				break;
			case EncounterState.MAIN_EVENT_FAILED_DECAY:
				EnterEncounterState(EncounterState.MAIN_EVENT_FAILED_DECAY);
				break;
			case EncounterState.OUTRO_SUCCES:
				if (outroSuccesSequence != null)
				{
					handler.StartEnteringSequence(outroSuccesSequence, this, turnOffPlayerDuringSequences, setInterruptPlayer: true, instantly: false, setPausePlayer: false, setAllowPhoneOnAfterSequence: true, null, skippable: true, lowerVolumeDuringSequence);
					break;
				}
				EnterEncounterState(EncounterState.OUTRO_SUCCES);
				OnOutro.Invoke();
				if (WorldHandler.instance.currentEncounter == this)
				{
					WorldHandler.instance.MakeStuffAvailableAgainCauseEncounterOver();
				}
				SetEncounterState(EncounterState.CLOSED);
				Complete();
				break;
			case EncounterState.OUTRO_FAIL:
				if (outroFailSequence != null)
				{
					handler.StartEnteringSequence(outroFailSequence, this, turnOffPlayerDuringSequences, setInterruptPlayer: true, instantly: false, setPausePlayer: false, setAllowPhoneOnAfterSequence: true, null, skippable: true, lowerVolumeDuringSequence);
					break;
				}
				EnterEncounterState(EncounterState.OUTRO_FAIL);
				OnOutro.Invoke();
				Fail();
				if (restartImmediatelyOnFail)
				{
					ActivateEncounterInstantIntro();
					break;
				}
				if (WorldHandler.instance.currentEncounter == this)
				{
					WorldHandler.instance.MakeStuffAvailableAgainCauseEncounterOver();
				}
				SetEncounterState(EncounterState.CLOSED);
				break;
			}
		}

		public virtual void StartMainEvent()
		{
			ChangeSkybox(toEncounter: true);
		}

		public override void UpdateGameplayEvent()
		{
			if (!available)
			{
				return;
			}
			stateTimer += Core.dt;
			switch (state)
			{
			case EncounterState.CLOSED:
				if (!win && ShouldOpen() && !player.IsTooBusyForSuddenCutscenes())
				{
					SetEncounterState(EncounterState.OPEN_STARTUP);
				}
				break;
			case EncounterState.OPEN_STARTUP:
				if (stateTimer >= openStartupDuration)
				{
					SetEncounterState(EncounterState.OPEN);
				}
				break;
			case EncounterState.MAIN_EVENT:
				UpdateMainEvent();
				break;
			case EncounterState.MAIN_EVENT_SUCCES_DECAY:
				if (stateTimer > outroDelay)
				{
					SetEncounterState(EncounterState.OUTRO_SUCCES);
				}
				break;
			case EncounterState.MAIN_EVENT_FAILED_DECAY:
				if (stateTimer > outroDelay)
				{
					SetEncounterState(EncounterState.OUTRO_FAIL);
				}
				break;
			case EncounterState.OPEN:
			case EncounterState.INTRO:
				break;
			}
		}

		private void ChangeSkybox(bool toEncounter)
		{
			if (!(uniqueSkybox != null))
			{
				return;
			}
			if (toEncounter)
			{
				if (originalSkybox == null)
				{
					originalSkybox = RenderSettings.skybox;
					RenderSettings.skybox = uniqueSkybox;
				}
			}
			else if (originalSkybox != null)
			{
				RenderSettings.skybox = originalSkybox;
			}
		}

		public virtual void StartEncounterStraightFromIntro()
		{
			ActivateEncounter();
		}

		public virtual void UpdateMainEvent()
		{
		}

		public virtual void ActivateEncounter()
		{
			player.ClearMultipliersDone();
			player.ResetHP();
			SetEncounterState(EncounterState.INTRO);
		}

		public virtual void ActivateEncounterInstantIntro()
		{
			instantIntro = true;
			ActivateEncounter();
		}

		public virtual void RestartEncounter()
		{
			ActivateEncounter();
		}

		public virtual void EnterEncounterState(EncounterState setState)
		{
			if (setState == EncounterState.MAIN_EVENT && (bool)stuffToTurnOnDuringEncounter)
			{
				stuffToTurnOnDuringEncounter.SetActive(value: true);
			}
			if (setState == EncounterState.INTRO)
			{
				if (GetRespawner() != null && GetRespawner() != failSpawner)
				{
					SetPlayerAtSpawner(GetRespawner());
				}
				else if (startSpawner != null)
				{
					SetPlayerAtSpawner(startSpawner);
				}
			}
			if (setState == EncounterState.CLOSED && (bool)stuffToTurnOnDuringEncounter)
			{
				stuffToTurnOnDuringEncounter.SetActive(value: false);
			}
		}

		public override void EnterSequence(PlayableDirector sequence)
		{
			if (sequence == openingSequence)
			{
				EnterEncounterState(EncounterState.OPEN);
			}
			else if (sequence == introSequence)
			{
				EnterEncounterState(EncounterState.INTRO);
				OnIntro.Invoke();
			}
			else if (sequence == outroSuccesSequence)
			{
				EnterEncounterState(EncounterState.OUTRO_SUCCES);
				OnOutro.Invoke();
			}
			else if (sequence == outroFailSequence)
			{
				EnterEncounterState(EncounterState.OUTRO_FAIL);
				OnOutro.Invoke();
			}
		}

		public override void ExitSequence(PlayableDirector sequence, bool answeredYes = true, bool restartCurrentEncounter = false)
		{
			if (sequence == openingSequence)
			{
				shownOpening = true;
				SequenceHandler.instance.LetPlayerExitSequence();
			}
			else if (sequence == introSequence)
			{
				SetEncounterState(EncounterState.MAIN_EVENT);
				SequenceHandler.instance.LetPlayerExitSequence();
			}
			else if (sequence == outroSuccesSequence)
			{
				SetEncounterState(EncounterState.CLOSED);
				Complete(sequence);
				if (WorldHandler.instance.currentEncounter == this)
				{
					WorldHandler.instance.MakeStuffAvailableAgainCauseEncounterOver();
				}
			}
			else if (sequence == outroFailSequence)
			{
				SetEncounterState(EncounterState.CLOSED);
				Fail(sequence);
				if (restartImmediatelyOnFail)
				{
					ActivateEncounterInstantIntro();
				}
				else if (WorldHandler.instance.currentEncounter == this)
				{
					WorldHandler.instance.MakeStuffAvailableAgainCauseEncounterOver();
				}
			}
		}

		private void FinishEncounterAudioVisuals()
		{
			bool flag = doNotRestartStageMusicAtEnd || (Core.Instance.BaseModule.CurrentStage == Stage.downhill && this is DreamEncounter);
			if (customMusicTrack != null && customMusicTrackInstance != null && !flag)
			{
				stageManager.StartMusic();
				UnityEngine.Object.Destroy(customMusicTrackInstance);
			}
			ChangeSkybox(toEncounter: false);
		}

		private void FinishEncounterExit()
		{
			if (OnCompleted.GetPersistentEventCount() == 0 || !player.IsBusyWithSequence())
			{
				SequenceHandler.instance.LetPlayerExitSequence();
			}
			currentlyActive = false;
		}

		private void Complete(PlayableDirector curSequence = null)
		{
			FinishEncounterAudioVisuals();
			SpawnOnComplete();
			OnCompleted.Invoke();
			FinishEncounterExit();
			WriteToData();
		}

		private void Fail(PlayableDirector curSequence = null)
		{
			FinishEncounterAudioVisuals();
			OnFailed.Invoke();
			SpawnOnFail();
			FinishEncounterExit();
			ResetEncounter();
			WriteToData();
		}

		private void SpawnOnFinish(Transform spawner)
		{
			if ((bool)spawner)
			{
				WorldHandler.instance.PlaceCurrentPlayerAt(spawner);
				PlayerSpawner component = spawner.GetComponent<PlayerSpawner>();
				if (component != null)
				{
					component.SetReached();
				}
			}
			player.Revive();
		}

		public void SpawnOnFail()
		{
			SpawnOnFinish(failSpawner);
		}

		public void SpawnOnComplete()
		{
			SpawnOnFinish(completeSpawner);
		}

		public Transform GetRespawner()
		{
			if (checkpoints != null && checkpoints.Length != 0 && currentCheckpoint > -1)
			{
				return checkpoints[currentCheckpoint].spawnLocation;
			}
			return failSpawner;
		}

		public virtual void CheckpointReached(Collider trigger)
		{
			if (checkpoints == null || checkpoints.Length == 0)
			{
				return;
			}
			for (int i = 0; i < checkpoints.Length; i++)
			{
				if (checkpoints[i].trigger == trigger)
				{
					currentCheckpoint = i;
					if (deathZoneTeleporter != null)
					{
						deathZoneTeleporter.teleportTo = GetRespawner();
					}
					WriteToData();
					break;
				}
			}
		}

		public virtual void ResetEncounter()
		{
		}

		public void QuitEncounter()
		{
			ChangeSkybox(toEncounter: false);
			SetEncounterState(EncounterState.CLOSED);
			currentlyActive = false;
			ResetEncounter();
			WriteToData();
		}

		public virtual void GraffitiDoneSignal(bool win)
		{
			if (win)
			{
				SetEncounterState(EncounterState.MAIN_EVENT_SUCCES_DECAY);
			}
			else
			{
				SetEncounterState(EncounterState.MAIN_EVENT_FAILED_DECAY);
			}
		}

		public virtual void JostleStreetlifeSignal()
		{
			if (mustDodgeStreetlife)
			{
				SetEncounterState(EncounterState.MAIN_EVENT_FAILED_DECAY);
			}
		}

		public bool IsWinnable()
		{
			return state == EncounterState.MAIN_EVENT;
		}

		public virtual void TriggerDetectPlayer(Collider trigger, Player player)
		{
		}

		public override void SetDefaultData(StageProgress stageProgress)
		{
			progressableData = stageProgress.CreateProgressableData(uid, ProgressableType.ENCOUNTER_PROGRESS);
			EncounterProgress obj = (EncounterProgress)progressableData;
			obj.canOpen = canOpen;
			obj.completed = win;
			obj.currentlyActive = currentlyActive;
		}

		public override void ReadFromData()
		{
			EncounterProgress encounterProgress = (EncounterProgress)progressableData;
			canOpen = encounterProgress.canOpen;
			win = encounterProgress.completed;
			currentlyActive = encounterProgress.currentlyActive;
			if (currentlyActive)
			{
				if (!assumeActivatingEncounterFromReadDataMeansWeFailed)
				{
					currentCheckpoint = encounterProgress.currentCheckpoint;
					ActivateEncounter();
				}
				else
				{
					SetPlayerAtSpawner(failSpawner);
					currentlyActive = false;
					WriteToData();
				}
			}
		}

		public override void WriteToData()
		{
			EncounterProgress obj = (EncounterProgress)progressableData;
			obj.canOpen = canOpen;
			obj.completed = win;
			obj.currentlyActive = currentlyActive;
			obj.currentCheckpoint = currentCheckpoint;
			WorldHandler.instance.SavePosToNearestReachedRespawner();
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void PlayerDied()
		{
			SetEncounterState(EncounterState.MAIN_EVENT_FAILED_DECAY);
		}

		public virtual void CheatComplete()
		{
			SetEncounterState(EncounterState.MAIN_EVENT_SUCCES_DECAY);
		}

		internal void SetPlayerAsCharacter(Characters character)
		{
			Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			CharacterProgress characterProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character);
			currentPlayer.SetCharacter(character, characterProgress.outfit);
			currentPlayer.InitVisual();
			currentPlayer.SetCurrentMoveStyleEquipped(characterProgress.moveStyle);
			if (WorldHandler.instance.characterSelect != null)
			{
				WorldHandler.instance.DestroyCharacterSelect();
			}
		}

		internal bool CompletedEncounter()
		{
			return win;
		}

		public void SetPlayerAsMetalhead()
		{
			SetPlayerAsCharacter(Characters.metalHead);
		}
	}
}
