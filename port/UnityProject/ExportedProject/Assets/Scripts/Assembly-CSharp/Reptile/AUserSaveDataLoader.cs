using System.Collections;

namespace Reptile
{
	public abstract class AUserSaveDataLoader : IUserSaveDataLoader
	{
		protected delegate IEnumerator FailureDelegateASync(ATransaction transaction, RetryDelegateASync retryDelegateASync, FailureDelegateASync failureDelegate);

		protected delegate IEnumerator RetryDelegateASync(ATransaction transaction, FailureDelegateASync failureDelegate, RetryDelegateASync retryDelegateASync);

		private const string SAVE_DATA_TEXT_KEY = "ERROR_SAVE_DATA_TEXT";

		protected bool isSuccess = true;

		protected ErrorHandlingResult errorHandlingResult;

		protected SaveManager saveManager;

		protected AStorage storage;

		protected AErrorHandler errorHandler;

		protected UIManager uIManager;

		protected IGameTextLocalizer localizer;

		public bool IsSuccess => isSuccess;

		public AUserSaveDataLoader(SaveManager saveManager, AStorage storage, AErrorHandler errorHandler, UIManager uIManager, IGameTextLocalizer localizer)
		{
			this.saveManager = saveManager;
			this.storage = storage;
			this.errorHandler = errorHandler;
			this.uIManager = uIManager;
			this.localizer = localizer;
		}

		public void ResetLoader()
		{
			isSuccess = true;
		}

		public abstract IEnumerator LoadSaveDataASync();

		protected IEnumerator DoTransactionASync(ATransaction transaction, FailureDelegateASync failureDelegateASync, RetryDelegateASync retryDelegateASync)
		{
			yield return new TransactionYieldInstruction(transaction, storage);
			if (transaction.Status == ATransaction.TransactionStatus.RETRY)
			{
				yield return RetryTransactionASync(transaction, retryDelegateASync, failureDelegateASync);
			}
			else if (!transaction.IsSuccess)
			{
				yield return failureDelegateASync(transaction, retryDelegateASync, failureDelegateASync);
			}
		}

		protected IEnumerator HandleLoadUserSaveDataErrorASync(ATransaction transaction, RetryDelegateASync retryDelegateASync, FailureDelegateASync failureDelegateASync)
		{
			if (transaction.Status != ATransaction.TransactionStatus.FILE_NOT_FOUND)
			{
				errorHandlingResult = ErrorHandlingResult.NONE;
				IError error = HandleTransactionError(transaction);
				if (error != null)
				{
					yield return new ErrorYieldInstruction(error);
					yield return HandleErrorHandlingResultASync(error, transaction, retryDelegateASync, failureDelegateASync);
				}
			}
		}

		protected IEnumerator LoadDirectoryInfoASync(ATransaction transaction, FailureDelegateASync failureDelegateASync, RetryDelegateASync retryDelegateASync)
		{
			yield return new TransactionYieldInstruction(transaction, storage);
			if (transaction.Status == ATransaction.TransactionStatus.RETRY)
			{
				transaction.ScheduleForReset();
				storage.EnqueueTransaction(transaction);
				saveManager.AddLoadTransactionToTracker(transaction);
				yield return LoadDirectoryInfoASync(transaction, failureDelegateASync, retryDelegateASync);
			}
			if (transaction.HasError)
			{
				yield return failureDelegateASync(transaction, retryDelegateASync, failureDelegateASync);
			}
		}

		protected IEnumerator HandleErrorHandlingResultASync(IError error, ATransaction transaction, RetryDelegateASync retryDelegateASync, FailureDelegateASync failureDelegateASync)
		{
			errorHandlingResult = error.ErrorHandlingResult;
			if (errorHandlingResult != ErrorHandlingResult.RESOLVED && errorHandlingResult != ErrorHandlingResult.DELETED)
			{
				if (errorHandlingResult == ErrorHandlingResult.NONE || errorHandlingResult == ErrorHandlingResult.FAILED)
				{
					HandleFailedErrorHandling();
				}
				else if (errorHandlingResult == ErrorHandlingResult.RETRY)
				{
					yield return RetryTransactionASync(transaction, retryDelegateASync, failureDelegateASync);
				}
			}
		}

		private IError HandleTransactionError(ATransaction transaction)
		{
			_ = transaction.Status;
			return errorHandler.HandleLoadTransactionError(transaction, uIManager.Overlay, saveManager, localizer.GetUserInterfaceText("ERROR_SAVE_DATA_TEXT"), string.Empty);
		}

		private IEnumerator RetryTransactionASync(ATransaction transaction, RetryDelegateASync retryDelegateASync, FailureDelegateASync failureDelegateASync)
		{
			transaction.ScheduleForReset();
			storage.EnqueueTransaction(transaction);
			saveManager.AddLoadTransactionToTracker(transaction);
			while (transaction.IsScheduledForReset)
			{
				yield return null;
			}
			yield return retryDelegateASync(transaction, failureDelegateASync, retryDelegateASync);
		}

		private void HandleFailedErrorHandling()
		{
			isSuccess = false;
		}
	}
}
