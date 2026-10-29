using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class WorldHandler : MonoBehaviour
	{
		private int maxAttackers = 2;

		private int maxThreat = 8;

		private List<BasicEnemy> weightedList = new List<BasicEnemy>();

		public static WorldHandler instance;

		private SceneObjectsRegister sceneObjectsRegister;

		public GameObject playerPrefab;

		public GameObject cameraPrefab;

		public GameObject environmentalAudioSourcePrefab;

		internal List<BasicEnemy> subscribedEnemies = new List<BasicEnemy>();

		internal List<BasicEnemy> attackingEnemies = new List<BasicEnemy>();

		internal List<BasicEnemy> circlingEnemies = new List<BasicEnemy>();

		internal List<BasicEnemy> waitingtoAttackEnemies = new List<BasicEnemy>();

		internal List<BasicEnemy> waitingtoCircleEnemies = new List<BasicEnemy>();

		internal PlayerSpawner stageEntrySpawner;

		private Player currentPlayer;

		public CharacterSelect characterSelect;

		internal int requiredREP;

		internal int totalREPInCurrentStage;

		internal Encounter currentEncounter;

		private float savePosTimer = 4f;

		private CharacterConstructor characterConstructor;

		public GraffitiArtInfo graffitiArtInfo;

		private SequenceHandler sequenceHandler;

		private StoryManager storyManager;

		private bool combatBlocked;

		private float currentCombatBlockerDuration;

		private float currentCombatBlockerStartTime;

		private int overworldMaxAttackers = 1;

		private int overworldMaxThreat = 8;

		private int battleMaxAttackers = -1;

		private int battleMaxThreat = -1;

		private Transform currentCameraTransform;

		private Vector3 currentCameraPosition = Vector3.zero;

		private Camera currentCamera;

		private List<AudioSource> audioSources = new List<AudioSource>();

		private int noneHash = Animator.StringToHash("none");

		private int idleHash = Animator.StringToHash("idle");

		private int fallHash = Animator.StringToHash("fall");

		private SaveManager saveManager;

		private SaveSlotData currentSaveSlot;

		private Collider[] ambientColliders = new Collider[5];

		public DynamicJunkUpdater dynamicJunkHandler;

		public Camera CurrentCamera => currentCamera;

		public Transform CurrentCameraTransform => currentCameraTransform;

		public Vector3 CurrentCameraPosition => currentCameraPosition;

		public StoryManager StoryManager => storyManager;

		public SceneObjectsRegister SceneObjectsRegister => sceneObjectsRegister;

		public void Awake()
		{
			instance = this;
			dynamicJunkHandler = GetComponent<DynamicJunkUpdater>();
		}

		public void OnDestroy()
		{
			instance = null;
			StopUpdatingWorld();
			dynamicJunkHandler = null;
			graffitiArtInfo = null;
			characterConstructor = null;
			storyManager = null;
			if (characterSelect != null)
			{
				DestroyCharacterSelect();
			}
		}

		public void StopUpdatingWorld()
		{
			Core.OnUpdate -= UpdateWorldHandler;
			Core.OnFixedUpdate -= FixedUpdateWorldHandler;
			Core.OnLateUpdate -= ArbiterCombat;
			if (sceneObjectsRegister != null)
			{
				sceneObjectsRegister.Dispose();
			}
			sceneObjectsRegister = null;
		}

		public void RegisterAllSceneObjects()
		{
			sceneObjectsRegister = new SceneObjectsRegister();
			sceneObjectsRegister.RegisterAllSceneObjects();
		}

		public void Init(CharacterConstructor characterConstructor, GraffitiArtInfo graffitiArtInfo, StoryManager storyManager, SaveManager saveManager)
		{
			this.characterConstructor = characterConstructor;
			this.graffitiArtInfo = graffitiArtInfo;
			this.storyManager = storyManager;
			this.saveManager = saveManager;
			sequenceHandler = GetComponent<SequenceHandler>();
			sequenceHandler.Init();
			sceneObjectsRegister.InitAllSceneObjects();
			totalREPInCurrentStage = sceneObjectsRegister.GetTotalGraffitiREP();
			Core.OnUpdate += UpdateWorldHandler;
			Core.OnFixedUpdate += FixedUpdateWorldHandler;
			Core.OnLateUpdate += ArbiterCombat;
			maxAttackers = overworldMaxAttackers;
			maxThreat = overworldMaxThreat;
			SavePosToNearestReachedRespawner();
		}

		public void SetNPCAvailabilityBasedOnPlayer()
		{
			foreach (NPC nPC in sceneObjectsRegister.NPCs)
			{
				nPC.SetAvailable(nPC.character != currentPlayer.character);
			}
		}

		public void SetNPCVisuals()
		{
			Characters[] array = (Characters[])Enum.GetValues(typeof(Characters));
			foreach (Characters character in array)
			{
				CharacterProgress characterProgress = saveManager.CurrentSaveSlot.GetCharacterProgress(character);
				if (characterProgress != null)
				{
					SetNPCOutfit(character, characterProgress.outfit);
				}
			}
		}

		public List<AProgressable> GetAllProgressablesInScene()
		{
			return sceneObjectsRegister.progressables;
		}

		public PlayerSpawner GetPlayerSpawnPoint(Stage newStage, Stage previousStage, SaveManager saveManager)
		{
			PlayerSpawner playerSpawner = null;
			if (previousStage == Stage.NONE || previousStage == Stage.MAX || previousStage == newStage)
			{
				playerSpawner = GetDefaultPlayerSpawnPoint();
				if (playerSpawner != null && saveManager.CurrentSaveSlot.HasRespawnSaved(newStage))
				{
					StageProgress currentStageProgress = saveManager.CurrentSaveSlot.GetCurrentStageProgress();
					playerSpawner.transform.SetPositionAndRotation(currentStageProgress.respawnPos, Quaternion.Euler(currentStageProgress.respawnRot));
				}
			}
			else
			{
				playerSpawner = GetPlayerSpawnpointFromStageTransition(previousStage);
			}
			return playerSpawner;
		}

		private PlayerSpawner GetDefaultPlayerSpawnPoint()
		{
			PlayerSpawner playerSpawner = sceneObjectsRegister.playerSpawners.Find((PlayerSpawner candidate) => candidate.isDefaultSpawnPoint);
			if (playerSpawner == null)
			{
				if (sceneObjectsRegister.playerSpawners.Count > 0)
				{
					return sceneObjectsRegister.playerSpawners[0];
				}
				return null;
			}
			return playerSpawner;
		}

		public void SavePosToNearestReachedRespawner()
		{
			Transform transform = ((!(instance.currentEncounter != null)) ? GetRespawnPoint(currentPlayer.tf.position).transform : instance.currentEncounter.GetRespawner());
			if (transform != null)
			{
				StageProgress currentStageProgress = saveManager.CurrentSaveSlot.GetCurrentStageProgress();
				currentStageProgress.respawnPos = transform.position;
				currentStageProgress.respawnRot = transform.rotation.eulerAngles;
				saveManager.SaveCurrentSaveSlot();
			}
		}

		public PlayerSpawner GetRespawnPoint(Vector3 closestToPos)
		{
			PlayerSpawner playerSpawner = null;
			float num = float.MaxValue;
			List<PlayerSpawner> list = sceneObjectsRegister.playerSpawners.FindAll((PlayerSpawner candidate) => candidate.isRespawner);
			foreach (PlayerSpawner item in list.FindAll((PlayerSpawner candidate) => candidate.PositionInsideBounds(closestToPos)))
			{
				if (item.isReached)
				{
					float sqrMagnitude = (closestToPos - item.transform.position).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						playerSpawner = item;
					}
				}
			}
			if (playerSpawner == null)
			{
				foreach (PlayerSpawner item2 in list)
				{
					if (item2.isReached)
					{
						float sqrMagnitude2 = (closestToPos - item2.transform.position).sqrMagnitude;
						if (sqrMagnitude2 < num)
						{
							num = sqrMagnitude2;
							playerSpawner = item2;
						}
					}
				}
			}
			if (playerSpawner == null)
			{
				return GetDefaultPlayerSpawnPoint();
			}
			return playerSpawner;
		}

		private PlayerSpawner GetPlayerSpawnpointFromStageTransition(Stage previousStage)
		{
			StageTransition stageTransition = null;
			foreach (StageTransition stageTransition2 in sceneObjectsRegister.stageTransitions)
			{
				if (stageTransition2.GetStageToLoad() == previousStage || stageTransition2.CanAlsoComeInFromStage(previousStage))
				{
					stageTransition = stageTransition2;
					break;
				}
			}
			if (stageTransition == null || stageTransition.PlayerSpawner == null)
			{
				if (!(stageTransition == null) && stageTransition.PlayerSpawner == null)
				{
					MonoBehaviour.print("could not find spawner of transition " + stageTransition.gameObject.name);
				}
				return GetDefaultPlayerSpawnPoint();
			}
			return stageTransition.PlayerSpawner;
		}

		public Player CreateHumanPlayerAt(Transform spawner)
		{
			currentPlayer = CreatePlayerObjectAt(spawner);
			currentPlayer.cam = UnityEngine.Object.Instantiate(cameraPrefab).GetComponent<GameplayCamera>();
			SetCurrentCamera(currentPlayer.cam.cam);
			return currentPlayer;
		}

		public Player SetupAIPlayerAt(Transform spawner, Characters character, PlayerType playerType, int outfit = 0, MoveStyle moveStyleEquipped = MoveStyle.SKATEBOARD, Crew crew = Crew.PLAYERS)
		{
			Player player = CreatePlayerObjectAt(spawner);
			RegisterPlayer(player);
			InitPlayer(player, character, outfit, playerType, moveStyleEquipped, crew);
			player.SwitchToEquippedMovestyle(moveStyleEquipped != MoveStyle.ON_FOOT, doAirTrick: false, changeProp: true, showEffect: false);
			player.AI.SetHasMoveStyle(moveStyleEquipped != MoveStyle.ON_FOOT);
			return player;
		}

		public Player CreateTempRacer(NPC linkedNPC, NPC.TempRacer tempRacerSetup, NPC.Dialogue hookedUpToNPCDialogue = null)
		{
			bool num = hookedUpToNPCDialogue != null;
			Transform spawnFrom = tempRacerSetup.spawnFrom;
			if (num)
			{
				spawnFrom = hookedUpToNPCDialogue.npc.animators[0].transform;
			}
			Player player = SetupAIPlayerAt(spawnFrom, tempRacerSetup.character, PlayerType.TEMP_RACER_AI, tempRacerSetup.outfit, tempRacerSetup.moveStyle, tempRacerSetup.crew);
			player.AI.linkedNPC = linkedNPC;
			if (num)
			{
				if (linkedNPC.CanDoConversationStarter(NPC.ConversationStarter.REQUIRED_PHOTOS_OF_TEMPRACER))
				{
					linkedNPC.photoObjective = player.gameObject.AddComponent<MissionPhotoObjective>();
					linkedNPC.photoObjective.InitPhotoObjective(player.interactionCollider, null);
				}
				player.AI.SetHookedUpToNPCDialogue(hookedUpToNPCDialogue);
				OutfitSwappableCharacter componentInChildren = linkedNPC.GetComponentInChildren<OutfitSwappableCharacter>(includeInactive: true);
				if (componentInChildren != null)
				{
					player.SetOutfit(componentInChildren.Outfit);
				}
			}
			else if (tempRacerSetup.npcAtEnd != null)
			{
				tempRacerSetup.npcAtEnd.ConnectTempRacerToOpenThisNPC(player.AI);
				OutfitSwappableCharacter componentInChildren2 = tempRacerSetup.npcAtEnd.GetComponentInChildren<OutfitSwappableCharacter>(includeInactive: true);
				if (componentInChildren2 != null)
				{
					player.SetOutfit(componentInChildren2.Outfit);
				}
			}
			PlayerAIPath pathFromPathID = GetPathFromPathID(tempRacerSetup.pathIDNo);
			if (pathFromPathID != null)
			{
				player.AI.StartOnPath(pathFromPathID);
			}
			else if (tempRacerSetup.path == null)
			{
				player.AI.StartClosestTargetOnAnyPath();
			}
			else
			{
				player.AI.StartOnPath(tempRacerSetup.path);
			}
			spawnFrom.gameObject.SetActive(value: false);
			return player;
		}

		private Player CreatePlayerObjectAt(Transform spawner)
		{
			Player component = UnityEngine.Object.Instantiate(playerPrefab, spawner.position, spawner.rotation).GetComponent<Player>();
			PlacePlayerAt(component, spawner, stopAbility: false);
			component.tf.SetAsFirstSibling();
			return component;
		}

		public void InitPlayerObject(Player player, Characters character, int outfit = 0, MoveStyle moveStyleEquipped = MoveStyle.SKATEBOARD, Crew crew = Crew.PLAYERS)
		{
			InitPlayer(player, character, outfit, PlayerType.HUMAN, moveStyleEquipped, crew);
		}

		private void InitPlayer(Player player, Characters character, int outfit = 0, PlayerType playerType = PlayerType.HUMAN, MoveStyle moveStyleEquipped = MoveStyle.SKATEBOARD, Crew crew = Crew.PLAYERS)
		{
			player.Init(characterConstructor, character, outfit, playerType, moveStyleEquipped, crew);
			player.name = "Player_" + playerType.ToString() + (sceneObjectsRegister.players.Count - 1);
		}

		public void RemovePlayer(Player player)
		{
			sceneObjectsRegister.players.Remove(player);
		}

		public void RegisterPlayer(Player player)
		{
			sceneObjectsRegister.RegisterPlayer(player);
		}

		public void PlaceCurrentPlayerAt(Transform spawner, bool stopAbility = true)
		{
			PlaceCurrentPlayerAt(spawner.position, spawner.rotation, stopAbility);
		}

		public void PlaceCurrentPlayerAt(Vector3 pos, Quaternion rot, bool stopAbility = true)
		{
			PlacePlayerAt(currentPlayer, pos, rot, stopAbility);
		}

		public void PlacePlayerAt(Player player, Transform spawner, bool stopAbility = true)
		{
			PlacePlayerAt(player, spawner.position, spawner.rotation, stopAbility);
		}

		public void PlacePlayerAt(Player player, Vector3 pos, Quaternion rot, bool stopAbility = true)
		{
			if (stopAbility)
			{
				player.CompletelyStop();
			}
			else
			{
				player.SetVelocity(Vector3.zero);
			}
			Vector3 vector = pos;
			float num = 1f;
			int layerMask = 1041;
			if (Physics.Raycast(vector + Vector3.up * num, Vector3.down, out var hitInfo, 2f * num, layerMask))
			{
				vector.y -= hitInfo.distance - num;
				player.skipLanding = true;
				if ((bool)player.anim)
				{
					player.anim.Play("idle", -1, 0f);
					player.PlayAnim(idleHash);
				}
			}
			else if ((bool)player.anim)
			{
				player.anim.Play("fall", -1, 0f);
				player.PlayAnim(fallHash);
			}
			player.SetPosAndRotHard(vector, rot);
			player.timeSinceSpawn = 0f;
			if ((bool)player.cam)
			{
				player.cam.ResetCameraPositionRotation();
			}
		}

		public Player GetCurrentPlayer()
		{
			return currentPlayer;
		}

		private void UpdateCombatBlockers()
		{
			if (combatBlocked && currentCombatBlockerStartTime + currentCombatBlockerDuration < Time.time)
			{
				combatBlocked = false;
			}
		}

		public void ArbiterCombat()
		{
			UpdateCombatBlockers();
			if (battleMaxAttackers == -1 || battleMaxThreat == -1)
			{
				maxAttackers = WantedManager.instance.GetMaxAttacksForWantedLevel();
				maxThreat = WantedManager.instance.GetMaxThreatForWantedLevel();
			}
			else
			{
				maxAttackers = battleMaxAttackers;
				maxThreat = battleMaxThreat;
			}
			int num = 0;
			int count = attackingEnemies.Count;
			for (int i = 0; i < count; i++)
			{
				num += attackingEnemies[i].threat;
			}
			if (count < maxAttackers && num < maxThreat && !combatBlocked)
			{
				int count2 = waitingtoAttackEnemies.Count;
				if (count2 > 0)
				{
					weightedList.Clear();
					for (int j = 0; j < count2; j++)
					{
						BasicEnemy basicEnemy = waitingtoAttackEnemies[j];
						if (!(basicEnemy != null))
						{
							continue;
						}
						int combatArbiterBasePriority = basicEnemy.combatArbiterBasePriority;
						int num2 = combatArbiterBasePriority;
						num2 += basicEnemy.turnsWithoutAttack * combatArbiterBasePriority;
						basicEnemy.turnsWithoutAttack++;
						Vector3 position = basicEnemy.transform.position;
						if (IsReallyInScreen(position))
						{
							num2 += combatArbiterBasePriority;
						}
						else if (!IsKindaInScreen(position))
						{
							num2--;
						}
						if (basicEnemy.threat > maxThreat - num)
						{
							num2 /= 10;
						}
						if (num2 > 0)
						{
							for (int k = 0; k < num2; k++)
							{
								weightedList.Add(basicEnemy);
							}
						}
					}
					if (weightedList.Count > 0)
					{
						int index = UnityEngine.Random.Range(0, weightedList.Count);
						BasicEnemy basicEnemy2 = weightedList[index];
						if (basicEnemy2 != null && !attackingEnemies.Contains(basicEnemy2))
						{
							basicEnemy2.GrantAttackPermission();
							waitingtoAttackEnemies.Remove(basicEnemy2);
							attackingEnemies.Add(basicEnemy2);
							basicEnemy2.turnsWithoutAttack = 0;
						}
					}
				}
			}
			else
			{
				for (int num3 = attackingEnemies.Count - 1; num3 > -1; num3--)
				{
					if (attackingEnemies[num3] == null)
					{
						attackingEnemies.RemoveAt(num3);
					}
				}
			}
			if (circlingEnemies.Count < 4)
			{
				if (waitingtoCircleEnemies.Count <= 0)
				{
					return;
				}
				weightedList.Clear();
				foreach (BasicEnemy waitingtoCircleEnemy in waitingtoCircleEnemies)
				{
					if (!(waitingtoCircleEnemy != null))
					{
						continue;
					}
					int combatArbiterBasePriority2 = waitingtoCircleEnemy.combatArbiterBasePriority;
					int num4 = combatArbiterBasePriority2;
					num4 += waitingtoCircleEnemy.turnsWithoutAttack * combatArbiterBasePriority2;
					waitingtoCircleEnemy.turnsWithoutAttack++;
					if (IsReallyInScreen(waitingtoCircleEnemy.transform.position))
					{
						num4 += combatArbiterBasePriority2;
					}
					else if (!IsKindaInScreen(waitingtoCircleEnemy.transform.position))
					{
						num4--;
					}
					if (waitingtoCircleEnemy.threat > maxThreat - num)
					{
						num4 /= 2;
					}
					if (num4 > 0)
					{
						for (int l = 0; l < num4; l++)
						{
							weightedList.Add(waitingtoCircleEnemy);
						}
					}
				}
				if (weightedList.Count > 0)
				{
					int index2 = UnityEngine.Random.Range(0, weightedList.Count);
					BasicEnemy basicEnemy3 = weightedList[index2];
					if (basicEnemy3 != null && !circlingEnemies.Contains(basicEnemy3))
					{
						basicEnemy3.GrantCirclingPermission();
						waitingtoCircleEnemies.Remove(basicEnemy3);
						circlingEnemies.Add(basicEnemy3);
						basicEnemy3.turnsWithoutAttack = 0;
					}
				}
				return;
			}
			for (int num5 = circlingEnemies.Count - 1; num5 > -1; num5--)
			{
				if (circlingEnemies[num5] == null)
				{
					circlingEnemies.RemoveAt(num5);
				}
			}
		}

		public void UpdateWorldHandler()
		{
			_ = currentPlayer.tf.position;
			savePosTimer -= Core.dt;
			if (savePosTimer <= 0f)
			{
				savePosTimer = 10f;
				SavePosToNearestReachedRespawner();
			}
			sequenceHandler.UpdateSequenceHandler();
			UpdateCurrentCameraPosition();
			if (!currentPlayer.IsBusyWithSequence())
			{
				int num = int.MaxValue;
				List<GameplayEvent> gameplayEvents = sceneObjectsRegister.gameplayEvents;
				int count = gameplayEvents.Count;
				for (int i = 0; i < count; i++)
				{
					GameplayEvent gameplayEvent = gameplayEvents[i];
					if (!gameplayEvent.HasActiveREPRequirement())
					{
						continue;
					}
					Encounter encounter = gameplayEvent as Encounter;
					if (encounter != null)
					{
						if (!encounter.CompletedEncounter())
						{
							num = Mathf.Min(num, gameplayEvent.requirement);
						}
					}
					else
					{
						num = Mathf.Min(num, gameplayEvent.requirement);
					}
				}
				List<NPC> nPCs = sceneObjectsRegister.NPCs;
				int count2 = nPCs.Count;
				for (int j = 0; j < count2; j++)
				{
					NPC nPC = nPCs[j];
					if (nPC.HasActiveREPRequirement())
					{
						num = Mathf.Min(num, nPC.requirement);
					}
				}
				if (num < int.MaxValue)
				{
					SetRequiredREP(num);
				}
			}
			List<Player> players = sceneObjectsRegister.players;
			for (int num2 = players.Count - 1; num2 >= 0; num2--)
			{
				Player player = players[num2];
				if (player.enabled)
				{
					player.UpdatePlayer();
				}
			}
			List<GameplayEvent> gameplayEvents2 = sceneObjectsRegister.gameplayEvents;
			for (int num3 = gameplayEvents2.Count - 1; num3 >= 0; num3--)
			{
				gameplayEvents2[num3].UpdateGameplayEvent();
			}
			List<NPC> nPCs2 = sceneObjectsRegister.NPCs;
			for (int num4 = nPCs2.Count - 1; num4 >= 0; num4--)
			{
				nPCs2[num4].UpdateGameplayEvent();
			}
			List<Pickup> pickups = sceneObjectsRegister.pickups;
			int count3 = pickups.Count;
			for (int k = 0; k < count3; k++)
			{
				pickups[k].UpdatePickup();
			}
		}

		public void FixedUpdateWorldHandler()
		{
			for (int num = sceneObjectsRegister.players.Count - 1; num >= 0; num--)
			{
				if (sceneObjectsRegister.players[num].enabled)
				{
					sceneObjectsRegister.players[num].FixedUpdatePlayer();
				}
			}
		}

		private void UpdateCurrentCameraPosition()
		{
			currentCameraPosition = currentCameraTransform.position;
		}

		public void SetRequiredREP(int REP)
		{
			requiredREP = REP;
		}

		public void ResetRequiredREP()
		{
			requiredREP = 0;
		}

		public int AmountGrafSpotsDone()
		{
			int num = 0;
			foreach (GraffitiSpot grafSpot in sceneObjectsRegister.grafSpots)
			{
				if (grafSpot.ClaimedByPlayableCrew())
				{
					num++;
				}
			}
			return num;
		}

		public List<GraffitiSpot> GetGrafSpots(bool onlyUnclaimed)
		{
			List<GraffitiSpot> list = new List<GraffitiSpot>();
			foreach (GraffitiSpot grafSpot in sceneObjectsRegister.grafSpots)
			{
				if ((onlyUnclaimed && !grafSpot.ClaimedByPlayableCrew()) || !onlyUnclaimed)
				{
					list.Add(grafSpot);
				}
			}
			return list;
		}

		public void AllowRedoGrafspots()
		{
			List<GraffitiSpot> grafSpots = sceneObjectsRegister.grafSpots;
			int count = grafSpots.Count;
			for (int i = 0; i < count; i++)
			{
				grafSpots[i].allowRedo = true;
			}
		}

		public void StartEncounter(Encounter startEncounter)
		{
			currentEncounter = startEncounter;
			DestroyCharacterSelect();
			RemoveTempRacers();
			PublicToilet.CloseAllToilets();
			StartCoroutine(MakeUnavailableDuringEncounter(startEncounter));
		}

		private IEnumerator MakeUnavailableDuringEncounter(Encounter startEncounter)
		{
			yield return null;
			for (int i = 0; i < startEncounter.makeUnavailableDuringEncounter.Length; i++)
			{
				GameplayEvent component = startEncounter.makeUnavailableDuringEncounter[i].GetComponent<GameplayEvent>();
				if (component != null)
				{
					component.SetAvailable(set: false);
				}
				ProgressObject component2 = startEncounter.makeUnavailableDuringEncounter[i].GetComponent<ProgressObject>();
				if (component2 != null)
				{
					component2.SetAvailable(set: false);
				}
			}
			List<GameplayEvent> gameplayEvents = sceneObjectsRegister.gameplayEvents;
			for (int j = 0; j < gameplayEvents.Count; j++)
			{
				Encounter encounter = gameplayEvents[j] as Encounter;
				if (encounter != null && encounter != startEncounter)
				{
					encounter.SetAvailable(set: false);
				}
			}
			List<NPC> nPCs = sceneObjectsRegister.NPCs;
			for (int k = 0; k < nPCs.Count; k++)
			{
				NPC nPC = nPCs[k];
				if (nPC.unavailableForEveryEncounterExceptThis == null || nPC.unavailableForEveryEncounterExceptThis != startEncounter)
				{
					nPC.SetAvailable(set: false);
				}
			}
			List<GraffitiSpot> grafSpots = sceneObjectsRegister.grafSpots;
			for (int l = 0; l < grafSpots.Count; l++)
			{
				if (grafSpots[l].deactivateDuringEncounters && grafSpots[l].isOpen)
				{
					grafSpots[l].gameObject.SetActive(value: false);
				}
			}
		}

		public void SetNPCAvailabilityBasedOnCharacterSelect(CharacterSelectCharacter[] characters)
		{
			List<Characters> list = new List<Characters> { GetCurrentPlayer().character };
			for (int i = 0; i < characters.Length; i++)
			{
				if (characters[i] != null)
				{
					list.Add(characters[i].character);
				}
			}
			foreach (NPC nPC in sceneObjectsRegister.NPCs)
			{
				nPC.SetAvailable(!list.Contains(nPC.character));
			}
		}

		public void SetNPCAvailabilityBasedOnCharacter(Characters oldChar, Characters newChar)
		{
			if (oldChar == newChar)
			{
				return;
			}
			foreach (NPC nPC in sceneObjectsRegister.NPCs)
			{
				if (nPC.character == oldChar)
				{
					nPC.SetAvailable(set: true);
				}
				else if (nPC.character == newChar)
				{
					nPC.SetAvailable(set: false);
				}
			}
		}

		public bool CharacterIsAlreadyNPCInScene(Characters c)
		{
			foreach (NPC nPC in sceneObjectsRegister.NPCs)
			{
				if (nPC.character == c && nPC.isActive && nPC.dialogueLevel <= nPC.highestDialogLevel)
				{
					return true;
				}
			}
			return false;
		}

		public void SetNPCOutfit(Characters character, int outfit)
		{
			foreach (NPC nPC in sceneObjectsRegister.NPCs)
			{
				OutfitSwappableCharacter[] componentsInChildren = nPC.GetComponentsInChildren<OutfitSwappableCharacter>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i].Character == character)
					{
						Material mat = currentPlayer.CharacterConstructor.CreateCharacterMaterial(componentsInChildren[i].Character, outfit);
						componentsInChildren[i].SetMaterialAndOutfit(mat, outfit);
					}
				}
			}
			foreach (Player player in sceneObjectsRegister.players)
			{
				if (player.isAI && player.AI != null && player.AI.linkedNPC != null && player.AI.linkedNPC.GetComponentInChildren<OutfitSwappableCharacter>(includeInactive: true) != null && player.character == character && player.CharacterConstructor.GetCharacterMaterials()[(int)character, outfit] != null)
				{
					player.SetOutfit(outfit);
				}
			}
		}

		public void CreateCharacterSelect(Player p, CharacterSelectSpot spot)
		{
			DestroyCharacterSelect();
			characterSelect = UnityEngine.Object.Instantiate(spot.characterSelectPrefab);
			characterSelect.Init(p, spot);
			Core.OnUpdate += characterSelect.CharSelectUpdate;
		}

		public void DestroyCharacterSelect()
		{
			if (characterSelect != null)
			{
				_ = new CharacterSelectCharacter[0];
				UnityEngine.Object.Destroy(characterSelect.gameObject);
				Core.OnUpdate -= characterSelect.CharSelectUpdate;
				WantedManager.OnWanted -= characterSelect.OnWanted;
			}
		}

		public void RemoveTempRacers(NPC linkedToNPC = null)
		{
			for (int num = sceneObjectsRegister.players.Count - 1; num >= 0; num--)
			{
				if (sceneObjectsRegister.players[num].playerType == PlayerType.TEMP_RACER_AI && (linkedToNPC == null || linkedToNPC == sceneObjectsRegister.players[num].AI.linkedNPC))
				{
					if (sceneObjectsRegister.players[num].AI.activateNPC)
					{
						sceneObjectsRegister.players[num].AI.linkedNPC.activeTempRacerWithDialogue = false;
					}
					UnityEngine.Object.Destroy(sceneObjectsRegister.players[num].gameObject);
					sceneObjectsRegister.players.Remove(sceneObjectsRegister.players[num]);
				}
			}
		}

		public void MakeStuffAvailableAgainCauseEncounterOver()
		{
			List<GameplayEvent> gameplayEvents = sceneObjectsRegister.gameplayEvents;
			for (int i = 0; i < gameplayEvents.Count; i++)
			{
				gameplayEvents[i].SetAvailable(set: true);
			}
			SetNPCAvailabilityBasedOnPlayer();
			List<ProgressObject> progressObjects = sceneObjectsRegister.progressObjects;
			for (int j = 0; j < progressObjects.Count; j++)
			{
				progressObjects[j].SetAvailable(set: true);
				progressObjects[j].SetTriggerable(set: true);
			}
			List<GraffitiSpot> grafSpots = sceneObjectsRegister.grafSpots;
			for (int k = 0; k < grafSpots.Count; k++)
			{
				if (grafSpots[k].deactivateDuringEncounters && grafSpots[k].isOpen)
				{
					grafSpots[k].gameObject.SetActive(value: true);
				}
			}
			PublicToilet.OpenAllToilets();
			currentEncounter = null;
		}

		public void ActivateHumanPlayerGameObject(bool set)
		{
			Player player = GetCurrentPlayer();
			player.gameObject.SetActive(set);
			player.curAnim = noneHash;
		}

		public bool IsKindaInScreen(Vector3 pos)
		{
			return Vector3.Dot(pos - currentCamera.transform.position, currentCamera.transform.forward) > 0.35f;
		}

		public bool IsReallyInScreen(Vector3 pos)
		{
			Vector3 vector = currentCamera.WorldToViewportPoint(pos);
			if (vector.x > 0f && vector.y > 0f && vector.y <= 1f && vector.x <= 1f)
			{
				return vector.z > 0f;
			}
			return false;
		}

		public void SubscribeToCombatArbiter(BasicEnemy enemy)
		{
			if (!subscribedEnemies.Contains(enemy))
			{
				subscribedEnemies.Add(enemy);
			}
		}

		public void UnsubscribeToCombatArbiter(BasicEnemy enemy)
		{
			if (subscribedEnemies.Contains(enemy))
			{
				subscribedEnemies.Remove(enemy);
			}
			if (attackingEnemies.Contains(enemy))
			{
				attackingEnemies.Remove(enemy);
			}
			if (waitingtoAttackEnemies.Contains(enemy))
			{
				waitingtoAttackEnemies.Remove(enemy);
			}
			if (circlingEnemies.Contains(enemy))
			{
				circlingEnemies.Remove(enemy);
			}
			if (waitingtoCircleEnemies.Contains(enemy))
			{
				waitingtoCircleEnemies.Remove(enemy);
			}
			enemy.RetractAttackPermission();
			enemy.RetractCirclingPermission();
		}

		public void RequestAttack(BasicEnemy enemy)
		{
			if (subscribedEnemies.Contains(enemy) && !waitingtoAttackEnemies.Contains(enemy))
			{
				waitingtoAttackEnemies.Add(enemy);
			}
		}

		public void ReleaseAttack(BasicEnemy enemy)
		{
			if (attackingEnemies.Contains(enemy))
			{
				attackingEnemies.Remove(enemy);
				enemy.RetractAttackPermission();
			}
		}

		public void RequestCirclingPosition(BasicEnemy enemy)
		{
			if (subscribedEnemies.Contains(enemy) && !waitingtoCircleEnemies.Contains(enemy))
			{
				waitingtoCircleEnemies.Add(enemy);
			}
		}

		public void ReleaseCirclingPosition(BasicEnemy enemy)
		{
			if (circlingEnemies.Contains(enemy))
			{
				circlingEnemies.Remove(enemy);
				enemy.RetractCirclingPermission();
			}
		}

		public void AddCombatBlocker(float duration)
		{
			combatBlocked = true;
			currentCombatBlockerDuration = duration;
			currentCombatBlockerStartTime = Time.time;
			for (int i = 0; i < attackingEnemies.Count; i++)
			{
				attackingEnemies[i].RetractAttackPermission();
			}
			attackingEnemies.Clear();
		}

		public void SortEnemiesByDistance()
		{
			subscribedEnemies.Sort((BasicEnemy a, BasicEnemy b) => (currentPlayer.tf.position - a.tf.position).sqrMagnitude.CompareTo((currentPlayer.tf.position - b.tf.position).sqrMagnitude));
		}

		public void SetBattleValues(int attackers, int threat)
		{
			battleMaxAttackers = attackers;
			battleMaxThreat = threat;
		}

		public void ResetbattleValues()
		{
			battleMaxThreat = (battleMaxAttackers = -1);
		}

		private AudioSource GetAvailableAudioSource()
		{
			foreach (AudioSource audioSource in audioSources)
			{
				if (!audioSource.isPlaying)
				{
					return audioSource;
				}
			}
			return null;
		}

		private AudioSource AddAudioSource()
		{
			AudioSource component = UnityEngine.Object.Instantiate(environmentalAudioSourcePrefab).transform.GetComponent<AudioSource>();
			audioSources.Add(component);
			return component;
		}

		public bool IsPlayingEnvironmentAudio(AudioClipID audioClipId)
		{
			AudioClip audioClipFromID = Core.Instance.AudioManager.GetAudioClipFromID(SfxCollectionID.EnvironmentSfx, audioClipId);
			foreach (AudioSource audioSource in audioSources)
			{
				if (audioSource.clip == audioClipFromID && audioSource.isPlaying)
				{
					return true;
				}
			}
			return false;
		}

		public AudioSource GetSpatialAudioSource(Vector3 position)
		{
			AudioSource audioSource = null;
			audioSource = GetAvailableAudioSource();
			if (audioSource == null)
			{
				audioSource = AddAudioSource();
			}
			audioSource.transform.position = position;
			return audioSource;
		}

		public static int GetGameplayCameraCullingMask()
		{
			return -1292392941;
		}

		public PlayerAIPath GetPathFromPathID(int pathID)
		{
			PlayerAIPath result = null;
			if (pathID <= 0)
			{
				return result;
			}
			foreach (PlayerAIPath path in sceneObjectsRegister.paths)
			{
				if (path.pathID == pathID)
				{
					return result;
				}
			}
			return result;
		}

		public void SetCurrentCamera(Camera camera)
		{
			Camera camera2 = camera;
			if (camera2 == null)
			{
				camera2 = currentPlayer.cam.cam;
				if (!(camera2 != null))
				{
					return;
				}
				camera2.enabled = true;
			}
			else if (currentCamera == currentPlayer.cam.cam)
			{
				currentCamera.enabled = false;
			}
			currentCamera = camera2;
			currentCamera.enabled = true;
			currentCameraTransform = camera2.transform;
			currentCameraPosition = currentCameraTransform.position;
			Core core = Core.Instance;
			if ((bool)core)
			{
				AudioManager audioManager = core.AudioManager;
				if (audioManager != null)
				{
					AudioManager.SetActiveAudioListener(audioManager, currentCamera.GetComponent<AudioListener>());
				}
			}
			StartCoroutine(DelayedCameraBasedChecks());
		}

		private IEnumerator DelayedCameraBasedChecks()
		{
			yield return null;
			int num = Physics.OverlapSphereNonAlloc(currentCameraPosition, 1f, ambientColliders, 524289, QueryTriggerInteraction.Collide);
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				AmbientTrigger component = ambientColliders[i].GetComponent<AmbientTrigger>();
				if (component != null)
				{
					component.ambientManager.InAmbientTriggerInstant(component);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				RenderSettings.sun.GetComponent<AmbientManager>().RevertAmbient(0f);
			}
			List<StreetLifeCluster> streetlifeClusters = sceneObjectsRegister.streetlifeClusters;
			for (int j = 0; j < streetlifeClusters.Count; j++)
			{
				streetlifeClusters[j].ResetStreetlifeIfActive();
			}
		}

		public Rigidbody SpawnJunk(DynamicJunk junkToSpawn, Vector3 position, Quaternion rotation)
		{
			return dynamicJunkHandler.AddJunk(junkToSpawn, position, rotation);
		}
	}
}
