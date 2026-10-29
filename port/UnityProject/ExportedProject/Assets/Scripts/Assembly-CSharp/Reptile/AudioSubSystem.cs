using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Audio;

namespace Reptile
{
	public class AudioSubSystem : IAudioSubSystem, IDisposable
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct AudioChannelIDComparer : IEqualityComparer<AudioChannelID>
		{
			public bool Equals(AudioChannelID x, AudioChannelID y)
			{
				return x == y;
			}

			public int GetHashCode(AudioChannelID obj)
			{
				return (int)obj;
			}
		}

		public const float MIN_SCALING_VOLUME = 0.0001f;

		public const string AUDIO_MIXER_MASTER_GROUP_NAME = "Master";

		private const string VOLUME_PARAMETER_AFFIX = "Volume";

		private const string AUDIO_MIXER_ASSETBUNDLE_NAME = "coreassets";

		private const string AUDIO_MIXER_NAME = "mastermixer";

		private AudioMixer masterAudioMixer;

		private Dictionary<AudioChannelID, AudioMixerGroup> audioMixerGroupDictionary;

		private Dictionary<AudioChannelID, string> audioMixerChannelIdVolumeDescriptions;

		public AudioSubSystem(ref AudioMixer masterAudioMixer, Assets assets)
		{
			this.masterAudioMixer = masterAudioMixer;
			GenerateAudioMixerGroupDictionary();
		}

		public void Dispose()
		{
			masterAudioMixer = null;
			audioMixerGroupDictionary = null;
			audioMixerChannelIdVolumeDescriptions = null;
		}

		public AudioMixerGroup GetAudioChannel(AudioChannelID audioChannelID)
		{
			return FindAudioChannel(audioChannelID);
		}

		public void AddAudioSourceToAudioChannel(AudioSource audioSource, AudioChannelID audioChannelID)
		{
			audioSource.outputAudioMixerGroup = GetAudioChannel(audioChannelID);
		}

		private void GenerateAudioMixerGroupDictionary()
		{
			AudioMixerGroup[] array = masterAudioMixer.FindMatchingGroups(AudioChannelID.Master.ToString());
			audioMixerGroupDictionary = new Dictionary<AudioChannelID, AudioMixerGroup>(default(AudioChannelIDComparer));
			audioMixerChannelIdVolumeDescriptions = new Dictionary<AudioChannelID, string>(default(AudioChannelIDComparer));
			AudioChannelID result = AudioChannelID.NONE;
			for (int i = 0; i < array.Length; i++)
			{
				if (Enum.TryParse<AudioChannelID>(array[i].name, out result))
				{
					audioMixerGroupDictionary.Add(result, array[i]);
					audioMixerChannelIdVolumeDescriptions.Add(result, array[i].name + "Volume");
				}
			}
		}

		public float GetChannelVolumeScale(AudioChannelID audioChannelID, float volume01Clamped)
		{
			string audioChannelIDVolumeDescription = GetAudioChannelIDVolumeDescription(audioChannelID);
			float value = 0f;
			masterAudioMixer.GetFloat(audioChannelIDVolumeDescription, out value);
			return ConvertVolumeDecibelToVolume01(value, volume01Clamped);
		}

		private float ConvertVolumeDecibelToVolume01(float audioChannelVolumeDecibel, float volume01Clamped)
		{
			return Mathf.Pow(10f, audioChannelVolumeDecibel * 0.05f) / volume01Clamped;
		}

		public void SetChannelVolumeScale(AudioChannelID audioChannelID, float audioChannelVolume01, float volume01Clamped)
		{
			float value = ConvertVolume01ToVolumeDecibel(audioChannelVolume01, volume01Clamped);
			string audioChannelIDVolumeDescription = GetAudioChannelIDVolumeDescription(audioChannelID);
			masterAudioMixer.SetFloat(audioChannelIDVolumeDescription, value);
		}

		private float ConvertVolume01ToVolumeDecibel(float audioChannelVolume01, float volume01Clamped)
		{
			audioChannelVolume01 = Mathf.Max(audioChannelVolume01, 0.0001f);
			return Mathf.Log10(audioChannelVolume01 * volume01Clamped) * 20f;
		}

		private string GetAudioChannelIDVolumeDescription(AudioChannelID audioChannelID)
		{
			string value = string.Empty;
			audioMixerChannelIdVolumeDescriptions.TryGetValue(audioChannelID, out value);
			return value;
		}

		private AudioMixerGroup FindAudioChannel(AudioChannelID audioChannelId)
		{
			AudioMixerGroup value = null;
			audioMixerGroupDictionary.TryGetValue(audioChannelId, out value);
			return value;
		}
	}
}
