using System.Collections;

namespace Reptile
{
	public class UserSaveSlotSaveDataLoader : AUserSaveDataLoader
	{
		public UserSaveSlotSaveDataLoader(SaveManager saveManager, AStorage storage, AErrorHandler errorHandler, UIManager uIManager, IGameTextLocalizer localizer)
			: base(saveManager, storage, errorHandler, uIManager, localizer)
		{
		}

		public override IEnumerator LoadSaveDataASync()
		{
			yield return LoadUserSaveSlotsASync();
		}

		public override string ToString()
		{
			return "User save slots";
		}

		private IEnumerator LoadUserSaveSlotsASync()
		{
			DirectoryInfoTransaction directoryInfoTransaction = saveManager.LoadSaveSlotDirectoryInfo();
			yield return LoadDirectoryInfoASync(directoryInfoTransaction, base.HandleLoadUserSaveDataErrorASync, base.LoadDirectoryInfoASync);
			if (isSuccess)
			{
				yield return LoadUserSaveSlotsFromDirectoryInfoASync(directoryInfoTransaction);
			}
		}

		private IEnumerator LoadUserSaveSlotsFromDirectoryInfoASync(DirectoryInfoTransaction directoryInfoTransaction)
		{
			TransactionsYieldInstruction loadSaveSlotsTransaction = saveManager.LoadSaveSlots(directoryInfoTransaction);
			yield return loadSaveSlotsTransaction;
			yield return OnLoadUserSaveSlotsDoneASync(loadSaveSlotsTransaction);
		}

		private IEnumerator RetryInPlace(ATransaction transaction)
		{
			transaction.ScheduleForReset();
			saveManager.AddLoadTransactionToTracker(transaction);
			storage.EnqueueTransaction(transaction);
			yield return new TransactionYieldInstruction(transaction, storage);
		}

		private IEnumerator OnLoadUserSaveSlotsDoneASync(TransactionsYieldInstruction loadSaveSlotsYieldInstruction)
		{
			int transactionCount = loadSaveSlotsYieldInstruction.GetTransactionsCount();
			for (int i = 0; i < transactionCount; i++)
			{
				ATransaction loadSaveSlotTransaction = loadSaveSlotsYieldInstruction.GetTransaction(i);
				if (loadSaveSlotTransaction is LoadTransaction<SaveSlotData>)
				{
					while (loadSaveSlotTransaction.Status == ATransaction.TransactionStatus.RETRY)
					{
						yield return RetryInPlace(loadSaveSlotTransaction);
					}
					yield return HandleLoadUserSaveSlotFileTransactionASync(loadSaveSlotTransaction);
				}
			}
		}

		private IEnumerator HandleLoadUserSaveSlotFileTransactionASync(ATransaction transaction)
		{
			LoadTransaction<SaveSlotData> loadTransaction = transaction as LoadTransaction<SaveSlotData>;
			if (loadTransaction.IsSuccess)
			{
				HandleLoadUserSaveSlotSuccess(loadTransaction);
			}
			else
			{
				yield return HandleLoadUserSaveSlotErrorASync(loadTransaction);
			}
		}

		private void HandleLoadUserSaveSlotSuccess(LoadTransaction<SaveSlotData> loadSaveSlotTransaction)
		{
			SaveSlotData data = loadSaveSlotTransaction.Data;
			saveManager.AddSaveSlotData(data, loadSaveSlotTransaction.FileName);
		}

		private IEnumerator HandleLoadUserSaveSlotErrorASync(LoadTransaction<SaveSlotData> loadSaveSlotTransaction)
		{
			if (loadSaveSlotTransaction.Status != ATransaction.TransactionStatus.FILE_NOT_FOUND)
			{
				yield return HandleLoadUserSaveDataErrorASync(loadSaveSlotTransaction, base.DoTransactionASync, base.HandleLoadUserSaveDataErrorASync);
			}
		}
	}
}
