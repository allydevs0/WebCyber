namespace Reptile
{
	public class SwitchCorruptedDataSaveError : ABaseError, ITransactionError
	{
		private ATransaction transaction;

		public ATransaction Transaction => transaction;

		public SwitchCorruptedDataSaveError(ATransaction saveTransaction)
			: base(AErrorHandler.ErrorType.CORRUPTED_SAVE_DATA)
		{
			transaction = saveTransaction;
		}

		public override void HandleError()
		{
			if (transaction is AFileTransaction)
			{
				HandleFileTransaction();
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

		private void HandleFileTransaction()
		{
			ChangeStatus(ErrorHandlingResult.RESOLVED);
			FinishErrorHandling();
		}
	}
}
