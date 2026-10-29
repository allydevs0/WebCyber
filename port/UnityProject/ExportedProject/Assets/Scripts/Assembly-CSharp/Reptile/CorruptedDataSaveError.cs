namespace Reptile
{
	public class CorruptedDataSaveError : ABaseError, ITransactionError
	{
		private const int MAX_RETRIES = 3;

		private ATransaction transaction;

		private AStorage storage;

		public ATransaction Transaction => transaction;

		public CorruptedDataSaveError(ATransaction transaction, AStorage storage)
			: base(AErrorHandler.ErrorType.CORRUPTED_SAVE_DATA)
		{
			this.transaction = transaction;
			this.storage = storage;
		}

		public override void HandleError()
		{
			if (transaction is AFileTransaction)
			{
				HandleFileTransaction(transaction as AFileTransaction);
			}
			else
			{
				HandleOtherTransactionTypes();
			}
		}

		private void HandleOtherTransactionTypes()
		{
			ChangeStatus(ErrorHandlingResult.FAILED);
			FinishErrorHandling();
		}

		private void HandleFileTransaction(AFileTransaction fileTransaction)
		{
			storage.DeleteDataFromStorage(fileTransaction.FullPath, fileTransaction.IsLocal);
			ErrorHandlingResult errorHandlingResult = ErrorHandlingResult.RETRY;
			if (fileTransaction.Attempts >= 3)
			{
				errorHandlingResult = ErrorHandlingResult.FAILED;
			}
			ChangeStatus(errorHandlingResult);
			FinishErrorHandling();
		}
	}
}
