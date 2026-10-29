using System;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	[CreateAssetMenu(fileName = "Achievement", menuName = "Reptile/Achievements/Achievement")]
	public class Achievement : ScriptableObject
	{
		[Tooltip("This name has to be unique and one sequence of characters")]
		[SerializeField]
		private string achievementName = string.Empty;

		[SerializeField]
		private string description = string.Empty;

		[SerializeField]
		private int rewardValue;

		[SerializeField]
		private string icon = string.Empty;

		[SerializeField]
		private string steamAPIName = string.Empty;

		private bool isUnlocked;

		public string AchievementName => achievementName;

		public bool IsUnlocked => isUnlocked;

		public int RewardValue => rewardValue;

		public string Description => description;

		public string Icon => icon;

		public string SteamAPIName => steamAPIName;

		public void UnlockAchievement()
		{
			isUnlocked = true;
		}

		public void Reset()
		{
			isUnlocked = false;
		}
	}
}
