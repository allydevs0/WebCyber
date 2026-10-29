using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using Rewired.Integration.UnityUI;
using Steamworks;
using UnityEngine;
using UnityEngine.Audio;

namespace Reptile
{
	public class Core : MonoBehaviour
	{
		public delegate void OnCoreInitializedHandler();

		public delegate void OnUpdateHandler();

		public delegate void OnFixedUpdateHandler();

		public delegate void OnLateUpdateHandler();

		public delegate void OnCoreUpdateHandler();

		public delegate void OnCoreUpdateUnpausedHandler();

		public delegate void ScreenSizeChangedDelegate(int newWidth, int newHeight);

		private enum ReturnIntroStates
		{
			NONE = 0,
			REQUESTED = 1,
			PREPARING = 2
		}

		public const float DEFAULT_DT = 1f / 60f;

		private const float TARGET_FRAME_RATE = 60f;

		private const float MAX_TIME_FOR_SAVING = 15f;

		private static Core instance;

		public SceneLibrary sceneLibrary;

		public AGameDebug gameDebug;

		public SfxLibrary sfxLibrary;

		public LocalizationData localizerData;

		public PlatformData platformData;

		public AudioSource musicAudioSource;

		public AudioSource musicBlendingAudioSource;

		public AudioSource gameplaySfxAudioSource;

		public AudioSource uiSfxAudioSource;

		public AudioSource voicesSfxAudioSource;

		public AudioSource[] ambientAudioSources;

		public GameObject errorQueuePrefab;

		public ChapterMusic chapterMusic;

		public StageAmbience stageAmbience;

		[SerializeField]
		private GameObject uiRootPrefab;

		[SerializeField]
		private GameObject baseModulePrefab;

		[SerializeField]
		private GameObject rewiredInputManagerPrefab;

		[SerializeField]
		private MusicLibrary musicLibrary;

		[SerializeField]
		private AudioMixer masterAudioMixer;

		[SerializeField]
		private GameParameters gameParameters;

		[SerializeField]
		private InputControllerGuidData inputControllerGuidData;

		[SerializeField]
		private AchievementLibrary achievementLibrary;

		[SerializeField]
		private DownloadableContentLibrary downloadableContentLibrary;

		private APlatform platform;

		private SaveManager saveManager;

		private AudioManager audioManager;

		private SceneLoader sceneLoader;

		private Assets assets;

		private UIManager uiManager;

		private BaseModule baseModule;

		private Logger logger;

		private IGameTextLocalizer gameTextLocalizer;

		private IAudioSubSystem audioSubSystem;

		private GameInput gameInput;

		private List<IInterruptable> interuptables;

		private GameVersion gameVersion;

		private int corePaused;

		private bool isCoreInitialized;

		private int returnToIntroState;

		private string returnIntroErrorMessage;

		private int currentScreenWidth;

		private int currentScreenHeight;

		public Assets Assets => assets;

		public SaveManager SaveManager => saveManager;

		public AudioManager AudioManager => audioManager;

		public UIManager UIManager => uiManager;

		public BaseModule BaseModule => baseModule;

		public Logger Logger => logger;

		public APlatform Platform => platform;

		public IGameTextLocalizer Localizer => gameTextLocalizer;

		public IAudioSubSystem AudioSubSystem => audioSubSystem;

		public GameInput GameInput => gameInput;

		public bool IsCoreInitialized => isCoreInitialized;

		public GameVersion GameVersion => gameVersion;

		public static float dt
		{
			get
			{
				return Time.deltaTime;
			}
			set
			{
			}
		}

		public static Core Instance
		{
			get
			{
				if (instance == null)
				{
					instance = UnityEngine.Object.FindObjectOfType<Core>();
				}
				return instance;
			}
		}

		public bool IsCorePaused => corePaused != 0;

		public static event OnCoreInitializedHandler OnCoreInitialized;

		public static event OnUpdateHandler OnUpdate;

		public static event OnUpdateHandler OnAlwaysUpdate;

		public static event OnFixedUpdateHandler OnFixedUpdate;

		public static event OnLateUpdateHandler OnLateUpdate;

		public static event OnCoreUpdateHandler OnCoreUpdatePaused;

		public static event OnCoreUpdateUnpausedHandler OnCoreUpdateUnPaused;

		public static event ScreenSizeChangedDelegate OnScreenSizeChanged;

