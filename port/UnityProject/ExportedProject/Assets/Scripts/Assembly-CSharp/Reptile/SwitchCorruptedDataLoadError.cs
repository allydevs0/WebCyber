namespace Reptile
{
	public class SwitchCorruptedDataLoadError : ABaseError, ITransactionError
	{
		private Overlay overlay;

		private AStorage storage;

		private ATransaction transaction;

		private string dataObjectName;

		private string customPopupMessage;

		private bool isWaitingForPopup;

		public ATransaction Transaction => transaction;

		public SwitchCorruptedDataLoadError(ATransaction loadTransaction, Overlay overlay, AStorage storage, string dataObjectName, string customPopupMessage)
			: base(AErrorHandler.ErrorType.CORRUPTED_SAVE_DATA)
		{
			transaction = loadTransaction;
			this.overlay = overlay;
			this.storage = storage;
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
			ChangeStatus(ErrorHandlingResult.FAILED);
			isWaitingForPopup = true;
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
