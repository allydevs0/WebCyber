using System;
using System.Collections;
using System.Collections.Generic;
using Reptile.Phone;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

namespace Reptile
{
	public class StageManager
	{
		public delegate void OnStageInitializedDelegate();

		private const int NO_GRAFFITISPOT_FOUND_INDEX = -1;

		private readonly int[] MENU_CONTROLLER_MAPS = new int[1] { 1 };

		public PlayableDirector introSequence;

		private SaveManager saveManager;

		private BaseModule baseModule;

		private WorldHandler worldHandler;

		private WantedManager wantedManager;

		private Assets assets;

		internal StoryManager storyManager;

		private GameInput gameInput;

		private ChapterMusic chapterMusic;

		private Achievements achievements;

		private CharacterConstructor characterConstructor;

		private StageProgress currentStageProgress;

		private IMusicLibraryPlayer musicLibraryPlayer;

		private IMusicPlayer musicPlayer;

		private GamePrefabLoader gamePrefabLoader;

		private ADownloadableContent downloadableContent;

		private float currentExtendLoadingScreenProgress;

		private float maximalExtendLoadingScreenSeconds;

		public bool IsExtendingLoadingScreen => maximalExtendLoadingScreenSeconds > 0f;

		public bool IsPlayerInSequence => worldHandler.GetCurrentPlayer().sequenceState == SequenceState.IN_SEQUENCE;

		public static event OnStageInitializedDelegate OnStageInitialized;

		public static event OnStageInitializedDelegate OnStagePostInitialization;

		public StageManager(SaveManager saveManager, BaseModule baseModule, Assets assets, GameInput gameInput, ChapterMusic chapterMusic, Achievements achievements, ADownloadableContent downloadableContent)
		{
			this.saveManager = saveManager;
			this.baseModule = baseModule;
			this.assets = assets;
			this.gameInput = gameInput;
			this.chapterMusic = chapterMusic;
			this.achievements = achievements;
			this.downloadableContent = downloadableContent;
			musicLibraryPlayer = Core.Instance.AudioManager.MusicLibraryPlayer;
			musicPlayer = Core.Instance.AudioManager.MusicPlayer;
			Player.OnExitTriggerHit += OnExitTriggerHit;
		}

		public void Dispose()
		{
			if (characterConstructor != null)
			{
				characterConstructor.Dispose();
			}
			musicPlayer.ForcePaused();
			musicPlayer.Clear();
			if (Mapcontroller.Instance != null)
			{
				Mapcontroller.Instance.DisposeMap();
			}
			gamePrefabLoader.Dispose();
			worldHandler.StopUpdatingWorld();
			saveManager = null;
			baseModule = null;
			worldHandler = null;
			characterConstructor = null;
			musicLibraryPlayer = null;
			musicPlayer = null;
			storyManager = null;
			Player.OnExitTriggerHit -= OnExitTriggerHit;
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(OnPostRender));
		}

		public IEnumerator ExtendLoadingScreen(float maximalExtendLoadingScreenSeconds)
		{
			if (maximalExtendLoadingScreenSeconds == 0f)
			{
				yield break;
			}
			this.maximalExtendLoadingScreenSeconds = maximalExtendLoadingScreenSeconds;
			float startTime = Time.realtimeSinceStartup;
			float currentTime = startTime;
			float endTime = startTime + maximalExtendLoadingScreenSeconds;
			Player player = worldHandler.GetCurrentPlayer();
			while (currentTime < endTime)
			{
				if (player.sequenceState == SequenceState.NONE)
				{
					currentTime = Time.realtimeSinceStartup;
					currentExtendLoadingScreenProgress = (currentTime - startTime) / maximalExtendLoadingScreenSeconds;
				}
				else if (player.sequenceState == SequenceState.ENTERING)
				{
					currentExtendLoadingScreenProgress = 1f;
				}
				else if (player.sequenceState == SequenceState.IN_SEQUENCE)
				{
					break;
				}
				yield return null;
			}
			DoStagePostInitialization();
			this.maximalExtendLoadingScreenSeconds = 0f;
		}

		public float GetExtendedLoadingScreenProgress()
		{
			return currentExtendLoadingScreenProgress;
		}

		public void OnExitTriggerHit(Player player, StageTransition exit)
		{
			exit.PlayerHitTrigger(player, this);
		}

