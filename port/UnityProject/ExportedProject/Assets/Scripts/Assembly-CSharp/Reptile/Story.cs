using System.Collections.Generic;

namespace Reptile
{
	public static class Story
	{
		public enum ObjectiveID
		{
			NONE = -1,
			EscapePoliceStation = 0,
			JoinTheCrew = 1,
			BeatFranks = 2,
			DJChallenge1 = 3,
			GoToSquare = 4,
			BeatEclipse = 5,
			DJChallenge2 = 6,
			BeatDotExe = 7,
			DJChallenge3 = 8,
			BeatSamurai = 9,
			DJChallenge4 = 10,
			BeatOsaka = 11,
			DJChallenge5 = 12,
			FinalBoss = 13,
			HangOut = 14,
			SearchForPrince = 15,
			SearchForPrince2 = 16,
			SearchForPrince3 = 17,
			SearchForPrince4 = 18,
			MAX = 19
		}

		public enum Chapter
		{
			NONE = -1,
			CHAPTER_1 = 1,
			CHAPTER_2 = 2,
			CHAPTER_3 = 3,
			CHAPTER_4 = 4,
			CHAPTER_5 = 5,
			CHAPTER_6 = 6
		}

		public struct ObjectiveInfo
		{
			public ObjectiveID id;

			public Stage startStage;

			public Stage targetStage;

			public Chapter chapter;
		}

