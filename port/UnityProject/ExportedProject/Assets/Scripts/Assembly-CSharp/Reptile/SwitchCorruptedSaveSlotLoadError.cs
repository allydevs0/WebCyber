namespace Reptile
{
	public class SwitchCorruptedSaveSlotLoadError : ABaseError, ITransactionError
	{
		private const string SAVE_SLOT_DATA_LOCALIZATION_KEY = "ERROR_SAVE_SLOT_DATA";

		private Overlay overlay;

		private SaveManager saveManager;

		private LoadTransaction<SaveSlotData> loadSaveSlotTransaction;

		private AStorage storage;

		private LoadTransaction<SaveSlotData> loadSaveSlotBackupTransaction;

		private bool isWaitingForPopup;

		public ATransaction Transaction => loadSaveSlotTransaction;

		public SwitchCorruptedSaveSlotLoadError(Overlay overlay, SaveManager saveManager, LoadTransaction<SaveSlotData> loadSaveSlotTransaction, AStorage storage)
			: base(AErrorHandler.ErrorType.CORRUPTED_SAVE_SLOT)
		{
			this.overlay = overlay;
			this.saveManager = saveManager;
			this.loadSaveSlotTransaction = loadSaveSlotTransaction;
			this.storage = storage;
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
			overlay.ShowPopup("ERROR_DELETE_CORRUPTED_DATA", "BT_OK", "BT_CANCEL", OnClickedOkButton, OnClickedCancelButton, "ERROR_SAVE_SLOT_DATA");
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
			ChangeStatus(ErrorHandlingResult.FAILED);
			isWaitingForPopup = true;
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
