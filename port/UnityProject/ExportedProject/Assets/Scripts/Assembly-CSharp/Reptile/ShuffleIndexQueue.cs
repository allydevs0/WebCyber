using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class ShuffleIndexQueue : AIndexQueue
	{
		private List<int> shuffledIndexQueue = new List<int>();

		private System.Random rnd = new System.Random();

		public ShuffleIndexQueue(int noIndex)
			: base(noIndex)
		{
			shuffledIndexQueue = new List<int>();
		}

		public override int GetNextInQueue(int currentSongIndex, int nextPosition)
		{
			int num = shuffledIndexQueue.IndexOf(currentSongIndex);
			if (num + nextPosition >= shuffledIndexQueue.Count)
			{
				return GetFirstInShuffledQueue();
			}
			return shuffledIndexQueue[num + nextPosition];
		}

		public int GetFirstInShuffledQueue()
		{
			if (shuffledIndexQueue.Count <= 0)
			{
				return noIndex;
			}
			return shuffledIndexQueue[0];
		}

		public int GetLastInShuffledQueue()
		{
			if (shuffledIndexQueue.Count <= 0)
			{
				return noIndex;
			}
			return shuffledIndexQueue[shuffledIndexQueue.Count - 1];
		}

		public override int GetPreviousInQueue(int currentSongIndex, int previousPosition)
		{
			int num = shuffledIndexQueue.IndexOf(currentSongIndex);
			if (num - previousPosition < 0)
			{
				return GetLastInShuffledQueue();
			}
			return shuffledIndexQueue[num - previousPosition];
		}

		protected override void UpdateQueue(int nSongs)
		{
			indexQueue.Clear();
			for (int i = 0; i < nSongs; i++)
			{
				indexQueue.Add(i);
			}
		}

		private void CreateShuffleQueue()
		{
			shuffledIndexQueue = indexQueue;
			for (int num = shuffledIndexQueue.Count - 1; num >= 0; num--)
			{
				int index = rnd.Next(num + 1);
				int value = shuffledIndexQueue[index];
				shuffledIndexQueue[index] = shuffledIndexQueue[num];
				shuffledIndexQueue[num] = value;
			}
		}

		public override void RefreshAfterChange(int currentTrack)
		{
			Debug.Log(currentTrack);
			CreateShuffleQueue();
			if (shuffledIndexQueue.Count > 2)
			{
				int num = shuffledIndexQueue.IndexOf(currentTrack);
				Debug.Log("index of current track in shuffle queue " + currentTrack);
				int num2 = num - 1;
				if (num2 < 0)
				{
					num2 = shuffledIndexQueue.Count - 1;
				}
				int index = shuffledIndexQueue.IndexOf(0);
				shuffledIndexQueue[index] = shuffledIndexQueue[num2];
				shuffledIndexQueue[num2] = 0;
				for (int i = 0; i < shuffledIndexQueue.Count; i++)
				{
					Debug.Log("shuffled result " + shuffledIndexQueue[i]);
				}
			}
		}
	}
}