		private static Dictionary<ObjectiveID, ObjectiveInfo> storyObjectives = new Dictionary<ObjectiveID, ObjectiveInfo>
		{
			{
				ObjectiveID.EscapePoliceStation,
				new ObjectiveInfo
				{
					id = ObjectiveID.EscapePoliceStation,
					startStage = Stage.Prelude,
					targetStage = Stage.Prelude,
					chapter = Chapter.CHAPTER_1
				}
			},
			{
				ObjectiveID.JoinTheCrew,
				new ObjectiveInfo
				{
					id = ObjectiveID.JoinTheCrew,
					startStage = Stage.hideout,
					targetStage = Stage.hideout,
					chapter = Chapter.CHAPTER_1
				}
			},
			{
				ObjectiveID.BeatFranks,
				new ObjectiveInfo
				{
					id = ObjectiveID.BeatFranks,
					startStage = Stage.hideout,
					targetStage = Stage.downhill,
					chapter = Chapter.CHAPTER_1
				}
			},
			{
				ObjectiveID.DJChallenge1,
				new ObjectiveInfo
				{
					id = ObjectiveID.DJChallenge1,
					startStage = Stage.downhill,
					targetStage = Stage.downhill,
					chapter = Chapter.CHAPTER_1
				}
			},
			{
				ObjectiveID.GoToSquare,
				new ObjectiveInfo
				{
					id = ObjectiveID.GoToSquare,
					startStage = Stage.hideout,
					targetStage = Stage.square,
					chapter = Chapter.CHAPTER_2
				}
			},
			{
				ObjectiveID.BeatEclipse,
				new ObjectiveInfo
				{
					id = ObjectiveID.BeatEclipse,
					startStage = Stage.square,
					targetStage = Stage.tower,
					chapter = Chapter.CHAPTER_2
				}
			},
			{
				ObjectiveID.DJChallenge2,
				new ObjectiveInfo
				{
					id = ObjectiveID.DJChallenge2,
					startStage = Stage.tower,
					targetStage = Stage.tower,
					chapter = Chapter.CHAPTER_2
				}
			},
			{
				ObjectiveID.BeatDotExe,
				new ObjectiveInfo
				{
					id = ObjectiveID.BeatDotExe,
					startStage = Stage.hideout,
					targetStage = Stage.Mall,
					chapter = Chapter.CHAPTER_3
				}
			},
			{
				ObjectiveID.DJChallenge3,
				new ObjectiveInfo
				{
					id = ObjectiveID.DJChallenge3,
					startStage = Stage.Mall,
					targetStage = Stage.Mall,
					chapter = Chapter.CHAPTER_3
				}
			},
			{
				ObjectiveID.SearchForPrince,
				new ObjectiveInfo
				{
					id = ObjectiveID.SearchForPrince,
					startStage = Stage.hideout,
					targetStage = Stage.downhill,
					chapter = Chapter.CHAPTER_4
				}
			},
			{
				ObjectiveID.SearchForPrince2,
				new ObjectiveInfo
				{
					id = ObjectiveID.SearchForPrince2,
					startStage = Stage.downhill,
					targetStage = Stage.square,
					chapter = Chapter.CHAPTER_4
				}
			},
			{
				ObjectiveID.SearchForPrince3,
				new ObjectiveInfo
				{
					id = ObjectiveID.SearchForPrince3,
					startStage = Stage.square,
					targetStage = Stage.tower,
					chapter = Chapter.CHAPTER_4
				}
			},
			{
				ObjectiveID.SearchForPrince4,
				new ObjectiveInfo
				{
					id = ObjectiveID.SearchForPrince4,
					startStage = Stage.tower,
					targetStage = Stage.osaka,
					chapter = Chapter.CHAPTER_4
				}
			},
			{
				ObjectiveID.BeatSamurai,
				new ObjectiveInfo
				{
					id = ObjectiveID.BeatSamurai,
					startStage = Stage.hideout,
					targetStage = Stage.pyramid,
					chapter = Chapter.CHAPTER_4
				}
			},
			{
				ObjectiveID.DJChallenge4,
				new ObjectiveInfo
				{
					id = ObjectiveID.DJChallenge4,
					startStage = Stage.pyramid,
					targetStage = Stage.pyramid,
					chapter = Chapter.CHAPTER_4
				}
			},
			{
				ObjectiveID.BeatOsaka,
				new ObjectiveInfo
				{
					id = ObjectiveID.BeatOsaka,
					startStage = Stage.hideout,
					targetStage = Stage.osaka,
					chapter = Chapter.CHAPTER_5
				}
			},
			{
				ObjectiveID.DJChallenge5,
				new ObjectiveInfo
				{
					id = ObjectiveID.DJChallenge5,
					targetStage = Stage.osaka,
					chapter = Chapter.CHAPTER_5
				}
			},
			{
				ObjectiveID.FinalBoss,
				new ObjectiveInfo
				{
					id = ObjectiveID.FinalBoss,
					startStage = Stage.square,
					targetStage = Stage.square,
					chapter = Chapter.CHAPTER_5
				}
			},
			{
				ObjectiveID.HangOut,
				new ObjectiveInfo
				{
					id = ObjectiveID.HangOut,
					startStage = Stage.hideout,
					targetStage = Stage.hideout,
					chapter = Chapter.CHAPTER_6
				}
			}
		};

		public static ObjectiveInfo GetCurrentObjectiveInfo()
		{
			return storyObjectives[Core.Instance.SaveManager.CurrentSaveSlot.CurrentStoryObjective];
		}

		public static ObjectiveInfo GetObjectiveInfo(SaveSlotData saveSlotData)
		{
			return storyObjectives[saveSlotData.CurrentStoryObjective];
		}

		public static ObjectiveInfo GetFirstObjectiveInfoOfChapter(Chapter chapter)
		{
			foreach (KeyValuePair<ObjectiveID, ObjectiveInfo> storyObjective in storyObjectives)
			{
				if (storyObjective.Value.chapter == chapter)
				{
					return storyObjective.Value;
				}
			}
			return storyObjectives[ObjectiveID.EscapePoliceStation];
		}

		public static Stage GetPreviousStageOfObjective(ObjectiveID objective)
		{
			if (storyObjectives.ContainsKey(objective - 1))
			{
				return storyObjectives[objective - 1].targetStage;
			}
			return Stage.NONE;
		}
	}
}
