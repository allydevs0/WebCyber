using System;
using System.Collections.Generic;
using System.IO;

namespace Reptile
{
	public class SaveSlotHandler : IDisposable
	{
		public const string SAVE_SLOT_FILE_NAME = "GameProgress";

		public const string SAVE_SLOT_FILE_EXT = ".brp";

		public const string SAVE_SLOT_FILE_FORMAT = "GameProgress{0}.brp";

		public const int MAX_SAVE_SLOTS = 100;

		private List<string> directorySaveSlotFileNames;

		private Dictionary<int, string> loadedSaveSlotFileNames;

		private List<SaveSlotData> saveSlots;

		private SaveSlotData currentSaveSlot;

		private AStorage storage;

		public SaveSlotData CurrentSaveSlot => currentSaveSlot;

		public int SaveSlotCount => saveSlots.Count;

		public SaveSlotHandler(AStorage storage)
		{
			this.storage = storage;
			saveSlots = new List<SaveSlotData>();
			loadedSaveSlotFileNames = new Dictionary<int, string>();
			directorySaveSlotFileNames = new List<string>();
		}

		public void Dispose()
		{
			saveSlots = null;
			loadedSaveSlotFileNames = null;
			directorySaveSlotFileNames = null;
			currentSaveSlot = null;
			storage = null;
		}

		public void AddNewSaveSlot(int saveSlotId)
		{
			SaveSlotData newSaveSlotData = CreateNewSaveSlotData(saveSlotId);
			string text = GenerateNewSaveSlotFileName(saveSlotId);
			directorySaveSlotFileNames.Add(text);
			AddNewSaveSlotData(newSaveSlotData, text);
		}

		public void AddSaveSlot(SaveSlotData saveSlotData, string saveSlotFileName)
		{
			if (saveSlotFileName.Contains("_backup"))
			{
				saveSlotFileName = GenerateDefaultSaveSlotFileFromBackupFileName(saveSlotFileName);
			}
			AddSaveSlotData(saveSlotData, saveSlotFileName);
		}

		public void ClearSaveSlots()
		{
			saveSlots.Clear();
			ClearCurrentSaveSlot();
		}

		public void ClearCurrentSaveSlot()
		{
			currentSaveSlot = null;
		}

		public SaveSlotData GetSaveSlotData(int saveSlotId)
		{
			for (int i = 0; i < saveSlots.Count; i++)
			{
				if (saveSlots[i].saveSlotId == saveSlotId)
				{
					return saveSlots[i];
				}
			}
			return null;
		}

		public SaveSlotData FindLastPlayedSaveSlot()
		{
			SaveSlotData saveSlotData = null;
			for (int i = 0; i < saveSlots.Count; i++)
			{
				if (saveSlotData == null)
				{
					saveSlotData = saveSlots[i];
				}
				else if (saveSlots[i].lastTimePlayed > saveSlotData.lastTimePlayed)
				{
					saveSlotData = saveSlots[i];
				}
			}
			return saveSlotData;
		}

		public SaveSlotData GetFirstOccupiedSaveSlot()
		{
			if (saveSlots.Count <= 0)
			{
				return null;
			}
			return saveSlots[0];
		}

		public bool IsSaveSlotOccupied(int saveSlotId)
		{
			return GetSaveSlotData(saveSlotId) != null;
		}

		public void SetCurrentSaveSlotDataBySlotId(int saveSlotId)
		{
			currentSaveSlot = GetSaveSlotData(saveSlotId);
		}

		public ATransaction SaveSaveSlot(int saveSlotId)
		{
			if (saveSlotId <= -1)
			{
				return null;
			}
			string saveSlotFileName = GetSaveSlotFileName(saveSlotId);
			if (saveSlotFileName.Equals(string.Empty))
			{
				return null;
			}
			return SaveSaveSlotData(saveSlotFileName);
		}

