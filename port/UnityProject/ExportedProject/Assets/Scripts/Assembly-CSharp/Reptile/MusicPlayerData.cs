using UnityEngine;

namespace Reptile
{
	public class MusicPlayerData
	{
		private const string WAV_EXT = ".wav";

		private const string OGG_EXT = ".ogg";

		private string title = string.Empty;

		private string audioClipName = string.Empty;

		private string artist = string.Empty;

		private string path = string.Empty;

		private AudioType audioType = AudioType.OGGVORBIS;

		private AudioClip audioClip;

		private bool isLoading;

		private bool failedLoading;

		private bool isAsset;

		public bool IsAsset => isAsset;

		public string AudioClipName
		{
			get
			{
				if (isAsset)
				{
					if (audioClip.loadState != AudioDataLoadState.Loaded)
					{
						return string.Empty;
					}
					return audioClip.name;
				}
				return audioClipName;
			}
		}

		public string Title => title;

		public string Artist => artist;

		public string Path => path;

		public bool IsLoaded
		{
			get
			{
				if (audioClip != null)
				{
					return audioClip.loadState == AudioDataLoadState.Loaded;
				}
				return false;
			}
		}

		public bool IsLoading => isLoading;

		public bool FailedLoading => failedLoading;

		public float PlaybackLength
		{
			get
			{
				if (audioClip.loadState != AudioDataLoadState.Loaded)
				{
					return 0f;
				}
				return audioClip.length;
			}
		}

		public AudioClip AudioClip => audioClip;

		public AudioType AudioType => audioType;

		public MusicPlayerData(string path)
		{
			this.path = path;
			string text = path.Substring(path.LastIndexOf("\\") + 1);
			title = text.Substring(0, text.LastIndexOf('.'));
			audioClipName = title;
			artist = string.Empty;
			DetermineAudioType(text);
		}

		public MusicPlayerData(AudioClip audioClip)
		{
			this.audioClip = audioClip;
			audioClipName = audioClip.name;
			title = audioClipName;
			artist = string.Empty;
			isAsset = true;
		}

		public MusicPlayerData(MusicTrack musicTrack)
		{
			audioClip = musicTrack.AudioClip;
			audioClipName = audioClip.name;
			title = musicTrack.Title;
			artist = musicTrack.Artist;
			isAsset = true;
		}

		public void Dispose()
		{
			if (isAsset)
			{
				audioClip.UnloadAudioData();
				return;
			}
			Object.Destroy(audioClip);
			audioClip = null;
		}

		public void StartLoading()
		{
			isLoading = true;
		}

		public void FinishedLoading()
		{
			isLoading = false;
		}

		public void FinishedLoading(AudioClip audioClip)
		{
			isLoading = false;
			this.audioClip = audioClip;
		}

		public void FinishedWithError(string error)
		{
			isLoading = false;
			failedLoading = true;
		}

		private void DetermineAudioType(string fileName)
		{
			string text = fileName.Substring(audioClipName.Length);
			if (text.Equals(".wav"))
			{
				audioType = AudioType.WAV;
			}
			else if (text.Equals(".ogg"))
			{
				audioType = AudioType.OGGVORBIS;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is MusicPlayerData)
			{
				if (!audioClip.Equals((obj as MusicPlayerData).audioClip))
				{
					return base.Equals(obj);
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
