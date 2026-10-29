using System;
using System.Collections.Generic;

namespace Reptile
{
	public class MusicPlayerBuffer : IDisposable
	{
		private List<MusicPlayerData> musicPlayerDataBuffer;

		public MusicPlayerBuffer()
		{
			musicPlayerDataBuffer = new List<MusicPlayerData>();
		}

		public void Dispose()
		{
			Clear();
			musicPlayerDataBuffer = null;
		}

		public void Clear()
		{
			UnloadAllBufferedSongs();
			musicPlayerDataBuffer.Clear();
		}

		public void BufferMusicTrack(MusicTrack musicTrackToLoad)
		{
			if (!(musicTrackToLoad == null))
			{
				MusicPlayerData musicPlayerData = FindMusicPlayerDataByMusicTrack(musicTrackToLoad);
				if (musicPlayerData == null)
				{
					musicPlayerData = CreateNewMusicPlayerDataObject(musicTrackToLoad);
				}
				BufferMusicPlayerData(musicPlayerData);
			}
		}

		public void BufferMusicTracks(MusicTrack[] musicTracks)
		{
			for (int i = 0; i < musicTracks.Length; i++)
			{
				BufferMusicTrack(musicTracks[i]);
			}
		}

		public void UnloadMusicTrack(MusicTrack musicTrack)
		{
			MusicPlayerData musicPlayerData = FindMusicPlayerDataByMusicTrack(musicTrack);
			if (musicPlayerData != null)
			{
				UnloadMusicPlayerData(musicPlayerData);
				RemoveDataFromBuffer(musicPlayerData);
			}
		}

		public void RemoveDataFromBuffer(MusicPlayerData musicPlayerData)
		{
			musicPlayerDataBuffer.Remove(musicPlayerData);
		}

		public MusicPlayerData GetBufferedMusicPlayerDataForMusicTrack(MusicTrack musicTrack)
		{
			if (musicTrack == null)
			{
				return null;
			}
			return FindMusicPlayerDataByMusicTrack(musicTrack);
		}

		private void UnloadAllBufferedSongs()
		{
			for (int i = 0; i < musicPlayerDataBuffer.Count; i++)
			{
				if (musicPlayerDataBuffer[i].IsLoaded)
				{
					UnloadMusicPlayerData(musicPlayerDataBuffer[i]);
				}
			}
		}

		internal void UnloadAllBufferedSongsExcept(MusicTrack musicTrack)
		{
			for (int num = musicPlayerDataBuffer.Count - 1; num >= 0; num--)
			{
				if (musicPlayerDataBuffer[num].AudioClip == null)
				{
					RemoveDataFromBuffer(musicPlayerDataBuffer[num]);
				}
				else if (musicPlayerDataBuffer[num].AudioClip != musicTrack.AudioClip && musicPlayerDataBuffer[num].IsLoaded)
				{
					UnloadMusicPlayerData(musicPlayerDataBuffer[num]);
					RemoveDataFromBuffer(musicPlayerDataBuffer[num]);
				}
			}
		}

		private void UnloadMusicPlayerData(MusicPlayerData musicPlayerData)
		{
			musicPlayerData?.Dispose();
		}

		private MusicPlayerData FindMusicPlayerDataByMusicTrack(MusicTrack musicTrack)
		{
			int num = musicPlayerDataBuffer.FindIndex((MusicPlayerData musicPlayerData) => musicPlayerData.AudioClip.Equals(musicTrack.AudioClip));
			if (num < 0)
			{
				return null;
			}
			return musicPlayerDataBuffer[num];
		}

		private MusicPlayerData CreateNewMusicPlayerDataObject(MusicTrack fromMusicTrack)
		{
			return new MusicPlayerData(fromMusicTrack);
		}

		private void BufferMusicPlayerData(MusicPlayerData musicPlayerData)
		{
			if (CanLoadData(musicPlayerData) || !musicPlayerData.IsLoaded || !musicPlayerDataBuffer.Contains(musicPlayerData))
			{
				musicPlayerDataBuffer.Add(musicPlayerData);
				musicPlayerData.StartLoading();
				LoadMusicPlayerData(musicPlayerData);
			}
		}

		private void LoadMusicPlayerData(MusicPlayerData musicPlayerData)
		{
			if (musicPlayerData.IsAsset)
			{
				Core.Instance.Assets.LoadMusicAsset(musicPlayerData);
			}
			else
			{
				Core.Instance.Assets.LoadMusicAssetFromDisk(musicPlayerData);
			}
		}

		private bool CanLoadData(MusicPlayerData musicPlayerData)
		{
			if (!musicPlayerData.IsLoaded && !musicPlayerData.IsLoading)
			{
				return !musicPlayerData.FailedLoading;
			}
			return false;
		}
	}
}
