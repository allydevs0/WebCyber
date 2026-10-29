using System.Collections;

namespace Reptile
{
	public class UserSettingsSaveDataLoader : AUserSaveDataLoader
	{
		private AUser user;

		public UserSettingsSaveDataLoader(SaveManager saveManager, AStorage storage, AErrorHandler errorHandler, UIManager uIManager, AUser user, IGameTextLocalizer localizer)
			: base(saveManager, storage, errorHandler, uIManager, localizer)
		{
			this.user = user;
		}

		public override IEnumerator LoadSaveDataASync()
		{
			yield return LoadUserSettingsASync();
		}

		public override string ToString()
		{
			return "User settings";
		}

		private IEnumerator LoadUserSettingsASync()
		{
			yield return HandleLoadSettingsSteam();
		}

		private IEnumerator HandleLoadSettingsSteam()
		{
			ATransaction loadSettingsTransaction = saveManager.LoadSettings();
			yield return DoTransactionASync(loadSettingsTransaction, base.HandleLoadUserSaveDataErrorASync, base.DoTransactionASync);
			if (isSuccess && loadSettingsTransaction.Status != ATransaction.TransactionStatus.FILE_NOT_FOUND && errorHandlingResult != ErrorHandlingResult.DELETED)
			{
				TransferSettingsToLocalDisk();
			}
			else
			{
				yield return LoadSettingsLocal();
			}
		}

		private void TransferSettingsToLocalDisk()
		{
			user.SetUserSettings();
			storage.DeleteDataFromStorage("GameSettings.brs", "SaveData", isLocal: false);
			saveManager.SaveSettings();
		}

		private IEnumerator LoadSettingsLocal()
		{
			ATransaction loadSettingsTransaction = saveManager.LoadSettingsLocal();
			yield return DoTransactionASync(loadSettingsTransaction, base.HandleLoadUserSaveDataErrorASync, base.DoTransactionASync);
			yield return HandleLoadSettingsFinished(loadSettingsTransaction);
		}

		private IEnumerator HandleLoadSettingsFinished(ATransaction loadSettingsTransaction)
		{
			if (isSuccess)
			{
				HandleLoadSettingsSuccess(loadSettingsTransaction);
			}
			yield break;
		}

		private void ResetUserSettings()
		{
			saveManager.ResetSettings();
			user.SetUserSettings();
		}

		private void HandleLoadSettingsSuccess(ATransaction loadSettingsTransaction)
		{
			if (loadSettingsTransaction.IsSuccess || errorHandlingResult == ErrorHandlingResult.RESOLVED)
			{
				user.SetUserSettings();
			}
			else if (loadSettingsTransaction.Status == ATransaction.TransactionStatus.FILE_NOT_FOUND || errorHandlingResult == ErrorHandlingResult.DELETED)
			{
				CreateNewUserSettings();
			}
		}

		private void CreateNewUserSettings()
		{
			saveManager.ResetSettings();
			saveManager.Settings.gameplaySettings.currentLanguage = localizer.Language;
			user.SetUserSettings();
			saveManager.SaveSettings();
		}
	}
}
