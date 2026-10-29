using System.Collections;

namespace Reptile
{
	public class UserPlayerStatsLoader : AUserSaveDataLoader
	{
		public UserPlayerStatsLoader(SaveManager saveManager, AStorage storage, AErrorHandler errorHandler, UIManager uIManager, IGameTextLocalizer localizer)
			: base(saveManager, storage, errorHandler, uIManager, localizer)
		{
		}

		public override IEnumerator LoadSaveDataASync()
		{
			ATransaction loadTransaction = saveManager.LoadPlayerStats();
			yield return DoTransactionASync(loadTransaction, base.HandleLoadUserSaveDataErrorASync, base.DoTransactionASync);
			if (isSuccess)
			{
				HandleLoadPlayerStatsSuccess(loadTransaction);
			}
		}

		private void HandleLoadPlayerStatsSuccess(ATransaction loadTransaction)
		{
			if (loadTransaction.Status == ATransaction.TransactionStatus.FILE_NOT_FOUND)
			{
				saveManager.SavePlayerStats();
			}
		}
	}
}
