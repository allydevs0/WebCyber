using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class OptionsMenuGameTab : AOptionsMenuTab
	{
		[SerializeField]
		private TextMeshProSlider cameraControlSensitivitySlider;

		[SerializeField]
		private MultiOptionButton invertYAxisField;

		[SerializeField]
		private MultiOptionButton gamepadVibrationField;

		[SerializeField]
		private MultiOptionButton languageField;

		[SerializeField]
		private MultiOptionButton frameRateLockField;

		[SerializeField]
		private GameObject frameRateLockGameObject;

		[SerializeField]
		private LocalizationNameFont languageFieldLocalization;

		[SerializeField]
		private TextMeshProMenuButton applyLanguageButton;

		[SerializeField]
		private TMProLocalizationAddOn applyLanguageLocalization;

		[SerializeField]
		private TextMeshProMenuButtonFont applyLanguageLocalizationFont;

		[SerializeField]
		private Button resetDefaultsButton;

		private GameplaySettings gameplaySettings;

		private IGameTextLocalizer localizer;

		private IGameTextLocalizer.LanguagesLocalizedOptionText allAvailableLanguages;

		private string unappliedLanguage;

		public override void Activate()
		{
			base.Activate();
			RefreshGameSettings();
			cameraControlSensitivitySlider.onValueChanged.AddListener(OnCameraControlSensitivitySliderValueChanged);
			invertYAxisField.OnChanged += OnInvertYAxisSettingChanged;
			gamepadVibrationField.OnChanged += OnGamepadVibrationSettingChanged;
			languageField.OnChanged += OnLanguageSettingChanged;
		}

		public override void Deactivate()
		{
			base.Deactivate();
			cameraControlSensitivitySlider.onValueChanged.RemoveListener(OnCameraControlSensitivitySliderValueChanged);
			invertYAxisField.OnChanged -= OnInvertYAxisSettingChanged;
			gamepadVibrationField.OnChanged -= OnGamepadVibrationSettingChanged;
			unappliedLanguage = string.Empty;
			languageField.OnChanged -= OnLanguageSettingChanged;
		}

		public void ApplyLanguage()
		{
			if (localizer.ApplyNewLocalizationLanguage(unappliedLanguage))
			{
				gameplaySettings.currentLanguage = localizer.Language;
				RefreshGameSettings();
			}
		}

		public void ResetToDefaults()
		{
			gameplaySettings.aimSensitivity = 0.5f;
			gameplaySettings.invertY = false;
			gameplaySettings.rumble = true;
			gameplaySettings.currentLanguage = localizer.DefaultLanguage;
			localizer.UpdateLocalization(gameplaySettings.currentLanguage);
			saveManager.SaveSettings();
			RefreshGameSettings();
		}

		protected override void Awake()
		{
			base.Awake();
			gameplaySettings = saveManager.Settings.gameplaySettings;
			localizer = Core.Instance.Localizer;
			allAvailableLanguages = localizer.AllAvailableLanguages;
			AdjustGameTabBasedOnPlatform();
			SetupMultiButtonOptions();
		}

		private void AdjustGameTabBasedOnPlatform()
		{
			frameRateLockGameObject.gameObject.SetActive(value: false);
			Navigation navigation = languageField.navigation;
			navigation.selectOnDown = applyLanguageButton;
			languageField.navigation = navigation;
			Navigation navigation2 = applyLanguageButton.navigation;
			navigation2.selectOnUp = languageField;
			applyLanguageButton.navigation = navigation2;
		}

		private void RefreshGameSettings()
		{
			string text = localizer.ConvertToLanguageName(gameplaySettings.currentLanguage);
			RefreshCameraControlSensitivityOption();
			RefreshOnOffField(invertYAxisField, gameplaySettings.invertY);
			RefreshOnOffField(gamepadVibrationField, gameplaySettings.rumble);
			RefreshLanguageField(text);
			SetupMultiButtonOptions();
			RefreshApplyLanguageField(text, languageField.CurrentIndex);
		}

		private void RefreshCameraControlSensitivityOption()
		{
			cameraControlSensitivitySlider.SetSliderValue(gameplaySettings.aimSensitivity);
		}

		private void RefreshOnOffField(MultiOptionButton multiOptionButton, bool currentSavedSetting)
		{
			string gameplayOnOffOption = UILocalizationUtility.GetGameplayOnOffOption(localizer, currentSavedSetting);
			multiOptionButton.SetLabelText(gameplayOnOffOption);
		}

		private void RefreshLanguageField(string language)
		{
			languageField.SetLabelText(language);
			languageFieldLocalization.UpdateFont(language);
		}

		private void RefreshApplyLanguageField(string languageName, int languageIndex)
		{
			SystemLanguage language = localizer.ConvertToLanguage(languageName);
			string text = allAvailableLanguages.languagesApplyText[languageIndex];
			applyLanguageButton.SetText(text);
			applyLanguageLocalizationFont.UpdateTextMeshProFonts(language);
		}

		private void OnCameraControlSensitivitySliderValueChanged(float newValue)
		{
			gameplaySettings.aimSensitivity = cameraControlSensitivitySlider.CalculatePercentile();
		}

		private void OnInvertYAxisSettingChanged(MultiOptionButton multiOptionButton)
		{
			gameplaySettings.invertY = invertYAxisField.CurrentIndex == 0;
			RefreshOnOffField(invertYAxisField, gameplaySettings.invertY);
		}

		private void OnGamepadVibrationSettingChanged(MultiOptionButton multiOptionButton)
		{
			gameplaySettings.rumble = gamepadVibrationField.CurrentIndex == 0;
			Core.Instance.GameInput.SetVibrationOnCurrentController(0.38f, 0.38f, 0.2f);
			RefreshOnOffField(gamepadVibrationField, gameplaySettings.rumble);
		}

		private void OnLanguageSettingChanged(MultiOptionButton multiOptionButton)
		{
			unappliedLanguage = allAvailableLanguages.languagesNames[multiOptionButton.CurrentIndex];
			RefreshLanguageField(unappliedLanguage);
			RefreshApplyLanguageField(unappliedLanguage, multiOptionButton.CurrentIndex);
		}

		private void SetupMultiButtonOptions()
		{
			SetupOnOffField(invertYAxisField, gameplaySettings.invertY);
			SetupOnOffField(gamepadVibrationField, gameplaySettings.rumble);
			SetupLanguageField();
		}

		private void SetupOnOffField(MultiOptionButton multiOptionButton, bool currentSavedSetting)
		{
			string gameplayOnOffOption = UILocalizationUtility.GetGameplayOnOffOption(localizer, currentSavedSetting);
			int currentIndex = 0;
			if (!currentSavedSetting)
			{
				currentIndex = 1;
			}
			multiOptionButton.SetupOptions(gameplayOnOffOption, currentIndex, 2);
		}

		private void SetupLanguageField()
		{
			string text = localizer.ConvertToLanguageName(gameplaySettings.currentLanguage);
			int currentIndex = 0;
			for (int i = 0; i < allAvailableLanguages.languagesNames.Length; i++)
			{
				if (text.Equals(allAvailableLanguages.languagesNames[i]))
				{
					currentIndex = i;
					break;
				}
			}
			languageField.SetupOptions(text, currentIndex, allAvailableLanguages.languagesNames.Length);
		}
	}
}
