using Tayx.Graphy;
using Tayx.Graphy.Utils;
using UnityEngine.SceneManagement;

namespace Reptile
{
	public static class DebugUtility
	{
		public static void RestartCurrentStage(BaseModule baseModule)
		{
			Stage stage = Utility.SceneNameToStage(SceneManager.GetActiveScene().name);
			if (stage != Stage.NONE)
			{
				baseModule.SaveBeforeStageExit();
				baseModule.SwitchStage(stage, Stage.NONE);
			}
		}

		public static void ResetAchievements(APlatform platform, SaveManager saveManager)
		{
			if (platform.HasAchievementsSupport)
			{
				platform.Achievements.ClearAchievements();
				saveManager.ResetAchievementsCache();
				saveManager.ResetPlayerStats();
			}
		}

		public static void ToggleGraphy()
		{
			G_Singleton<GraphyManager>.Instance.ToggleActive();
		}

		public static void CycleGraphyPreset()
		{
			G_Singleton<GraphyManager>.Instance.ToggleModes();
		}

		public static void SetTimeScale(float factor)
		{
		}
	}
}