		public void Awake()
		{
			logger = new Logger(2000);
			if (instance == null)
			{
				instance = this;
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public void StartCore(Assets assets, GameVersion gameVersion)
		{
			this.assets = assets;
			this.gameVersion = gameVersion;
			PrintCoreStartUp();
			SetApplicationParameters();
			CreateSubSystems();
			CreateInteruptableList();
			currentScreenWidth = Screen.width;
			currentScreenHeight = Screen.height;
			isCoreInitialized = true;
			if (OnCoreInitialized != null)
			{
				OnCoreInitialized();
			}
		}

		public void StartGame()
		{
			baseModule.StartGame();
		}

		public void StartGameToStage(Stage stage)
		{
			baseModule.StartGameToStage(stage);
		}

		private void PrintCoreStartUp()
		{
			StackTraceLogType stackTraceLogType = Application.GetStackTraceLogType(LogType.Log);
			Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
			Debug.Log("Core.Awake(): Version: " + gameVersion.ToString());
			Debug.Log("Core.Awake(): Initial quality setting: " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
			Debug.Log("Core.Awake(): Initial resolution: " + Screen.currentResolution.ToString());
			Debug.Log("Core.Awake(): Initial language: " + Application.systemLanguage);
			Debug.Log("Core.Awake(): Platform data loaded: " + platformData.name);
			Application.SetStackTraceLogType(LogType.Log, stackTraceLogType);
		}

		private void SetApplicationParameters()
		{
			Application.targetFrameRate = -1;
		}

		private void CreateSubSystems()
		{
			platform = APlatform.CreatePlatformInstance();
			uiManager = UnityEngine.Object.Instantiate(uiRootPrefab, base.transform).GetComponent<UIManager>();
			baseModule = UnityEngine.Object.Instantiate(baseModulePrefab, base.transform).GetComponent<BaseModule>();
			saveManager = new SaveManager(platform.Storage, uiManager, platform.ErrorHandler, achievementLibrary);
			gameInput = new GameInput(rewiredInputManagerPrefab, saveManager, inputControllerGuidData, platform);
			audioSubSystem = new AudioSubSystem(ref masterAudioMixer, assets);
			audioManager = new AudioManager(sfxLibrary, musicLibrary, saveManager.Settings.audioSettings, musicAudioSource, musicBlendingAudioSource, gameplaySfxAudioSource, uiSfxAudioSource, voicesSfxAudioSource, stageAmbience, ambientAudioSources, audioSubSystem);
			sceneLoader = base.gameObject.AddComponent<SceneLoader>();
			gameTextLocalizer = new TextMeshProGameTextLocalizer(Application.systemLanguage, SystemLanguage.English, localizerData, platformData.languageData, platformData.buttonSpriteData, gameInput);
			gameDebug = platform.CreateDebugInstanceForPlatform(baseModule, saveManager, gameInput, uiManager);
			platform.Init(saveManager, uiManager.uiEventSystemPrefab.GetComponentInChildren<RewiredStandaloneInputModule>(), platformData, gameInput, uiManager, audioManager, gameTextLocalizer, achievementLibrary, downloadableContentLibrary);
			baseModule.Init(assets, sceneLoader, saveManager, uiManager, sceneLibrary, gameParameters, platform.User, gameInput, gameDebug, audioManager, platform.DownloadableContent);
			uiManager.Init(baseModule, gameTextLocalizer, platformData.buttonSpriteData, saveManager, gameInput);
			InitializeDoTween();
		}

		private void InitializeDoTween()
		{
			OnUpdate += () =>
			{
				DOTween.ManualUpdate(dt, 1f / 60f);
			};
		}

		private void CreateInteruptableList()
		{
			interuptables = new List<IInterruptable>();
			interuptables.Add(platform.Storage);
			interuptables.Add(platform.ErrorHandler);
			interuptables.Add(saveManager);
		}

		private void Start()
		{
			audioManager.SetAudioSettings(saveManager.Settings.audioSettings);
		}

		public void Update()
		{
			DetectScreenSizeChange();
			gameDebug.Update();
			platform.Update();
			OnAlwaysUpdate?.Invoke();
			if (corePaused == 0)
			{
				UpdateWithExceptionHandling();
			}
			if (Interlocked.CompareExchange(ref returnToIntroState, 2, 1) == 1)
			{
				baseModule.ShowErrorAndReturnToIntro(returnIntroErrorMessage);
				Interlocked.Exchange(ref returnToIntroState, 0);
			}
		}

		private void UpdateWithExceptionHandling()
		{
			try
			{
				SendUpdateEvent();
			}
			catch (Exception exception)
			{
				HandleExceptionInGame(exception);
			}
		}

		public void HandleExceptionInGame(Exception exception)
		{
			platform.HandleExceptionInGame(exception);
		}

		private void SendUpdateEvent()
		{
			if (OnUpdate != null)
			{
				OnUpdate();
			}
		}

		public void FixedUpdate()
		{
			if (corePaused == 0)
			{
				FixedUpdateWithExceptionHandling();
			}
		}

		private void FixedUpdateWithExceptionHandling()
		{
			try
			{
				SendFixedUpdateEvent();
			}
			catch (Exception exception)
			{
				HandleExceptionInGame(exception);
			}
		}

		private void SendFixedUpdateEvent()
		{
			if (OnFixedUpdate != null)
			{
				OnFixedUpdate();
			}
		}

		public void LateUpdate()
		{
			if (corePaused == 0)
			{
				LateUpdateWithExceptionHandling();
			}
		}

		private void LateUpdateWithExceptionHandling()
		{
			try
			{
				SendLateUpdateEvent();
				if (audioManager != null && audioManager.initialized)
				{
					audioManager.AmbienceUpdate();
				}
			}
			catch (Exception exception)
			{
				HandleExceptionInGame(exception);
			}
		}

		private void SendLateUpdateEvent()
		{
			if (OnLateUpdate != null)
			{
				OnLateUpdate();
			}
		}

		private void OnDestroy()
		{
			if (GetInstanceID() == instance.GetInstanceID())
			{
				instance = null;
			}
		}

		private void OnApplicationPause(bool pause)
		{
			if (isCoreInitialized)
			{
				if (pause)
				{
					HandleForcedGamePause();
				}
				else
				{
					HandleForcedGameUnpause();
				}
			}
		}

		private void OnApplicationFocus(bool focus)
		{
			if (!(baseModule == null) && focus)
			{
				baseModule.RestoreMouseInputState();
			}
		}

		public void HandleForcedGamePause()
		{
			WriteToSaveFiles();
			PauseCore(PauseType.Forced);
		}

		public void HandleForcedGameUnpause()
		{
			UnPauseCore(PauseType.Forced);
		}

		private void OnApplicationQuit()
		{
			if (!(instance == null) && isCoreInitialized)
			{
				HandleApplicationQuit();
			}
		}

		public void HandleApplicationQuit()
		{
			try
			{
				InteruptWorkOnSubSystems();
				WriteToSaveFiles();
			}
			catch (Exception exception)
			{
				Debug.Log("Core.HandleApplicationQuit(): Found exception while quitting game: ");
				Debug.LogException(exception);
			}
			finally
			{
				DestroySubSystems();
			}
		}

		private void InteruptWorkOnSubSystems()
		{
			if (interuptables == null)
			{
				return;
			}
			foreach (IInterruptable interuptable in interuptables)
			{
				interuptable.InterruptWork();
			}
		}

		public void WriteToSaveFiles()
		{
			if (platform == null || !platform.User.IsLoggedIn || !platform.Storage.IsAllowedToDoFileOperations)
			{
				return;
			}
			Debug.Log("Flushing save files...");
			SaveAllPlayerFiles();
			DateTime dateTime = DateTime.Now + TimeSpan.FromSeconds(15.0);
			_ = DateTime.Now;
			while (platform.Storage.IsAnyTransactionPending || platform.Storage.IsSaving || saveManager.IsSaving)
			{
				bool flag = false;
				if (DateTime.Now > dateTime)
				{
					flag = true;
				}
				SteamAPI.RunCallbacks();
				platform.Update();
				if (!platform.Storage.IsAllowedToDoFileOperations | flag)
				{
					throw new Exception("Could not finish saving to safe file, the thread was aborted prematurely!");
				}
			}
		}

		internal void ReturnToIntroFromThread(string errorMessage)
		{
			if (Interlocked.CompareExchange(ref returnToIntroState, 2, 0) == 0)
			{
				returnIntroErrorMessage = errorMessage;
				Interlocked.Exchange(ref returnToIntroState, 1);
			}
		}

		internal IEnumerator WaitForSavingToBeDone()
		{
			if (platform == null || !platform.User.IsLoggedIn || !platform.Storage.IsAllowedToDoFileOperations)
			{
				yield break;
			}
			DateTime endTime = DateTime.Now + TimeSpan.FromSeconds(15.0);
			_ = DateTime.Now;
			while (platform.Storage.IsAnyTransactionPending || platform.Storage.IsSaving || saveManager.IsSaving)
			{
				bool flag = false;
				if (DateTime.Now > endTime)
				{
					flag = true;
				}
				SteamAPI.RunCallbacks();
				platform.Update();
				if (!platform.Storage.IsAllowedToDoFileOperations | flag)
				{
					break;
				}
				yield return null;
			}
		}

		private void SaveAllPlayerFiles()
		{
			if (saveManager.HasCurrentSaveSlot)
			{
				saveManager.SaveCurrentSaveSlotImmediate();
				saveManager.SaveCurrentSaveSlotBackup();
			}
			saveManager.SaveSettings();
			saveManager.SaveSaveSlotsSettings();
			saveManager.SavePlayerStats();
			saveManager.SaveAchievementsCache();
		}

		private void DestroySubSystems()
		{
			if (isCoreInitialized)
			{
				platform.Dispose();
				platform = null;
				gameDebug.Dispose();
				gameDebug = null;
				logger.Dispose();
				logger = null;
				assets = null;
				uiManager = null;
				sceneLoader = null;
				baseModule = null;
				gameInput.Dispose();
				gameInput = null;
				saveManager.Dispose();
				saveManager = null;
				sceneLibrary = null;
				audioManager.Dispose();
				audioManager = null;
				audioSubSystem.Dispose();
				audioSubSystem = null;
				DestroyInteruptableList();
				isCoreInitialized = false;
			}
		}

		private void DestroyInteruptableList()
		{
			interuptables.Clear();
			interuptables = null;
		}

		public void PauseCore(PauseType pauseType)
		{
			if (corePaused != 0)
			{
				corePaused |= (int)pauseType;
				return;
			}
			corePaused = (int)pauseType;
			if (OnCoreUpdatePaused != null)
			{
				OnCoreUpdatePaused();
			}
		}

		public void UnPauseCore(PauseType pauseType)
		{
			if (corePaused != 0)
			{
				corePaused &= (int)(~pauseType);
				if (corePaused == 0 && OnCoreUpdateUnPaused != null)
				{
					OnCoreUpdateUnPaused();
				}
			}
		}

		public void TogglePauseCore(PauseType pauseType)
		{
			if (corePaused != 0)
			{
				UnPauseCore(pauseType);
			}
			else
			{
				PauseCore(pauseType);
			}
		}

		public void CleanupUpdateEvents()
		{
			CleanupUpdateEvent();
			CleanupFixedUpdateEvent();
			CleanupLateUpdateEvent();
		}

		private void CleanupUpdateEvent()
		{
			if (OnUpdate != null)
			{
				OnUpdate = (OnUpdateHandler)RemoveEmptyMonoBehavioursFromEvent(OnUpdate);
			}
		}

		private void CleanupFixedUpdateEvent()
		{
			if (OnFixedUpdate != null)
			{
				OnFixedUpdate = (OnFixedUpdateHandler)RemoveEmptyMonoBehavioursFromEvent(OnFixedUpdate);
			}
		}

		private void CleanupLateUpdateEvent()
		{
			if (OnLateUpdate != null)
			{
				OnLateUpdate = (OnLateUpdateHandler)RemoveEmptyMonoBehavioursFromEvent(OnLateUpdate);
			}
		}

		private MulticastDelegate RemoveEmptyMonoBehavioursFromEvent(MulticastDelegate eventAsDelegate)
		{
			Delegate[] invocationList = eventAsDelegate.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				if (invocationList[i].Target is MonoBehaviour && invocationList[i].Target as MonoBehaviour == null)
				{
					eventAsDelegate = (MulticastDelegate)Delegate.Remove(eventAsDelegate, invocationList[i]);
				}
			}
			return eventAsDelegate;
		}

		private void DetectScreenSizeChange()
		{
			if (currentScreenWidth != Screen.width || currentScreenHeight != Screen.height)
			{
				if (OnScreenSizeChanged != null)
				{
					OnScreenSizeChanged(Screen.width, Screen.height);
				}
				currentScreenWidth = Screen.width;
				currentScreenHeight = Screen.height;
			}
		}
	}
}
