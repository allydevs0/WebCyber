using System;
using System.Collections.Generic;

namespace Reptile
{
	public class MusicTrackQueue : IDisposable
	{
		private List<MusicTrack> currentMusicTracks;

		private AIndexQueue indexQueue;

		private MusicPlayerBuffer musicPlayerBuffer;

		private int currentTrackIndex = -1;

		private uint nPreviousTracksBuffered;

		private uint nNextTracksBuffered;

		public MusicTrack CurrentMusicTrack
		{
			get
			{
				if (currentTrackIndex < 0)
				{
					return null;
				}
				return currentMusicTracks[currentTrackIndex];
			}
		}

		public int CurrentTrackIndex => currentTrackIndex;

		public int AmountOfTracks => currentMusicTracks.Count;

		public bool HasCurrentActiveTrack => currentTrackIndex != -1;

		public MusicTrackQueue(AIndexQueue indexQueue, MusicPlayerBuffer musicPlayerBuffer, uint nPreviousTracksBuffered, uint nNextTracksBuffered)
		{
			this.indexQueue = indexQueue;
			this.musicPlayerBuffer = musicPlayerBuffer;
			this.nPreviousTracksBuffered = nPreviousTracksBuffered;
			this.nNextTracksBuffered = nNextTracksBuffered;
			currentMusicTracks = new List<MusicTrack>();
		}

		public void Dispose()
		{
			Clear();
			musicPlayerBuffer.Dispose();
			musicPlayerBuffer = null;
			indexQueue = null;
			currentMusicTracks = null;
		}

		public void Clear()
		{
			currentMusicTracks.Clear();
			ClearBuffer();
			ClearMusicQueue();
		}

		public void ClearTracks()
		{
			currentMusicTracks.Clear();
		}

		public void ClearExceptCurrent()
		{
			MusicTrack musicTrack = GetMusicTrack(currentTrackIndex);
			currentMusicTracks.Clear();
			currentMusicTracks.Add(musicTrack);
			musicPlayerBuffer.UnloadAllBufferedSongsExcept(musicTrack);
			ClearMusicQueue();
			AddToMusicQueue(musicTrack);
			currentTrackIndex = 0;
		}

		public void ClearBuffer()
		{
			musicPlayerBuffer.Clear();
		}

		public void ClearMusicQueue()
		{
			indexQueue.ClearQueue();
		}

		public MusicTrack GetMusicTrack(int index)
		{
			if (index < 0 || index >= currentMusicTracks.Count)
			{
				return null;
			}
			return currentMusicTracks[index];
		}

		public void SetCurrentTrack(MusicTrack musicTrack)
		{
			if (HasMusicTrack(musicTrack))
			{
				SetCurrentTrack(currentMusicTracks.IndexOf(musicTrack));
			}
		}

		public void SetCurrentTrack(int index)
		{
			currentTrackIndex = index;
		}

		public void AddMusicTrack(MusicTrack musicTrack)
		{
			if (!HasMusicTrack(musicTrack))
			{
				currentMusicTracks.Add(musicTrack);
			}
		}

		public void RemoveMusicTrack(MusicTrack musicTrack)
		{
			currentMusicTracks.Remove(musicTrack);
		}

		public void AddAllCurrentTracksToQueue()
		{
			for (int i = 0; i < currentMusicTracks.Count; i++)
			{
				AddToMusicQueue(currentMusicTracks[i]);
			}
		}

		public void BufferTracksInQueue()
		{
			BufferCurrentTrack();
			BufferPreviousTracksInQueue();
			BufferNextTracksInQueue();
		}

		public void SelectNextTrack()
		{
			if (AmountOfTracks > 0)
			{
				int nextInQueue = indexQueue.GetNextInQueue(currentTrackIndex, 1);
				int previousInQueue = indexQueue.GetPreviousInQueue(currentTrackIndex, (int)nPreviousTracksBuffered);
				EvaluateNextTrack(nextInQueue);
				EvaluateTrackToUnload(previousInQueue);
			}
		}

		public void SelectPreviousTrack()
		{
			if (AmountOfTracks > 0)
			{
				int previousInQueue = indexQueue.GetPreviousInQueue(currentTrackIndex, 1);
				int nextInQueue = indexQueue.GetNextInQueue(currentTrackIndex, (int)nNextTracksBuffered);
				EvaluateNextTrack(previousInQueue);
				EvaluateTrackToUnload(nextInQueue);
			}
		}

		public void AddMusicTracksToQueue(MusicTrack[] musicTracks)
		{
			for (int i = 0; i < musicTracks.Length; i++)
			{
				if (HasMusicTrack(musicTracks[i]))
				{
					AddToMusicQueue(musicTracks[i]);
				}
			}
		}

