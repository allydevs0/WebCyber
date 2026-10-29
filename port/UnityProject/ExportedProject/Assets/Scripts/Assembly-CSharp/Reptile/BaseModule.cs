using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reptile
{
	public class BaseModule : MonoBehaviour
	{
		public delegate void UserLoginFinishedDelegate(bool success, AUser user);

		public static readonly int[] IN_GAME_INPUT_MAPS = new int[2] { 0, 6 };

		public static readonly int[] IN_GAME_GRAFFITI_INPUT_MAPS = new int[1] { 4 };

		public static readonly int[] MENU_INPUT_MAPS = new int[1] { 1 };

		private const float EXTENDED_LOADING_SCREEN_TIME = 0.5f;

		internal static bool waitForSaveAndLogout;

		private static BaseModule instance = null;

		internal int lastPlayingSongOnStageExistSamples;

		internal int lastPlayingSongOnStageExistInstanceID;

		internal Stage lastPlayingSongOnStageExistStageID;

		internal Coroutine introLoggingInCoroutine;

		private SceneLoader sceneLoader;

		private Assets assets;

		private SaveManager saveManager;

		private UIManager uiManager;

		private SceneLibrary sceneLibrary;

		private StageManager stageManager;

		private GameParameters gameParameters;

		private AUser user;

		private GameInput gameInput;

		private AGameDebug gameDebug;

		private IMusicLibraryPlayer musicLibraryPlayer;

		private IMusicPlayer musicPlayer;

		private AudioManager audioManager;

		private List<ASceneSetupInstruction> currentSceneSetupInstructions;

		private ADownloadableContent downloadableContent;

		[SerializeField]
		private DownloadableContentItem characterDownloadableContent;

		private Stage currentStage = Stage.NONE;

		private int isGamePaused;

		private string coreSceneName = string.Empty;

		private bool isGoingBackToIntro;

		public Stage CurrentStage => currentStage;

		public StageManager StageManager => stageManager;

		public bool IsInGamePaused => isGamePaused != 0;

		public bool IsPlayingInStage => currentStage != Stage.NONE;

		public bool IsLoading => currentSceneSetupInstructions.Count > 0;

		public event UserLoginFinishedDelegate OnUserLoginFinished;

		public void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else
			{
				UnityEngine.Object.Destroy(this);
			}
		}

		public void OnDestroy()
		{
			if (GetInstanceID() == instance.GetInstanceID())
			{
				assets = null;
				sceneLoader = null;
				saveManager = null;
				uiManager = null;
				sceneLibrary = null;
				gameParameters = null;
				user = null;
				DisposeStageManager();
				DisposeSceneSetupInstructions();
				instance = null;
			}
		}

		public void Init(Assets assets, SceneLoader sceneLoader, SaveManager saveManager, UIManager uiManager, SceneLibrary sceneLibrary, GameParameters gameParameters, AUser user, GameInput gameInput, AGameDebug gameDebug, AudioManager audioManager, ADownloadableContent downloadableContent)
		{
			this.assets = assets;
			this.sceneLoader = sceneLoader;
			this.saveManager = saveManager;
			this.uiManager = uiManager;
			this.sceneLibrary = sceneLibrary;
			this.gameParameters = gameParameters;
			this.user = user;
			this.gameInput = gameInput;
			this.gameDebug = gameDebug;
			this.audioManager = audioManager;
			musicLibraryPlayer = audioManager.MusicLibraryPlayer;
			musicPlayer = audioManager.MusicPlayer;
			this.downloadableContent = downloadableContent;
			waitForSaveAndLogout = false;
			coreSceneName = Path.GetFileNameWithoutExtension(sceneLibrary.coreScenePath);
			currentSceneSetupInstructions = new List<ASceneSetupInstruction>();
		}

		public bool HasPauseType(PauseType pauseType)
		{
			return ((uint)isGamePaused & (uint)pauseType) == (uint)pauseType;
		}

		private void DisposeStageManager()
		{
			if (stageManager != null)
			{
				stageManager.Dispose();
				stageManager = null;
			}
		}

		private void DisposeSceneSetupInstructions()
		{
			if (currentSceneSetupInstructions != null)
			{
				currentSceneSetupInstructions.Clear();
				currentSceneSetupInstructions = null;
			}
		}

		public void StartGame()
		{
			gameInput.SetUICursorMode();
		}

		public void StartLoginAfterIntro()
		{
			introLoggingInCoroutine = StartCoroutine(StartLoginAfterIntroRoutine());
		}

		private IEnumerator StartLoginAfterIntroRoutine()
		{
			ShowLoadingScreen();
			yield return user.LoginASync();
			if (user.IsLoggedIn)
			{
				yield return HandleUserLoggedIn();
			}
			HideLoadingScreen();
			TriggerPostLoginAttemptEvents(user.IsLoggedIn);
			introLoggingInCoroutine = null;
		}

		public void StartGameToStage(Stage stage)
		{
			StartCoroutine(StartGameToStageRoutine(stage));
		}

		private IEnumerator StartGameToStageRoutine(Stage stage)
		{
			ShowLoadingScreen();
			yield return user.LoginASync();
			if (user.IsLoggedIn)
			{
				yield return user.LoadUserSaveData();
				TriggerPostLoginAttemptEvents(user.IsLoggedIn);
				uiManager.InstantiateMainMenuSceneUI();
				StartGameFromCurrentSaveSlotToStage(stage);
			}
			else
			{
				GoBackToIntro();
			}
		}

		private IEnumerator HandleUserLoggedIn()
		{
			yield return user.LoadUserSaveData();
			if (user.IsLoggedIn)
			{
				yield return LoadMainMenuAfterLogin();
			}
		}

		private void TriggerPostLoginAttemptEvents(bool success)
		{
			if (OnUserLoginFinished != null)
			{
				OnUserLoginFinished(success, user);
			}
		}

		private IEnumerator LoadMainMenuAfterLogin()
		{
			uiManager.InstantiateMainMenuSceneUI();
			ASceneSetupInstruction sceneLoadInstruction = PrepareUnloadIntroInstruction();
			StartSceneSetup(sceneLoadInstruction);
			ASceneSetupInstruction sceneLoadInstruction2 = PrepareLoadMainMenuSceneInstruction();
			StartSceneSetup(sceneLoadInstruction2);
			while (IsLoading)
			{
				yield return null;
			}
		}

		public void LoadIntro()
		{
			ASceneSetupInstruction sceneLoadInstruction = new LoadSceneInstruction(Path.GetFileNameWithoutExtension(sceneLibrary.introScenePath), setActiveAfterLoad: true);
			StartSceneSetup(sceneLoadInstruction);
		}

		private ASceneSetupInstruction PrepareUnloadIntroInstruction()
		{
			return new UnloadSceneInstruction(Path.GetFileNameWithoutExtension(sceneLibrary.introScenePath), string.Empty);
		}

		public void LoadMainMenuScene()
		{
			ASceneSetupInstruction sceneLoadInstruction = PrepareLoadMainMenuSceneInstruction();
			StartSceneSetup(sceneLoadInstruction);
		}

		private ASceneSetupInstruction PrepareLoadMainMenuSceneInstruction()
		{
			return new LoadSceneInstruction(Path.GetFileNameWithoutExtension(sceneLibrary.mainMenuScenePath), setActiveAfterLoad: true);
		}

		public void UnloadMainMenuScene()
		{
			ASceneSetupInstruction sceneLoadInstruction = new UnloadSceneInstruction(Path.GetFileNameWithoutExtension(sceneLibrary.mainMenuScenePath), coreSceneName);
			StartSceneSetup(sceneLoadInstruction);
		}

		public void LoadCreditsScene()
		{
			ASceneSetupInstruction sceneLoadInstruction = new LoadSceneInstruction(Path.GetFileNameWithoutExtension(sceneLibrary.creditsScenePath), setActiveAfterLoad: true);
			StartSceneSetup(sceneLoadInstruction);
		}

		public void UnloadCreditsScene()
		{
			ASceneSetupInstruction sceneLoadInstruction = new UnloadSceneInstruction(Path.GetFileNameWithoutExtension(sceneLibrary.creditsScenePath), coreSceneName);
			StartSceneSetup(sceneLoadInstruction);
		}

		public void ShowMainMenu()
		{
			Core.Instance.UnPauseCore(PauseType.ForceOff);
			gameInput.DisableAllControllerMaps();
			gameInput.EnableControllerMap(1);
			uiManager.ShowMainMenu();
			StartMainMenuSceneMusic();
		}

		public void StopMainMenuSceneMusic()
		{
			musicPlayer.Pause();
			musicPlayer.Clear();
		}

		public void ShowLoadingScreen()
		{
			uiManager.overlay.ShowLoadingScreen();
			gameInput.SetCursorVisibility(visible: false);
			gameInput.SetCursorLockMode(CursorLockMode.None);
		}

		public void HideLoadingScreen()
		{
			uiManager.overlay.HideLoadingScreen();
		}

		public void LoadScene(string sceneName)
		{
			ASceneSetupInstruction sceneLoadInstruction = new LoadSceneInstruction(sceneName, setActiveAfterLoad: true);
			StartSceneSetup(sceneLoadInstruction);
		}

		public void StartNewGameForSaveSlot(int slotId)
		{
			saveManager.CreateNewSaveSlot(slotId);
			StartNewGame();
		}

		private void StartNewGame()
		{
			Stage stage = Utility.SceneNameToStage(gameParameters.startFromStage);
			LoadStage(stage);
		}

		public void StartGameFromSaveSlot(int slotId)
		{
			saveManager.SetCurrentSaveSlot(slotId);
			StartGameFromCurrentSaveSlot();
		}

		public void StartGameFromCurrentSaveSlot()
		{
			LoadStage(saveManager.CurrentSaveSlot.CurrentStage);
		}

		public void StartGameFromCurrentSaveSlotToStage(Stage stage)
		{
			SaveSlotData saveSlotData = saveManager.FindLastPlayedSaveSlot();
			if (saveSlotData == null)
			{
				LoadMainMenuScene();
				return;
			}
			saveManager.SetCurrentSaveSlot(saveSlotData.saveSlotId);
			if (saveManager.CurrentSaveSlot.CurrentStage == Stage.Prelude)
			{
				LoadStage(saveManager.CurrentSaveSlot.CurrentStage);
			}
			else
			{
				LoadStage(stage);
			}
		}

		public void LoadStage(Stage stage)
		{
			ASceneSetupInstruction sceneLoadInstruction = new LoadStageASyncInstruction(stage, setActiveAfterLoad: true, 0.5f);
			StartSceneSetup(sceneLoadInstruction);
		}

		public void SwitchStage(Stage newStage)
		{
			BreakDownStage();
			ASceneSetupInstruction sceneLoadInstruction = new SwitchStageInstruction(newStage, currentStage, 0.5f, coreSceneName);
			StartSceneSetup(sceneLoadInstruction);
		}

		public void SwitchStage(Stage newStage, Stage fromStage)
		{
			BreakDownStage();
			ASceneSetupInstruction sceneLoadInstruction = new SwitchStageInstruction(newStage, fromStage, 0.5f, coreSceneName);
			StartSceneSetup(sceneLoadInstruction);
		}

		public void UnloadCurrentStage()
		{
			BreakDownStage();
			ASceneSetupInstruction sceneLoadInstruction = new UnloadStageASyncInstruction(currentStage, coreSceneName);
			StartSceneSetup(sceneLoadInstruction);
		}

		public void StartSceneSetup(ASceneSetupInstruction sceneLoadInstruction)
		{
			sceneLoadInstruction.SetupCoroutine = StartCoroutine(PrepareScene(sceneLoadInstruction));
		}

		private IEnumerator PrepareScene(ASceneSetupInstruction sceneSetupInstruction)
		{
			audioManager.SetSfxVolume(0f);
			OnSceneSetupCoroutineStarted(sceneSetupInstruction);
			while (!currentSceneSetupInstructions[0].Equals(sceneSetupInstruction))
			{
				yield return null;
			}
			assets.ResetLoading();
			yield return sceneSetupInstruction.ExecuteInstruction(assets, sceneLoader);
			yield return OnSceneSetupCoroutineEnded(sceneSetupInstruction);
		}

		private void OnSceneSetupCoroutineStarted(ASceneSetupInstruction sceneSetupInstruction)
		{
			if (currentSceneSetupInstructions.Count == 0 && !sceneSetupInstruction.SceneName.Equals("intro"))
			{
				ShowLoadingScreen();
			}
			if (sceneSetupInstruction is UnloadSceneInstruction)
			{
				RemoveLoadInstructionForScene(sceneSetupInstruction.SceneName);
			}
			currentSceneSetupInstructions.Add(sceneSetupInstruction);
		}

		private void RemoveLoadInstructionForScene(string sceneName)
		{
			ASceneSetupInstruction aSceneSetupInstruction = FindLoadInstructionForScene(sceneName);
			if (aSceneSetupInstruction != null)
			{
				StopLoadInstruction(aSceneSetupInstruction);
			}
		}

		private ASceneSetupInstruction FindLoadInstructionForScene(string sceneName)
		{
			return currentSceneSetupInstructions.Find((ASceneSetupInstruction currentInstruction) => CompareLoadInstructionForScene(currentInstruction, sceneName));
		}

		private bool CompareLoadInstructionForScene(ASceneSetupInstruction sceneSetupInstruction, string sceneName)
		{
			if (sceneSetupInstruction is LoadSceneInstruction || sceneSetupInstruction is LoadSceneASyncInstruction)
			{
				return sceneSetupInstruction.SceneName.Equals(sceneName);
			}
			return false;
		}

		private void StopLoadInstruction(ASceneSetupInstruction sceneSetupInstruction)
		{
			Coroutine setupCoroutine = sceneSetupInstruction.SetupCoroutine;
			if (setupCoroutine != null)
			{
				StopCoroutine(setupCoroutine);
			}
			currentSceneSetupInstructions.Remove(sceneSetupInstruction);
		}

		private IEnumerator OnSceneSetupCoroutineEnded(ASceneSetupInstruction sceneSetupInstruction)
		{
			if (sceneSetupInstruction.IsStage)
			{
				HandleStageTransition(sceneSetupInstruction);
			}
			else
			{
				HandleSceneTransition();
			}
			currentSceneSetupInstructions.Remove(sceneSetupInstruction);
			if (!IsLoading)
			{
				yield return HandleFinishedLoading(sceneSetupInstruction);
			}
		}

		private IEnumerator HandleFinishedLoading(ASceneSetupInstruction sceneSetupInstruction)
		{
			if (sceneSetupInstruction.ExtendedLoadingTime > 0f)
			{
				yield return WaitForExtendedLoadingTime(sceneSetupInstruction);
			}
			else if (sceneSetupInstruction.IsStage)
			{
				HandleStageFullyLoaded();
			}
			audioManager.SetSfxVolume(saveManager.Settings.audioSettings.sfxVolume);
			HideLoadingScreen();
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sceneLibrary.introScenePath);
			if (sceneSetupInstruction is LoadSceneInstruction && sceneSetupInstruction.SceneName.Equals(fileNameWithoutExtension))
			{
				isGoingBackToIntro = false;
			}
		}

		private IEnumerator WaitForExtendedLoadingTime(ASceneSetupInstruction sceneSetupInstruction)
		{
			if (sceneSetupInstruction.IsStage)
			{
				yield return stageManager.ExtendLoadingScreen(sceneSetupInstruction.ExtendedLoadingTime);
				HandleStageFullyLoaded();
			}
		}

		private void HandleStageFullyLoaded()
		{
			if (!stageManager.IsPlayerInSequence)
			{
				EnablePlayerInput();
			}
			gameInput.SetGameCursorMode();
		}

		private void HandleStageTransition(ASceneSetupInstruction sceneSetupInstruction)
		{
			if (sceneSetupInstruction is SwitchStageInstruction)
			{
				HandleSwitchStageInstruction((SwitchStageInstruction)sceneSetupInstruction);
			}
			else if (sceneSetupInstruction is LoadStageASyncInstruction)
			{
				HandleLoadStageInstruction((LoadStageASyncInstruction)sceneSetupInstruction);
			}
			else if (sceneSetupInstruction is UnloadStageASyncInstruction)
			{
				HandleUnloadStageInstruction();
			}
		}

		private void HandleSwitchStageInstruction(SwitchStageInstruction switchStageInstruction)
		{
			SetupNewStage(switchStageInstruction.PreviousStage, switchStageInstruction.NewStage);
		}

		private void HandleLoadStageInstruction(LoadStageASyncInstruction loadStageASyncInstruction)
		{
			Stage newStage = Utility.SceneNameToStage(loadStageASyncInstruction.SceneName);
			SetupNewStage(Stage.NONE, newStage);
		}

		private void HandleUnloadStageInstruction()
		{
			currentStage = Stage.NONE;
			Core.Instance.AudioManager.SetStageAudio(Stage.NONE);
			if (Utility.IsActiveSceneMainMenuScene)
			{
				ShowMainMenu();
			}
		}

		private void HandleSceneTransition()
		{
			if (Utility.IsActiveSceneMainMenuScene)
			{
				ShowMainMenu();
			}
		}

		public void SetupNewStage(Stage previousStage, Stage newStage)
		{
			try
			{
				Core.Instance.UnPauseCore(PauseType.ForceOff);
				GamePrefabLoader assetsLoader = assets.GetAssetsLoader<GamePrefabLoader>();
				uiManager.InstantiateStageSceneUI(assetsLoader.GameplayUIPrefab);
				stageManager = new StageManager(saveManager, this, assets, gameInput, Core.Instance.chapterMusic, Core.Instance.Platform.Achievements, downloadableContent);
				stageManager.SetupStage(previousStage, newStage, characterDownloadableContent);
				currentStage = newStage;
				GameInput.OnControllerDisconnected += OnControllerDisconnected;
			}
			catch (Exception exception)
			{
				HandleExceptionInStageSetup(exception);
			}
		}

		private void EnablePlayerInput()
		{
			if (!isGoingBackToIntro)
			{
				gameInput.DisableAllControllerMaps();
				gameInput.EnableControllerMaps(IN_GAME_INPUT_MAPS);
			}
		}

		private void HandleExceptionInStageSetup(Exception exception)
		{
			Core.Instance.HandleExceptionInGame(exception);
		}

		private void BreakDownStage()
		{
			Core.Instance.PauseCore(PauseType.Forced);
			KillSceneSpawnedTweens();
			DisposeStageManager();
			uiManager.DisposeGameplayMenu();
			audioManager.ClearAllGameplayLoopingSfx();
			GameInput.OnControllerDisconnected -= OnControllerDisconnected;
		}

		private void KillSceneSpawnedTweens()
		{
			DOTween.PauseAll();
			List<Tween> list = DOTween.PausedTweens();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null && list[i].id != null && !list[i].stringId.Contains("Animation_"))
				{
					list[i].Kill();
				}
			}
		}

		public void UnPauseGame(PauseType pauseType)
		{
			if (isGamePaused == 0)
			{
				return;
			}
			isGamePaused &= (int)(~pauseType);
			Core.Instance.UnPauseCore(pauseType);
			if (isGamePaused == 0)
			{
				RestoreMouseInputState();
				stageManager.RestorePlayerInputFromPause();
				audioManager.EndForcedVolume();
				if (!Core.Instance.IsCorePaused)
				{
					audioManager.ResumeAllGameplayLoopingSfx();
				}
			}
		}

		public void PauseGame(PauseType pauseType)
		{
			if (uiManager.Overlay.IsShowingLoadingScreen)
			{
				return;
			}
			if (isGamePaused != 0 || (SequenceHandler.instance.IsInSequenceTransition() && pauseType != PauseType.Prompt))
			{
				isGamePaused |= (int)pauseType;
				Core.Instance.PauseCore(pauseType);
				return;
			}
			Core.Instance.PauseCore(pauseType);
			EnableUIControllerMaps();
			isGamePaused = (int)pauseType;
			audioManager.PauseAllGameplayLoopingSfx();
			if (IsPlayingInStage)
			{
				audioManager.BeginForcedVolume();
			}
		}

		internal void ShowErrorAndReturnToIntro(string errorMessageToShow)
		{
			if (!isGoingBackToIntro)
			{
				isGoingBackToIntro = true;
				if (errorMessageToShow == null)
				{
					errorMessageToShow = "DEFAULT_RETURN_POPUP";
				}
				EnableUIControllerMaps();
				uiManager.Overlay.ShowPopup(errorMessageToShow, "BT_OK", GoBackToIntro);
			}
		}

		public void GoBackToIntro()
		{
			UnPauseGame(PauseType.ForceOff);
			if (uiManager.IsShowingAnyMenu)
			{
				uiManager.DisableCurrentMenu();
			}
			if (currentStage == Stage.NONE || currentStage == Stage.MAX)
			{
				GoBackToIntroFromAnyScene();
			}
			else
			{
				GoBackToIntroFromStage();
			}
			StartCoroutine(WaitForSaveAndLogoutUser());
		}

		internal IEnumerator WaitForSaveAndLogoutUser()
		{
			waitForSaveAndLogout = true;
			yield return Core.Instance.WaitForSavingToBeDone();
			Core.Instance.Platform.User.LogOut();
			waitForSaveAndLogout = false;
		}

		public void QuitCurrentGameSession()
		{
			SaveBeforeStageExit();
			UnPauseGame(PauseType.ForceOff);
			UnloadCurrentStage();
			LoadMainMenuScene();
		}

		public void SaveBeforeStageExit()
		{
			saveManager.SaveBeforeStageExit();
		}

		public void RestoreMouseInputState()
		{
			if (IsPlayingInStage)
			{
				RestoreGameMouseInputState();
			}
			else
			{
				gameInput.SetUICursorMode();
			}
		}

		public void GoToCreditsFromStage()
		{
			SaveBeforeStageExit();
			UnloadCurrentStage();
			gameInput.EnableControllerMap(1);
			LoadCreditsScene();
		}

		private void RestoreGameMouseInputState()
		{
			if (uiManager.IsShowingAnyMenu && uiManager.AllowMouseInputInCurrentMenu)
			{
				gameInput.SetUICursorMode();
			}
			else
			{
				gameInput.SetGameCursorMode();
			}
		}

		private void EnableUIControllerMaps()
		{
			gameInput.DisableAllControllerMaps();
			gameInput.EnableControllerMaps(MENU_INPUT_MAPS);
		}

		private void OnControllerDisconnected(ControllerType controllerType, int controllerId)
		{
			_ = 2;
		}

		private void GoBackToIntroFromAnyScene()
		{
			string text = SceneManager.GetActiveScene().name;
			if (!text.Equals("core"))
			{
				ASceneSetupInstruction sceneLoadInstruction = new UnloadSceneInstruction(text, string.Empty);
				StartSceneSetup(sceneLoadInstruction);
			}
			else
			{
				for (int num = currentSceneSetupInstructions.Count - 1; num >= 0; num--)
				{
					StopLoadInstruction(currentSceneSetupInstructions[num]);
				}
			}
			LoadIntro();
		}

		private void GoBackToIntroFromStage()
		{
			SaveBeforeStageExit();
			UnloadCurrentStage();
			LoadIntro();
		}

		private void StartMainMenuSceneMusic()
		{
			musicLibraryPlayer.AddTrackByID(MusicTrackID.MainMenu_Music);
			musicLibraryPlayer.StartTrackByID(MusicTrackID.MainMenu_Music);
		}

		public float GetCurrentLoadingProgress()
		{
			if (IsLoading)
			{
				return currentSceneSetupInstructions[0].GetLoadingProgress(assets);
			}
			if (stageManager != null && stageManager.IsExtendingLoadingScreen)
			{
				return stageManager.GetExtendedLoadingScreenProgress();
			}
			return 0f;
		}
	}
}
