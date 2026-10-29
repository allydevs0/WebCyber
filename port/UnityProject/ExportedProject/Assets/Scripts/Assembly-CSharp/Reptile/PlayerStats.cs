using System;
using System.IO;
using UnityEngine;

namespace Reptile
{
	public class PlayerStats : ISaveable
	{
		private const int version = 0;

		public GraffitiStats graffitiStats;

		public BoosterStats boosterStats;

		public PhotoStats photoStats;

		public PlayerStats()
		{
			ResetPlayerStats();
		}

		private void ResetPlayerStats()
		{
			graffitiStats = new GraffitiStats();
			boosterStats = new BoosterStats();
			photoStats = new PhotoStats();
		}

		public void Dispose()
		{
			graffitiStats.UnregisterAll();
			boosterStats.UnregisterAll();
			photoStats.UnregisterAll();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(0);
			int num = graffitiStats.statTrackers.Length;
			writer.Write(num);
			for (int i = 0; i < num; i++)
			{
				graffitiStats.statTrackers[i].Write(writer);
			}
			num = boosterStats.statTrackers.Length;
			writer.Write(num);
			for (int j = 0; j < num; j++)
			{
				boosterStats.statTrackers[j].Write(writer);
			}
			num = photoStats.statTrackers.Length;
			writer.Write(num);
			for (int k = 0; k < num; k++)
			{
				photoStats.statTrackers[k].Write(writer);
			}
		}

		public void Read(BinaryReader reader)
		{
			if (reader.ReadInt32() == 0)
			{
				ReadStats(reader, graffitiStats);
				ReadStats(reader, boosterStats);
				ReadStats(reader, photoStats);
			}
			else
			{
				Debug.LogError("[PlayerStats] Save file has unsupported version number!");
			}
		}

		private void ReadStats<T, A>(BinaryReader reader, GenericStats<T, A> playerStats) where T : struct, IConvertible
		{
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				switch ((TrackerType)reader.ReadInt32())
				{
				case TrackerType.SUM:
					playerStats.statTrackers[i] = new StatTrackerSUM();
					break;
				case TrackerType.MAX:
					playerStats.statTrackers[i] = new StatTrackerMAX();
					break;
				case TrackerType.MIN:
					playerStats.statTrackers[i] = new StatTrackerMIN();
					break;
				case TrackerType.REPLACE:
					playerStats.statTrackers[i] = new StatTrackerREPLACE();
					break;
				}
				playerStats.statTrackers[i].Read(reader);
			}
		}
	}
}
