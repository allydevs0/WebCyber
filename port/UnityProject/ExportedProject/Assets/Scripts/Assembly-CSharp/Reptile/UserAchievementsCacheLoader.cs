using System.Collections;

namespace Reptile
{
	public class UserAchievementsCacheLoader : AUserSaveDataLoader
	{
		public UserAchievementsCacheLoader(SaveManager saveManager, AStorage storage, AErrorHandler errorHandler, UIManager uIManager, IGameTextLocalizer localizer)
			: base(saveManager, storage, errorHandler, uIManager, localizer)
		{
		}

		public override IEnumerator LoadSaveDataASync()
		{
			ATransaction loadTransaction = saveManager.LoadAchievementsCache();
			yield return DoTransactionASync(loadTransaction, base.HandleLoadUserSaveDataErrorASync, base.DoTransactionASync);
			if (isSuccess)
			{
				HandleLoadAchievementCacheSuccess(loadTransaction);
			}
		}

		private void HandleLoadAchievementCacheSuccess(ATransaction loadTransaction)
		{
			if (loadTransaction.Status == ATransaction.TransactionStatus.FILE_NOT_FOUND)
			{
				saveManager.SaveAchievementsCache();
			}
		}
	}
}
