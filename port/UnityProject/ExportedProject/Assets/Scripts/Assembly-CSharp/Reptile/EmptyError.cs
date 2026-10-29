namespace Reptile
{
	public class EmptyError : ABaseError
	{
		private ATransaction transaction;

		private int maxAttempts;

		public EmptyError(ATransaction transaction, int maxAttempts)
			: base(AErrorHandler.ErrorType.EMPTY)
		{
			this.transaction = transaction;
			this.maxAttempts = maxAttempts;
		}

		public override void HandleError()
		{
			if (transaction.Attempts < maxAttempts)
			{
				RetryTransaction();
			}
			else
			{
				ResolveTransaction();
			}
		}

		private void RetryTransaction()
		{
			ChangeStatus(ErrorHandlingResult.RETRY);
			FinishErrorHandling();
		}

		private void ResolveTransaction()
		{
			ChangeStatus(ErrorHandlingResult.RESOLVED);
			FinishErrorHandling();
		}
	}
}
