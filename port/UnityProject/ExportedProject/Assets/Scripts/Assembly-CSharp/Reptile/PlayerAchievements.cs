using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class PlayerAchievements : MonoBehaviour
	{
		private const float IN_THE_FLOW_ACHIEVEMENT_TIME_SEC = 240f;

		private const float NICE_ACHIEVEMENT_TIME_SEC = 69f;

		private const float IRON_LEGS_HEIGHT = 32f;

		private float inTheFlowAchievementTotalTime;

		private float niceAchievementTotalTime;

		private Achievements achievements;

		private MovementMotor movementMotor;

		private Transform playerTransform;

		private Player player;

		private StageAchievements stageAchievements;

		private WorldHandler worldHandler;

		private bool startedFalling;

		private bool startedComboing;

		private bool startedManualTrick;

		private bool hasUnlockedIronLegsAchievement;

		private bool hasUnlockedInTheFlowAchievement;

		private bool hasUnlockedNiceAchievement;

		private bool hasUnlockedStageGraffitiAchievement;

		private bool hasUnlockedStageScoreAchievement;

		private bool hasUnlockedPhotoAchievement;

		private Stage[] stagesContainingGraffitiAchievements;

		private bool hasUnlockedOldhead;

		private Coroutine unlockCharacterCoroutine;

		private float initialPosition;

		public void StopFalling()
		{
			startedFalling = false;
		}

		internal void CheckAndUnlockOldHead()
		{
			if (!hasUnlockedOldhead && Core.Instance.SaveManager.CurrentSaveSlot.HasAllGraffitiBombedAchievementsLocally(stagesContainingGraffitiAchievements))
			{
				unlockCharacterCoroutine = Core.Instance.StartCoroutine(UnlockCharacterWhenPossible(Characters.oldheadPlayer));
			}
		}

		public void CheckStageGraffitiAchievement()
		{
			if (!(stageAchievements != null))
			{
				return;
			}
			bool flag = Core.Instance.SaveManager.CurrentSaveSlot.IsGraffitiBombedAchievementLocallyUnlocked(stageAchievements.Stage);
			if (hasUnlockedStageGraffitiAchievement && flag)
			{
				return;
			}
			List<GraffitiSpot> grafSpots = worldHandler.GetGrafSpots(onlyUnclaimed: false);
			for (int i = 0; i < grafSpots.Count; i++)
			{
				if (!grafSpots[i].ClaimedByPlayableCrew() && !(grafSpots[i] is GraffitiSpotFinisher))
				{
					return;
				}
			}
			if (!hasUnlockedStageGraffitiAchievement)
			{
				Achievement graffitiAchievement = stageAchievements.GraffitiAchievement;
				achievements.UnlockAchievement(graffitiAchievement);
			}
			hasUnlockedStageGraffitiAchievement = true;
			Core.Instance.SaveManager.CurrentSaveSlot.UnlockGraffitiBombedAchievementLocallyForStage(stageAchievements.Stage);
			CheckAndUnlockOldHead();
		}

		protected void Awake()
		{
			if (!Core.Instance.Platform.HasAchievementsSupport)
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
			Stage currentStage = Utility.GetCurrentStage();
			player = GetComponent<Player>();
			achievements = Core.Instance.Platform.Achievements;
			movementMotor = player.motor;
			playerTransform = player.transform;
			stageAchievements = achievements.GetStageAchievements(currentStage);
			worldHandler = WorldHandler.instance;
			stagesContainingGraffitiAchievements = achievements.GetStagesWithGraffitiAchievement();
			DetermineAchievementsUnlockState();
			if (!achievements.IsLocal)
			{
				Core.OnUpdate += UpdatePlayerAchievements;
			}
		}

		protected void OnDestroy()
		{
			stagesContainingGraffitiAchievements = null;
			if ((bool)Core.Instance && unlockCharacterCoroutine != null)
			{
				Core.Instance.StopCoroutine(unlockCharacterCoroutine);
			}
			Core.OnUpdate -= UpdatePlayerAchievements;
		}

		private void DetermineAchievementsUnlockState()
		{
			hasUnlockedIronLegsAchievement = achievements.HasUnlockedAchievement(AchievementType.IronLegs);
			hasUnlockedInTheFlowAchievement = achievements.HasUnlockedAchievement(AchievementType.InTheFlow);
			hasUnlockedNiceAchievement = achievements.HasUnlockedAchievement(AchievementType.Nice);
			hasUnlockedPhotoAchievement = achievements.HasUnlockedAchievement(AchievementType.PhotoGeneric);
			if (!hasUnlockedPhotoAchievement)
			{
				CheckAndUnlockPhotoAchievement();
			}
			if (stageAchievements != null)
			{
				hasUnlockedStageGraffitiAchievement = achievements.HasUnlockedAchievement(stageAchievements.GraffitiAchievement);
				hasUnlockedStageScoreAchievement = achievements.HasUnlockedAchievement(stageAchievements.ScoreAchievement);
				hasUnlockedOldhead = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(Characters.oldheadPlayer).unlocked;
				CheckAndUnlockOldHead();
			}
		}

		internal static bool CanNotShowNotification(WorldHandler worldHandler, Player player)
		{
			if (!player.IsTooBusyForSuddenCutscenes() && !SequenceHandler.instance.isBusy && !(worldHandler.currentEncounter != null) && !player.IsDead() && !Core.Instance.IsCorePaused)
			{
				return Core.Instance.UIManager.overlay.IsShowingLoadingScreen;
			}
			return true;
		}

		private IEnumerator UnlockCharacterWhenPossible(Characters character)
		{
			WorldHandler worldHandler = WorldHandler.instance;
			while (worldHandler == null)
			{
				worldHandler = WorldHandler.instance;
				yield return null;
			}
			Player player = worldHandler.GetCurrentPlayer();
			while (player == null)
			{
				player = worldHandler.GetCurrentPlayer();
				yield return null;
			}
			while (CanNotShowNotification(worldHandler, player))
			{
				while (CanNotShowNotification(worldHandler, player))
				{
					yield return null;
				}
				yield return null;
			}
			string characterName = Core.Instance.Localizer.GetCharacterName(character);
			Core.Instance.UIManager.ShowNotification("NOTIFICATION_JOIN", characterName);
			Core.Instance.SaveManager.CurrentSaveSlot.UnlockCharacter(character);
		}

		private void UpdatePlayerAchievements()
		{
			bool flag = player.IsNotInCutsceneOrGettingCalled();
			if (!hasUnlockedIronLegsAchievement)
			{
				UpdateIronLegsAchievement();
			}
			if (!hasUnlockedInTheFlowAchievement & flag)
			{
				UpdateInTheFlowAchievement(Time.deltaTime);
			}
			if (!hasUnlockedNiceAchievement & flag)
			{
				UpdateNiceAchievement(Time.deltaTime);
			}
			if (stageAchievements != null)
			{
				UpdateStageAchievements();
			}
		}

		private void UpdateIronLegsAchievement()
		{
			if (!movementMotor.isGrounded && !startedFalling && movementMotor.velocity.y < 0f)
			{
				StartFalling();
			}
			else if (movementMotor.isGrounded && startedFalling)
			{
				FinishedFalling();
			}
		}

		private void FinishedFalling()
		{
			if (initialPosition - playerTransform.position.y >= 32f)
			{
				achievements.UnlockAchievement(AchievementType.IronLegs);
				hasUnlockedIronLegsAchievement = true;
			}
			startedFalling = false;
		}

		private void StartFalling()
		{
			startedFalling = true;
			initialPosition = playerTransform.position.y;
		}

		private void UpdateInTheFlowAchievement(float deltaTime)
		{
			bool flag = player.IsComboing();
			if (flag && !startedComboing)
			{
				StartTrackingComboTime();
			}
			else if (!flag && startedComboing)
			{
				StopTrackingComboTime();
			}
			if (startedComboing)
			{
				UpdateTrackingComboTime(deltaTime);
			}
		}

		private void StartTrackingComboTime()
		{
			startedComboing = true;
			inTheFlowAchievementTotalTime = 0f;
		}

		private void StopTrackingComboTime()
		{
			startedComboing = false;
			inTheFlowAchievementTotalTime = 0f;
		}

		private void UpdateTrackingComboTime(float deltaTime)
		{
			inTheFlowAchievementTotalTime += deltaTime;
			if (inTheFlowAchievementTotalTime >= 240f)
			{
				achievements.UnlockAchievement(AchievementType.InTheFlow);
				hasUnlockedInTheFlowAchievement = true;
				StopTrackingComboTime();
			}
		}

		private void UpdateNiceAchievement(float deltaTime)
		{
			bool isPerformingManualTrick = player.IsPerformingManualTrick;
			if (isPerformingManualTrick && !startedManualTrick)
			{
				StartTrackingManualTrickTime();
			}
			else if (!isPerformingManualTrick && startedManualTrick)
			{
				StopTrackingManualTrickTime();
			}
			if (startedManualTrick)
			{
				UpdateTrackingManualTrickTime(deltaTime);
			}
		}

		private void StartTrackingManualTrickTime()
		{
			startedManualTrick = true;
			niceAchievementTotalTime = 0f;
		}

		private void StopTrackingManualTrickTime()
		{
			startedManualTrick = false;
			niceAchievementTotalTime = 0f;
		}

		private void UpdateTrackingManualTrickTime(float deltaTime)
		{
			niceAchievementTotalTime += deltaTime;
			if (niceAchievementTotalTime >= 69f)
			{
				achievements.UnlockAchievement(AchievementType.Nice);
				hasUnlockedNiceAchievement = true;
				StopTrackingManualTrickTime();
			}
		}

		private void UpdateStageAchievements()
		{
			if (!hasUnlockedStageScoreAchievement)
			{
				UpdateStageScoreAchievement();
			}
		}

		private void UpdateStageScoreAchievement()
		{
			if (Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().highestCombo >= (float)stageAchievements.StageScore)
			{
				achievements.UnlockAchievement(stageAchievements.ScoreAchievement);
				hasUnlockedStageScoreAchievement = true;
			}
		}

		internal static void CheckAndUnlockPhotoAchievement()
		{
			int num = 0;
			int num2 = 0;
			StageProgress[] allStageProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetAllStageProgress();
			for (int i = 0; i < allStageProgress.Length; i++)
			{
				ProgressableData[] progressablesOfType = allStageProgress[i].GetProgressablesOfType(typeof(PhotoObjectProgress));
				num += progressablesOfType.Length;
				for (int j = 0; j < progressablesOfType.Length; j++)
				{
					num2 += Convert.ToInt32((progressablesOfType[j] as PhotoObjectProgress).hasPhotographedObject);
				}
			}
			if (num > 0 && num2 >= num)
			{
				Core.Instance.Platform.Achievements.UnlockAchievement(AchievementType.PhotoGeneric);
			}
		}
	}
}
