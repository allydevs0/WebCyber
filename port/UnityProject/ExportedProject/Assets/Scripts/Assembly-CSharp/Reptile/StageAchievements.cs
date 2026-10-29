using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	[CreateAssetMenu(fileName = "StageAchievements", menuName = "Reptile/Achievements/Stage Achievements")]
	public class StageAchievements : ScriptableObject
	{
		[SerializeField]
		private Stage stage;

		[SerializeField]
		private int stageScore;

		[SerializeField]
		private Achievement scoreAchievement;

		[SerializeField]
		private Achievement graffitiAchievement;

		public Stage Stage => stage;

		public Achievement ScoreAchievement => scoreAchievement;

		public Achievement GraffitiAchievement => graffitiAchievement;

		public int StageScore => stageScore;

		public Achievement[] GetAllStageAchievements()
		{
			List<Achievement> list = new List<Achievement>();
			if (scoreAchievement != null)
			{
				list.Add(scoreAchievement);
			}
			if (graffitiAchievement != null)
			{
				list.Add(graffitiAchievement);
			}
			return list.ToArray();
		}
	}
}
