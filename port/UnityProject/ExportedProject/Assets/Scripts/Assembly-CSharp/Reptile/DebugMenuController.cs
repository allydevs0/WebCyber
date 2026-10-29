using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Reptile
{
	public class DebugMenuController : AMenuController
	{
		private AGameDebug gameDebug;

		private UserStatusHandler userStatusHandler;

		private GraphicsStatusHandler graphicsStatusHandler;

		private Player currentPlayer;

		private GameObject storyGameObject;

		private SaveManager saveManager;

		private BaseModule baseModule;

		[SerializeField]
		private GameObject userStatus;

		[SerializeField]
		private GameObject graphicsStatus;

		[SerializeField]
		private GameObject userMenuButton;

		[SerializeField]
		private GameObject characterMenuButton;

		[SerializeField]
		private GameObject gameMenuButton;

		[SerializeField]
		private GameObject[] buttonGroups;

		public void InitDebugMenu(AGameDebug gameDebug, BaseModule baseModule, SaveManager saveManager, UIManager uIManager)
		{
			this.gameDebug = gameDebug;
			this.baseModule = baseModule;
			this.saveManager = saveManager;
			base.uIManager = uIManager;
			base.gameObject.SetActive(value: false);
			userStatusHandler = userStatus.GetComponent<UserStatusHandler>();
			userStatusHandler.SetCurrentUser();
			graphicsStatusHandler = graphicsStatus.GetComponent<GraphicsStatusHandler>();
			graphicsStatusHandler.SetGraphicsStatus();
			SceneManager.sceneUnloaded += OnSceneUnloaded;
		}

		public override void Activate()
		{
			base.Activate();
			SetupInGameButtons();
			ShowSubMenu(buttonGroups[0]);
		}

		public override GameObject GetFirstFocusGameObject()
		{
			GameObject gameObject = null;
			for (int i = 0; i < buttonGroups.Length; i++)
			{
				if (buttonGroups[i].activeInHierarchy)
				{
					gameObject = buttonGroups[i];
					break;
				}
			}
			if (gameObject == null)
			{
				return null;
			}
			return GetFirstActiveObjectInButtonGroup(gameObject);
		}

		public void ShowSubMenu(GameObject buttonGroup)
		{
			ShowButtonGroup(buttonGroup);
		}

		public bool GoBackInMenu()
		{
			bool activeSelf = buttonGroups[0].activeSelf;
			if (activeSelf)
			{
				Deactivate();
				return activeSelf;
			}
			Activate();
			return activeSelf;
		}

		public void ShowMainMenu()
		{
			Activate();
		}

		public void ToggleGraphy()
		{
			gameDebug.ToggleFPSCounter();
		}

		public void CycleGraphyPreset()
		{
			gameDebug.CycleFPSCounterPreset();
		}

		public void RestartStage()
		{
			gameDebug.RestartStage();
			ExitDebugMenu();
		}

		public void ExitDebugMenu()
		{
			gameDebug.ToggleDebugMenu();
		}

		public void ShowUserMenu(GameObject userMenuButtonGroup)
		{
			ShowSubMenu(userMenuButtonGroup);
			ActivateUserStatus();
		}

		public void ShowGraphicsMenu(GameObject graphicsMenuButtonGroup)
		{
			ShowSubMenu(graphicsMenuButtonGroup);
			ActivateGraphicsStatus();
		}

		public void GoToStage(string stageName)
		{
			if (baseModule.IsLoading)
			{
				return;
			}
			Stage stage = Utility.SceneNameToStage(stageName);
			SetCurrentSaveSlot();
			if (Utility.GetIsCurrentSceneStage())
			{
				if (baseModule.IsInGamePaused)
				{
					baseModule.UnPauseGame(PauseType.ForceOff);
				}
				baseModule.SwitchStage(stage);
			}
			else if (Utility.IsActiveSceneMainMenuScene)
			{
				uIManager.HideMainMenu();
				baseModule.UnloadMainMenuScene();
				baseModule.LoadStage(stage);
			}
			else
			{
				baseModule.LoadStage(stage);
			}
			ExitDebugMenu();
			uIManager.ClearMenuStack();
		}

		public void GoToStageFromStage(Stage newStage, Stage oldStage)
		{
			if (baseModule.IsLoading)
			{
				return;
			}
			SetCurrentSaveSlot();
			if (Utility.GetIsCurrentSceneStage())
			{
				if (baseModule.IsInGamePaused)
				{
					baseModule.UnPauseGame(PauseType.ForceOff);
				}
				baseModule.SwitchStage(newStage, oldStage);
			}
			else if (Utility.IsActiveSceneMainMenuScene)
			{
				uIManager.HideMainMenu();
				baseModule.UnloadMainMenuScene();
				baseModule.SwitchStage(newStage, oldStage);
			}
			else
			{
				baseModule.SwitchStage(newStage, oldStage);
			}
			ExitDebugMenu();
			uIManager.ClearMenuStack();
		}

		public void GoToChapter(int chapter)
		{
			if (baseModule.IsLoading)
			{
				return;
			}
			Player player = WorldHandler.instance.GetCurrentPlayer();
			if (!(player == null))
			{
				player.LockBoostpack(set: false);
				player.LockSpraycan(set: false);
				player.LockPhone(set: false);
				Core.Instance.SaveManager.CurrentSaveSlot.characterSelectLocked = false;
				Core.Instance.SaveManager.CurrentSaveSlot.taxiLocked = false;
				player.LockCharacterSelect(set: false);
				player.LockCameraApp(set: false);
				player.LockSwitchToEquippedMoveStyle(set: false);
				if (chapter < 6)
				{
					WorldHandler.instance.GetCurrentPlayer().SetCharacter(Characters.metalHead);
				}
				else
				{
					WorldHandler.instance.GetCurrentPlayer().SetCharacter(Characters.legendFace);
				}
				WorldHandler.instance.GetCurrentPlayer().InitVisual();
				Story.ObjectiveInfo firstObjectiveInfoOfChapter = Story.GetFirstObjectiveInfoOfChapter((Story.Chapter)chapter);
				if (chapter == 1)
				{
					Core.Instance.SaveManager.CurrentSaveSlot.maxWantedStars = 2f;
					Core.Instance.SaveManager.SaveCurrentSaveSlot();
				}
				if (chapter == 2)
				{
					Core.Instance.SaveManager.CurrentSaveSlot.maxWantedStars = 2f;
					Core.Instance.SaveManager.SaveCurrentSaveSlot();
				}
				if (chapter == 3)
				{
					Core.Instance.SaveManager.CurrentSaveSlot.maxWantedStars = 3f;
					Core.Instance.SaveManager.SaveCurrentSaveSlot();
				}
				if (chapter == 4)
				{
					Core.Instance.SaveManager.CurrentSaveSlot.maxWantedStars = 4f;
					Core.Instance.SaveManager.SaveCurrentSaveSlot();
				}
				if (chapter == 5)
				{
					Core.Instance.SaveManager.CurrentSaveSlot.maxWantedStars = 5f;
					Core.Instance.SaveManager.SaveCurrentSaveSlot();
				}
				if (chapter > 5)
				{
					Core.Instance.SaveManager.CurrentSaveSlot.maxWantedStars = 6f;
					Core.Instance.SaveManager.SaveCurrentSaveSlot();
				}
				baseModule.StageManager.SetStoryObjective(firstObjectiveInfoOfChapter.id);
				GoToStageFromStage(firstObjectiveInfoOfChapter.startStage, Story.GetPreviousStageOfObjective(firstObjectiveInfoOfChapter.id));
			}
		}

		public void SetMaxWanted(int maxWanted)
		{
			Core.Instance.SaveManager.CurrentSaveSlot.maxWantedStars = maxWanted;
			WantedManager.instance.SetDebugStarAmount(maxWanted);
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void ToggleMute()
		{
			Core.Instance.AudioManager.ToggleMute();
		}

		public void ToggleMusic()
		{
			Core.Instance.AudioManager.ToggleMuteMusic();
		}

		public void ToggleSfx()
		{
			Core.Instance.AudioManager.ToggleMuteSfx();
		}

		public void CycleQualitySettings()
		{
			gameDebug.CycleQualitySettings();
			graphicsStatusHandler.SetGraphicsStatus();
		}

		public void LoginUser()
		{
			AUser user = Core.Instance.Platform.User;
			StartCoroutine(user.LoginASync());
		}

		public void RefreshUserStatus()
		{
			userStatusHandler.SetCurrentUserStatus();
		}

		public void LogoutUser()
		{
			Core.Instance.Platform.User.LogOut();
			userStatusHandler.SetCurrentUserStatus();
		}

		public void SelectNextPlayerCharacter()
		{
			currentPlayer.SwitchCharacterForTests(1);
		}

		public void SelectPreviousPlayerCharacter()
		{
			currentPlayer.SwitchCharacterForTests(-1);
		}

		public void SelectNextPlayerMoveStyle()
		{
			currentPlayer.ChangeMoveStyleForTests(1);
		}

		public void SelectPreviousMoveStyle()
		{
			currentPlayer.ChangeMoveStyleForTests(-1);
		}

		public void IncreasePlayerRep()
		{
			WorldHandler.instance.GetCurrentPlayer().ShowAddRep(5);
			Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().AddRep(5);
			WantedManager.instance.AddDebugeCrime();
		}

		public void ToggleStoryGameObject()
		{
			if (!(storyGameObject == null))
			{
				storyGameObject.SetActive(!storyGameObject.activeSelf);
			}
		}

		public void AllowStageTransitionsEntry()
		{
			List<StageTransition> stageTransitions = WorldHandler.instance.SceneObjectsRegister.stageTransitions;
			for (int i = 0; i < stageTransitions.Count; i++)
			{
				stageTransitions[i].AllowEntryWithoutSave();
			}
		}

		public void SetScreenResolutionTo1080p()
		{
			SetScreenResolution(1920, 1080);
		}

		public void SetScreenResolutionTo720p()
		{
			SetScreenResolution(1280, 720);
		}

		public void DisableVSync()
		{
			QualitySettings.vSyncCount = 0;
		}

		public void EnableVSync()
		{
			QualitySettings.vSyncCount = 1;
		}

		public void EnableVSyncHalf()
		{
			QualitySettings.vSyncCount = 2;
		}

		public void ToggleInfiniteBoost()
		{
		}

		public void SetLocalizerLanguage(string languageName)
		{
			SystemLanguage result = SystemLanguage.Unknown;
			if (Enum.TryParse<SystemLanguage>(languageName, out result))
			{
				gameDebug.UpdateLocalizationLanguage(result);
			}
		}

		public void ClearAchievements()
		{
			DebugUtility.ResetAchievements(Core.Instance.Platform, Core.Instance.SaveManager);
		}

		public void ResetSettingsSaveFile()
		{
			Core.Instance.SaveManager.ResetSettings();
			Core.Instance.SaveManager.SaveSettings();
		}

		public void DeletePhotosFromCloud()
		{
		}

		public void StartDreamEncounter()
		{
		}

		public void PrintRewiredControllerStatus()
		{
			RewiredUtility.PrintAllRewiredConnectedControllers();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			SceneManager.sceneUnloaded -= OnSceneUnloaded;
		}

		protected override void HandlePressedBackButton()
		{
			base.HandlePressedBackButton();
			ExitDebugMenu();
		}

		private void OnSceneUnloaded(Scene scene)
		{
			if (Utility.SceneNameToStage(scene.name) != Stage.NONE)
			{
				currentPlayer = null;
			}
		}

		private void DisableUserMenuButton()
		{
			userMenuButton.SetActive(value: false);
		}

		private void SetupInGameButtons()
		{
			bool flag = WorldHandler.instance != null;
			ToggleCharacterMenuButton(flag);
			ToggleGameMenuButton(flag);
			SetPlayerInstance(flag);
			FindStoryGameObjectInScene();
		}

		private void ToggleCharacterMenuButton(bool enable)
		{
			characterMenuButton.SetActive(enable);
		}

		private void ToggleGameMenuButton(bool enable)
		{
			gameMenuButton.SetActive(enable);
		}

		private void SetPlayerInstance(bool hasValidWorldHandler)
		{
			if (hasValidWorldHandler)
			{
				currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			}
		}

		private void ShowButtonGroup(GameObject buttonGroup)
		{
			for (int i = 0; i < buttonGroups.Length; i++)
			{
				bool active = buttonGroups[i].Equals(buttonGroup);
				buttonGroups[i].SetActive(active);
			}
			DeactivateUserStatus();
			DeActivateGraphicsStatus();
			SetFirstActiveButtonInButtonGroup(buttonGroup);
		}

		private void SetFirstActiveButtonInButtonGroup(GameObject buttonGroup)
		{
			GameObject firstActiveObjectInButtonGroup = GetFirstActiveObjectInButtonGroup(buttonGroup);
			uIManager.SetSelectedGameObject(firstActiveObjectInButtonGroup);
		}

		private GameObject GetFirstActiveObjectInButtonGroup(GameObject buttonGroup)
		{
			Button[] componentsInChildren = buttonGroup.GetComponentsInChildren<Button>();
			GameObject result = null;
			if (componentsInChildren.Length != 0)
			{
				result = componentsInChildren[0].gameObject;
			}
			return result;
		}

		private void ActivateUserStatus()
		{
			userStatus.SetActive(value: true);
			userStatusHandler.SetCurrentUserStatus();
		}

		private void DeactivateUserStatus()
		{
			userStatus.SetActive(value: false);
		}

		private void ActivateGraphicsStatus()
		{
			graphicsStatus.SetActive(value: true);
			graphicsStatusHandler.SetGraphicsStatus();
		}

		private void DeActivateGraphicsStatus()
		{
			graphicsStatus.SetActive(value: false);
		}

		private void SetCurrentSaveSlot()
		{
			if (!saveManager.HasCurrentSaveSlot)
			{
				if (saveManager.IsSaveSlotOccupied(0))
				{
					saveManager.SetCurrentSaveSlot(0);
				}
				else
				{
					saveManager.CreateNewSaveSlot(0);
				}
			}
		}

		private void FindStoryGameObjectInScene()
		{
			if (storyGameObject != null)
			{
				return;
			}
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				if (rootGameObjects[i].name.ToLower().Contains("story"))
				{
					storyGameObject = rootGameObjects[i];
					break;
				}
			}
		}

		private void SetScreenResolution(int width, int height)
		{
			Screen.SetResolution(width, height, fullscreen: true);
		}
	}
}
