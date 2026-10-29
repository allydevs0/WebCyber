using System.Runtime.CompilerServices;
using UnityEngine;

namespace Reptile
{
	public class VideoSettingsManager
	{
		public struct VideoOptionPair<T>
		{
			public T option;

			public string name;

			public VideoOptionPair(T key, string value)
			{
				option = key;
				name = value;
			}
		}

		public struct AvailableVideoOptions
		{
			public VideoOptionPair<FullScreenMode>[] fullScreenModes;

			public VideoOptionPair<Resolution>[] resolutions;

			public VideoOptionPair<int>[] vSync;

			public VideoOptionPair<int>[] qualityPresets;

			public VideoOptionPair<int>[] antiAliasing;

			public VideoOptionPair<AnisotropicFiltering>[] anisotropicFiltering;

			public VideoOptionPair<VideoSettings.ShadowsQualityPreset>[] shadows;

			public VideoOptionPair<float>[] lodDistances;
		}

		public struct VideoSettingInfo<T>
		{
			public int currentIndex;

			public int maxOptions;

			public string name;
		}

		public struct VideoSettingsSet
		{
			public VideoSettingInfo<FullScreenMode> fullScreenMode;

			public VideoSettingInfo<Resolution> resolution;

			public VideoSettingInfo<int> vSync;

			public VideoSettingInfo<int> qualityPreset;

			public VideoSettingInfo<int> antiAliasing;

			public VideoSettingInfo<AnisotropicFiltering> anisotropicFiltering;

			public VideoSettingInfo<VideoSettings.ShadowsQualityPreset> shadows;

			public VideoSettingInfo<float> lodDistance;
		}

		public delegate bool CompareOptions<T>(T option1, T option2);

		private VideoSettings cachedVideoSettings;

		private VideoSettings tempVideoSettings = new VideoSettings();

		private AvailableVideoOptions availableVideoOptions;

		private int customQualityPresetInAvailableOptionsIndex;

		public void Init(AvailableVideoOptions allowedVideoOptionsForPlatform)
		{
			availableVideoOptions = allowedVideoOptionsForPlatform;
			for (int i = 0; i < availableVideoOptions.qualityPresets.Length; i++)
			{
				if (availableVideoOptions.qualityPresets[i].name.Equals("VS_CUSTOM"))
				{
					customQualityPresetInAvailableOptionsIndex = i;
					break;
				}
			}
		}

		public void InitializeVideoSettings(VideoSettings videoSettings)
		{
			cachedVideoSettings = videoSettings;
			Resolution resolution = cachedVideoSettings.GetResolution();
			SetVideoSettingsFromPreset(cachedVideoSettings.qualityPreset);
			SetScreenVideoSettings(resolution, Screen.fullScreenMode);
			InitializeAvailableOptions();
			Apply();
			BackupVideoSettings();
		}

		public void ResetToDefault(SettingsData settingsData)
		{
			settingsData.ResetVideoSettings();
			cachedVideoSettings = settingsData.videoSettings;
			Resolution highestScreenResolution = GetHighestScreenResolution();
			SetVideoSettingsFromPreset(cachedVideoSettings.qualityPreset);
			SetScreenVideoSettings(highestScreenResolution, Screen.fullScreenMode);
			Apply();
		}

