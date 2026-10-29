using System.Collections;

namespace Reptile
{
	public class UserSaveSlotSettingsDataLoader : AUserSaveDataLoader
	{
		public UserSaveSlotSettingsDataLoader(SaveManager saveManager, AStorage storage, AErrorHandler errorHandler, UIManager uIManager, IGameTextLocalizer localizer)
			: base(saveManager, storage, errorHandler, uIManager, localizer)
		{
		}

		public override IEnumerator LoadSaveDataASync()
		{
			yield return LoadUserSaveSlotSettingsASync();
		}

		public override string ToString()
		{
			return "User save slot settings";
		}

		private IEnumerator LoadUserSaveSlotSettingsASync()
		{
			ATransaction loadSaveSlotSettingsTransaction = saveManager.LoadSaveSlotSettings();
			yield return DoTransactionASync(loadSaveSlotSettingsTransaction, base.HandleLoadUserSaveDataErrorASync, base.DoTransactionASync);
			if (isSuccess)
			{
				HandleLoadUserSaveSlotSettingsSuccess(loadSaveSlotSettingsTransaction);
			}
		}

		private void HandleLoadUserSaveSlotSettingsSuccess(ATransaction loadSaveSlotSettingsTransaction)
		{
			if (loadSaveSlotSettingsTransaction.IsSuccess || errorHandlingResult == ErrorHandlingResult.RESOLVED)
			{
				saveManager.SetCurrentSaveSlotFromSettings();
			}
			else if (loadSaveSlotSettingsTransaction.Status == ATransaction.TransactionStatus.FILE_NOT_FOUND || errorHandlingResult == ErrorHandlingResult.DELETED)
			{
				saveManager.AttemptSetCurrentSaveSlot();
			}
		}
	}
}
