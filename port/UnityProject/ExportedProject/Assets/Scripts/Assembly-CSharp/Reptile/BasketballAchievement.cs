using UnityEngine;

namespace Reptile
{
	public class BasketballAchievement : MonoBehaviour
	{
		[SerializeField]
		private Junk[] allowedJunkTriggers = new Junk[0];

		private bool hasAchievementSupport = true;

		private Achievements achievements;

		protected void Awake()
		{
			hasAchievementSupport = Core.Instance.Platform.HasAchievementsSupport;
			if (hasAchievementSupport)
			{
				achievements = Core.Instance.Platform.Achievements;
			}
		}

		protected void OnTriggerEnter(Collider other)
		{
			if (!hasAchievementSupport)
			{
				return;
			}
			Junk component = other.GetComponent<Junk>();
			if (component == null)
			{
				return;
			}
			for (int i = 0; i < allowedJunkTriggers.Length; i++)
			{
				if (component.Equals(allowedJunkTriggers[i]))
				{
					achievements.UnlockAchievement(AchievementType.BasketballAchievement);
					break;
				}
			}
		}
	}
}
