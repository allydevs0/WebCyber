using System.Collections.Generic;

namespace Reptile
{
	public abstract class AIndexQueue
	{
		protected List<int> indexQueue = new List<int>();

		protected int noIndex = -1;

		public int QueueCount => indexQueue.Count;

		public AIndexQueue(int noIndex)
		{
			this.noIndex = noIndex;
			indexQueue = new List<int>();
		}

		public int GetFirstInQueue()
		{
			if (indexQueue.Count <= 0)
			{
				return noIndex;
			}
			return indexQueue[0];
		}

		public int GetLastInQueue()
		{
			if (indexQueue.Count <= 0)
			{
				return noIndex;
			}
			return indexQueue[indexQueue.Count - 1];
		}

		public void AddToQueue(int songIndex)
		{
			indexQueue.Add(songIndex);
		}

		public void RemoveFromQueue(int songIndex)
		{
			indexQueue.Remove(songIndex);
		}

		public void ClearQueue()
		{
			indexQueue.Clear();
		}

		public abstract int GetNextInQueue(int currentSongIndex, int nextPosition);

		public abstract int GetPreviousInQueue(int currentSongIndex, int previousPosition);

		protected abstract void UpdateQueue(int nSongs);

		public abstract void RefreshAfterChange(int currentTrack);
	}
}
