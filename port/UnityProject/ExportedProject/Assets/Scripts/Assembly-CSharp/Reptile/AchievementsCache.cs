using System.IO;
using UnityEngine;

namespace Reptile
{
	public class AchievementsCache : ISaveable
	{
		public AchievementLibrary achievementLibrary;

		public int totalPointsObtained;

		public int version;

		public AchievementsCache(AchievementLibrary achievementLibrary)
		{
			this.achievementLibrary = achievementLibrary;
		}

		public void Write(BinaryWriter writer)
		{
			Achievement[] achievements = achievementLibrary.Achievements;
			writer.Write(version);
			writer.Write(achievements.Length);
			for (int i = 1; i < achievements.Length; i++)
			{
				writer.Write(achievements[i].IsUnlocked);
			}
			writer.Write(totalPointsObtained);
		}

		public void Read(BinaryReader reader)
		{
			version = reader.ReadInt32();
			if (version > 0)
			{
				Debug.Log("[Achievements] Incorrect achievements cache version detected");
			}
			if (version != 0)
			{
				return;
			}
			int num = reader.ReadInt32();
			for (int i = 1; i < num; i++)
			{
				if (reader.ReadBoolean())
				{
					achievementLibrary.UnlockAchievement(i);
				}
			}
			totalPointsObtained = reader.ReadInt32();
		}
	}
}