		public void SetupStage(Stage prevStage, Stage newStage, DownloadableContentItem characterDownloadableContent)
		{
			CharacterLoader assetsLoader = assets.GetAssetsLoader<CharacterLoader>();
			MoveStyleLoader assetsLoader2 = assets.GetAssetsLoader<MoveStyleLoader>();
			gamePrefabLoader = assets.GetAssetsLoader<GamePrefabLoader>();
			characterConstructor = new CharacterConstructor(assetsLoader, assetsLoader2);
			characterConstructor.ConstructAllCharacterVisuals();
			storyManager = new StoryManager();
			LoadStageProgress(newStage);
			SetupWorldHandler(gamePrefabLoader.WorldHandlerPrefab, gamePrefabLoader.WantedManagerPrefab, newStage, prevStage, characterDownloadableContent);
			ApplyLoadedStageProgress();
			storyManager.Init(newStage);
			InitializeMapController(gamePrefabLoader.MapControllerPrefab, newStage);
			RestoreCurrentPlayerInput();
			Core.Instance.AudioManager.SetStageAudio(newStage);
			Core.Instance.AudioManager.ResetTemporaryVolumes();
			if (OnStageInitialized != null)
			{
				OnStageInitialized();
			}
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(OnPostRender));
		}

		public void SetStoryObjective(Story.ObjectiveID newObjective)
		{
			storyManager.SetStoryObjective(newObjective);
		}

		public bool IsPauseMenuAllowed()
		{
			if (SequenceHandler.instance.IsInSequenceTransition())
			{
				return false;
			}
			if (worldHandler == null)
			{
				return false;
			}
			if (IsExtendingLoadingScreen)
			{
				return false;
			}
			Player currentPlayer = worldHandler.GetCurrentPlayer();
			if (currentPlayer != null)
			{
				return IsPlayerAllowingPauseMenu(currentPlayer);
			}
			return true;
		}

		private bool IsPlayerAllowingPauseMenu(Player player)
		{
			if (player.ability == null)
			{
				return true;
			}
			if (!player.ability.Equals(player.dieAbility))
			{
				return !player.ability.Equals(player.danceAbility);
			}
			return false;
		}

		private void SetupWorldHandler(GameObject worldHandlerPrefab, GameObject wantedManagerPrefab, Stage newStage, Stage prevStage, DownloadableContentItem characterDownloadableContent)
		{
			CreateWorldHandler(worldHandlerPrefab);
			CreateWantedManager(wantedManagerPrefab);
			worldHandler.RegisterAllSceneObjects();
			Characters playerCharacter = GetPlayerCharacter(characterDownloadableContent);
			PlayerSpawner playerSpawnPoint = worldHandler.GetPlayerSpawnPoint(newStage, prevStage, saveManager);
			playerSpawnPoint.isReached = true;
			worldHandler.stageEntrySpawner = playerSpawnPoint;
			Player player = worldHandler.CreateHumanPlayerAt(playerSpawnPoint.transform);
			GraffitiLoader assetsLoader = assets.GetAssetsLoader<GraffitiLoader>();
			worldHandler.RegisterPlayer(player);
			worldHandler.Init(characterConstructor, assetsLoader.GraffitiArtInfo, storyManager, saveManager);
			CharacterProgress characterProgress = saveManager.CurrentSaveSlot.GetCharacterProgress(playerCharacter);
			MoveStyle moveStyleEquipped = characterProgress.moveStyle;
			if (newStage == Stage.Prelude)
			{
				moveStyleEquipped = MoveStyle.INLINE;
			}
			worldHandler.InitPlayerObject(player, playerCharacter, characterProgress.outfit, moveStyleEquipped);
			worldHandler.PlaceCurrentPlayerAt(playerSpawnPoint.transform);
			worldHandler.SetNPCAvailabilityBasedOnPlayer();
			worldHandler.SetNPCVisuals();
			wantedManager.Init();
		}

		private void CreateWorldHandler(GameObject worldHandlerPrefab)
		{
			worldHandler = UnityEngine.Object.Instantiate(worldHandlerPrefab).GetComponent<WorldHandler>();
		}

		private void CreateWantedManager(GameObject wantedManagerPrefab)
		{
			wantedManager = UnityEngine.Object.Instantiate(wantedManagerPrefab).GetComponent<WantedManager>();
		}

		public void RestartStage()
		{
			Stage currentStage = Utility.GetCurrentStage();
			ExitCurrentStage(currentStage, currentStage);
		}

		public void ExitCurrentStage(Stage nextStage, Stage prevStageSpecifiedForSpawner = Stage.NONE)
		{
			baseModule.StartCoroutine(ExitCurrentStageAtEndOfFrame(nextStage, prevStageSpecifiedForSpawner));
		}

		public void ExitCurrentStageImmediately(Stage nextStage, Stage prevStageSpecifiedForSpawner = Stage.NONE)
		{
			saveManager.CurrentSaveSlot.CurrentStage = nextStage;
			baseModule.lastPlayingSongOnStageExistStageID = nextStage;
			baseModule.lastPlayingSongOnStageExistSamples = musicPlayer.CurrentTrackSamples;
			baseModule.lastPlayingSongOnStageExistInstanceID = musicPlayer.GetMusicTrack(musicPlayer.CurrentTrackIndex).GetInstanceID();
			baseModule.SaveBeforeStageExit();
			if (prevStageSpecifiedForSpawner == Stage.NONE)
			{
				baseModule.SwitchStage(nextStage);
			}
			else
			{
				baseModule.SwitchStage(nextStage, prevStageSpecifiedForSpawner);
			}
		}

		public void RestoreCurrentPlayerInput()
		{
			int[] playerCurrentInputMaps = GetPlayerCurrentInputMaps();
			gameInput.DisableAllControllerMaps();
			gameInput.EnableControllerMaps(playerCurrentInputMaps);
		}

		public void RestorePlayerInputFromPause()
		{
			Player currentPlayer = worldHandler.GetCurrentPlayer();
			int[] controllerMapCategoryIDs = ((!SequenceHandler.instance.IsInSequence() && !SequenceHandler.instance.IsInSequenceTransition() && !currentPlayer.IsSelectingCharacter) ? GetPlayerCurrentInputMaps() : BaseModule.MENU_INPUT_MAPS);
			gameInput.DisableAllControllerMaps();
			gameInput.EnableControllerMaps(controllerMapCategoryIDs);
		}

		public void CheckUniqueGraffitiAchievement()
		{
			if (!Core.Instance.Platform.HasAchievementsSupport || achievements.HasUnlockedAchievement(AchievementType.StateOfTheArt))
			{
				return;
			}
			List<GraffitiSpot> stageGraffitiSpots = worldHandler.SceneObjectsRegister.grafSpots;
			for (int num = stageGraffitiSpots.Count - 1; num >= 0; num--)
			{
				if (stageGraffitiSpots[num] is GraffitiSpotFinisher)
				{
					stageGraffitiSpots.Remove(stageGraffitiSpots[num]);
				}
			}
			int i;
			for (i = 0; i < stageGraffitiSpots.Count; i++)
			{
				if (!stageGraffitiSpots[i].ClaimedByPlayableCrew() || stageGraffitiSpots.FindIndex((GraffitiSpot listGraffitiSpot) => FindUniqueGraffiti(listGraffitiSpot, stageGraffitiSpots[i])) > -1)
				{
					return;
				}
			}
			achievements.UnlockAchievement(AchievementType.StateOfTheArt);
		}

		private bool FindUniqueGraffiti(GraffitiSpot graffitiSpot, GraffitiSpot compareTo)
		{
			if (graffitiSpot.topGraffitiArt == null)
			{
				return false;
			}
			if (compareTo.topGraffitiArt == null)
			{
				return false;
			}
			if (graffitiSpot.Equals(compareTo))
			{
				return false;
			}
			return graffitiSpot.topGraffitiArt.title.Equals(compareTo.topGraffitiArt.title);
		}

		internal void StartOrContinueStageTrack(MusicTrack musicTrack, int playbackSamples)
		{
			MusicTrack musicTrack2 = musicPlayer.GetMusicTrack(musicPlayer.CurrentTrackIndex);
			if (musicTrack2 == null || musicTrack2.AudioClip != musicTrack.AudioClip)
			{
				musicPlayer.Clear();
				musicPlayer.AddAndBufferAsCurrentTrack(musicTrack);
				musicPlayer.PlayFrom(0, playbackSamples);
			}
			else if (!musicPlayer.IsPlaying)
			{
				musicPlayer.ClearExceptCurrentTrack();
				musicPlayer.PlayFrom(0, playbackSamples);
			}
		}

		internal void StartMusicForStage(Stage stageID, MusicTrackID trackID, int playbackSamples)
		{
			if (baseModule.lastPlayingSongOnStageExistStageID == stageID && playbackSamples == 0)
			{
				playbackSamples = baseModule.lastPlayingSongOnStageExistSamples;
			}
			MusicTrack musicTrackByID = musicLibraryPlayer.GetMusicTrackByID(trackID);
			StartOrContinueStageTrack(musicTrackByID, playbackSamples);
		}

		private MusicTrack GetCurrentNonChapterTrackToPlay(int playbackSamples, ref int playbackSamplesToPlay)
		{
			MusicTrack result = null;
			Player currentPlayer = worldHandler.GetCurrentPlayer();
			if ((bool)currentPlayer)
			{
				AUnlockable[] unlockables = currentPlayer.phone.GetAppInstance<AppMusicPlayer>().Unlockables;
				if (unlockables != null)
				{
					MusicTrack musicTrack = null;
					for (int i = 0; i < unlockables.Length; i++)
					{
						if (unlockables[i].GetInstanceID() == baseModule.lastPlayingSongOnStageExistInstanceID)
						{
							musicTrack = unlockables[i] as MusicTrack;
							break;
						}
					}
					if ((bool)musicTrack && !chapterMusic.IsChapterMusic(musicTrack))
					{
						result = musicTrack;
						if (playbackSamples == 0)
						{
							playbackSamplesToPlay = baseModule.lastPlayingSongOnStageExistSamples;
						}
					}
				}
			}
			return result;
		}

		private void StartMusicForStage(Stage stage, int playbackSamples = 0)
		{
			if (stage == Stage.Prelude)
			{
				StartMusicForStage(stage, MusicTrackID.Prelude_Music, playbackSamples);
				return;
			}
			int playbackSamplesToPlay = playbackSamples;
			MusicTrack currentNonChapterTrackToPlay = GetCurrentNonChapterTrackToPlay(playbackSamples, ref playbackSamplesToPlay);
			if (stage == Stage.hideout)
			{
				StartMusicForStage(stage, MusicTrackID.Hideout_Mixtape, playbackSamples);
			}
			else
			{
				if (currentNonChapterTrackToPlay == null)
				{
					Story.ObjectiveInfo currentObjectiveInfo = Story.GetCurrentObjectiveInfo();
					currentNonChapterTrackToPlay = chapterMusic.GetChapterMusic(currentObjectiveInfo.chapter);
					if (currentNonChapterTrackToPlay.GetInstanceID() == baseModule.lastPlayingSongOnStageExistInstanceID && playbackSamples == 0)
					{
						playbackSamplesToPlay = baseModule.lastPlayingSongOnStageExistSamples;
					}
				}
				StartOrContinueStageTrack(currentNonChapterTrackToPlay, playbackSamplesToPlay);
			}
			musicPlayer.RefreshMusicQueueForStage(chapterMusic, currentNonChapterTrackToPlay, stage);
		}

		public void StartMusic(int playbackSamples = 0)
		{
			Stage currentStage = baseModule.CurrentStage;
			StartMusicForStage(currentStage, playbackSamples);
		}

		private IEnumerator ExitCurrentStageAtEndOfFrame(Stage nextStage, Stage prevStageSpecifiedForSpawner = Stage.NONE)
		{
			yield return new WaitForEndOfFrame();
			ExitCurrentStageImmediately(nextStage, prevStageSpecifiedForSpawner);
		}

		private void LoadStageProgress(Stage newStage)
		{
			saveManager.CurrentSaveSlot.CurrentStage = newStage;
			saveManager.CurrentSaveSlot.SetCurrentStageProgress(newStage);
			currentStageProgress = saveManager.CurrentSaveSlot.GetStageProgress(newStage);
		}

		private void ApplyLoadedStageProgress()
		{
			List<AProgressable> allProgressablesInScene = worldHandler.GetAllProgressablesInScene();
			for (int i = 0; i < allProgressablesInScene.Count; i++)
			{
				allProgressablesInScene[i].InitProgressable(currentStageProgress);
			}
			if (currentStageProgress.IsFirstVisitToStage)
			{
				saveManager.SaveCurrentSaveSlot();
			}
		}

		private Characters GetPlayerCharacter(DownloadableContentItem characterDownloadableContent)
		{
			Characters characters = saveManager.CurrentSaveSlot.currentCharacter;
			if (!downloadableContent.IsDownloadableContentUnlocked(characterDownloadableContent) && (characters == Characters.skate || characters == Characters.robot))
			{
				characters = Characters.metalHead;
			}
			return characters;
		}

		private void OnPostRender(Camera camera)
		{
			if (camera.commandBufferCount <= 0)
			{
				return;
			}
			CommandBuffer[] commandBuffers = camera.GetCommandBuffers(CameraEvent.BeforeReflections);
			for (int i = 0; i < commandBuffers.Length; i++)
			{
				if (commandBuffers[i].name.Equals("Easy Decals"))
				{
					camera.RemoveCommandBuffer(CameraEvent.BeforeReflections, commandBuffers[i]);
					break;
				}
			}
		}

		private void InitializeMapController(GameObject mapControllerPrefab, Stage newStage)
		{
			UnityEngine.Object.Instantiate(mapControllerPrefab);
			Mapcontroller.Instance.InitMap(Mapcontroller.Instance.GetMap(newStage), WorldHandler.instance.GetCurrentPlayer(), storyManager);
			Mapcontroller.Instance.CreatePins();
			Mapcontroller.Instance.InitController();
			Mapcontroller.Instance.UpdateObjectivePins();
		}

		private int[] GetPlayerCurrentInputMaps()
		{
			if (worldHandler.GetCurrentPlayer().inGraffitiGame)
			{
				return BaseModule.IN_GAME_GRAFFITI_INPUT_MAPS;
			}
			return BaseModule.IN_GAME_INPUT_MAPS;
		}

		private void DoStagePostInitialization()
		{
			if (!musicPlayer.IsPlaying)
			{
				StartMusicForStage(baseModule.CurrentStage);
			}
			if (OnStagePostInitialization != null)
			{
				OnStagePostInitialization();
			}
		}
	}
}