		public ATransaction SaveSaveSlotBackup(int slotId)
		{
			if (slotId <= -1)
			{
				return null;
			}
			string saveSlotFileName = GetSaveSlotFileName(slotId);
			if (saveSlotFileName.Equals(string.Empty))
			{
				return null;
			}
			string backupFileName = AStorage.GetBackupFileName(saveSlotFileName);
			return storage.SaveBackupDataAsSaveSlotAtStorage(currentSaveSlot, backupFileName, "SaveData", DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
		}

		public DirectoryInfoTransaction LoadSaveSlotDirectoryInfo()
		{
			return storage.GetSlotsInfoFromStorage("SaveData");
		}

		public TransactionsYieldInstruction LoadSaveSlotsFromFile(DirectoryInfoTransaction directoryInfoTransaction)
		{
			GetExistingSaveSlotNames(directoryInfoTransaction);
			ATransaction[] array = new ATransaction[directorySaveSlotFileNames.Count];
			for (int i = 0; i < directorySaveSlotFileNames.Count; i++)
			{
				array[i] = LoadSaveSlot(directorySaveSlotFileNames[i]);
			}
			return new TransactionsYieldInstruction(array, storage);
		}

		public LoadTransaction<SaveSlotData> LoadSaveSlot(string fileName)
		{
			if (fileName.Equals(string.Empty))
			{
				return null;
			}
			SaveSlotData saveSlotData = new SaveSlotData();
			return LoadSaveSlotData(saveSlotData, fileName);
		}

		public LoadTransaction<SaveSlotData> LoadSaveSlotBackup(SaveSlotData saveSlotData, string saveSlotFileName)
		{
			string backupFileName = AStorage.GetBackupFileName(saveSlotFileName);
			return LoadSaveSlotData(saveSlotData, backupFileName);
		}

		public DeleteTransaction DeleteSaveSlot(int slotId)
		{
			DeleteSaveSlotDataBackup(slotId);
			DeleteTransaction deleteTransaction = DeleteSaveSlotData(slotId);
			RemoveSaveSlotBySlotId(slotId);
			RemoveSaveSlotFileName(slotId);
			directorySaveSlotFileNames.Remove(deleteTransaction.FileName);
			return deleteTransaction;
		}

		public DeleteTransaction DeleteSaveSlotDataBackup(int slotId)
		{
			string backupFileName = AStorage.GetBackupFileName(GetSaveSlotFileName(slotId));
			return storage.DeleteSaveSlotDataFromStorage(backupFileName, "SaveData");
		}

		public DeleteTransaction[] DeleteAllSaveSlots(DirectoryInfoTransaction directoryInfoTransaction)
		{
			GetExistingSaveSlotNames(directoryInfoTransaction);
			DeleteTransaction[] array = new DeleteTransaction[directorySaveSlotFileNames.Count];
			for (int i = 0; i < directorySaveSlotFileNames.Count; i++)
			{
				array[i] = storage.DeleteDataFromStorage(directorySaveSlotFileNames[i], "SaveData", isLocal: false);
			}
			return array;
		}

		private SaveSlotData CreateNewSaveSlotData(int saveSlotId)
		{
			return new SaveSlotData
			{
				saveSlotId = saveSlotId
			};
		}

		private void AddSaveSlotData(SaveSlotData saveSlotData, string saveSlotFileName)
		{
			SaveSlotData saveSlotData2 = GetSaveSlotData(saveSlotData.saveSlotId);
			if (saveSlotData2 == null)
			{
				AddNewSaveSlotData(saveSlotData, saveSlotFileName);
			}
			else if (!(saveSlotData2.lastTimePlayed >= saveSlotData.lastTimePlayed))
			{
				UpdateExistingSaveSlotData(saveSlotData, saveSlotData2, saveSlotFileName);
			}
		}

		private void AddNewSaveSlotData(SaveSlotData newSaveSlotData, string saveSlotFileName)
		{
			AddSaveSlotFileName(newSaveSlotData.saveSlotId, saveSlotFileName);
			saveSlots.Add(newSaveSlotData);
		}

		private void UpdateExistingSaveSlotData(SaveSlotData newSaveSlotData, SaveSlotData existingSaveSlotData, string saveSlotFileName)
		{
			saveSlots.Remove(existingSaveSlotData);
			AddNewSaveSlotData(newSaveSlotData, saveSlotFileName);
		}

		private string GetSaveSlotFileName(int saveSlotId)
		{
			if (loadedSaveSlotFileNames.ContainsKey(saveSlotId))
			{
				return loadedSaveSlotFileNames[saveSlotId];
			}
			return string.Empty;
		}

		private string GenerateNewSaveSlotFileName(int saveSlotId)
		{
			string text = GenerateDefaultSaveSlotFileName(saveSlotId);
			if (directorySaveSlotFileNames.Contains(text))
			{
				for (int i = saveSlotId + 1; i < 100; i++)
				{
					text = $"GameProgress{i}.brp";
					if (!directorySaveSlotFileNames.Contains(text))
					{
						break;
					}
				}
			}
			return text;
		}

		private string GenerateDefaultSaveSlotFileName(int saveSlotId)
		{
			return $"GameProgress{saveSlotId}.brp";
		}

		private string GenerateDefaultSaveSlotFileFromBackupFileName(string backupFileName)
		{
			return backupFileName.Replace("_backup", string.Empty);
		}

		private void AddSaveSlotFileName(int saveSlotId, string saveSlotFileName)
		{
			if (loadedSaveSlotFileNames.ContainsKey(saveSlotId))
			{
				loadedSaveSlotFileNames[saveSlotId] = saveSlotFileName;
			}
			else
			{
				loadedSaveSlotFileNames.Add(saveSlotId, saveSlotFileName);
			}
		}

		private ATransaction SaveSaveSlotData(string saveSlotFileName)
		{
			return storage.SaveDataAsSaveSlotAtStorage(currentSaveSlot, saveSlotFileName, "SaveData", DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
		}

		private void GetExistingSaveSlotNames(DirectoryInfoTransaction directoryInfoTransaction)
		{
			directorySaveSlotFileNames.Clear();
			string[] directoryFileNames = directoryInfoTransaction.DirectoryFileNames;
			for (int i = 0; i < directoryFileNames.Length; i++)
			{
				string fileName = Path.GetFileName(directoryFileNames[i]);
				string extension = Path.GetExtension(directoryFileNames[i]);
				if (fileName.Contains("GameProgress") && extension.Equals(".brp") && !fileName.Contains("_backup") && !fileName.Contains("Corrupted"))
				{
					directorySaveSlotFileNames.Add(fileName);
				}
			}
		}

		private LoadTransaction<SaveSlotData> LoadSaveSlotData(SaveSlotData saveSlotData, string saveSlotFileName)
		{
			return storage.LoadSaveSlotFromStorage(saveSlotData, saveSlotFileName, "SaveData", DataSerializerFactory.FormatType.COMPRESSED_GAME_DATA);
		}

		private void RemoveSaveSlotBySlotId(int slotId)
		{
			saveSlots.RemoveAll((SaveSlotData saveSlotData) => saveSlotData.saveSlotId == slotId);
		}

		private void RemoveSaveSlotFileName(int slotId)
		{
			loadedSaveSlotFileNames.Remove(slotId);
		}

		private DeleteTransaction DeleteSaveSlotData(int slotId)
		{
			string saveSlotFileName = GetSaveSlotFileName(slotId);
			if (saveSlotFileName.Equals(string.Empty))
			{
				return null;
			}
			return storage.DeleteSaveSlotDataFromStorage(saveSlotFileName, "SaveData");
		}
	}
}
