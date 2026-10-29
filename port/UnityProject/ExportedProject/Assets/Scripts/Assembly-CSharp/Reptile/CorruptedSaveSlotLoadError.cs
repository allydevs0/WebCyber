using System.IO;

namespace Reptile
{
	public class CorruptedSaveSlotLoadError : ABaseError, ITransactionError
	{
		private const string SAVE_SLOT_DATA_LOCALIZATION_KEY = "ERROR_SAVE_SLOT_DATA";

		private Overlay overlay;

		private SaveManager saveManager;

		private LoadTransaction<SaveSlotData> loadSaveSlotTransaction;

		private AStorage storage;

		private LoadTransaction<SaveSlotData> loadSaveSlotBackupTransaction;

		private IGameTextLocalizer gameTextLocalizer;

		private bool isWaitingForPopup;

		private string saveSlotDataName;

		public ATransaction Transaction => loadSaveSlotTransaction;

		public CorruptedSaveSlotLoadError(Overlay overlay, SaveManager saveManager, LoadTransaction<SaveSlotData> loadSaveSlotTransaction, AStorage storage, IGameTextLocalizer gameTextLocalizer)
			: base(AErrorHandler.ErrorType.CORRUPTED_SAVE_SLOT)
		{
			this.overlay = overlay;
			this.saveManager = saveManager;
			this.loadSaveSlotTransaction = loadSaveSlotTransaction;
			this.storage = storage;
			this.gameTextLocalizer = gameTextLocalizer;
			saveSlotDataName = gameTextLocalizer.GetUserInterfaceText("ERROR_SAVE_SLOT_DATA");
		}

		public override void UpdateError()
		{
			base.UpdateError();
			if (isWaitingForPopup)
			{
				UpdateWaitingForPopup();
			}
		}

		public override void HandleError()
		{
			LoadUserSaveSlotBackup(loadSaveSlotTransaction.Data, loadSaveSlotTransaction.FileName);
		}

		private void LoadUserSaveSlotBackup(SaveSlotData saveSlotData, string fileName)
		{
			loadSaveSlotBackupTransaction = saveManager.LoadSaveSlotBackup(saveSlotData, fileName);
			while (!loadSaveSlotBackupTransaction.IsDone)
			{
			}
			OnLoadUserSaveSlotBackupDone(loadSaveSlotBackupTransaction);
		}

		private void OnLoadUserSaveSlotBackupDone(LoadTransaction<SaveSlotData> backupTransaction)
		{
			if (backupTransaction.IsSuccess)
			{
				HandleLoadUserSaveSlotBackupSuccess(backupTransaction);
			}
			else
			{
				HandleLoadUserSaveSlotBackupError();
			}
		}

		private void HandleLoadUserSaveSlotBackupSuccess(LoadTransaction<SaveSlotData> loadSaveSlotBackupTransaction)
		{
			SaveSlotData data = loadSaveSlotBackupTransaction.Data;
			saveManager.AddSaveSlotData(data, loadSaveSlotBackupTransaction.FileName);
			ChangeStatus(ErrorHandlingResult.DELETED);
			FinishErrorHandling();
		}

		private void HandleLoadUserSaveSlotBackupError()
		{
			overlay.ShowPopup("ERROR_DELETE_CORRUPTED_DATA", "BT_OK", "BT_CANCEL", OnClickedOkButton, OnClickedCancelButton, saveSlotDataName);
		}

		private void OnClickedOkButton()
		{
			storage.DeleteDataFromStorage(loadSaveSlotTransaction.FullPath, loadSaveSlotTransaction.IsLocal);
			storage.DeleteDataFromStorage(loadSaveSlotBackupTransaction.FullPath, loadSaveSlotBackupTransaction.IsLocal);
			ChangeStatus(ErrorHandlingResult.DELETED);
			isWaitingForPopup = true;
		}

		private void OnClickedCancelButton()
		{
			storage.DeleteDataFromStorage(loadSaveSlotBackupTransaction.FullPath, loadSaveSlotBackupTransaction.IsLocal);
			MarkFileAsCorrupted(loadSaveSlotTransaction.FullPath);
			ChangeStatus(ErrorHandlingResult.DELETED);
			isWaitingForPopup = true;
		}

		private void MarkFileAsCorrupted(string oldFilePath)
		{
			string newFilePath = GenerateCorruptedFilePath(oldFilePath);
			storage.RenameFile(oldFilePath, newFilePath);
		}

		private string GenerateCorruptedFilePath(string filePath)
		{
			string path = Path.Combine(Path.GetFileNameWithoutExtension(filePath), "Corrupted", ".brcc");
			return Path.Combine(Path.GetDirectoryName(filePath), path);
		}

		private void UpdateWaitingForPopup()
		{
			if (!overlay.popup.IsEnabled)
			{
				isWaitingForPopup = false;
				FinishErrorHandling();
			}
		}
	}
}