		public void Apply()
		{
			ApplyVideoSettings(cachedVideoSettings);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void CopyVideoSettings(VideoSettings source, VideoSettings destination)
		{
			destination.qualityPreset = source.qualityPreset;
			destination.vSyncCount = source.vSyncCount;
			destination.antiAliasing = source.antiAliasing;
			destination.anisotropicFiltering = source.anisotropicFiltering;
			destination.shadowDistance = source.shadowDistance;
			destination.shadowResolution = source.shadowResolution;
			destination.shadowQuality = source.shadowQuality;
			destination.shadowmaskMode = source.shadowmaskMode;
			destination.shadowCascades = source.shadowCascades;
			destination.lodDistance = source.lodDistance;
		}

		public int BackupVideoSettings()
		{
			CopyVideoSettings(cachedVideoSettings, tempVideoSettings);
			return FindCurrentIndexOf(tempVideoSettings.qualityPreset, availableVideoOptions.qualityPresets);
		}

		public int ApplyBackupVideoSettings(int qualityPresetIndex)
		{
			int num = ((qualityPresetIndex == -1) ? customQualityPresetInAvailableOptionsIndex : qualityPresetIndex);
			_ = ref availableVideoOptions.qualityPresets[num];
			CopyVideoSettings(tempVideoSettings, cachedVideoSettings);
			ApplyVideoSettings(cachedVideoSettings);
			return num;
		}

		public void ApplyScreenResolution(Resolution resolution)
		{
			cachedVideoSettings.SetResolution(resolution);
			Apply();
		}

		public void ApplyFullScreenMode(FullScreenMode fullScreenMode)
		{
			cachedVideoSettings.fullScreenMode = fullScreenMode;
			Apply();
		}

		private void InitializeAvailableOptions()
		{
			RetrieveAvailableScreenOptions(ref availableVideoOptions);
			RetrieveAvailableOptionsFromPresets(ref availableVideoOptions);
		}

		private void SetVideoSettingsFromPreset(int qualityLevel)
		{
			QualitySettings.SetQualityLevel(qualityLevel);
			CopyQualitySettingsToCustom();
		}

		private void SetScreenVideoSettings(Resolution resolution, FullScreenMode fullScreenMode)
		{
			cachedVideoSettings.SetResolution(resolution);
			cachedVideoSettings.fullScreenMode = fullScreenMode;
		}

		private void ApplyVideoSettings(VideoSettings videoSettings)
		{
			ApplyResolution(videoSettings);
			ApplyQualitySettings(videoSettings);
			ApplyVSync(videoSettings);
		}

		private void ApplyResolution(VideoSettings videoSettings)
		{
			Resolution resolution = videoSettings.GetResolution();
			Screen.SetResolution(resolution.width, resolution.height, videoSettings.fullScreenMode);
		}

		private void ApplyVSync(VideoSettings videoSettings)
		{
			QualitySettings.vSyncCount = videoSettings.vSyncCount;
		}

		private void ApplyQualitySettings(VideoSettings videoSettings)
		{
			if (videoSettings.qualityPreset != availableVideoOptions.qualityPresets[customQualityPresetInAvailableOptionsIndex].option)
			{
				QualitySettings.SetQualityLevel(videoSettings.qualityPreset);
				return;
			}
			QualitySettings.SetQualityLevel(availableVideoOptions.qualityPresets[customQualityPresetInAvailableOptionsIndex].option);
			QualitySettings.antiAliasing = videoSettings.antiAliasing;
			QualitySettings.anisotropicFiltering = videoSettings.anisotropicFiltering;
			QualitySettings.shadowDistance = videoSettings.shadowDistance;
			QualitySettings.shadowResolution = videoSettings.shadowResolution;
			QualitySettings.shadows = videoSettings.shadowQuality;
			QualitySettings.shadowmaskMode = videoSettings.shadowmaskMode;
			QualitySettings.shadowCascades = videoSettings.shadowCascades;
			QualitySettings.lodBias = videoSettings.lodDistance;
		}

		public void CopyQualitySettingsToCustom()
		{
			if (QualitySettings.GetQualityLevel() != availableVideoOptions.qualityPresets[customQualityPresetInAvailableOptionsIndex].option)
			{
				cachedVideoSettings.antiAliasing = QualitySettings.antiAliasing;
				cachedVideoSettings.anisotropicFiltering = QualitySettings.anisotropicFiltering;
				cachedVideoSettings.shadowDistance = QualitySettings.shadowDistance;
				cachedVideoSettings.shadowResolution = QualitySettings.shadowResolution;
				cachedVideoSettings.shadowQuality = QualitySettings.shadows;
				cachedVideoSettings.shadowmaskMode = QualitySettings.shadowmaskMode;
				cachedVideoSettings.shadowCascades = QualitySettings.shadowCascades;
				cachedVideoSettings.lodDistance = QualitySettings.lodBias;
			}
		}

		private int FindCurrentIndexOf<T>(T currentSettingValue, VideoOptionPair<T>[] videoOptionPairs)
		{
			for (int i = 0; i < videoOptionPairs.Length; i++)
			{
				if (videoOptionPairs[i].option.Equals(currentSettingValue))
				{
					return i;
				}
			}
			return 0;
		}

		private void SetVideoSettingInfo<T>(ref VideoSettingInfo<T> infoToSet, T currentValue, VideoOptionPair<T>[] videoOptionPairs)
		{
			VideoSettingInfo<T> videoSettingInfo = default;
			int num = (videoSettingInfo.currentIndex = FindCurrentIndexOf(currentValue, videoOptionPairs));
			videoSettingInfo.maxOptions = videoOptionPairs.Length;
			videoSettingInfo.name = videoOptionPairs[num].name;
			infoToSet = videoSettingInfo;
		}

		internal VideoSettingsSet GetVideoSettingsInitialSet()
		{
			VideoSettingsSet result = default;
			SetVideoSettingInfo(ref result.fullScreenMode, cachedVideoSettings.fullScreenMode, availableVideoOptions.fullScreenModes);
			SetVideoSettingInfo(ref result.resolution, cachedVideoSettings.GetResolution(), availableVideoOptions.resolutions);
			SetVideoSettingInfo(ref result.vSync, cachedVideoSettings.vSyncCount, availableVideoOptions.vSync);
			SetVideoSettingInfo(ref result.qualityPreset, cachedVideoSettings.qualityPreset, availableVideoOptions.qualityPresets);
			if (cachedVideoSettings.qualityPreset != availableVideoOptions.qualityPresets[customQualityPresetInAvailableOptionsIndex].option)
			{
				SetVideoSettingInfo(ref result.antiAliasing, QualitySettings.antiAliasing, availableVideoOptions.antiAliasing);
				SetVideoSettingInfo(ref result.anisotropicFiltering, QualitySettings.anisotropicFiltering, availableVideoOptions.anisotropicFiltering);
				SetVideoSettingInfo(ref result.shadows, GetShadowPresetFromQualitySettings(), availableVideoOptions.shadows);
				SetVideoSettingInfo(ref result.lodDistance, QualitySettings.lodBias, availableVideoOptions.lodDistances);
			}
			else
			{
				SetVideoSettingInfo(ref result.antiAliasing, cachedVideoSettings.antiAliasing, availableVideoOptions.antiAliasing);
				SetVideoSettingInfo(ref result.anisotropicFiltering, cachedVideoSettings.anisotropicFiltering, availableVideoOptions.anisotropicFiltering);
				SetVideoSettingInfo(ref result.shadows, cachedVideoSettings.GetShadowsQualityPreset(), availableVideoOptions.shadows);
				SetVideoSettingInfo(ref result.lodDistance, cachedVideoSettings.lodDistance, availableVideoOptions.lodDistances);
			}
			return result;
		}

		private VideoOptionPair<T> GetOptionPair<T>(VideoOptionPair<T>[] availableVideoOptionsForSetting, int index)
		{
			if (index < 0 || index >= availableVideoOptionsForSetting.Length)
			{
				index = 0;
			}
			return availableVideoOptionsForSetting[index];
		}

		public VideoOptionPair<FullScreenMode> GetFullscreenOption(int index)
		{
			return GetOptionPair(availableVideoOptions.fullScreenModes, index);
		}

		public VideoOptionPair<Resolution> GetResolutionOption(int index)
		{
			return GetOptionPair(availableVideoOptions.resolutions, index);
		}

		public VideoOptionPair<int> SetVSyncOption(int index)
		{
			VideoOptionPair<int> optionPair = GetOptionPair(availableVideoOptions.vSync, index);
			cachedVideoSettings.vSyncCount = optionPair.option;
			return optionPair;
		}

		public VideoOptionPair<int> SetQualityOption(int index)
		{
			if (index == -1)
			{
				index = customQualityPresetInAvailableOptionsIndex;
			}
			VideoOptionPair<int> optionPair = GetOptionPair(availableVideoOptions.qualityPresets, index);
			cachedVideoSettings.qualityPreset = optionPair.option;
			return optionPair;
		}

		public VideoOptionPair<int> SetAntiAliasingOption(int index)
		{
			VideoOptionPair<int> optionPair = GetOptionPair(availableVideoOptions.antiAliasing, index);
			cachedVideoSettings.antiAliasing = optionPair.option;
			return optionPair;
		}

		public VideoOptionPair<AnisotropicFiltering> SetAnisotropicFilteringOption(int index)
		{
			VideoOptionPair<AnisotropicFiltering> optionPair = GetOptionPair(availableVideoOptions.anisotropicFiltering, index);
			cachedVideoSettings.anisotropicFiltering = optionPair.option;
			return optionPair;
		}

		public VideoOptionPair<VideoSettings.ShadowsQualityPreset> SetShadowsOption(int index)
		{
			VideoOptionPair<VideoSettings.ShadowsQualityPreset> optionPair = GetOptionPair(availableVideoOptions.shadows, index);
			cachedVideoSettings.shadowDistance = optionPair.option.shadowDistance;
			cachedVideoSettings.shadowResolution = optionPair.option.shadowResolution;
			cachedVideoSettings.shadowQuality = optionPair.option.shadowQuality;
			cachedVideoSettings.shadowmaskMode = optionPair.option.shadowmaskMode;
			cachedVideoSettings.shadowCascades = optionPair.option.shadowCascades;
			return optionPair;
		}

		public VideoOptionPair<float> SetLODDistanceOption(int index)
		{
			VideoOptionPair<float> optionPair = GetOptionPair(availableVideoOptions.lodDistances, index);
			cachedVideoSettings.lodDistance = optionPair.option;
			return optionPair;
		}

		private VideoSettings.ShadowsQualityPreset GetShadowPresetFromQualitySettings()
		{
			return new VideoSettings.ShadowsQualityPreset
			{
				shadowDistance = QualitySettings.shadowDistance,
				shadowResolution = QualitySettings.shadowResolution,
				shadowQuality = QualitySettings.shadows,
				shadowmaskMode = QualitySettings.shadowmaskMode,
				shadowCascades = QualitySettings.shadowCascades
			};
		}

		public static AvailableVideoOptions GetDefaultAvailableVideoOptions()
		{
			AvailableVideoOptions result = default;
			result.fullScreenModes = new VideoOptionPair<FullScreenMode>[4]
			{
				new VideoOptionPair<FullScreenMode>(FullScreenMode.ExclusiveFullScreen, "VS_FULLSCREEN_EXCLUSIVE"),
				new VideoOptionPair<FullScreenMode>(FullScreenMode.FullScreenWindow, "VS_FULLSCREEN_FULLWINDOW"),
				new VideoOptionPair<FullScreenMode>(FullScreenMode.Windowed, "VS_FULLSCREEN_WINDOWED"),
				new VideoOptionPair<FullScreenMode>(FullScreenMode.MaximizedWindow, "VS_FULLSCREEN_MAXIMIZEDWINDOW")
			};
			result.resolutions = new VideoOptionPair<Resolution>[0];
			result.vSync = new VideoOptionPair<int>[2]
			{
				new VideoOptionPair<int>(0, "SETTINGS_OFF"),
				new VideoOptionPair<int>(1, "SETTINGS_ON")
			};
			result.qualityPresets = new VideoOptionPair<int>[5]
			{
				new VideoOptionPair<int>(3, "VS_GENERIC_0"),
				new VideoOptionPair<int>(2, "VS_GENERIC_1"),
				new VideoOptionPair<int>(1, "VS_GENERIC_2"),
				new VideoOptionPair<int>(0, "VS_GENERIC_3"),
				new VideoOptionPair<int>(4, "VS_CUSTOM")
			};
			result.antiAliasing = new VideoOptionPair<int>[0];
			result.anisotropicFiltering = new VideoOptionPair<AnisotropicFiltering>[3]
			{
				new VideoOptionPair<AnisotropicFiltering>(AnisotropicFiltering.Enable, "TXT_ENABLED"),
				new VideoOptionPair<AnisotropicFiltering>(AnisotropicFiltering.Disable, "TXT_DISABLED"),
				new VideoOptionPair<AnisotropicFiltering>(AnisotropicFiltering.ForceEnable, "TXT_FORCED")
			};
			result.shadows = new VideoOptionPair<VideoSettings.ShadowsQualityPreset>[0];
			result.lodDistances = new VideoOptionPair<float>[0];
			return result;
		}

		private void RetrieveAvailableScreenOptions(ref AvailableVideoOptions availableVideoOptions)
		{
			Resolution[] resolutions = Screen.resolutions;
			int num = resolutions.Length;
			VideoOptionPair<Resolution>[] array = new VideoOptionPair<Resolution>[num];
			for (int i = 0; i < num; i++)
			{
				Resolution option = resolutions[i];
				array[i].option = option;
				array[i].name = $"{option.width}x{option.height}";
			}
			availableVideoOptions.resolutions = array;
		}

		private Resolution GetHighestScreenResolution()
		{
			return Screen.resolutions[^1];
		}

		private void RetrieveAvailableOptionsFromPresets(ref AvailableVideoOptions availableVideoOptions)
		{
			AvailableVideoOptions availableVideoOptions2 = availableVideoOptions;
			for (int num = availableVideoOptions2.qualityPresets.Length - 1; num >= 0; num--)
			{
				if (num != customQualityPresetInAvailableOptionsIndex)
				{
					QualitySettings.SetQualityLevel(availableVideoOptions2.qualityPresets[num].option, applyExpensiveChanges: false);
					CheckAndAddOption(ref availableVideoOptions2.vSync, QualitySettings.vSyncCount, CompareIntegerOptionsForSmaller);
					CheckAndAddOption(ref availableVideoOptions2.antiAliasing, QualitySettings.antiAliasing, CompareIntegerOptionsForSmaller);
					CheckAndAddOption(ref availableVideoOptions2.anisotropicFiltering, QualitySettings.anisotropicFiltering, CompareAnisotropicFilteringOptionsForSmaller);
					CheckAndAddOption(ref availableVideoOptions2.lodDistances, QualitySettings.lodBias, CompareFloatOptionsForSmaller, setGenericName: true, "VS_DISTANCE_");
					CheckAndAddOption(ref availableVideoOptions2.shadows, GetShadowPresetFromQualitySettings(), CompareShadowQualityOptionsForSmaller, setGenericName: true);
				}
			}
			availableVideoOptions = availableVideoOptions2;
		}

		private void CheckAndAddOption<T>(ref VideoOptionPair<T>[] videoOptionPairs, T optionToCheckAndAdd, CompareOptions<T> CompareOptionsForSmallerFunction, bool setGenericName = false, string genericNamePrefix = "VS_GENERIC_")
		{
			bool flag = false;
			for (int i = 0; i < videoOptionPairs.Length; i++)
			{
				flag |= videoOptionPairs[i].option.Equals(optionToCheckAndAdd);
			}
			if (!flag)
			{
				videoOptionPairs = AddToArray(videoOptionPairs, optionToCheckAndAdd, CompareOptionsForSmallerFunction, setGenericName, genericNamePrefix);
			}
		}

		private VideoOptionPair<T>[] AddToArray<T>(VideoOptionPair<T>[] optionPairs, T optionToAdd, CompareOptions<T> CompareOptionsForSmallerFunction, bool setGenericName, string genericNamePrefix)
		{
			int num = optionPairs.Length;
			VideoOptionPair<T>[] array = new VideoOptionPair<T>[num + 1];
			VideoOptionPair<T> videoOptionPair = new VideoOptionPair<T>(optionToAdd, optionToAdd.ToString());
			if (!setGenericName)
			{
				videoOptionPair.name = optionToAdd.ToString();
			}
			for (int i = 0; i < num; i++)
			{
				VideoOptionPair<T> videoOptionPair2 = optionPairs[i];
				if (CompareOptionsForSmallerFunction(videoOptionPair.option, videoOptionPair2.option))
				{
					array[i] = videoOptionPair;
					videoOptionPair = videoOptionPair2;
				}
				else
				{
					array[i] = videoOptionPair2;
				}
			}
			int num2 = num;
			array[num2] = videoOptionPair;
			if (setGenericName)
			{
				int num3 = num + 1;
				for (int j = 0; j < num3; j++)
				{
					array[j].name = genericNamePrefix + j;
				}
			}
			return array;
		}

		public bool CompareIntegerOptionsForSmaller(int option1, int option2)
		{
			return option1 < option2;
		}

		public bool CompareFloatOptionsForSmaller(float option1, float option2)
		{
			return option1 < option2;
		}

		public bool CompareAnisotropicFilteringOptionsForSmaller(AnisotropicFiltering option1, AnisotropicFiltering option2)
		{
			return option1 < option2;
		}

		public bool CompareShadowQualityOptionsForSmaller(VideoSettings.ShadowsQualityPreset option1, VideoSettings.ShadowsQualityPreset option2)
		{
			return option1.shadowDistance < option2.shadowDistance;
		}
	}
}
