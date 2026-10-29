using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class GameplayEvent : AProgressable, IInitializableSceneObject
	{
		public enum OpenOn
		{
			JUST_OPEN = 0,
			REQUIRED_REP = 1,
			REQUIRED_SCORE = 2,
			TEMP_RACER = 3,
			OBJECTIVE = 4,
			REQUIRED_COUNTER = 5,
			CHARACTER_IS_UNLOCKED = 6
		}

		[Header("Use AllowOpen() OR set to open itself here OR use a TEMP_RACER")]
		public bool canOpen;

		[Header("(even if canOpen is true, it won't be active until openOn is valid)")]
		[Header("This is what you need to do to open it")]
		public OpenOn openOn;

		public int requirement;

		public Story.ObjectiveID beTargetForObjective = Story.ObjectiveID.NONE;

		public int objectiveTargetOnlyFromDialogueLevel;

		public Story.ObjectiveID specifiedStoryObjectiveToAdvanceTo = Story.ObjectiveID.NONE;

		internal bool available = true;

		public PlayableDirector openingSequence;

		public float openStartupDuration;

		public Encounter unavailableForEveryEncounterExceptThis;

		public bool standDownEnemiesOnStartSequence = true;

		internal PlayerAI tempRacerToOpenThis;

		internal MissionPhotoObjective photoObjective;

		protected StoryManager storyManager;

		public MusicTrack customMusicTrack;

		internal MusicTrack customMusicTrackInstance;

		protected override void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			base.OnRegisterToSceneObjectsRegister(sceneObjectsRegister);
			sceneObjectsRegister.RegisterGameplayEvent(this);
		}

		public virtual void InitSceneObject()
		{
			storyManager = WorldHandler.instance.StoryManager;
		}

		public virtual void EnterSequence(PlayableDirector sequence)
		{
			if (standDownEnemiesOnStartSequence)
			{
				WantedManager.instance.StandDownEnemies();
			}
		}

		public virtual void ExitSequence(PlayableDirector sequence, bool invokeEvent = true, bool restartCurrentEncounter = false)
		{
			LetPlayerGo();
		}

		public virtual void LetPlayerGo()
		{
			SequenceHandler.instance.LetPlayerExitSequence();
		}

		public void AllowOpen()
		{
			canOpen = true;
			WriteToData();
		}

		public void CallPolice()
		{
			WantedManager.instance.ManuallySetWanted(1);
		}

		public void AllowPolice()
		{
			StageProgress currentStageProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress();
			if (currentStageProgress != null)
			{
				currentStageProgress.policeAllowed = true;
			}
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void DisallowPolice()
		{
			StageProgress currentStageProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress();
			if (currentStageProgress != null)
			{
				currentStageProgress.policeAllowed = false;
			}
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		internal virtual void SetAvailable(bool set)
		{
			available = set;
			base.gameObject.SetActive(set);
		}

		public void SetPlayerAtSpawner(Transform spawner)
		{
			WorldHandler.instance.PlaceCurrentPlayerAt(spawner);
			PlayerSpawner component = spawner.GetComponent<PlayerSpawner>();
			if (component != null)
			{
				component.SetReached();
			}
		}

		public void AdvanceStoryObjective()
		{
			if (specifiedStoryObjectiveToAdvanceTo != Story.ObjectiveID.NONE)
			{
				storyManager.SetStoryObjective(specifiedStoryObjectiveToAdvanceTo);
			}
		}

		public virtual void UpdateGameplayEvent()
		{
		}

		public virtual void FixedUpdateGameplayEvent()
		{
		}

		public void GoToHideoutStage()
		{
			Core.Instance.BaseModule.StageManager.ExitCurrentStage(Stage.hideout);
		}

		public virtual bool HasActiveREPRequirement()
		{
			if (requirement > 0 && openOn == OpenOn.REQUIRED_REP)
			{
				return canOpen;
			}
			return false;
		}

		public void AddCrewCounter()
		{
			Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().AddCrewCounter();
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void ShowNotification(string text)
		{
			Core.Instance.UIManager.ShowNotification(text);
		}

		public void ShowNotification(NotificationTextAsset text)
		{
			Core.Instance.UIManager.ShowNotification(text);
		}

		public void UnlockCameraAppForPlayer()
		{
			WorldHandler.instance.GetCurrentPlayer().LockCameraApp(set: false);
		}

		public override void SetDefaultData(StageProgress stageProgress)
		{
		}

		public override void ReadFromData()
		{
		}

		public override void WriteToData()
		{
		}

		public virtual bool ShouldOpen()
		{
			if (!canOpen)
			{
				return false;
			}
			if (openOn == OpenOn.JUST_OPEN)
			{
				return true;
			}
			if (openOn == OpenOn.REQUIRED_REP)
			{
				return Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().reputation >= requirement;
			}
			if (openOn == OpenOn.REQUIRED_SCORE)
			{
				return WorldHandler.instance.GetCurrentPlayer().lastScore >= (float)requirement;
			}
			if (openOn == OpenOn.TEMP_RACER)
			{
				return tempRacerToOpenThis == null;
			}
			if (openOn == OpenOn.OBJECTIVE)
			{
				return beTargetForObjective == Story.GetCurrentObjectiveInfo().id;
			}
			if (openOn == OpenOn.REQUIRED_COUNTER)
			{
				return Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().crewCounter >= requirement;
			}
			return false;
		}

		public void UnlockCharacter(string character)
		{
			UnlockCharacterImpl(character, showNotification: true);
		}

		public void UnlockCharacterWIthoutNotification(string character)
		{
			UnlockCharacterImpl(character, showNotification: false);
		}

		public void UnlockCharacterImpl(string character, bool showNotification)
		{
			Characters characters = (Characters)Enum.Parse(typeof(Characters), character);
			AUser user = Core.Instance.Platform.User;
			GraffitiArtInfo graffitiArtInfo = WorldHandler.instance.graffitiArtInfo;
			switch (characters)
			{
			case Characters.frank:
			case Characters.prince:
			{
				GraffitiArt graffitiArt5 = graffitiArtInfo.FindCrewArt(Crew.FRANKS, GraffitiSize.M);
				user.GetUnlockableSaveDataFor(graffitiArt5.unlockable).IsUnlocked = true;
				graffitiArt5 = graffitiArtInfo.FindCrewArt(Crew.FRANKS, GraffitiSize.L);
				user.GetUnlockableSaveDataFor(graffitiArt5.unlockable).IsUnlocked = true;
				graffitiArt5 = graffitiArtInfo.FindCrewArt(Crew.FRANKS, GraffitiSize.XL);
				user.GetUnlockableSaveDataFor(graffitiArt5.unlockable).IsUnlocked = true;
				break;
			}
			case Characters.medusa:
			{
				GraffitiArt graffitiArt4 = graffitiArtInfo.FindCrewArt(Crew.MEDUSAS, GraffitiSize.M);
				user.GetUnlockableSaveDataFor(graffitiArt4.unlockable).IsUnlocked = true;
				graffitiArt4 = graffitiArtInfo.FindCrewArt(Crew.MEDUSAS, GraffitiSize.L);
				user.GetUnlockableSaveDataFor(graffitiArt4.unlockable).IsUnlocked = true;
				graffitiArt4 = graffitiArtInfo.FindCrewArt(Crew.MEDUSAS, GraffitiSize.XL);
				user.GetUnlockableSaveDataFor(graffitiArt4.unlockable).IsUnlocked = true;
				break;
			}
			case Characters.eightBall:
			case Characters.eightBallBoss:
			{
				GraffitiArt graffitiArt3 = graffitiArtInfo.FindCrewArt(Crew.ROBOTS, GraffitiSize.M);
				user.GetUnlockableSaveDataFor(graffitiArt3.unlockable).IsUnlocked = true;
				graffitiArt3 = graffitiArtInfo.FindCrewArt(Crew.ROBOTS, GraffitiSize.L);
				user.GetUnlockableSaveDataFor(graffitiArt3.unlockable).IsUnlocked = true;
				graffitiArt3 = graffitiArtInfo.FindCrewArt(Crew.ROBOTS, GraffitiSize.XL);
				user.GetUnlockableSaveDataFor(graffitiArt3.unlockable).IsUnlocked = true;
				break;
			}
			case Characters.boarder:
			{
				GraffitiArt graffitiArt2 = graffitiArtInfo.FindCrewArt(Crew.SAMURAIS, GraffitiSize.M);
				user.GetUnlockableSaveDataFor(graffitiArt2.unlockable).IsUnlocked = true;
				graffitiArt2 = graffitiArtInfo.FindCrewArt(Crew.SAMURAIS, GraffitiSize.L);
				user.GetUnlockableSaveDataFor(graffitiArt2.unlockable).IsUnlocked = true;
				graffitiArt2 = graffitiArtInfo.FindCrewArt(Crew.SAMURAIS, GraffitiSize.XL);
				user.GetUnlockableSaveDataFor(graffitiArt2.unlockable).IsUnlocked = true;
				break;
			}
			case Characters.dj:
			case Characters.futureGirl:
			{
				GraffitiArt graffitiArt = graffitiArtInfo.FindCrewArt(Crew.FUTURES, GraffitiSize.M);
				user.GetUnlockableSaveDataFor(graffitiArt.unlockable).IsUnlocked = true;
				graffitiArt = graffitiArtInfo.FindCrewArt(Crew.FUTURES, GraffitiSize.L);
				user.GetUnlockableSaveDataFor(graffitiArt.unlockable).IsUnlocked = true;
				graffitiArt = graffitiArtInfo.FindCrewArt(Crew.FUTURES, GraffitiSize.XL);
				user.GetUnlockableSaveDataFor(graffitiArt.unlockable).IsUnlocked = true;
				break;
			}
			}
			Core.Instance.SaveManager.CurrentSaveSlot.UnlockCharacter(characters);
			if (showNotification)
			{
				UIManager uIManager = Core.Instance.UIManager;
				string characterName = Core.Instance.Localizer.GetCharacterName(characters);
				uIManager.ShowNotification("NOTIFICATION_JOIN", characterName);
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.MenuSfx, AudioClipID.unlockNotification);
			}
		}

		public void ShowMoveStyleForPlayer()
		{
			Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			currentPlayer.characterVisual.SetMoveStyleVisualProps(currentPlayer, MoveStyle.INLINE);
		}

		public void UnlockSwitchToEquippedMoveStyleForPlayer()
		{
			WorldHandler.instance.GetCurrentPlayer().LockSwitchToEquippedMoveStyle(set: false);
		}

		public void UnlockDance(int num)
		{
			Core.Instance.SaveManager.CurrentSaveSlot.UnlockDance(num);
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void CloseCharacterSelect()
		{
			WorldHandler.instance.DestroyCharacterSelect();
		}

		public void SetMaxWantedStars(float stars)
		{
			Core.Instance.SaveManager.CurrentSaveSlot.maxWantedStars = stars;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		internal void StopCustomTrack()
		{
			Core.Instance.BaseModule.StageManager.StartMusic();
			UnityEngine.Object.Destroy(customMusicTrackInstance);
		}
	}
}
