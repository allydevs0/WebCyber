using System;
using System.Collections;
using Reptile.Phone;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Reptile
{
	public class ProgressObject : AProgressable, IInitializableSceneObject
	{
		public bool isActive = true;

		public float delay;

		[CharacterNamePicker]
		public int usePhone;

		[Tooltip("Set this to 0 to skip phone ringing")]
		[Range(0f, 10f)]
		public int timesToRingPhone = 3;

		public bool leavePhoneOpen;

		public bool lowerVolumeInSequence;

		public Story.ObjectiveID beTargetForObjective = Story.ObjectiveID.NONE;

		public bool showOnMapWhenObjectiveSet = true;

		public bool onlyTriggerOnObjective;

		public Story.ObjectiveID specifiedStoryObjectiveToAdvanceTo = Story.ObjectiveID.NONE;

		internal bool available = true;

		internal bool triggerable = true;

		public UnityEvent OnTrigger;

		public PlayableDirector sequenceOnTrigger;

		public bool sequenceSkippable = true;

		public bool instantSequence;

		public UnityEvent OnExitSequence;

		public NPC.TempRacer[] tempRacers;

		public bool checkForPlayerDuringTempRacerSpawn;

		private bool triggered;

		private StoryManager storyManager;

		public EmailMessage alternateCharacterEmail;

		private MusicTrack customMusicTrackInstance;

		protected override void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			base.OnRegisterToSceneObjectsRegister(sceneObjectsRegister);
			sceneObjectsRegister.RegisterProgressObject(this);
		}

		public void InitSceneObject()
		{
			base.gameObject.SetActive(isActive);
			storyManager = WorldHandler.instance.StoryManager;
		}

		public override void SetDefaultData(StageProgress stageProgress)
		{
			progressableData = stageProgress.CreateProgressableData(uid, ProgressableType.OBJECT_PROGRESS);
			((ObjectProgress)progressableData).isActive = isActive;
		}

		public override void ReadFromData()
		{
			ObjectProgress objectProgress = (ObjectProgress)progressableData;
			isActive = objectProgress.isActive;
			base.gameObject.SetActive(objectProgress.isActive);
			triggered = objectProgress.isTriggered;
			if (triggered && isActive)
			{
				triggered = false;
				Trigger();
			}
		}

		public override void WriteToData()
		{
			ObjectProgress obj = (ObjectProgress)progressableData;
			obj.isActive = isActive;
			obj.isTriggered = triggered;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void MakeActive()
		{
			isActive = true;
			base.gameObject.SetActive(value: true);
			WriteToData();
		}

		public void MakeUnactive()
		{
			isActive = false;
			base.gameObject.SetActive(value: false);
			WriteToData();
		}

		public void SetAvailable(bool set)
		{
			if (available != set)
			{
				available = set;
				if (!set)
				{
					base.gameObject.SetActive(set);
				}
				else
				{
					base.gameObject.SetActive(isActive);
				}
			}
		}

		public void SetTriggerable(bool set)
		{
			triggerable = set;
		}

		public void Trigger()
		{
			if (!triggered && isActive && available && triggerable && (!onlyTriggerOnObjective || Core.Instance.SaveManager.CurrentSaveSlot.CurrentStoryObjective == beTargetForObjective))
			{
				StartCoroutine(TriggerRoutine());
				triggered = true;
				WriteToData();
			}
		}

		public void ForceTrigger()
		{
			OnTrigger.Invoke();
			MakeUnactive();
			triggered = true;
			WriteToData();
		}

		private IEnumerator TriggerRoutine()
		{
			yield return new WaitForSeconds(delay);
			while (Core.Instance.IsCorePaused)
			{
				yield return null;
			}
			if (sequenceOnTrigger != null)
			{
				if (usePhone > 0)
				{
					Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
					Reptile.Phone.Phone phone = currentPlayer.phone;
					yield return phone.StartCoroutine(phone.RingPhone(currentPlayer, usePhone, timesToRingPhone));
				}
				bool skippable = sequenceSkippable;
				SequenceHandler.instance.StartEnteringSequence(sequenceOnTrigger, null, setHidePlayer: true, setInterruptPlayer: false, instantSequence, setPausePlayer: false, setAllowPhoneOnAfterSequence: true, this, skippable, lowerVolumeInSequence);
			}
			else
			{
				OnTrigger.Invoke();
				MakeUnactive();
			}
		}

		public void CreateTempRacer(int index = 0)
		{
			if (checkForPlayerDuringTempRacerSpawn && WorldHandler.instance.GetCurrentPlayer().character == tempRacers[index].character)
			{
				if (tempRacers[index].npcAtEnd != null)
				{
					tempRacers[index].npcAtEnd.AllowOpen();
				}
				return;
			}
			Player player = WorldHandler.instance.CreateTempRacer(null, tempRacers[index]);
			if ((bool)SequenceHandler.instance.sequence)
			{
				Transform transform = new GameObject("LookAt").transform;
				transform.parent = player.transform;
				transform.localPosition = Vector3.up * player.motor.GetCapsule().center.y * 0.5f;
				CutsceneCameraFollowTempRacer[] componentsInChildren = SequenceHandler.instance.sequence.GetComponentsInChildren<CutsceneCameraFollowTempRacer>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].lookAt = transform;
				}
			}
		}

		public void RemoveTempRacers()
		{
			WorldHandler.instance.RemoveTempRacers();
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
			Core.Instance.SaveManager.CurrentSaveSlot.UnlockCharacter(characters);
			if (showNotification)
			{
				UIManager uIManager = Core.Instance.UIManager;
				string characterName = Core.Instance.Localizer.GetCharacterName(characters);
				uIManager.ShowNotification("NOTIFICATION_JOIN", characterName);
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.MenuSfx, AudioClipID.unlockNotification);
			}
		}

		public void LockCharacter(string character)
		{
			Characters character2 = (Characters)Enum.Parse(typeof(Characters), character);
			Core.Instance.SaveManager.CurrentSaveSlot.LockCharacter(character2);
		}

		public void SendEmail(EmailMessage email)
		{
			Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			if (alternateCharacterEmail != null && Core.Instance.Localizer.CharacterEqualsCharacterID(currentPlayer.character, email.characterNameIDOfSender))
			{
				email = alternateCharacterEmail;
			}
			UnlockableSaveData unlockableSaveDataFor = Core.Instance.Platform.User.GetUnlockableSaveDataFor(email);
			bool isUnlocked = unlockableSaveDataFor.IsUnlocked;
			unlockableSaveDataFor.IsUnlocked = true;
			email.AssignTimestamp();
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
			if (!isUnlocked)
			{
				WorldHandler.instance.GetCurrentPlayer().phone.GetAppInstance<AppEmail>().PushNotification(email);
			}
		}

		public void ShowNotification(string text)
		{
			Core.Instance.UIManager.ShowNotification(text + " (unlocalized text!)");
		}

		public void ShowNotification(NotificationTextAsset text)
		{
			Core.Instance.UIManager.ShowNotification(text);
		}

		public void SetPlayerStartStory()
		{
			Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			currentPlayer.SetCharacter(Characters.headManNoJetpack);
			currentPlayer.InitVisual();
			currentPlayer.ActivateAbility(currentPlayer.sitAbility);
			currentPlayer.sitAbility.SkipStartSit();
			currentPlayer.PreludeAbilitySetup();
		}

		public void SetPlayerStartDemo()
		{
			WorldHandler.instance.GetCurrentPlayer().DemoAbilitySetup();
		}

		public void UnlockBoostAbilityForPlayer()
		{
			WorldHandler.instance.GetCurrentPlayer().LockBoostAbility(set: false);
		}

		public void UnlockCharacterSelectForPlayer()
		{
			Core.Instance.SaveManager.CurrentSaveSlot.characterSelectLocked = false;
		}

		public void UnlockPhoneForPlayer()
		{
			WorldHandler.instance.GetCurrentPlayer().LockPhone(set: false);
		}

		public void UnlockSpraycanForPlayer()
		{
			WorldHandler.instance.GetCurrentPlayer().LockSpraycan(set: false);
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

		public void SetPlayerAsMetalhead()
		{
			SetPlayerAsCharacter(Characters.metalHead);
		}

		public void SetPlayerAsLegendMetalhead()
		{
			SetPlayerAsCharacter(Characters.legendMetalHead);
		}

		public void SetPlayerAsLegendFace()
		{
			SetPlayerAsCharacter(Characters.legendFace);
		}

		public void SetPlayerAsCharacter(string character)
		{
			Characters playerAsCharacter = (Characters)Enum.Parse(typeof(Characters), character);
			SetPlayerAsCharacter(playerAsCharacter);
		}

		public void SetMaxWantedStars(float stars)
		{
			Core.Instance.SaveManager.CurrentSaveSlot.maxWantedStars = stars;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public virtual void EnterSequence(PlayableDirector sequence)
		{
			OnTrigger.Invoke();
		}

		public virtual void ExitSequence(PlayableDirector sequence, bool invokeEvent = true, bool restartCurrentEncounter = false)
		{
			LetPlayerGo();
			if (invokeEvent)
			{
				OnExitSequence.Invoke();
			}
			if (restartCurrentEncounter && WorldHandler.instance.currentEncounter != null)
			{
				WorldHandler.instance.currentEncounter.ActivateEncounter();
			}
			MakeUnactive();
			if (usePhone > 0)
			{
				WorldHandler.instance.GetCurrentPlayer().phone.HangupCall(leavePhoneOpen);
			}
		}

		public void AdvanceStoryObjective()
		{
			if (specifiedStoryObjectiveToAdvanceTo != Story.ObjectiveID.NONE)
			{
				storyManager.SetStoryObjective(specifiedStoryObjectiveToAdvanceTo);
			}
		}

		public void LoadCredits()
		{
			Core.Instance.BaseModule.GoToCreditsFromStage();
		}

		public void LetPlayerGo()
		{
			SequenceHandler.instance.LetPlayerExitSequence();
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
				WantedManager.instance.BlockWantedStatus(shouldBlock: true);
				currentStageProgress.policeAllowed = false;
			}
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
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

		public void GoToHideoutStage()
		{
			Core.Instance.BaseModule.StageManager.ExitCurrentStage(Stage.hideout);
		}

		public void StartCustomTrackFromEncounter(Encounter encounter)
		{
			Core.Instance.AudioManager.StartCustomTrack(encounter.customMusicTrack, ref customMusicTrackInstance);
		}

		public void StartCustomTrack(MusicTrack customMusicTrack)
		{
			Core.Instance.AudioManager.StartCustomTrack(customMusicTrack, ref customMusicTrackInstance);
		}

		public void StopCustomTrack()
		{
			UnityEngine.Object.Destroy(customMusicTrackInstance);
			Core.Instance.BaseModule.StageManager.StartMusic();
		}
	}
}
