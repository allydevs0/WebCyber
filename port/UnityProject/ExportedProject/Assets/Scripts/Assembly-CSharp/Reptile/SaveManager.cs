using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Reptile
{
	public class SaveManager : IDisposable, IInterruptable
	{
		public const string SAVE_DATA_DIRECTORY_NAME = "SaveData";

		public const string SETTINGS_FILE_NAME = "GameSettings.brs";

		public const string SAVE_SLOTS_SETTINGS_DATA_FILE_NAME = "SaveSlots.brss";

		public const string ACHIEVEMENTS_FILE_NAME = "GameAchievements.bra";

		public const string PLAYERSTATS_FILE_NAME = "GamePlayerStats.brps";

		public const int SAVE_PROGRESS_FILE_DELAY_MILLIS = 2500;

		private SettingsData settings;

		private SaveSettingsData saveSlotSettings;

		private PlayerStats playerStats;

		private AchievementsCache achievementsCache;

		private AchievementLibrary achievementLibrary;

		private List<ATransaction> saveTransactions;

		private List<ATransaction> loadTransactions;

		private AStorage storage;

		private AErrorHandler errorHandler;

		private UIManager uiManager;

		private CancellationTokenSource cancellationTokenSource;

		private Task currentSaveTask;

		private SaveSlotHandler saveSlotHandler;

		public SaveSlotData CurrentSaveSlot => saveSlotHandler.CurrentSaveSlot;

		public bool HasCurrentSaveSlot => saveSlotHandler.CurrentSaveSlot != null;

		public int UsedSaveSlotCount => saveSlotHandler.SaveSlotCount;

		public SettingsData Settings => settings;

		public PlayerStats PlayerStats => playerStats;

		public AchievementsCache AchievementsCache => achievementsCache;

		public bool IsSaving => saveTransactions.Count > 0;

		public bool IsLoading => loadTransactions.Count > 0;

		public SaveManager(AStorage storage, UIManager uiManager, AErrorHandler errorHandler, AchievementLibrary achievementLibrary)
		{
			this.storage = storage;
			this.uiManager = uiManager;
			this.errorHandler = errorHandler;
			this.achievementLibrary = achievementLibrary;
			saveTransactions = new List<ATransaction>();
			loadTransactions = new List<ATransaction>();
			saveSlotHandler = new SaveSlotHandler(storage);
			CreateNewSettingsData();
			CreateNewSaveSlotsSettingsData();
			CreateNewPlayerStats();
			CreateNewAchievementsCache();
		}

		public void Dispose()
		{
			saveTransactions.Clear();
			saveTransactions = null;
			loadTransactions.Clear();
			loadTransactions = null;
			storage = null;
			saveSlotHandler.Dispose();
			saveSlotHandler = null;
			StopSaveFileDelayTask();
			DisposeSettings();
			DisposeSaveSlotsSettings();
			DisposePlayerStats();
			DisposeAchievementsCache();
		}

		public void InterruptWork()
		{
			StopSaveFileDelayTask();
		}

		public void SaveBeforeStageExit()
		{
			StopSaveFileDelayTask();
			SaveCurrentSaveSlotImmediate();
			SaveCurrentSaveSlotBackup();
			SavePlayerStats();
			SaveAchievementsCache();
			saveSlotHandler.CurrentSaveSlot.InvalidateCurrentStageProgress();
		}

		public void SaveAllUserSaveData()
		{
			SaveSettings();
			SaveSaveSlotsSettings();
			SavePlayerStats();
			SaveAchievementsCache();
			SaveCurrentSaveSlot();
		}

		public void SaveCurrentSaveSlot()
		{
			if (currentSaveTask == null || currentSaveTask.IsCompleted)
			{
				SaveCurrentSaveSlotDelayed();
			}
		}

		public ATransaction SaveCurrentSaveSlotBackup()
		{
			ATransaction aTransaction = saveSlotHandler.SaveSaveSlotBackup(saveSlotSettings.currentSaveSlot);
			AddSaveTransactionToTracker(aTransaction);
			return aTransaction;
		}

		public ATransaction SaveSettings()
		{
			bool flag = false;
			flag = true;
			ATransaction aTransaction = storage.SaveDataAtStorage(settings, "GameSettings.brs", "SaveData", flag, DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
			AddSaveTransactionToTracker(aTransaction);
			return aTransaction;
		}

		public void AddSaveTransactionToTracker(ATransaction transaction)
		{
			AddSaveTransaction(transaction);
			transaction.OnTransactionDone += OnSaveTransactionDone;
		}

		public ATransaction SaveCurrentSaveSlotImmediate()
		{
			if (!HasCurrentSaveSlot)
			{
				return null;
			}
			ATransaction aTransaction = saveSlotHandler.SaveSaveSlot(saveSlotSettings.currentSaveSlot);
			AddSaveTransactionToTracker(aTransaction);
			return aTransaction;
		}

		public void SaveAchievementsCache()
		{
			if (Core.Instance.Platform.HasAchievementsSupport)
			{
				ATransaction transaction = storage.SaveDataAtStorage(achievementsCache, "GameAchievements.bra", "SaveData", isLocal: false, DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
				AddSaveTransactionToTracker(transaction);
			}
		}

		public void SavePlayerStats()
		{
			ATransaction transaction = storage.SaveDataAtStorage(playerStats, "GamePlayerStats.brps", "SaveData", isLocal: false, DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
			AddSaveTransactionToTracker(transaction);
		}

		public ATransaction LoadSettings()
		{
			ATransaction aTransaction = storage.LoadDataFromStorage(settings, "GameSettings.brs", "SaveData", isLocal: false, DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
			AddLoadTransactionToTracker(aTransaction);
			return aTransaction;
		}

		public ATransaction LoadSettingsLocal()
		{
			ATransaction aTransaction = storage.LoadDataFromStorage(settings, "GameSettings.brs", "SaveData", isLocal: true, DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
			AddLoadTransactionToTracker(aTransaction);
			return aTransaction;
		}

		public void AddLoadTransactionToTracker(ATransaction transaction)
		{
			AddLoadTransaction(transaction);
			transaction.OnTransactionDone += OnLoadTransactionDone;
		}

		public ATransaction LoadSaveSlotSettings()
		{
			ATransaction aTransaction = storage.LoadDataFromStorage(saveSlotSettings, "SaveSlots.brss", "SaveData", isLocal: false, DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
			AddLoadTransactionToTracker(aTransaction);
			return aTransaction;
		}

		public TransactionsYieldInstruction LoadSaveSlots(DirectoryInfoTransaction directoryInfoTransaction)
		{
			TransactionsYieldInstruction transactionsYieldInstruction = saveSlotHandler.LoadSaveSlotsFromFile(directoryInfoTransaction);
			AddLoadTransactions(transactionsYieldInstruction);
			return transactionsYieldInstruction;
		}

		public DirectoryInfoTransaction LoadSaveSlotDirectoryInfo()
		{
			DirectoryInfoTransaction directoryInfoTransaction = saveSlotHandler.LoadSaveSlotDirectoryInfo();
			AddLoadTransactionToTracker(directoryInfoTransaction);
			return directoryInfoTransaction;
		}

		public LoadTransaction<SaveSlotData> LoadSaveSlotBackup(SaveSlotData saveSlotData, string saveSlotFileName)
		{
			LoadTransaction<SaveSlotData> loadTransaction = saveSlotHandler.LoadSaveSlotBackup(saveSlotData, saveSlotFileName);
			AddLoadTransactionToTracker(loadTransaction);
			return loadTransaction;
		}

		public ATransaction LoadAchievementsCache()
		{
			if (!Core.Instance.Platform.HasAchievementsSupport)
			{
				return null;
			}
			ATransaction aTransaction = storage.LoadDataFromStorage(achievementsCache, "GameAchievements.bra", "SaveData", isLocal: false, DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
			AddLoadTransactionToTracker(aTransaction);
			return aTransaction;
		}

		public ATransaction LoadPlayerStats()
		{
			ATransaction aTransaction = storage.LoadDataFromStorage(playerStats, "GamePlayerStats.brps", "SaveData", isLocal: false, DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
			AddLoadTransactionToTracker(aTransaction);
			return aTransaction;
		}

		public DeleteTransaction DeleteSaveSlot(int slotId)
		{
			DeleteTransaction result = saveSlotHandler.DeleteSaveSlot(slotId);
			if (slotId == saveSlotSettings.currentSaveSlot)
			{
				ResetCurrentSaveSlot();
			}
			return result;
		}

		public void ResetSettings()
		{
			DisposeSettings();
			CreateNewSettingsData();
		}

		public void ResetSaveSlotSettings()
		{
			DisposeSaveSlotsSettings();
			CreateNewSaveSlotsSettingsData();
		}

		public void ResetPlayerStats()
		{
			DisposePlayerStats();
			CreateNewPlayerStats();
		}

		public void ResetAchievementsCache()
		{
			DisposeAchievementsCache();
			CreateNewAchievementsCache();
		}

		public void ResetSaveSlots()
		{
			saveSlotHandler.ClearSaveSlots();
		}

		public SaveSlotData GetSaveSlotData(int saveSlotId)
		{
			return saveSlotHandler.GetSaveSlotData(saveSlotId);
		}

		public SaveSlotData FindLastPlayedSaveSlot()
		{
			if (saveSlotHandler.CurrentSaveSlot == null)
			{
				return saveSlotHandler.FindLastPlayedSaveSlot();
			}
			return saveSlotHandler.CurrentSaveSlot;
		}

		public void CreateNewSaveSlot(int saveSlotId)
		{
			AddNewSaveSlotData(saveSlotId);
			SetCurrentSaveSlot(saveSlotId);
			SaveCurrentSaveSlot();
			SaveCurrentSaveSlotBackup();
		}

		public void AddSaveSlotData(SaveSlotData saveSlotData, string fileName)
		{
			saveSlotHandler.AddSaveSlot(saveSlotData, fileName);
		}

		public void SetCurrentSaveSlot(int newCurrentSaveSlotId)
		{
			SetCurrentSaveSlotIdInSettings(newCurrentSaveSlotId);
			SetCurrentSaveSlotFromSettings();
			SaveSaveSlotsSettings();
		}

		public void SetCurrentSaveSlotFromSettings()
		{
			saveSlotHandler.SetCurrentSaveSlotDataBySlotId(saveSlotSettings.currentSaveSlot);
		}

		public void AttemptSetCurrentSaveSlot()
		{
			SaveSlotData firstOccupiedSaveSlot = saveSlotHandler.GetFirstOccupiedSaveSlot();
			if (firstOccupiedSaveSlot != null)
			{
				SetCurrentSaveSlot(firstOccupiedSaveSlot.saveSlotId);
			}
		}

		public ATransaction SaveSaveSlotsSettings()
		{
			ATransaction aTransaction = storage.SaveDataAtStorage(saveSlotSettings, "SaveSlots.brss", "SaveData", isLocal: false, DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
			AddSaveTransactionToTracker(aTransaction);
			return aTransaction;
		}

		public bool IsSaveSlotOccupied(int saveSlotId)
		{
			return saveSlotHandler.IsSaveSlotOccupied(saveSlotId);
		}

		private void CreateNewSettingsData()
		{
			settings = new SettingsData();
		}

		private void CreateNewSaveSlotsSettingsData()
		{
			saveSlotSettings = new SaveSettingsData();
		}

		private void CreateNewPlayerStats()
		{
			playerStats = new PlayerStats();
		}

		private void CreateNewAchievementsCache()
		{
			if (Core.Instance.Platform.HasAchievementsSupport)
			{
				achievementsCache = new AchievementsCache(achievementLibrary);
			}
		}

		private void StopSaveFileDelayTask()
		{
			if (currentSaveTask != null && !currentSaveTask.IsCompleted)
			{
				cancellationTokenSource.Cancel();
			}
			currentSaveTask = null;
			cancellationTokenSource = null;
		}

		private void DisposeSettings()
		{
			settings = null;
		}

		private void DisposeSaveSlotsSettings()
		{
			saveSlotSettings = null;
		}

		private void DisposePlayerStats()
		{
			playerStats.Dispose();
			playerStats = null;
		}

		private void DisposeAchievementsCache()
		{
			achievementsCache = null;
		}

		private void SaveCurrentSaveSlotDelayed()
		{
			cancellationTokenSource = new CancellationTokenSource();
			Task task = Task.Delay(2500);
			currentSaveTask = task.ContinueWith((Task t) =>
			{
				ExecuteSaveProgressTask();
			}, cancellationTokenSource.Token);
		}

		private void ExecuteSaveProgressTask()
		{
			try
			{
				SaveCurrentSaveSlotImmediate();
			}
			catch (Exception)
			{
			}
		}

		private void AddSaveTransaction(ATransaction saveTransaction)
		{
			saveTransactions.Add(saveTransaction);
		}

		private void OnSaveTransactionDone(ATransaction saveTransaction)
		{
			saveTransaction.OnTransactionDone -= OnSaveTransactionDone;
			RemoveSaveTransaction(saveTransaction);
			if (saveTransaction.Status == ATransaction.TransactionStatus.RETRY)
			{
				RetryPreviousSaveTransaction(saveTransaction);
			}
			else if (saveTransaction.HasError)
			{
				HandleSaveTransactionError(saveTransaction);
			}
		}

		private void HandleSaveTransactionError(ATransaction saveTransaction)
		{
			IError error = errorHandler.HandleSaveTransactionError(saveTransaction, uiManager.Overlay);
			if (error != null)
			{
				error.OnErrorHandlerFinished += OnSaveErrorHandlingFinished;
			}
		}

		private void OnSaveErrorHandlingFinished(IError error)
		{
			error.OnErrorHandlerFinished -= OnSaveErrorHandlingFinished;
			if (error is ITransactionError)
			{
				ITransactionError transactionError = (ITransactionError)error;
				if (error.ErrorHandlingResult == ErrorHandlingResult.RETRY)
				{
					RetryPreviousSaveTransaction(transactionError.Transaction);
				}
			}
		}

		private void RetryPreviousSaveTransaction(ATransaction saveTransaction)
		{
			saveTransaction.ScheduleForReset();
			storage.EnqueueTransaction(saveTransaction);
			AddSaveTransactionToTracker(saveTransaction);
		}

		private void RemoveSaveTransaction(ATransaction saveTransaction)
		{
			saveTransactions.Remove(saveTransaction);
		}

		private void AddLoadTransaction(ATransaction loadTransaction)
		{
			loadTransactions.Add(loadTransaction);
		}

		private void OnLoadTransactionDone(ATransaction loadTransaction)
		{
			loadTransaction.OnTransactionDone -= OnLoadTransactionDone;
			RemoveLoadTransaction(loadTransaction);
		}

		private void RemoveLoadTransaction(ATransaction loadTransaction)
		{
			loadTransactions.Remove(loadTransaction);
		}

		private void AddLoadTransactions(IMultiTransactionHandler multiTransactionHandler)
		{
			int transactionsCount = multiTransactionHandler.GetTransactionsCount();
			for (int i = 0; i < transactionsCount; i++)
			{
				ATransaction transaction = multiTransactionHandler.GetTransaction(i);
				AddLoadTransactionToTracker(transaction);
			}
		}

		private void SetCurrentSaveSlotIdInSettings(int saveSlotId)
		{
			saveSlotSettings.currentSaveSlot = saveSlotId;
		}

		private void AddNewSaveSlotData(int saveSlotId)
		{
			saveSlotHandler.AddNewSaveSlot(saveSlotId);
		}

		private void ResetCurrentSaveSlot()
		{
			saveSlotHandler.ClearCurrentSaveSlot();
			SetCurrentSaveSlot(-1);
		}
	}
}
