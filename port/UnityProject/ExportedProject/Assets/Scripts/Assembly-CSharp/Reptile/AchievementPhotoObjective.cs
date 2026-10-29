using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class AchievementPhotoObjective : BasePhotoObjective
	{
		[SerializeField]
		private PhotoObjectiveProgressable photoObjectiveProgressable;

		private PhotoStats photoStats;

		private AudioManager audioManager;

		protected override void Awake()
		{
			base.Awake();
			if (!Core.Instance.Platform.HasAchievementsSupport)
			{
				Object.Destroy(this);
				return;
			}
			if (photoObjectiveProgressable == null)
			{
				Object.Destroy(this);
				return;
			}
			audioManager = Core.Instance.AudioManager;
			photoStats = Core.Instance.SaveManager.PlayerStats.photoStats;
		}

		protected override IEnumerator HandlePhotoSucces()
		{
			yield return new WaitForSeconds(1f);
			if (!photoObjectiveProgressable.HasPhotoGraphedObject)
			{
				audioManager.PlaySfxUI(SfxCollectionID.EnvironmentSfx, AudioClipID.MascotUnlock);
				photoObjectiveProgressable.MadePhotograph();
				if (!Core.Instance.Platform.Achievements.HasUnlockedAchievement(AchievementType.PhotoGeneric))
				{
					PlayerAchievements.CheckAndUnlockPhotoAchievement();
				}
			}
		}
	}
}
