using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class UserWeb : AUser
	{
		public UserWeb()
		{
			videoSettingsManager = new VideoSettingsManager();
		}

		protected override IUserId GetDefaultUserID()
		{
			userId = new UserIdWeb("Player");
			return userId;
		}

		protected override IEnumerator LoginUserASync()
		{
			isLoggedIn = true;
			videoSettingsManager.Init(VideoSettingsManager.GetDefaultAvailableVideoOptions());
			yield break;
		}

		protected override void LogoutUser()
		{
			isLoggedIn = false;
			userId = new UserIdWeb(string.Empty);
		}

		public override void SetUserSettings()
		{
			base.SetUserSettings();
			SetUserVideoSettings();
		}

		public override SystemLanguage GetUserLanguage()
		{
			if (saveManager != null && saveManager.Settings != null && saveManager.Settings.gameplaySettings != null)
			{
				return saveManager.Settings.gameplaySettings.currentLanguage;
			}
			return Application.systemLanguage;
		}
	}
}
