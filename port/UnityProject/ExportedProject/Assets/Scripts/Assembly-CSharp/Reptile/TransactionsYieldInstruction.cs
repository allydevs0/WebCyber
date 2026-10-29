using UnityEngine;

namespace Reptile
{
	public class TransactionsYieldInstruction : CustomYieldInstruction, IMultiTransactionHandler
	{
		private ATransaction[] transactions;

		private AStorage storage;

		public override bool keepWaiting => IsWaitingForTransactions();

		public TransactionsYieldInstruction(ATransaction[] transactions, AStorage storage)
		{
			this.transactions = transactions;
			this.storage = storage;
		}

		public ATransaction GetTransaction(int index)
		{
			return transactions[index];
		}

		public int GetTransactionsCount()
		{
			return transactions.Length;
		}

		private bool IsWaitingForTransactions()
		{
			if (!storage.IsAllowedToDoFileOperations)
			{
				return false;
			}
			for (int i = 0; i < transactions.Length; i++)
			{
				if (!transactions[i].IsDone)
				{
					return true;
				}
			}
			return false;
		}
	}
}
