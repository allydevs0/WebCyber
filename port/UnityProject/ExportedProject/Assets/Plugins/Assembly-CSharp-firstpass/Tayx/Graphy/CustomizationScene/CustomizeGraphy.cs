using System.Collections.Generic;
using Tayx.Graphy.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Tayx.Graphy.CustomizationScene
{
	public class CustomizeGraphy : MonoBehaviour
	{
		[Header("Customize Graphy")]
		[SerializeField]
		private G_CUIColorPicker m_colorPicker;

		[SerializeField]
		private Toggle m_backgroundToggle;

		[SerializeField]
		private Dropdown m_graphyModeDropdown;

		[SerializeField]
		private Button m_backgroundColorButton;

		[SerializeField]
		private Dropdown m_graphModulePositionDropdown;

		[Header("Fps")]
		[SerializeField]
		private Dropdown m_fpsModuleStateDropdown;

		[SerializeField]
		private InputField m_goodInputField;

		[SerializeField]
		private InputField m_cautionInputField;

		[SerializeField]
		private Button m_goodColorButton;

		[SerializeField]
		private Button m_cautionColorButton;

		[SerializeField]
		private Button m_criticalColorButton;

		[SerializeField]
		private Slider m_timeToResetMinMaxSlider;

		[SerializeField]
		private Slider m_fpsGraphResolutionSlider;

		[SerializeField]
		private Slider m_fpsTextUpdateRateSlider;

		[Header("Memory")]
		[SerializeField]
		private Dropdown m_ramModuleStateDropdown;

		[SerializeField]
		private Button m_reservedColorButton;

		[SerializeField]
		private Button m_allocatedColorButton;

		[SerializeField]
		private Button m_monoColorButton;

		[SerializeField]
		private Slider m_ramGraphResolutionSlider;

		[SerializeField]
		private Slider m_ramTextUpdateRateSlider;

		[Header("Audio")]
		[SerializeField]
		private Dropdown m_audioModuleStateDropdown;

		[SerializeField]
		private Button m_audioGraphColorButton;

		[SerializeField]
		private Dropdown m_findAudioListenerDropdown;

		[SerializeField]
		private Dropdown m_fttWindowDropdown;

		[SerializeField]
		private Slider m_spectrumSizeSlider;

		[SerializeField]
		private Slider m_audioGraphResolutionSlider;

		[SerializeField]
		private Slider m_audioTextUpdateRateSlider;

		[Header("Advanced")]
		[SerializeField]
		private Dropdown m_advancedModulePositionDropdown;

		[SerializeField]
		private Toggle m_advancedModuleToggle;

		[Header("Other")]
		[SerializeField]
		private Button m_musicButton;

		[SerializeField]
		private Button m_sfxButton;

		[SerializeField]
		private Slider m_musicVolumeSlider;

		[SerializeField]
		private Slider m_sfxVolumeSlider;

		[SerializeField]
		private AudioSource m_musicAudioSource;

		[SerializeField]
		private AudioSource m_sfxAudioSource;

		[SerializeField]
		private List<AudioClip> m_sfxAudioClips = new List<AudioClip>();

		private GraphyManager m_graphyManager;

		private void OnEnable()
		{
			m_graphyManager = G_Singleton<GraphyManager>.Instance;
			SetupCallbacks();
		}

		private void SetupCallbacks()
		{
			m_backgroundToggle.onValueChanged.RemoveAllListeners();
			m_backgroundColorButton.onClick.RemoveAllListeners();
			m_graphyModeDropdown.onValueChanged.RemoveAllListeners();
			m_graphModulePositionDropdown.onValueChanged.RemoveAllListeners();
			m_fpsModuleStateDropdown.onValueChanged.RemoveAllListeners();
			m_goodInputField.onValueChanged.RemoveAllListeners();
			m_cautionInputField.onValueChanged.RemoveAllListeners();
			m_goodColorButton.onClick.RemoveAllListeners();
			m_cautionColorButton.onClick.RemoveAllListeners();
			m_criticalColorButton.onClick.RemoveAllListeners();
			m_timeToResetMinMaxSlider.onValueChanged.RemoveAllListeners();
			m_fpsGraphResolutionSlider.onValueChanged.RemoveAllListeners();
			m_fpsTextUpdateRateSlider.onValueChanged.RemoveAllListeners();
			m_ramModuleStateDropdown.onValueChanged.RemoveAllListeners();
			m_reservedColorButton.onClick.RemoveAllListeners();
			m_allocatedColorButton.onClick.RemoveAllListeners();
			m_monoColorButton.onClick.RemoveAllListeners();
			m_ramGraphResolutionSlider.onValueChanged.RemoveAllListeners();
			m_ramTextUpdateRateSlider.onValueChanged.RemoveAllListeners();
			m_audioModuleStateDropdown.onValueChanged.RemoveAllListeners();
			m_audioGraphColorButton.onClick.RemoveAllListeners();
			m_findAudioListenerDropdown.onValueChanged.RemoveAllListeners();
			m_fttWindowDropdown.onValueChanged.RemoveAllListeners();
			m_spectrumSizeSlider.onValueChanged.RemoveAllListeners();
			m_audioGraphResolutionSlider.onValueChanged.RemoveAllListeners();
			m_audioTextUpdateRateSlider.onValueChanged.RemoveAllListeners();
			m_advancedModulePositionDropdown.onValueChanged.RemoveAllListeners();
			m_advancedModuleToggle.onValueChanged.RemoveAllListeners();
			m_musicButton.onClick.RemoveAllListeners();
			m_sfxButton.onClick.RemoveAllListeners();
			m_musicVolumeSlider.onValueChanged.RemoveAllListeners();
			m_sfxVolumeSlider.onValueChanged.RemoveAllListeners();
			m_backgroundToggle.onValueChanged.AddListener((bool value) =>
			{
				m_graphyManager.Background = value;
			});
			m_backgroundColorButton.onClick.AddListener(() =>
			{
				m_colorPicker.SetOnValueChangeCallback(null);
				m_colorPicker.Color = m_backgroundColorButton.GetComponent<Image>().color;
				m_colorPicker.SetOnValueChangeCallback((Color color) =>
				{
					m_backgroundColorButton.GetComponent<Image>().color = color;
					m_graphyManager.BackgroundColor = color;
				});
			});
			m_graphyModeDropdown.onValueChanged.AddListener((int value) =>
			{
				switch ((GraphyManager.Mode)value)
				{
				case GraphyManager.Mode.FULL:
					m_fpsGraphResolutionSlider.maxValue = 300f;
					m_ramGraphResolutionSlider.maxValue = 300f;
					m_audioGraphResolutionSlider.maxValue = 300f;
					break;
				case GraphyManager.Mode.LIGHT:
					m_fpsGraphResolutionSlider.maxValue = 128f;
					m_ramGraphResolutionSlider.maxValue = 128f;
					m_audioGraphResolutionSlider.maxValue = 128f;
					break;
				}
				m_graphyManager.GraphyMode = (GraphyManager.Mode)value;
			});
			m_graphModulePositionDropdown.onValueChanged.AddListener((int value) =>
			{
				m_graphyManager.GraphModulePosition = (GraphyManager.ModulePosition)value;
			});
			m_fpsModuleStateDropdown.onValueChanged.AddListener((int value) =>
			{
				m_graphyManager.FpsModuleState = (GraphyManager.ModuleState)value;
			});
			m_goodInputField.onValueChanged.AddListener((string value) =>
			{
				if (int.TryParse(value, out var result))
				{
					m_graphyManager.GoodFPSThreshold = result;
				}
			});
			m_cautionInputField.onValueChanged.AddListener((string value) =>
			{
				if (int.TryParse(value, out var result))
				{
					m_graphyManager.CautionFPSThreshold = result;
				}
			});
			m_goodColorButton.onClick.AddListener(() =>
			{
				m_colorPicker.SetOnValueChangeCallback(null);
				m_colorPicker.Color = m_goodColorButton.GetComponent<Image>().color;
				m_colorPicker.SetOnValueChangeCallback((Color color) =>
				{
					m_goodColorButton.GetComponent<Image>().color = color;
					m_graphyManager.GoodFPSColor = color;
				});
			});
			m_cautionColorButton.onClick.AddListener(() =>
			{
				m_colorPicker.SetOnValueChangeCallback(null);
				m_colorPicker.Color = m_cautionColorButton.GetComponent<Image>().color;
				m_colorPicker.SetOnValueChangeCallback((Color color) =>
				{
					m_cautionColorButton.GetComponent<Image>().color = color;
					m_graphyManager.CautionFPSColor = color;
				});
			});
			m_criticalColorButton.onClick.AddListener(() =>
			{
				m_colorPicker.SetOnValueChangeCallback(null);
				m_colorPicker.Color = m_criticalColorButton.GetComponent<Image>().color;
				m_colorPicker.SetOnValueChangeCallback((Color color) =>
				{
					m_criticalColorButton.GetComponent<Image>().color = color;
					m_graphyManager.CriticalFPSColor = color;
				});
			});
			m_timeToResetMinMaxSlider.onValueChanged.AddListener((float value) =>
			{
				m_graphyManager.TimeToResetMinMaxFps = (int)value;
			});
			m_fpsGraphResolutionSlider.onValueChanged.AddListener((float value) =>
			{
				m_graphyManager.FpsGraphResolution = (int)value;
			});
			m_fpsTextUpdateRateSlider.onValueChanged.AddListener((float value) =>
			{
				m_graphyManager.FpsTextUpdateRate = (int)value;
			});
			m_ramModuleStateDropdown.onValueChanged.AddListener((int value) =>
			{
				m_graphyManager.RamModuleState = (GraphyManager.ModuleState)value;
			});
			m_reservedColorButton.onClick.AddListener(() =>
			{
				m_colorPicker.SetOnValueChangeCallback(null);
				m_colorPicker.Color = m_reservedColorButton.GetComponent<Image>().color;
				m_colorPicker.SetOnValueChangeCallback((Color color) =>
				{
					m_reservedColorButton.GetComponent<Image>().color = color;
					m_graphyManager.ReservedRamColor = color;
				});
			});
			m_allocatedColorButton.onClick.AddListener(() =>
			{
				m_colorPicker.SetOnValueChangeCallback(null);
				m_colorPicker.Color = m_allocatedColorButton.GetComponent<Image>().color;
				m_colorPicker.SetOnValueChangeCallback((Color color) =>
				{
					m_allocatedColorButton.GetComponent<Image>().color = color;
					m_graphyManager.AllocatedRamColor = color;
				});
			});
			m_monoColorButton.onClick.AddListener(() =>
			{
				m_colorPicker.SetOnValueChangeCallback(null);
				m_colorPicker.Color = m_monoColorButton.GetComponent<Image>().color;
				m_colorPicker.SetOnValueChangeCallback((Color color) =>
				{
					m_monoColorButton.GetComponent<Image>().color = color;
					m_graphyManager.MonoRamColor = color;
				});
			});
			m_ramGraphResolutionSlider.onValueChanged.AddListener((float value) =>
			{
				m_graphyManager.RamGraphResolution = (int)value;
			});
			m_ramTextUpdateRateSlider.onValueChanged.AddListener((float value) =>
			{
				m_graphyManager.RamTextUpdateRate = (int)value;
			});
			m_audioModuleStateDropdown.onValueChanged.AddListener((int value) =>
			{
				m_graphyManager.AudioModuleState = (GraphyManager.ModuleState)value;
			});
			m_audioGraphColorButton.onClick.AddListener(() =>
			{
				m_colorPicker.SetOnValueChangeCallback(null);
				m_colorPicker.Color = m_audioGraphColorButton.GetComponent<Image>().color;
				m_colorPicker.SetOnValueChangeCallback((Color color) =>
				{
					m_audioGraphColorButton.GetComponent<Image>().color = color;
					m_graphyManager.AudioGraphColor = color;
				});
			});
			m_findAudioListenerDropdown.onValueChanged.AddListener((int value) =>
			{
				m_graphyManager.FindAudioListenerInCameraIfNull = (GraphyManager.LookForAudioListener)value;
			});
			m_fttWindowDropdown.onValueChanged.AddListener((int value) =>
			{
				m_graphyManager.FftWindow = (FFTWindow)value;
			});
			m_spectrumSizeSlider.onValueChanged.AddListener((float value) =>
			{
				m_graphyManager.SpectrumSize = (int)value;
			});
			m_audioGraphResolutionSlider.onValueChanged.AddListener((float value) =>
			{
				m_graphyManager.AudioGraphResolution = (int)value;
			});
			m_audioTextUpdateRateSlider.onValueChanged.AddListener((float value) =>
			{
				m_graphyManager.AudioTextUpdateRate = (int)value;
			});
			m_advancedModulePositionDropdown.onValueChanged.AddListener((int value) =>
			{
				m_graphyManager.AdvancedModulePosition = (GraphyManager.ModulePosition)value;
			});
			m_advancedModuleToggle.onValueChanged.AddListener((bool value) =>
			{
				m_graphyManager.AdvancedModuleState = ((!value) ? GraphyManager.ModuleState.OFF : GraphyManager.ModuleState.FULL);
			});
			m_musicButton.onClick.AddListener(ToggleMusic);
			m_sfxButton.onClick.AddListener(PlayRandomSFX);
			m_musicVolumeSlider.onValueChanged.AddListener((float value) =>
			{
				m_musicAudioSource.volume = value / 100f;
			});
			m_sfxVolumeSlider.onValueChanged.AddListener((float value) =>
			{
				m_sfxAudioSource.volume = value / 100f;
			});
		}

		private void ToggleMusic()
		{
			if (m_musicAudioSource.isPlaying)
			{
				m_musicAudioSource.Pause();
			}
			else
			{
				m_musicAudioSource.Play();
			}
		}

		private void PlayRandomSFX()
		{
			if (m_sfxAudioClips.Count > 0)
			{
				m_sfxAudioSource.clip = m_sfxAudioClips[Random.Range(0, m_sfxAudioClips.Count)];
				m_sfxAudioSource.Play();
			}
		}
	}
}
