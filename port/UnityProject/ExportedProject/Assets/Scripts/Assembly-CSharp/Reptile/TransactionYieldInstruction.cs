using UnityEngine;

namespace Reptile
{
	public class TransactionYieldInstruction : CustomYieldInstruction
	{
		private ATransaction transaction;

		private AStorage storage;

		public override bool keepWaiting
		{
			get
			{
				if (!storage.IsAllowedToDoFileOperations)
				{
					return false;
				}
				return !transaction.IsDone;
			}
		}

		public TransactionYieldInstruction(ATransaction transaction, AStorage storage)
		{
			this.transaction = transaction;
			this.storage = storage;
		}
	}
}
