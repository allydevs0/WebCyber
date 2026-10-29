using UnityEngine;

namespace Reptile
{
	public class FinalBossAchievement : MonoBehaviour
	{
		private Achievements achievements;

		private bool hasUnlockedFinalBossAchievement;

		private bool hasAchievementSupport;

		public void UnlockAchievement()
		{
			if (hasAchievementSupport && !hasUnlockedFinalBossAchievement)
			{
				achievements.UnlockAchievement(AchievementType.NowGoOutside);
			}
		}

		protected void Awake()
		{
			hasAchievementSupport = Core.Instance.Platform.HasAchievementsSupport;
			if (hasAchievementSupport)
			{
				achievements = Core.Instance.Platform.Achievements;
				hasUnlockedFinalBossAchievement = achievements.HasUnlockedAchievement(AchievementType.NowGoOutside);
			}
		}
	}
}
