using UnityEngine;

namespace Reptile
{
	public class BirdAchievement : MonoBehaviour
	{
		private Achievements achievements;

		[SerializeField]
		private StreetLifeCluster streetLifeCluster;

		protected void Awake()
		{
			if (Core.Instance.Platform.HasAchievementsSupport)
			{
				achievements = Core.Instance.Platform.Achievements;
				if (!achievements.HasUnlockedAchievement(AchievementType.FlyingRatsBegone))
				{
					StageManager.OnStageInitialized += OnStageInitialized;
				}
			}
		}

		protected void OnDestroy()
		{
			StageManager.OnStageInitialized -= OnStageInitialized;
			Core.OnUpdate -= UpdateAchievement;
		}

		private void OnStageInitialized()
		{
			Core.OnUpdate += UpdateAchievement;
		}

		private void UpdateAchievement()
		{
			for (int i = 0; i < streetLifeCluster.StreetLifeClusterCount; i++)
			{
				if (!streetLifeCluster.GetStreetLifeHot(i).isShook)
				{
					return;
				}
			}
			UnlockAchievement();
		}

		private void UnlockAchievement()
		{
			achievements.UnlockAchievement(AchievementType.FlyingRatsBegone);
			Core.OnUpdate -= UpdateAchievement;
		}
	}
}
