using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class OptionsMenuVideoTab : AOptionsMenuTab
	{
		private const int NO_QUALITY_PRESET = -1;

		[SerializeField]
		private MultiOptionButton screenModeOptions;

		[SerializeField]
		private MultiOptionButton screenResolutionOptions;

		[SerializeField]
		private MultiOptionButton screenVsyncOptions;

		[SerializeField]
		private MultiOptionButton qualityPresetOptions;

		[SerializeField]
		private MultiOptionButton antiAliasingOptions;

		[SerializeField]
		private MultiOptionButton anisotropicFilteringOptions;

		[SerializeField]
		private MultiOptionButton shadowQualityOptions;

		[SerializeField]
		private MultiOptionButton lodDistanceOptions;

		private IGameTextLocalizer localizer;

		private VideoSettingsManager videoSettingsManager;

		private SettingsData settingsData;

		private int originalUnappliedQualitySetting = -1;

		private VideoSettingsManager.VideoSettingsSet initialVideoSettings;

		public override void Activate()
		{
			base.Activate();
			initialVideoSettings = videoSettingsManager.GetVideoSettingsInitialSet();
			RefreshMainVideoOptions(initialVideoSettings);
			RefreshAdvancedVideoOptions(initialVideoSettings);
			SetupListeners();
			originalUnappliedQualitySetting = -1;
		}

		public override void Deactivate()
		{
			if (isActive)
			{
				RemoveListeners();
			}
			if (originalUnappliedQualitySetting != -1)
			{
				UpdateQualityPreset(originalUnappliedQualitySetting, qualityPresetOptions);
				videoSettingsManager.ApplyBackupVideoSettings(originalUnappliedQualitySetting);
				originalUnappliedQualitySetting = -1;
			}
			base.Deactivate();
		}

		public void ApplyVideoSettings()
		{
			videoSettingsManager.ApplyScreenResolution(videoSettingsManager.GetResolutionOption(screenResolutionOptions.CurrentIndex).option);
			videoSettingsManager.ApplyFullScreenMode(videoSettingsManager.GetFullscreenOption(screenModeOptions.CurrentIndex).option);
			videoSettingsManager.Apply();
			originalUnappliedQualitySetting = -1;
		}

		public void ResetToDefaults()
		{
			videoSettingsManager.ResetToDefault(settingsData);
			initialVideoSettings = videoSettingsManager.GetVideoSettingsInitialSet();
			StartCoroutine(RefreshAfterFrameUpdate());
		}

		private IEnumerator RefreshAfterFrameUpdate()
		{
			yield return 0;
			RefreshMainVideoOptions(initialVideoSettings);
			RefreshAdvancedVideoOptions(initialVideoSettings);
		}

		protected override void Awake()
		{
			base.Awake();
			localizer = Core.Instance.Localizer;
			videoSettingsManager = Core.Instance.Platform.User.VideoSettingsManager;
			settingsData = Core.Instance.SaveManager.Settings;
		}

		protected void OnDestroy()
		{
			RemoveListeners();
		}

		private void RefreshMainVideoOptions(VideoSettingsManager.VideoSettingsSet videoSettingsSet)
		{
			screenModeOptions.SetupOptions(localizer.GetUserInterfaceText(videoSettingsSet.fullScreenMode.name), videoSettingsSet.fullScreenMode.currentIndex, videoSettingsSet.fullScreenMode.maxOptions);
			screenResolutionOptions.SetupOptions(localizer.GetUserInterfaceText(videoSettingsSet.resolution.name), videoSettingsSet.resolution.currentIndex, videoSettingsSet.resolution.maxOptions);
			qualityPresetOptions.SetupOptions(localizer.GetUserInterfaceText(videoSettingsSet.qualityPreset.name), videoSettingsSet.qualityPreset.currentIndex, videoSettingsSet.qualityPreset.maxOptions);
			screenVsyncOptions.SetupOptions(localizer.GetUserInterfaceText(videoSettingsSet.vSync.name), videoSettingsSet.vSync.currentIndex, videoSettingsSet.vSync.maxOptions);
		}

		private void RefreshAdvancedVideoOptions(VideoSettingsManager.VideoSettingsSet videoSettingsSet)
		{
			antiAliasingOptions.SetupOptions(localizer.GetUserInterfaceText(videoSettingsSet.antiAliasing.name) + "x", videoSettingsSet.antiAliasing.currentIndex, videoSettingsSet.antiAliasing.maxOptions);
			anisotropicFilteringOptions.SetupOptions(localizer.GetUserInterfaceText(videoSettingsSet.anisotropicFiltering.name), videoSettingsSet.anisotropicFiltering.currentIndex, videoSettingsSet.anisotropicFiltering.maxOptions);
			shadowQualityOptions.SetupOptions(localizer.GetUserInterfaceText(videoSettingsSet.shadows.name), videoSettingsSet.shadows.currentIndex, videoSettingsSet.shadows.maxOptions);
			lodDistanceOptions.SetupOptions(localizer.GetUserInterfaceText(videoSettingsSet.lodDistance.name), videoSettingsSet.lodDistance.currentIndex, videoSettingsSet.lodDistance.maxOptions);
		}

		private void SetupListeners()
		{
			screenModeOptions.OnChanged += OnScreenModeChanged;
			screenResolutionOptions.OnChanged += OnScreenResolutionChanged;
			qualityPresetOptions.OnChanged += OnQualityPresetChanged;
			screenVsyncOptions.OnChanged += OnVsyncChanged;
			antiAliasingOptions.OnChanged += OnAntiAliasingChanged;
			anisotropicFilteringOptions.OnChanged += OnAnisotropicFilteringChanged;
			shadowQualityOptions.OnChanged += OnShadowQualityChanged;
			lodDistanceOptions.OnChanged += OnLodDistanceChanged;
		}

		private void OnScreenModeChanged(MultiOptionButton multiOptionButton)
		{
			VideoSettingsManager.VideoOptionPair<FullScreenMode> fullscreenOption = videoSettingsManager.GetFullscreenOption(multiOptionButton.CurrentIndex);
			AdjustMultiOptionSelectable(multiOptionButton, fullscreenOption);
		}

		private void AdjustMultiOptionSelectable<T>(MultiOptionButton multiOptionSelectable, VideoSettingsManager.VideoOptionPair<T> videoOptionPair)
		{
			string userInterfaceText = localizer.GetUserInterfaceText(videoOptionPair.name);
			multiOptionSelectable.SetLabelText(userInterfaceText);
		}

		private void OnScreenResolutionChanged(MultiOptionButton multiOptionButton)
		{
			VideoSettingsManager.VideoOptionPair<Resolution> resolutionOption = videoSettingsManager.GetResolutionOption(multiOptionButton.CurrentIndex);
			AdjustMultiOptionSelectable(multiOptionButton, resolutionOption);
		}

		private void OnQualityPresetChanged(MultiOptionButton multiOptionButton)
		{
			MarkVideoSettingsChange();
			UpdateQualityPreset(multiOptionButton.CurrentIndex, multiOptionButton);
		}

		protected void MarkVideoSettingsChange()
		{
			if (originalUnappliedQualitySetting == -1)
			{
				originalUnappliedQualitySetting = videoSettingsManager.BackupVideoSettings();
			}
		}

		private void UpdateQualityPreset(int qualityPresetIndex, MultiOptionButton multiOptionButton)
		{
			VideoSettingsManager.VideoOptionPair<int> videoOptionPair = videoSettingsManager.SetQualityOption(qualityPresetIndex);
			AdjustMultiOptionSelectable(multiOptionButton, videoOptionPair);
			videoSettingsManager.Apply();
			RefreshAdvancedVideoOptions(videoSettingsManager.GetVideoSettingsInitialSet());
		}

		private void OnVsyncChanged(MultiOptionButton multiOptionButton)
		{
			MarkVideoSettingsChange();
			VideoSettingsManager.VideoOptionPair<int> videoOptionPair = videoSettingsManager.SetVSyncOption(multiOptionButton.CurrentIndex);
			AdjustMultiOptionSelectable(multiOptionButton, videoOptionPair);
		}

		private void OnAntiAliasingChanged(MultiOptionButton multiOptionButton)
		{
			MarkVideoSettingsChange();
			videoSettingsManager.CopyQualitySettingsToCustom();
			VideoSettingsManager.VideoOptionPair<int> videoOptionPair = videoSettingsManager.SetAntiAliasingOption(multiOptionButton.CurrentIndex);
			AdjustMultiOptionSelectable(multiOptionButton, videoOptionPair);
			UpdateQualityPreset(-1, qualityPresetOptions);
		}

		private void OnAnisotropicFilteringChanged(MultiOptionButton multiOptionButton)
		{
			MarkVideoSettingsChange();
			videoSettingsManager.CopyQualitySettingsToCustom();
			VideoSettingsManager.VideoOptionPair<AnisotropicFiltering> videoOptionPair = videoSettingsManager.SetAnisotropicFilteringOption(multiOptionButton.CurrentIndex);
			AdjustMultiOptionSelectable(multiOptionButton, videoOptionPair);
			UpdateQualityPreset(-1, qualityPresetOptions);
		}

		private void OnShadowQualityChanged(MultiOptionButton multiOptionButton)
		{
			MarkVideoSettingsChange();
			videoSettingsManager.CopyQualitySettingsToCustom();
			VideoSettingsManager.VideoOptionPair<VideoSettings.ShadowsQualityPreset> videoOptionPair = videoSettingsManager.SetShadowsOption(multiOptionButton.CurrentIndex);
			AdjustMultiOptionSelectable(multiOptionButton, videoOptionPair);
			UpdateQualityPreset(-1, qualityPresetOptions);
		}

		private void OnLodDistanceChanged(MultiOptionButton multiOptionButton)
		{
			MarkVideoSettingsChange();
			videoSettingsManager.CopyQualitySettingsToCustom();
			VideoSettingsManager.VideoOptionPair<float> videoOptionPair = videoSettingsManager.SetLODDistanceOption(multiOptionButton.CurrentIndex);
			AdjustMultiOptionSelectable(multiOptionButton, videoOptionPair);
			UpdateQualityPreset(-1, qualityPresetOptions);
		}

		private void RemoveListeners()
		{
			screenModeOptions.OnChanged -= OnScreenModeChanged;
			screenResolutionOptions.OnChanged -= OnScreenResolutionChanged;
			qualityPresetOptions.OnChanged -= OnQualityPresetChanged;
			screenVsyncOptions.OnChanged -= OnVsyncChanged;
			antiAliasingOptions.OnChanged -= OnAntiAliasingChanged;
			anisotropicFilteringOptions.OnChanged -= OnAnisotropicFilteringChanged;
			shadowQualityOptions.OnChanged -= OnShadowQualityChanged;
			lodDistanceOptions.OnChanged -= OnLodDistanceChanged;
		}
	}
}
