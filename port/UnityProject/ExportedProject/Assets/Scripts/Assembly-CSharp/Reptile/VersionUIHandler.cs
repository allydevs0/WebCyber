using TMPro;
using UnityEngine;

namespace Reptile
{
	public class VersionUIHandler : MonoBehaviour
	{
		private const string VERSION_LOCALIZATION_KEY = "MAIN_MENU_VERSION";

		public TextMeshProUGUI versionText;

		private IGameTextLocalizer localizer;

		public void Awake()
		{
			localizer = Core.Instance.Localizer;
			localizer.OnLanguageChanged += OnLanguageChanged;
			SetVersionText();
		}

		protected void OnDestroy()
		{
			localizer.OnLanguageChanged -= OnLanguageChanged;
		}

		private void OnLanguageChanged(IGameTextLocalizer localizer)
		{
			SetVersionText();
		}

		private void SetVersionText()
		{
			string userInterfaceText = localizer.GetUserInterfaceText("MAIN_MENU_VERSION");
			versionText.text = "<allcaps>" + userInterfaceText + ": " + Core.Instance.GameVersion.ToString() + "</allcaps> - " + Core.Instance.Platform.User.UserName;
		}
	}
}
