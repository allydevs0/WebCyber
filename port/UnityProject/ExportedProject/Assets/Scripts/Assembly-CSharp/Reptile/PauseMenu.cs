using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace Reptile
{
	public class PauseMenu : AMenuController, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
	{
		private const int MAP_CONTROLLER_INPUT_MULTIPLIER = 80;

		[SerializeField]
		private TextMeshProUGUI currentStageText;

		[SerializeField]
		private TextMeshProUGUI highestComboValueText;

		[SerializeField]
		private TextMeshProUGUI currentRepText;

		[SerializeField]
		private TextMeshProUGUI currentObjectiveText;

		[SerializeField]
		private Image mapIcon;

		[SerializeField]
		private RawImage minimapDisplay;

		[SerializeField]
		private Color minimapLowColor = Color.red;

		[SerializeField]
		private Color minimapHighColor = Color.yellow;

		[SerializeField]
		private Color minimapBackgroundColor = new Color(0f, 0f, 0f, 0f);

		[SerializeField]
		private GameObject optionsMenuButton;

		[SerializeField]
		private TextMeshProUGUI collectablesCountText;

		private GameInput gameInput;

		private BaseModule baseModule;

		private SaveManager saveManager;

		private Mapcontroller mapControllerInstance;

		private RenderTexture minimapHighresTexture;

		private Resolution resolutionBaseUsedForMiniMap;

		private CultureInfo currentCulture;

		private WorldHandler worldHandler;

		private IGameTextLocalizer localizer;

		private bool cameraActiveBeforePause = true;

		private bool dragCamera;

		private bool showMinimap = true;

		private bool resumingGame;

		private bool foundMapInStage;

		public void OnPointerDown(PointerEventData pointerEventData)
		{
			if (pointerEventData.button == PointerEventData.InputButton.Right)
			{
				dragCamera = true;
			}
		}

		public void OnPointerUp(PointerEventData pointerEventData)
		{
			if (pointerEventData.button == PointerEventData.InputButton.Right)
			{
				dragCamera = false;
			}
		}

		public override void Activate()
		{
			base.Activate();
			mapControllerInstance = Mapcontroller.Instance;
			Resolution resolution = Core.Instance.SaveManager.Settings.videoSettings.GetResolution();
			if (resolutionBaseUsedForMiniMap.width != resolution.width || resolutionBaseUsedForMiniMap.height != resolution.height)
			{
				AllocateMinimapRenderTexture(resolution);
			}
			foundMapInStage = Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().mapFound;
			bool flag = false;
			DreamEncounter obj = worldHandler.SceneObjectsRegister.RetrieveDreamEncounter();
			flag = worldHandler.currentEncounter != null && worldHandler.currentEncounter.Equals(obj);
			RefreshLabels(flag);
			DetermineMinimapVisibility(flag);
		}

		public override void Deactivate()
		{
			base.Deactivate();
			if (resumingGame)
			{
				ActivateGame();
				resumingGame = false;
			}
		}

		public void Init(UIManager uIManager, GameInput gameInput, BaseModule baseModule, SaveManager saveManager, IGameTextLocalizer localizer)
		{
			base.uIManager = uIManager;
			this.gameInput = gameInput;
			this.baseModule = baseModule;
			this.saveManager = saveManager;
			this.localizer = localizer;
			currentCulture = CultureInfo.CurrentCulture;
			textMeshProMenuButtons = base.gameObject.GetComponentsInChildren<TextMeshProMenuButton>();
			AllocateMinimapRenderTexture(saveManager.Settings.videoSettings.GetResolution());
		}

		public void PauseFromGame()
		{
			if (worldHandler == null)
			{
				worldHandler = WorldHandler.instance;
			}
			cameraActiveBeforePause = worldHandler.CurrentCamera.enabled;
			worldHandler.CurrentCamera.enabled = false;
			baseModule.PauseGame(PauseType.PauseMenu);
		}

		public void AllocateMinimapRenderTexture(Resolution resolution)
		{
			resolutionBaseUsedForMiniMap = resolution;
			int width = (int)((float)resolutionBaseUsedForMiniMap.width / 2.6966f);
			int height = (int)((float)resolutionBaseUsedForMiniMap.height / 1.7308f);
			minimapHighresTexture = new RenderTexture(width, height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt);
		}

		public void QuitCurrentGame()
		{
			if (!HasConfirmedAction())
			{
				baseModule.QuitCurrentGameSession();
			}
		}

		public void ResumeCurrentGame()
		{
			if (CanSelectAction() && !resumingGame)
			{
				resumingGame = true;
				uIManager.HidePauseMenu();
			}
		}

		public void ShowOptionsMenu()
		{
			if (CanSelectAction())
			{
				uIManager.DisableCurrentMenu();
				mapControllerInstance.DisableMapCamera();
				uIManager.ShowOptionsMenu();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (uIManager != null && uIManager.IsInitialized)
			{
				uIManager.AbsolutePopIfPresent(this);
			}
			gameInput = null;
			baseModule = null;
			uIManager = null;
			saveManager = null;
			mapControllerInstance = null;
		}

		private void Update()
		{
			if (showMinimap)
			{
				UpdateMapController();
			}
		}

		protected override void HandlePressedBackButton()
		{
			base.HandlePressedBackButton();
			if (!base.IsTransitioning)
			{
				if (EventSystem.current.sendNavigationEvents)
				{
					audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.cancel);
				}
				ResumeCurrentGame();
			}
		}

		private void UpdateMapController()
		{
			mapControllerInstance.CameraZoom += gameInput.GetAxis(67) * Time.deltaTime;
			if (dragCamera || gameInput.IsPlayerControllerJoystick())
			{
				mapControllerInstance.CameraRotateYOffset += gameInput.GetAxis(13) * 80f * Time.deltaTime;
				mapControllerInstance.CameraRotateXOffset += gameInput.GetAxis(14) * 80f * Time.deltaTime;
			}
		}

		private void RefreshLabels(bool inDream)
		{
			SaveSlotData currentSaveSlot = saveManager.CurrentSaveSlot;
			StageProgress currentStageProgress = currentSaveSlot.GetCurrentStageProgress();
			currentStageText.text = localizer.GetStageName(currentStageProgress.stageID);
			highestComboValueText.text = FormattingUtility.FormatPlayerScore(currentCulture, currentStageProgress.highestCombo);
			currentObjectiveText.text = localizer.GetObjectiveText(currentSaveSlot.CurrentStoryObjective);
			RefreshCurrentRepText(currentStageProgress, inDream);
			RefreshCurrentCollectablesText(currentStageProgress);
		}

		private void RefreshCurrentRepText(StageProgress currentStageProgress, bool inDream)
		{
			if (worldHandler.requiredREP > 0)
			{
				currentRepText.text = string.Format(currentCulture, "{0:000}/{1:000}", currentStageProgress.reputation, worldHandler.requiredREP);
			}
			else if (foundMapInStage && !inDream)
			{
				currentRepText.text = string.Format(currentCulture, "{0:000}/{1:000}", currentStageProgress.reputation, worldHandler.totalREPInCurrentStage);
			}
			else
			{
				currentRepText.text = string.Format(currentCulture, "{0:000}", currentStageProgress.reputation);
			}
		}

		private void RefreshCurrentCollectablesText(StageProgress currentStageProgress)
		{
			int num = 0;
			int num2 = 0;
			VendingMachine[] array = Object.FindObjectsOfType<VendingMachine>();
			for (int i = 0; i < array.Length; i++)
			{
				for (int j = 0; j < array[i].rewards.Length; j++)
				{
					if (array[i].rewards[j] == VendingMachine.Reward.UNLOCKABLE_DROP)
					{
						num++;
						if (Core.Instance.Platform.User.GetUnlockableSaveDataFor(array[i].unlockableDrop.GetComponent<Pickup>().unlock).IsUnlocked)
						{
							num2++;
						}
					}
				}
			}
			collectablesCountText.text = string.Format(currentCulture, "{0:000}/{1:000}", currentStageProgress.GetCollectableFoundCount() + num2, currentStageProgress.GetCollectableCount() + num);
		}

		private void SetupMinimap()
		{
			minimapDisplay.gameObject.SetActive(value: true);
			minimapDisplay.texture = minimapHighresTexture;
			mapControllerInstance.SetCameraRenderTexture(minimapHighresTexture);
			mapControllerInstance.SetMapCameraBackgroundColor(minimapBackgroundColor);
			mapControllerInstance.EnableMapCamera();
			mapControllerInstance.SetMapCameraToInGameCamera();
			mapControllerInstance.SetMapGradient(minimapLowColor, minimapHighColor);
			mapControllerInstance.CurrentCameraMode = Mapcontroller.CameraModes.FollowPlayerPauseScreen;
		}

		private void DisableMinimap()
		{
			minimapDisplay.gameObject.SetActive(value: false);
		}

		private void DetermineMinimapVisibility(bool inDream)
		{
			showMinimap = !inDream;
			bool flag = false;
			if (showMinimap)
			{
				SetupMinimap();
				flag = foundMapInStage;
			}
			else
			{
				DisableMinimap();
			}
			mapIcon.enabled = flag;
		}

		private void DetermineInGameMinimapCameraActivation()
		{
			if (worldHandler.GetCurrentPlayer().phone.IsUsingMinimap)
			{
				mapControllerInstance.EnableMapCamera();
			}
			else
			{
				mapControllerInstance.DisableMapCamera();
			}
		}

		private bool CanSelectAction()
		{
			if (base.IsTransitioning)
			{
				return false;
			}
			if (HasConfirmedAction())
			{
				return false;
			}
			return true;
		}

		private bool HasConfirmedAction()
		{
			for (int i = 0; i < textMeshProMenuButtons.Length; i++)
			{
				if (textMeshProMenuButtons[i].IsBlinking)
				{
					return true;
				}
			}
			return false;
		}

		private void ActivateGame()
		{
			worldHandler.CurrentCamera.enabled = cameraActiveBeforePause;
			cameraActiveBeforePause = true;
			if (showMinimap)
			{
				DetermineInGameMinimapCameraActivation();
			}
			mapControllerInstance.RestoreDefaultMapControllerState();
			baseModule.UnPauseGame(PauseType.PauseMenu);
		}
	}
}
