using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	[CreateAssetMenu(fileName = "AchievementLibrary", menuName = "Reptile/Achievements/Achievement Library")]
	public class AchievementLibrary : ScriptableObject
	{
		[SerializeField]
		private Achievement[] genericAchievements = new Achievement[0];

		[SerializeField]
		private StageAchievements[] stageAchievements = new StageAchievements[0];

		[SerializeField]
		[HideInInspector]
		private Achievement[] achievements = new Achievement[0];

		public Achievement[] Achievements => achievements;

		public void UnlockAchievement(int index)
		{
			if (index < achievements.Length)
			{
				achievements[index].UnlockAchievement();
			}
		}

		public bool HasUnlockedAchievement(int index)
		{
			if (index >= achievements.Length)
			{
				return false;
			}
			return achievements[index].IsUnlocked;
		}

		public void ResetAchievements()
		{
			for (int i = 0; i < achievements.Length; i++)
			{
				achievements[i].Reset();
			}
		}

		public Achievement GetAchievement(int index)
		{
			if (index >= achievements.Length)
			{
				return null;
			}
			return achievements[index];
		}

		public StageAchievements GetStageAchievements(Stage stage)
		{
			for (int i = 0; i < stageAchievements.Length; i++)
			{
				if (stageAchievements[i].Stage == stage)
				{
					return stageAchievements[i];
				}
			}
			return null;
		}

		public Stage[] GetStagesContainingGraffitiAchievements()
		{
			List<Stage> list = new List<Stage>(stageAchievements.Length);
			for (int i = 0; i < stageAchievements.Length; i++)
			{
				if (stageAchievements[i].GraffitiAchievement != null)
				{
					list.Add(stageAchievements[i].Stage);
				}
			}
			return list.ToArray();
		}

		public AchievementType FindAchievementType(Achievement achievement)
		{
			for (int i = 0; i < achievements.Length; i++)
			{
				if (achievements[i].Equals(achievement))
				{
					return (AchievementType)i;
				}
			}
			return AchievementType.NULL;
		}

		public Achievement GetAchievement(string steamAPIName)
		{
			for (int i = 0; i < achievements.Length; i++)
			{
				if (achievements[i].SteamAPIName.Equals(steamAPIName))
				{
					return achievements[i];
				}
			}
			return null;
		}
	}
}
