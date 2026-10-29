using System;
using UnityEngine.Audio;

namespace Reptile
{
	public interface IAudioSubSystem : IDisposable
	{
		AudioMixerGroup GetAudioChannel(AudioChannelID audioChannelID);

		void SetChannelVolumeScale(AudioChannelID audioChannelID, float volumeScale, float volume01Clamped);

		float GetChannelVolumeScale(AudioChannelID audioChannelID, float volume01Clamped);
	}
}
