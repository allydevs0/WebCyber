using UnityEngine;

namespace Reptile
{
	public class OptionsMenuAudioTab : AOptionsMenuTab
	{
		[SerializeField]
		private TextMeshProSlider masterVolumeSlider;

		[SerializeField]
		private TextMeshProSlider sfxVolumeSlider;

		[SerializeField]
		private TextMeshProSlider musicVolumeSlider;

		private AudioSettings audioSettings;

		private AudioManager audioManager;

		public override void Activate()
		{
			base.Activate();
			if (Core.Instance.BaseModule.IsPlayingInStage)
			{
				audioManager.SetForcedVolumeToNormal();
			}
			ListenForSliderValueChanges();
			RefreshAllSliderValues();
		}

		public override void Deactivate()
		{
			if (isActive)
			{
				if (Core.Instance.BaseModule.IsPlayingInStage)
				{
					audioManager.RefreshTemporaryVolumeValues();
					audioManager.SetForcedVolumeToLower();
				}
				StopListeningForSliderValueChanges();
			}
			base.Deactivate();
		}

		protected override void Awake()
		{
			base.Awake();
			audioSettings = Core.Instance.SaveManager.Settings.audioSettings;
			audioManager = Core.Instance.AudioManager;
		}

		private void ListenForSliderValueChanges()
		{
			masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderValueChanged);
			sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeSliderValueChanged);
			musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderValueChanged);
		}

		private void OnMasterVolumeSliderValueChanged(float newValue)
		{
			audioSettings.masterVolume = masterVolumeSlider.CalculatePercentile();
			SetUserAudioSettings();
			audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.selectionMove);
		}

		private void OnSfxVolumeSliderValueChanged(float newValue)
		{
			audioSettings.sfxVolume = sfxVolumeSlider.CalculatePercentile();
			SetUserAudioSettings();
			audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.selectionMove);
		}

		private void OnMusicVolumeSliderValueChanged(float newValue)
		{
			audioSettings.musicVolume = musicVolumeSlider.CalculatePercentile();
			SetUserAudioSettings();
			audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.selectionMove);
		}

		private void RefreshAllSliderValues()
		{
			RefreshGlobalVolumeValue();
			RefreshSFXVolumeValue();
			RefreshMusicVolumeValue();
		}

		private void RefreshGlobalVolumeValue()
		{
			masterVolumeSlider.SetSliderValue(audioSettings.masterVolume);
		}

		private void RefreshSFXVolumeValue()
		{
			sfxVolumeSlider.SetSliderValue(audioSettings.sfxVolume);
		}

		private void RefreshMusicVolumeValue()
		{
			musicVolumeSlider.SetSliderValue(audioSettings.musicVolume);
		}

		private void StopListeningForSliderValueChanges()
		{
			masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderValueChanged);
			sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeSliderValueChanged);
			musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeSliderValueChanged);
		}

		private void SetUserAudioSettings()
		{
			audioManager.SetAudioSettings(audioSettings);
		}
	}
}
