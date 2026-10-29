namespace Reptile
{
	public class DefaultIndexQueue : AIndexQueue
	{
		public DefaultIndexQueue(int noIndex)
			: base(noIndex)
		{
		}

		public override int GetNextInQueue(int currentSongIndex, int nextPosition)
		{
			int num = indexQueue.IndexOf(currentSongIndex);
			if (num + nextPosition >= indexQueue.Count)
			{
				return GetFirstInQueue();
			}
			return indexQueue[num + nextPosition];
		}

		public override int GetPreviousInQueue(int currentSongIndex, int previousPosition)
		{
			int num = indexQueue.IndexOf(currentSongIndex);
			if (num - previousPosition < 0)
			{
				return GetLastInQueue();
			}
			return indexQueue[num - previousPosition];
		}

		protected override void UpdateQueue(int nSongs)
		{
			indexQueue.Clear();
			for (int i = 0; i < nSongs; i++)
			{
				indexQueue.Add(i);
			}
		}

		public override void RefreshAfterChange(int currentTrack)
		{
		}
	}
}
