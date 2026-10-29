using System;

namespace Reptile
{
	public abstract class ATransaction
	{
		public enum TransactionStatus
		{
			BUSY = 0,
			EXCEPTION = 1,
			FILE_NOT_FOUND = 2,
			NO_STORAGE = 3,
			CORRUPTED_DATA = 4,
			NO_ACCESS = 5,
			SUCCESS = 6,
			RETRY = 7
		}

		public delegate void TransactionDoneDelegate(ATransaction transaction);

		private TransactionStatus status;

		private Exception exception;

		private int attempts;

		private bool isScheduledForReset;

		public TransactionStatus Status => status;

		public bool IsDone => status != TransactionStatus.BUSY;

		public bool IsSuccess => status == TransactionStatus.SUCCESS;

		public bool HasError
		{
			get
			{
				if (IsDone)
				{
					return status != TransactionStatus.SUCCESS;
				}
				return false;
			}
		}

		public int Attempts => attempts;

		public bool IsScheduledForReset => isScheduledForReset;

		public Exception Exception
		{
			get
			{
				return exception;
			}
			set
			{
				exception = value;
			}
		}

		public event TransactionDoneDelegate OnTransactionDone;

		public ATransaction()
		{
			ResetTransaction();
		}

		public abstract void StartTransaction(AStorage storage);

		public void ChangeStatus(TransactionStatus newStatus)
		{
			status = newStatus;
			if (status != TransactionStatus.BUSY)
			{
				attempts++;
				if (OnTransactionDone != null)
				{
					OnTransactionDone(this);
				}
			}
		}

		public void ScheduleForReset()
		{
			isScheduledForReset = true;
		}

		public void ResetTransaction()
		{
			status = TransactionStatus.BUSY;
			isScheduledForReset = false;
		}

		public override string ToString()
		{
			return $"Transaction status: {status}";
		}
	}
}
