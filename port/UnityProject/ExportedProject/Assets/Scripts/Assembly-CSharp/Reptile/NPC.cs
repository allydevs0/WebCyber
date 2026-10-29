using System;
using System.Collections.Generic;
using Reptile.Phone;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Reptile
{
	public class NPC : GameplayEvent
	{
		public enum ConversationStarter
		{
			DANCE_TOPROCK = 0,
			TALK = 1,
			TRIGGER = 2,
			RIGHT_AWAY = 3,
			REQUIRED_REP = 4,
			REQUIRED_SCORE = 5,
			EXTERNAL = 6,
			TALK_GIRL = 7,
			NONE = 8,
			TALK_AND_REQUIRED_REP = 9,
			DANCE_POPPING = 10,
			DANCE_HOUSE = 11,
			DANCE_BREAK = 12,
			DANCE_HIPHOP = 13,
			DANCE_LOCKING = 14,
			REQUIRED_PHOTOS_OF_TEMPRACER = 15,
			TAXI_DANCE = 16,
			REQUIRED_PHOTOS_OF_NPC = 17,
			TALK_RED = 18
		}

		[Serializable]
		public class Dialogue
		{
			public int dialogueLevel;

			public bool enterSequenceInstantly;

			public bool skippable = true;

			public bool disabledExitOnInput;

			public bool lowerVolumeDuringDialogue = true;

			public bool beDynamicTempRacer;

			public bool placePlayerAtSnapPosition = true;

			public bool hidePlayer;

			public bool hideNPC;

			public bool playCharacterTalkVoice;

			public ConversationStarter conversationStarter;

			public PlayableDirector sequence;

			public UnityEvent OnStartSequence;

			public UnityEvent OnEndSequence;

			internal NPC npc;

			internal int index;

			public void Init(NPC setNpc, int setIndex)
			{
				npc = setNpc;
				index = setIndex;
			}

			public void ExitSequence(PlayableDirector exitSequence)
			{
				OnEndSequence.Invoke();
				if (OnEndSequence.GetPersistentEventCount() == 0 || !npc.player.IsBusyWithSequence())
				{
					npc.LetPlayerGo();
				}
			}
		}

		[Serializable]
		public class TempRacer
		{
			public Crew crew = Crew.FRANKS;

			public Characters character = Characters.frank;

			public int outfit;

			public MoveStyle moveStyle = MoveStyle.BMX;

			public Transform spawnFrom;

			public PlayerAIPath path;

			public int pathIDNo = -1;

			public NPC npcAtEnd;
		}

		[Header("It turns itself off if it's not open yet")]
		[Header("Put any trigger inside the NPC")]
		private float openTimer;

		public string anim;

		[Header("If dialogueLevel is above any dialogue, NPC is cleared")]
		[Header("Always do all progress at the end of the sequence (or both)")]
		public Dialogue[] dialogues;

		[Header("spawnFrom will get turned off")]
		[Header("path and npc are optional")]
		public TempRacer[] tempRacers = new TempRacer[1];

		public bool checkForPlayerDuringTempRacerSpawn;

		public Characters character = Characters.NONE;

		public AudioClipID useMiscTalkVoice = AudioClipID.NONE;

		public Crew crew;

		internal bool activeTempRacerWithDialogue;

		internal int dialogueLevel = -1;

		internal int highestDialogLevel;

		internal Animator[] animators;

		private Player player;

		internal float lookAtPlayer;

		private float lookAtPlayerTimer;

		private bool canLookAtPlayer;

		private Transform head;

		private List<LookAtIKComponent> lookAtIK = new List<LookAtIKComponent>();

		private Transform playerPosSnap;

		internal bool isActive;

		private SaveSlotData currentSaveSlot;

		private SequenceHandler sequenceHandler;

		private WorldHandler worldHandler;

		private EmailMessage alternateCharacterEmail;

		public float playerLookAtHeightForNonHumanNPC = 0.3f;

		public bool playSoftBounceAnimDuringTalk;

		private bool doingBouncingAnim;

		[Tooltip("This is for NPCs that have a REP requirement set somewhere in there dialogue which is not their first dialogue.")]
		[SerializeField]
		private bool showRequiredRepForDialogueInPauseMenu;

		protected override void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			sceneObjectsRegister.RegisterProgressableObject(this);
			sceneObjectsRegister.RegisterNPC(this);
		}

		public override void InitSceneObject()
		{
			base.InitSceneObject();
			currentSaveSlot = Core.Instance.SaveManager.CurrentSaveSlot;
			sequenceHandler = SequenceHandler.instance;
			worldHandler = WorldHandler.instance;
			player = worldHandler.GetCurrentPlayer();
			List<Animator> list = new List<Animator>(GetComponentsInChildren<Animator>());
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].avatar == null || !list[num].avatar.isHuman)
				{
					list.Remove(list[num]);
				}
			}
			animators = list.ToArray();
			for (int i = 0; i < animators.Length; i++)
			{
				LookAtIKComponent component = animators[i].GetComponent<LookAtIKComponent>();
				if (component != null)
				{
					lookAtIK.Add(component);
				}
			}
			canLookAtPlayer = lookAtIK.Count > 0;
			if (canLookAtPlayer)
			{
				head = animators[0].GetBoneTransform(HumanBodyBones.Head);
			}
			for (int j = 0; j < dialogues.Length; j++)
			{
				dialogues[j].Init(this, j);
				if (dialogues[j].dialogueLevel > highestDialogLevel)
				{
					highestDialogLevel = dialogues[j].dialogueLevel;
				}
			}
			playerPosSnap = base.transform.Find("playerSnapPosition");
		}

		public Vector3 GetLookAtPos()
		{
			if (canLookAtPlayer)
			{
				return head.position + Vector3.up * 0.125f;
			}
			return base.transform.position + Vector3.up * playerLookAtHeightForNonHumanNPC;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			player = null;
			currentSaveSlot = null;
			sequenceHandler = null;
			worldHandler = null;
		}

		public override void UpdateGameplayEvent()
		{
			if (!available || player.IsTooBusyForSuddenCutscenes())
			{
				return;
			}
			if (dialogueLevel == -1 && ShouldOpen())
			{
				openTimer += Core.dt;
				if (openTimer > openStartupDuration)
				{
					if (openOn == OpenOn.REQUIRED_REP && requirement != 0)
					{
						worldHandler.ResetRequiredREP();
						requirement = 0;
					}
					if (openingSequence != null)
					{
						sequenceHandler.StartEnteringSequence(openingSequence, this, setHidePlayer: true, setInterruptPlayer: false);
					}
					else
					{
						SetDialogueLevel(0);
					}
				}
				return;
			}
			HandleLookAtPlayer();
			if (requirement > 0 && base.gameObject.activeInHierarchy)
			{
				if (currentSaveSlot.GetCurrentStageProgress().reputation >= requirement && CanDoConversationStarter(ConversationStarter.REQUIRED_REP))
				{
					FindAndStartDialogue(ConversationStarter.REQUIRED_REP);
					worldHandler.ResetRequiredREP();
				}
				else if (player.lastScore >= (float)requirement && CanDoConversationStarter(ConversationStarter.REQUIRED_SCORE))
				{
					FindAndStartDialogue(ConversationStarter.REQUIRED_SCORE);
				}
				else if (activeTempRacerWithDialogue && (bool)photoObjective && photoObjective.SuccesfulPhotoCount >= requirement && CanDoConversationStarter(ConversationStarter.REQUIRED_PHOTOS_OF_TEMPRACER))
				{
					FindAndStartDialogue(ConversationStarter.REQUIRED_PHOTOS_OF_TEMPRACER);
				}
				else if ((bool)photoObjective && photoObjective.SuccesfulPhotoCount >= requirement && CanDoConversationStarter(ConversationStarter.REQUIRED_PHOTOS_OF_NPC))
				{
					FindAndStartDialogue(ConversationStarter.REQUIRED_PHOTOS_OF_NPC);
				}
			}
			FindAndStartDialogue(ConversationStarter.RIGHT_AWAY);
			CheckCreateTempRacer();
			if (doingBouncingAnim)
			{
				PlayDefaultAnim();
			}
		}

		public void HandleLookAtPlayer()
		{
			if (base.gameObject.activeInHierarchy && canLookAtPlayer)
			{
				if (lookAtPlayer > 0f)
				{
					lookAtPlayerTimer = Mathf.Min(lookAtPlayerTimer + Core.dt * 1.8f, 0.55f);
				}
				else
				{
					lookAtPlayerTimer = Mathf.Max(lookAtPlayerTimer - Core.dt * 1.55f, 0f);
				}
				for (int i = 0; i < lookAtIK.Count; i++)
				{
					lookAtIK[i].weight = lookAtPlayerTimer;
					lookAtIK[i].targetPosition = player.characterVisual.head.position + Vector3.up * 0.125f;
				}
				lookAtPlayer -= Core.dt;
			}
		}

		public override bool ShouldOpen()
		{
			if (canOpen && openOn == OpenOn.CHARACTER_IS_UNLOCKED)
			{
				return Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character).unlocked;
			}
			return base.ShouldOpen();
		}

		public override void EnterSequence(PlayableDirector sequence)
		{
			base.EnterSequence(sequence);
			if (sequence == openingSequence)
			{
				SetNPCActive(set: true);
				return;
			}
			Dialogue dialogOfSequence = GetDialogOfSequence(sequence);
			SetNPCGraphicsActive(!dialogOfSequence.hideNPC, inSequence: true);
			player.talkAbility.sequence = sequence;
			player.talkAbility.StartTalking(this, dialogOfSequence.index, dialogOfSequence.playCharacterTalkVoice);
			dialogOfSequence.OnStartSequence.Invoke();
		}

		public override void ExitSequence(PlayableDirector sequence, bool invokeEvent = true, bool restartCurrentEncounter = false)
		{
			Dialogue dialogOfSequence = GetDialogOfSequence(sequence);
			if (dialogOfSequence != null && dialogOfSequence.hideNPC)
			{
				SetNPCGraphicsActive(set: true);
			}
			if (!invokeEvent)
			{
				LetPlayerGo();
			}
			else if (sequence == openingSequence)
			{
				SetDialogueLevel(0);
				sequenceHandler.LetPlayerExitSequence();
			}
			else
			{
				dialogOfSequence.ExitSequence(sequence);
			}
			if (restartCurrentEncounter && WorldHandler.instance.currentEncounter != null)
			{
				WorldHandler.instance.currentEncounter.ActivateEncounter();
			}
		}

		private Dialogue GetDialogOfSequence(PlayableDirector sequence)
		{
			for (int i = 0; i < dialogues.Length; i++)
			{
				if (dialogues[i].sequence == sequence)
				{
					return dialogues[i];
				}
			}
			return null;
		}

		public bool CanDoConversationStarter(ConversationStarter conversationStarter)
		{
			if (!available)
			{
				return false;
			}
			for (int i = 0; i < dialogues.Length; i++)
			{
				if (dialogues[i].dialogueLevel == dialogueLevel && dialogues[i].conversationStarter == conversationStarter)
				{
					return true;
				}
			}
			return false;
		}

		public bool StartTalkingWithConvoStarter(ConversationStarter conversationStarter)
		{
			return FindAndStartDialogue(conversationStarter);
		}

		public void StartDialogueByNumber(int num)
		{
			player.ActivateAbility(player.talkAbility);
			activeTempRacerWithDialogue = false;
			Dialogue dialogue = dialogues[num];
			bool skippable = dialogue.skippable;
			sequenceHandler.StartEnteringSequence(dialogue.sequence, this, dialogue.hidePlayer, setInterruptPlayer: false, dialogue.enterSequenceInstantly, setPausePlayer: false, setAllowPhoneOnAfterSequence: true, null, skippable, dialogue.lowerVolumeDuringDialogue, null, dialogue.disabledExitOnInput);
		}

		public Dialogue GetDialogueByNumber(int num)
		{
			return dialogues[num];
		}

		private bool FindAndStartDialogue(ConversationStarter conversationStarter)
		{
			for (int i = 0; i < dialogues.Length; i++)
			{
				Dialogue dialogue = dialogues[i];
				if (dialogue.dialogueLevel == dialogueLevel && dialogue.conversationStarter == conversationStarter)
				{
					player.ActivateAbility(player.talkAbility);
					activeTempRacerWithDialogue = false;
					bool skippable = dialogue.skippable;
					sequenceHandler.StartEnteringSequence(dialogue.sequence, this, dialogue.hidePlayer, setInterruptPlayer: false, dialogue.enterSequenceInstantly, setPausePlayer: false, setAllowPhoneOnAfterSequence: true, null, skippable, dialogue.lowerVolumeDuringDialogue, null, dialogue.disabledExitOnInput);
					return true;
				}
			}
			return false;
		}

		public void TalkSequenceIsSet(int dialogueIndex)
		{
			for (int i = 0; i < dialogues.Length; i++)
			{
				if (dialogues[i].dialogueLevel == dialogues[dialogueIndex].dialogueLevel && dialogues[i].beDynamicTempRacer)
				{
					worldHandler.RemoveTempRacers(this);
					animators[0].gameObject.SetActive(value: true);
					break;
				}
			}
			if (!(playerPosSnap == null) && dialogues[dialogueIndex].placePlayerAtSnapPosition)
			{
				player.SetPosAndRotHard(playerPosSnap.position, playerPosSnap.rotation);
				PlayerSpawner component = playerPosSnap.GetComponent<PlayerSpawner>();
				if (component != null)
				{
					component.SetReached();
				}
			}
		}

		public void OnOpeningSequenceEnd(PlayableDirector sequence)
		{
		}

		public void StartTalking()
		{
			if (dialogueLevel < 0)
			{
				SetDialogueLevel(0);
			}
			if (!FindAndStartDialogue(ConversationStarter.TALK) && !FindAndStartDialogue(ConversationStarter.EXTERNAL))
			{
				FindAndStartDialogue(ConversationStarter.TRIGGER);
			}
		}

		public void AdvanceDialogLevel()
		{
			if (dialogueLevel < 0)
			{
				SetDialogueLevel(1);
			}
			else
			{
				SetDialogueLevel(dialogueLevel + 1);
			}
		}

		public void CompleteNPC()
		{
			SetDialogueLevel(highestDialogLevel + 1);
		}

		public void AdvanceDialogAndStartTalking()
		{
			AdvanceDialogLevel();
			StartTalking();
		}

		public void SetDialogueLevel(int num)
		{
			dialogueLevel = num;
			ActiveBasedOnDialogueLevel();
			if (IsOnValidDialogueLevel())
			{
				canOpen = true;
			}
			if (CanDoConversationStarter(ConversationStarter.REQUIRED_PHOTOS_OF_NPC) && photoObjective == null)
			{
				photoObjective = base.gameObject.AddComponent<MissionPhotoObjective>();
				photoObjective.InitPhotoObjective(GetComponentInChildren<MeshCollider>(), null);
			}
			if (activeTempRacerWithDialogue)
			{
				bool flag = false;
				for (int i = 0; i < dialogues.Length; i++)
				{
					if (dialogues[i].dialogueLevel == dialogueLevel && dialogues[i].beDynamicTempRacer)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					RemoveTempRacers();
					if (available && isActive)
					{
						ShowNPC();
					}
				}
			}
			else if (available && isActive)
			{
				ShowNPC();
			}
			WriteToData();
		}

		private void CheckCreateTempRacer()
		{
			if (!base.gameObject.activeInHierarchy || activeTempRacerWithDialogue)
			{
				return;
			}
			for (int i = 0; i < dialogues.Length; i++)
			{
				if (dialogues[i].dialogueLevel == dialogueLevel && dialogues[i].beDynamicTempRacer)
				{
					CreateTempRacer(0, dialogues[i]);
					activeTempRacerWithDialogue = true;
				}
			}
		}

		public override void LetPlayerGo()
		{
			sequenceHandler.LetPlayerExitSequence();
			player.talkAbility.StopTalking();
		}

		public void CreateTempRacer(int index = 0)
		{
			CreateTempRacer(index, null);
		}

		public void CreateTempRacer(int index, Dialogue hookedUpToNPCDialogue)
		{
			if (checkForPlayerDuringTempRacerSpawn && WorldHandler.instance.GetCurrentPlayer().character == tempRacers[index].character)
			{
				if (tempRacers[index].npcAtEnd != null)
				{
					tempRacers[index].npcAtEnd.AllowOpen();
				}
				return;
			}
			Player player = worldHandler.CreateTempRacer(this, tempRacers[index], hookedUpToNPCDialogue);
			if ((bool)sequenceHandler.sequence)
			{
				Transform transform = new GameObject("LookAt").transform;
				transform.parent = player.transform;
				transform.localPosition = Vector3.up * player.motor.GetCapsule().center.y * 0.5f;
				CutsceneCameraFollowTempRacer[] componentsInChildren = sequenceHandler.sequence.GetComponentsInChildren<CutsceneCameraFollowTempRacer>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].lookAt = transform;
				}
			}
		}

		public void ConnectTempRacerToOpenThisNPC(PlayerAI setRacer)
		{
			setRacer.activateNPC = true;
			setRacer.linkedNPC = this;
			tempRacerToOpenThis = setRacer;
			AllowOpen();
		}

		public void RemoveTempRacers()
		{
			activeTempRacerWithDialogue = false;
			worldHandler.RemoveTempRacers(this);
		}

		public void PaintGraffitiSpot(GraffitiSpot graffitiSpot)
		{
			graffitiSpot.Paint((crew == Crew.NONE) ? tempRacers[0].crew : crew, null);
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
				currentPlayer.phone.GetAppInstance<AppEmail>().PushNotification(email);
			}
		}

		public void SetPlayerAsHeadmanWithJetpack()
		{
			Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			currentPlayer.SetCharacter(Characters.headMan);
			currentPlayer.InitVisual();
		}

		public void UnlockBoostpackForPlayer()
		{
			player.LockBoostpack(set: false);
		}

		public void UnlockSpraycanForPlayer()
		{
			player.LockSpraycan(set: false);
		}

		public void UnlockPhoneForPlayer()
		{
			player.LockPhone(set: false);
		}

		public void UnlockDanceForPlayer()
		{
			player.LockCharacterSelect(set: false);
		}

		public void UnlockCharacterSelect()
		{
			player.LockCharacterSelect(set: false);
		}

		public void UnlockTaxi()
		{
			Core.Instance.SaveManager.CurrentSaveSlot.taxiLocked = false;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void ChangeMovestyle(int num)
		{
			player.SetCurrentMoveStyleEquipped((MoveStyle)num);
			player.SwitchToEquippedMovestyle(set: true);
			Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(player.character).moveStyle = (MoveStyle)num;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void ChangeCharacterByName(string charName)
		{
			Characters setChar = (Characters)Enum.Parse(typeof(Characters), charName);
			player.SetCharacter(setChar, Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(setChar).outfit);
			player.InitVisual();
			player.SetCurrentMoveStyleEquipped(Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(setChar).moveStyle);
		}

		public void ChangeCharacter(int charNum)
		{
			player.SetCharacter((Characters)charNum, Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character).outfit);
			player.InitVisual();
			player.SetCurrentMoveStyleEquipped(Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress((Characters)charNum).moveStyle);
		}

		public void TaxiFound()
		{
			Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().taxiFound = true;
		}

		public void HideNPC()
		{
			SetNPCGraphicsActive(set: false);
		}

		public void ShowNPC()
		{
			SetNPCGraphicsActive(set: true);
		}

		private void ActiveBasedOnDialogueLevel()
		{
			SetNPCActive(available && IsOnValidDialogueLevel());
		}

		private bool IsOnValidDialogueLevel()
		{
			if (dialogueLevel != -1)
			{
				return dialogueLevel <= highestDialogLevel;
			}
			return false;
		}

		private void SetNPCActive(bool set)
		{
			isActive = set;
			base.gameObject.SetActive(set);
			if (set)
			{
				PlayAnimations();
			}
		}

		private void SetNPCGraphicsActive(bool set, bool inSequence = false)
		{
			if (animators != null)
			{
				for (int i = 0; i < animators.Length; i++)
				{
					animators[i].gameObject.SetActive(set);
				}
				if (set)
				{
					PlayAnimations(inSequence);
				}
			}
		}

		private void PlayAnimations(bool inSequence = false)
		{
			if (animators != null)
			{
				if (inSequence && playSoftBounceAnimDuringTalk)
				{
					PlayCharacterBounceAnim();
				}
				else
				{
					PlayDefaultAnim();
				}
			}
		}

		private void PlayDefaultAnim()
		{
			doingBouncingAnim = false;
			if (anim == "")
			{
				return;
			}
			for (int i = 0; i < animators.Length; i++)
			{
				if (animators[i].gameObject.activeInHierarchy)
				{
					animators[i].Play(anim);
				}
			}
		}

		private void PlayCharacterBounceAnim()
		{
			doingBouncingAnim = true;
			for (int i = 0; i < animators.Length; i++)
			{
				if (animators[i].gameObject.activeInHierarchy)
				{
					animators[i].Play(CharacterVisual.GetCharacterBounceAnim(character));
				}
			}
		}

		public override void SetDefaultData(StageProgress stageProgress)
		{
			progressableData = stageProgress.CreateProgressableData(uid, ProgressableType.NPC_PROGRESS);
			NPCProgress obj = (NPCProgress)progressableData;
			obj.canOpen = canOpen;
			obj.dialogueLVL = dialogueLevel;
			ActiveBasedOnDialogueLevel();
		}

		public override void ReadFromData()
		{
			NPCProgress nPCProgress = (NPCProgress)progressableData;
			dialogueLevel = nPCProgress.dialogueLVL;
			canOpen = nPCProgress.canOpen;
			ActiveBasedOnDialogueLevel();
			if (CanDoConversationStarter(ConversationStarter.REQUIRED_PHOTOS_OF_NPC) && photoObjective == null)
			{
				photoObjective = base.gameObject.AddComponent<MissionPhotoObjective>();
				photoObjective.InitPhotoObjective(GetComponentInChildren<MeshCollider>(), null);
			}
		}

		public override void WriteToData()
		{
			NPCProgress obj = (NPCProgress)progressableData;
			obj.dialogueLVL = dialogueLevel;
			obj.canOpen = canOpen;
			worldHandler.SavePosToNearestReachedRespawner();
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		internal override void SetAvailable(bool set)
		{
			if (available == set)
			{
				if (!available && tempRacerToOpenThis != null)
				{
					if (tempRacerToOpenThis != null)
					{
						AllowOpen();
					}
					worldHandler.RemoveTempRacers(this);
				}
				return;
			}
			available = set;
			if (!available)
			{
				if (activeTempRacerWithDialogue || tempRacerToOpenThis != null)
				{
					if (tempRacerToOpenThis != null)
					{
						AllowOpen();
					}
					worldHandler.RemoveTempRacers(this);
					activeTempRacerWithDialogue = false;
				}
				SetNPCActive(set: false);
			}
			else
			{
				ActiveBasedOnDialogueLevel();
			}
		}

		public override bool HasActiveREPRequirement()
		{
			bool result = false;
			if (requirement != 0 && available)
			{
				bool num = dialogueLevel == -1 && openOn == OpenOn.REQUIRED_REP && canOpen;
				bool flag = base.gameObject.activeInHierarchy && (CanDoConversationStarter(ConversationStarter.REQUIRED_REP) || showRequiredRepForDialogueInPauseMenu);
				result = num | flag;
			}
			return result;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			_ = base.transform.Find("playerSnapPosition") == null;
		}
	}
}
