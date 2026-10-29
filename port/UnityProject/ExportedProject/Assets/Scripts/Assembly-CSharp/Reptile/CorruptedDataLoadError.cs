using System.IO;

namespace Reptile
{
	public class CorruptedDataLoadError : ABaseError, ITransactionError
	{
		public const string CORRUPTED_NAME = "Corrupted";

		public const string CORRUPTED_FILE_EXT = ".brcc";

		private Overlay overlay;

		private AStorage storage;

		private ATransaction transaction;

		private string dataObjectName;

		private string customPopupMessage;

		private bool isWaitingForPopup;

		public ATransaction Transaction => transaction;

		public CorruptedDataLoadError(Overlay overlay, ATransaction transaction, AStorage storage, string dataObjectName, string customPopupMessage)
			: base(AErrorHandler.ErrorType.CORRUPTED_SAVE_DATA)
		{
			this.overlay = overlay;
			this.storage = storage;
			this.transaction = transaction;
			this.dataObjectName = dataObjectName;
			this.customPopupMessage = customPopupMessage;
		}

		public override void HandleError()
		{
			if (transaction is AFileTransaction)
			{
				overlay.ShowPopup("ERROR_DELETE_CORRUPTED_DATA", "BT_OK", "BT_CANCEL", OnClickedOkButtonFileTransaction, OnClickedCancelButtonFileTransaction, dataObjectName);
			}
			else
			{
				overlay.ShowPopup("ERROR_CORRUPTED_DATA", "BT_OK", OnClickedOkButton, dataObjectName, customPopupMessage);
			}
		}

		public override void UpdateError()
		{
			base.UpdateError();
			if (isWaitingForPopup)
			{
				UpdateWaitingForPopup();
			}
		}

		private void OnClickedOkButton()
		{
			ChangeStatus(ErrorHandlingResult.DELETED);
			isWaitingForPopup = true;
		}

		private void OnClickedOkButtonFileTransaction()
		{
			AFileTransaction aFileTransaction = (AFileTransaction)transaction;
			storage.DeleteDataFromStorage(aFileTransaction.FullPath, aFileTransaction.IsLocal);
			ChangeStatus(ErrorHandlingResult.DELETED);
			isWaitingForPopup = true;
		}

		private void OnClickedCancelButtonFileTransaction()
		{
			AFileTransaction aFileTransaction = (AFileTransaction)transaction;
			MarkFileAsCorrupted(aFileTransaction.FullPath);
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
				FinishErrorHandling();
				isWaitingForPopup = false;
			}
		}
	}
}