		public void UpdateLoadingCurrentTrack()
		{
			if (HasCurrentActiveTrack)
			{
				UpdateLoadingTrack(currentMusicTracks[currentTrackIndex]);
			}
		}

		public MusicPlayerData GetCurrentMusicPlayerData()
		{
			if (!HasCurrentActiveTrack)
			{
				return null;
			}
			return musicPlayerBuffer.GetBufferedMusicPlayerDataForMusicTrack(CurrentMusicTrack);
		}

		public void RemoveAllButCurrentMusicTrack()
		{
			MusicTrack currentMusicTrack = CurrentMusicTrack;
			for (int i = 0; i < currentMusicTracks.Count; i++)
			{
				if (!currentMusicTracks[i].Equals(currentMusicTrack))
				{
					musicPlayerBuffer.UnloadMusicTrack(currentMusicTracks[i]);
				}
			}
			currentMusicTracks.Clear();
			currentMusicTracks.Add(currentMusicTrack);
			ResetIndexQueue();
		}

		private bool HasMusicTrack(MusicTrack musicTrack)
		{
			return currentMusicTracks.Contains(musicTrack);
		}

		private void AddToMusicQueue(MusicTrack musicTrack)
		{
			indexQueue.AddToQueue(currentMusicTracks.IndexOf(musicTrack));
		}

		private void BufferCurrentTrack()
		{
			BufferTrackAtIndex(currentTrackIndex);
		}

		private void BufferTrackAtIndex(int index)
		{
			if (index >= 0 && index < currentMusicTracks.Count)
			{
				musicPlayerBuffer.BufferMusicTrack(currentMusicTracks[index]);
			}
		}

		private void BufferPreviousTracksInQueue()
		{
			for (int i = currentTrackIndex; i < nPreviousTracksBuffered; i++)
			{
				int num = i + 1;
				if (num < currentMusicTracks.Count)
				{
					int previousInQueue = indexQueue.GetPreviousInQueue(currentTrackIndex, num);
					BufferTrackAtIndex(previousInQueue);
					continue;
				}
				break;
			}
		}

		private void BufferNextTracksInQueue()
		{
			for (int i = currentTrackIndex; i < nNextTracksBuffered; i++)
			{
				int num = i + 1;
				if (num < currentMusicTracks.Count)
				{
					int nextInQueue = indexQueue.GetNextInQueue(currentTrackIndex, num);
					BufferTrackAtIndex(nextInQueue);
					continue;
				}
				break;
			}
		}

		private void EvaluateNextTrack(int nextTrackIndex)
		{
			currentTrackIndex = nextTrackIndex;
			BufferTrackAtIndex(nextTrackIndex);
		}

		private void EvaluateTrackToUnload(int unloadMusicTrackIndex)
		{
			if (indexQueue.QueueCount > nNextTracksBuffered + nPreviousTracksBuffered + 1)
			{
				UnloadMusicTrackAtIndex(unloadMusicTrackIndex);
			}
		}

		private void UnloadMusicTrackAtIndex(int musicTrackIndex)
		{
			musicPlayerBuffer.UnloadMusicTrack(currentMusicTracks[musicTrackIndex]);
		}

		private void UpdateLoadingTrack(MusicTrack musicTrack)
		{
			MusicPlayerData bufferedMusicPlayerDataForMusicTrack = musicPlayerBuffer.GetBufferedMusicPlayerDataForMusicTrack(musicTrack);
			if (bufferedMusicPlayerDataForMusicTrack == null || bufferedMusicPlayerDataForMusicTrack.FailedLoading)
			{
				FailedLoadingMusicTrack(musicTrack);
			}
		}

		private void FailedLoadingMusicTrack(MusicTrack musicTrack)
		{
			int num = currentMusicTracks.IndexOf(musicTrack);
			if (currentTrackIndex == num)
			{
				currentTrackIndex = -1;
			}
			RemoveMusicTrack(musicTrack);
			musicPlayerBuffer.UnloadMusicTrack(musicTrack);
			ResetIndexQueue();
		}

		private void ResetIndexQueue()
		{
			ClearMusicQueue();
			AddAllCurrentTracksToQueue();
		}

		public void RefreshIndexQueue()
		{
			indexQueue.RefreshAfterChange(currentTrackIndex);
		}

		public void UpdateMusicQueueForStage(MusicTrack trackToPlay)
		{
			indexQueue.ClearQueue();
			int num = 0;
			for (int i = 0; i < currentMusicTracks.Count; i++)
			{
				indexQueue.AddToQueue(i);
				if (trackToPlay == currentMusicTracks[i])
				{
					num = i;
				}
			}
			currentTrackIndex = num;
			indexQueue.RefreshAfterChange(currentTrackIndex);
			BufferTracksInQueue();
		}

		public void ReplaceIndexQueue(AIndexQueue newIndexQueue)
		{
			indexQueue = newIndexQueue;
		}
	}
}
