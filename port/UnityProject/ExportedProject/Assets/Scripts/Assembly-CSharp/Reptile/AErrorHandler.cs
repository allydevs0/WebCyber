using UnityEngine;

namespace Reptile
{
	public abstract class AErrorHandler : IInterruptable
	{
		public enum ErrorType
		{
			NONE = 0,
			NO_DISK_SPACE = 1,
			CORRUPTED_SAVE_DATA = 2,
			CORRUPTED_SAVE_SLOT = 3,
			NO_ACCESS = 4,
			EMPTY = 5
		}

		private ErrorQueue errorQueue;

		private bool isStopped;

		public bool IsHandlingError => errorQueue.IsHandlingError;

		public AErrorHandler(GameObject errorQueuePrefab)
		{
			InstantiateErrorQueue(errorQueuePrefab);
		}

		public void InterruptWork()
		{
			isStopped = true;
			errorQueue.InterruptWork();
		}

		public void Dispose()
		{
			isStopped = true;
			errorQueue.Dispose();
			errorQueue = null;
		}

		public bool HandleCustomError(IError error)
		{
			if (isStopped)
			{
				return false;
			}
			return AddErrorToQueue(error);
		}

		public IError HandleLoadTransactionError(ATransaction loadTransaction, Overlay overlay, SaveManager saveManager, string dataObjectName, string customPopupMessage)
		{
			if (isStopped)
			{
				return null;
			}
			if (loadTransaction.Status == ATransaction.TransactionStatus.CORRUPTED_DATA)
			{
				return HandleCorruptedLoadTransaction(loadTransaction, overlay, saveManager, dataObjectName, customPopupMessage);
			}
			if (loadTransaction.Status == ATransaction.TransactionStatus.NO_ACCESS)
			{
				return HandleNoAccessToDataError(loadTransaction, overlay);
			}
			return null;
		}

		public IError HandleSaveTransactionError(ATransaction saveTransaction, Overlay overlay)
		{
			if (isStopped)
			{
				return null;
			}
			if (saveTransaction.Status == ATransaction.TransactionStatus.EXCEPTION)
			{
				Debug.Log("Encountered unhandeled exception in transaction!");
			}
			if (saveTransaction.Status == ATransaction.TransactionStatus.NO_STORAGE)
			{
				return HandleNoDiskSpaceSaveError(saveTransaction, overlay);
			}
			if (saveTransaction.Status == ATransaction.TransactionStatus.CORRUPTED_DATA)
			{
				return HandleCorruptedDataSaveError(saveTransaction, overlay);
			}
			if (saveTransaction.Status == ATransaction.TransactionStatus.NO_ACCESS)
			{
				return HandleNoAccessToDataError(saveTransaction, overlay);
			}
			return null;
		}

		public IError HandleDeleteTransactionError(ATransaction deleteTransaction, Overlay overlay)
		{
			if (isStopped)
			{
				return null;
			}
			if (deleteTransaction.Status == ATransaction.TransactionStatus.NO_ACCESS)
			{
				return HandleNoAccessToDataError(deleteTransaction, overlay);
			}
			return null;
		}

		private IError HandleCorruptedLoadTransaction(ATransaction loadTransaction, Overlay overlay, SaveManager saveManager, string dataObjectName, string customPopupMessage)
		{
			if (isStopped)
			{
				return null;
			}
			if (loadTransaction is AFileTransaction && (loadTransaction as AFileTransaction).IsSaveSlotTransaction)
			{
				return HandleCorruptedSaveSlotLoadError(loadTransaction, overlay, saveManager);
			}
			return HandleCorruptedDataLoadError(loadTransaction, overlay, dataObjectName, customPopupMessage);
		}

		public abstract IError HandleNoDiskSpaceSaveError(ATransaction saveTransaction, Overlay overlay);

		public abstract IError HandleCorruptedDataLoadError(ATransaction loadTransaction, Overlay overlay, string dataObjectName, string customPopupMessage);

		public abstract IError HandleCorruptedSaveSlotLoadError(ATransaction loadTransaction, Overlay overlay, SaveManager saveManager);

		public abstract IError HandleCorruptedDataSaveError(ATransaction saveTransaction, Overlay overlay);

		protected bool AddErrorToQueue(IError error)
		{
			return errorQueue.AddError(error);
		}

		protected virtual IError HandleNoAccessToDataError(ATransaction transaction, Overlay overlay)
		{
			if (transaction.Status != ATransaction.TransactionStatus.NO_ACCESS)
			{
				return null;
			}
			IError error = new NoAccessToDataError(transaction, overlay);
			AddErrorToQueue(error);
			return error;
		}

		private void InstantiateErrorQueue(GameObject errorQueuePrefab)
		{
			GameObject gameObject = Object.Instantiate(errorQueuePrefab, Core.Instance.transform);
			errorQueue = gameObject.GetComponent<ErrorQueue>();
		}
	}
}
