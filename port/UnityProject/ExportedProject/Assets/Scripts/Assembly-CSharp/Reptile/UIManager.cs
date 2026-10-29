using System.Collections;
using Reptile.Phone;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reptile
{
	public class UIManager : MonoBehaviour
	{
		public GameObject overlayUIRootPrefab;

		public GameObject mainMenuUIPrefab;

		public GameObject optionsMenuUIPrefab;

		public GameObject optionsMenuUIConsolesPrefab;

		public GameObject uiEventSystemPrefab;

		public GameObject debugUIPrefab;

		public GameObject saveSlotsMenuUIPrefab;

		public PhoneContacts characterInfo;

		internal Canvas canvas;

		internal Overlay overlay;

		internal DieMenu dieMenu;

		internal PauseMenu pauseMenu;

		internal OptionsMenu optionsMenu;

		internal EffectsUI effects;

		internal GameplayUI gameplay;

		internal DialogueUI dialogueUI;

		internal CharacterSelectUI characterSelectUI;

		internal DanceAbilityUI danceAbilityUI;

		internal TaxiUI taxiUI;

		internal SaveSlotMenu saveSlotsMenu;

		internal MainMenuManager mainMenu;

		internal DebugMenuController debugMenuUIRoot;

		internal GameNotificationUI gameNotificationUI;

		internal StyleSwitchMenu styleSwitchUI;

		internal OutfitSwitchMenu outfitSwitchUI;

		private Transform UIRoot;

		private GameObject overlayUIRoot;

		private GameObject gameplayMenuUIRoot;

		private GameObject optionsMenuUIRoot;

		private GameObject saveslotMenuUIRoot;

		private IGameTextLocalizer gameTextLocalizer;

		private SaveManager saveManager;

		private TextMeshProButtonSpriteHandler buttonSpriteHandler;

		private TextMeshProButtonGlyphFormatter buttonGlyphFormatter;

		private GameInput gameInput;

		private MenuNavigationController menuNavigationController;

		private BaseModule baseModule;

		private EventSystem uiEventSystem;

		private Coroutine forceForcusOnTopMenuEndOfFrameCoroutine;

		private bool isInitialized;

		public Overlay Overlay => overlay;

		public bool IsInitialized => isInitialized;

		public bool IsShowingAnyMenu => menuNavigationController.IsShowingAnyMenu;

		public bool AllowMouseInputInCurrentMenu => menuNavigationController.CurrentMenuAllowsMouseInput;

		public bool IsShowingNotification => menuNavigationController.IsMenuPartOfMenuStack(gameNotificationUI);

		public bool IsTopMenu(IUIMenuController menu)
		{
			return menuNavigationController.IsMenuTopMenu(menu);
		}

		public bool IsMenuTransitioning(IUIMenuController menu)
		{
			return menuNavigationController.IsTransitioningMenu(menu);
		}

		public void Update()
		{
			PollForButtonPressed();
		}

		public void Init(BaseModule baseModule, IGameTextLocalizer localizer, ButtonSpriteData buttonSpriteData, SaveManager saveManager, GameInput gameInput)
		{
			isInitialized = true;
			GameObject gameObject = Object.Instantiate(uiEventSystemPrefab, base.transform);
			uiEventSystem = gameObject.GetComponent<EventSystem>();
			menuNavigationController = GetComponent<MenuNavigationController>();
			menuNavigationController.Init(uiEventSystem, gameInput);
			this.baseModule = baseModule;
			gameTextLocalizer = localizer;
			this.saveManager = saveManager;
			this.gameInput = gameInput;
			UIRoot = base.transform;
			InstantiateOverlay();
			buttonSpriteHandler = new TextMeshProButtonSpriteHandler(buttonSpriteData, gameInput);
			buttonGlyphFormatter = gameTextLocalizer.GetButtonGlyphTextFormatter();
			gameInput.FirstRewiredPlayer.AddInputEventDelegate(OnAnyButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed);
			gameInput.FirstRewiredPlayer.AddInputEventDelegate(OnAnyButtonPressed, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed);
		}

		public void InstantiateMainMenuSceneUI()
		{
			InstantiateMainMenu();
			InstantiateOptionsMenu();
			InstatiateSaveSlotsMenu();
		}

		public void InstantiateStageSceneUI(GameObject gameplayUIPrefab)
		{
			InstantiateGameMenu(gameplayUIPrefab);
		}

		public GameObject InstantiateDebugMenuUI()
		{
			return null;
		}

		public void DisposeGameplayMenu()
		{
			if (gameplayMenuUIRoot != null)
			{
				Object.Destroy(gameplayMenuUIRoot);
			}
			dieMenu = null;
			pauseMenu = null;
			dialogueUI = null;
			effects = null;
			danceAbilityUI = null;
			taxiUI = null;
			gameplay = null;
			characterSelectUI = null;
			gameNotificationUI = null;
			styleSwitchUI = null;
			outfitSwitchUI = null;
		}

		public void SetSelectedGameObject(GameObject button)
		{
			menuNavigationController.SetSelectedGameObject(button);
		}

		public void ShowMainMenu()
		{
			menuNavigationController.PushNewMenu(mainMenu);
		}

		public void HideMainMenu()
		{
			menuNavigationController.DisableCurrentMenu();
		}

		public void RemoveMainMenu()
		{
			menuNavigationController.AbsolutePopIfPresent(mainMenu);
		}

		public void ShowPauseMenu()
		{
			if (!(pauseMenu == null) && !pauseMenu.IsEnabled && (!(debugMenuUIRoot != null) || !debugMenuUIRoot.IsEnabled) && menuNavigationController.CurrentMenuAllowsPauseMenu && baseModule.StageManager.IsPauseMenuAllowed())
			{
				menuNavigationController.PushNewMenu(pauseMenu);
				pauseMenu.PauseFromGame();
				menuNavigationController.ForceFocusOnTopMenu();
			}
		}

		public void HidePauseMenu()
		{
			if (!(pauseMenu == null) && pauseMenu.IsEnabled)
			{
				menuNavigationController.PopCurrentMenu();
			}
		}

		public void HidePauseMenuInstant()
		{
			if (!(pauseMenu == null) && pauseMenu.IsEnabled)
			{
				menuNavigationController.PopCurrentMenuInstant();
			}
		}

		public void ShowDieMenu()
		{
			if (!dieMenu.Show())
			{
				menuNavigationController.PushNewMenu(dieMenu);
			}
		}

		public void ActivateDieMenu()
		{
			dieMenu.Activate();
		}

		public void HideDieMenu()
		{
			SetSelectedGameObject(null);
			menuNavigationController.PopCurrentMenuInstant();
		}

		public void ShowOptionsMenu()
		{
			menuNavigationController.PushNewMenu(optionsMenu);
		}

		public void HideOptionsMenu()
		{
			menuNavigationController.PopCurrentMenu();
		}

		public void ShowSlotsMenu()
		{
			menuNavigationController.PushNewMenu(saveSlotsMenu);
		}

		public void HideSlotsMenu()
		{
			menuNavigationController.PopCurrentMenu();
		}

		public void ShowDialogueOptions(IUIMenuController dialogueOptions)
		{
			menuNavigationController.PushNewMenu(dialogueOptions);
		}

		public void HideDialogueOptions()
		{
			menuNavigationController.PopCurrentMenuEndOfFrame();
		}

		public void ShowNotification(string text, params string[] textsToInsert)
		{
			gameNotificationUI.SetNotificationText(text, textsToInsert);
			if (!IsShowingNotification)
			{
				PushNewMenuInstant(gameNotificationUI);
			}
		}

		public void ShowNotification(NotificationTextAsset textAsset, params string[] textsToInsert)
		{
			gameNotificationUI.SetNotificationText(textAsset, textsToInsert);
			if (!IsShowingNotification)
			{
				PushNewMenuInstant(gameNotificationUI);
			}
		}

		public void PushNewMenu(IUIMenuController newMenu)
		{
			menuNavigationController.PushNewMenu(newMenu);
		}

		public void PushNewMenuInstant(IUIMenuController newMenu)
		{
			menuNavigationController.PushNewMenuInstant(newMenu);
		}

		public void PopCurrentMenu()
		{
			menuNavigationController.PopCurrentMenu();
		}

		public void PopMenu(IUIMenuController menuToPop)
		{
			menuNavigationController.PopMenu(menuToPop);
		}

		public void DisableCurrentMenu()
		{
			menuNavigationController.DisableCurrentMenu();
		}

		public void PopCurrentMenuInstant()
		{
			menuNavigationController.PopCurrentMenuInstant();
		}

		public void PopAllMenusInstant()
		{
			menuNavigationController.PopAllMenusInstant();
		}

		public void ShowPreviousMenu()
		{
			menuNavigationController.ShowPreviousMenu();
		}

		public void ShowPreviousMenuInstant()
		{
			menuNavigationController.ShowPreviousMenuInstant();
		}

		public void AbsolutePopIfPresent(IUIMenuController menuRootObject)
		{
			menuNavigationController.AbsolutePopIfPresent(menuRootObject);
		}

		public void PushNewMenuEndOfFrame(IUIMenuController newMenu)
		{
			menuNavigationController.PushNewMenuEndOfFrame(newMenu);
		}

		public void PopCurrentMenuEndOfFrame()
		{
			menuNavigationController.PopCurrentMenuEndOfFrame();
		}

		public void ForceFocusOnTopMenu()
		{
			menuNavigationController.ForceFocusOnTopMenu();
		}

		public void RemoveMenuIfJustPopped(IUIMenuController menuToCheck)
		{
			menuNavigationController.RemoveMenuIfJustPopped(menuToCheck);
		}

		public void ClearMenuStack()
		{
			menuNavigationController.Clear();
		}

		public void SetLocalizationValueOnUIText(TextMeshProUGUI textMesh, IGameTextLocalizer localizer, params string[] textsToInsert)
		{
			string text = textMesh.text;
			textMesh.text = localizer.GetUserInterfaceText(text, textsToInsert);
			UpdateTextMeshProCurrentControllerButtonGlyph(textMesh);
		}

		public void SetLocalizationValueOnDialogueText(TextMeshProUGUI textMesh, IGameTextLocalizer localizer, params string[] textsToInsert)
		{
			string text = textMesh.text;
			textMesh.text = localizer.GetDialogueText(text, textsToInsert);
			UpdateTextMeshProCurrentControllerButtonGlyph(textMesh);
		}

		public void SetDialogueLocalization(string text, TextMeshProUGUI textMesh)
		{
			textMesh.text = text;
			SetLocalizationValueOnDialogueText(textMesh, gameTextLocalizer);
		}

		public void SetUILocalization(string text, TextMeshProUGUI textMesh)
		{
			textMesh.text = text;
			SetLocalizationValueOnUIText(textMesh, gameTextLocalizer);
		}

		public void ShowTextMeshProCurrentGamepadActionButtonGlyph(TextMeshProUGUI textMesh, int actionId)
		{
			int currentControllerActionElementId = gameInput.RewiredMappingHandler.GetCurrentControllerActionElementId(actionId, ControllerType.Joystick);
			SetTextMeshProGamepadButtonGlyph(textMesh, currentControllerActionElementId);
		}

		public void ShowTextMeshProCurrentControllerActionButtonGlyph(TextMeshProUGUI textMesh, int actionId)
		{
			SetTextMeshProCurrentControllerButtonGlyph(textMesh, actionId);
		}

		private void OnAnyButtonPressed(InputActionEventData inputActionEventData)
		{
			if (menuNavigationController.IsShowingAnyMenu && !inputActionEventData.IsCurrentInputSource(ControllerType.Mouse) && uiEventSystem.currentSelectedGameObject == null && uiEventSystem.sendNavigationEvents)
			{
				ForceFocusOnTopMenuDelayed();
			}
		}

		private void ForceFocusOnTopMenuDelayed()
		{
			if (forceForcusOnTopMenuEndOfFrameCoroutine == null)
			{
				forceForcusOnTopMenuEndOfFrameCoroutine = StartCoroutine(ForceForcusOnTopMenuEndOfFrame());
			}
		}

		private IEnumerator ForceForcusOnTopMenuEndOfFrame()
		{
			yield return new WaitForEndOfFrame();
			forceForcusOnTopMenuEndOfFrameCoroutine = null;
			ForceFocusOnTopMenu();
		}

		private void OnDestroy()
		{
			isInitialized = false;
			DisposeOverlay();
			DisposeMainMenu();
			DisposeGameplayMenu();
			Object.Destroy(uiEventSystem);
			if (gameInput != null && gameInput.FirstRewiredPlayer != null)
			{
				gameInput.FirstRewiredPlayer.RemoveInputEventDelegate(OnAnyButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed);
				gameInput.FirstRewiredPlayer.RemoveInputEventDelegate(OnAnyButtonPressed, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed);
			}
		}

		private void InstantiateOverlay()
		{
			overlayUIRoot = Object.Instantiate(overlayUIRootPrefab);
			overlayUIRoot.transform.SetParent(UIRoot, worldPositionStays: false);
			InitOverlay();
		}

		private void InitOverlay()
		{
			overlay = overlayUIRoot.GetComponentInChildren<Overlay>();
			overlay.Init();
		}

		private void InstantiateMainMenu()
		{
			Transform transform = Object.Instantiate(mainMenuUIPrefab).transform;
			transform.SetParent(UIRoot, worldPositionStays: false);
			mainMenu = transform.GetComponent<MainMenuManager>();
			InitMainMenu();
		}

		private void InitMainMenu()
		{
			mainMenu.Init(baseModule, this, saveManager);
			HideMenu(mainMenu);
		}

		private void HideMenu(IUIMenuController menuToHide)
		{
			menuToHide.Deactivate();
			menuToHide.IsEnabled = false;
		}

		private void InstantiateOptionsMenu()
		{
			optionsMenuUIRoot = Object.Instantiate(optionsMenuUIPrefab);
			optionsMenuUIRoot.transform.SetParent(UIRoot, worldPositionStays: false);
			InitOptionsMenu();
			HideMenu(optionsMenu);
		}

		private void InitOptionsMenu()
		{
			optionsMenu = optionsMenuUIRoot.GetComponentInChildren<OptionsMenu>();
		}

		private void InstatiateSaveSlotsMenu()
		{
			saveslotMenuUIRoot = Object.Instantiate(saveSlotsMenuUIPrefab);
			saveslotMenuUIRoot.transform.SetParent(UIRoot, worldPositionStays: false);
			InitSaveslotsMenu();
			HideMenu(saveSlotsMenu);
		}

		private void InitSaveslotsMenu()
		{
			saveSlotsMenu = saveslotMenuUIRoot.GetComponent<SaveSlotMenu>();
		}

		private void InstantiateGameMenu(GameObject gameplayUIPrefab)
		{
			gameplayMenuUIRoot = Object.Instantiate(gameplayUIPrefab);
			gameplayMenuUIRoot.transform.SetParent(UIRoot, worldPositionStays: false);
			gameplayMenuUIRoot.transform.SetSiblingIndex(1);
			InitGameMenu();
		}

		private void InitGameMenu()
		{
			canvas = gameplayMenuUIRoot.GetComponent<Canvas>();
			dieMenu = gameplayMenuUIRoot.GetComponentInChildren<DieMenu>(includeInactive: true);
			pauseMenu = gameplayMenuUIRoot.GetComponentInChildren<PauseMenu>(includeInactive: true);
			dialogueUI = gameplayMenuUIRoot.GetComponentInChildren<DialogueUI>(includeInactive: true);
			effects = gameplayMenuUIRoot.GetComponentInChildren<EffectsUI>(includeInactive: true);
			danceAbilityUI = gameplayMenuUIRoot.GetComponentInChildren<DanceAbilityUI>(includeInactive: true);
			taxiUI = gameplayMenuUIRoot.GetComponentInChildren<TaxiUI>(includeInactive: true);
			gameplay = gameplayMenuUIRoot.GetComponentInChildren<GameplayUI>(includeInactive: true);
			characterSelectUI = gameplayMenuUIRoot.GetComponentInChildren<CharacterSelectUI>(includeInactive: true);
			gameNotificationUI = gameplayMenuUIRoot.GetComponentInChildren<GameNotificationUI>(includeInactive: true);
			styleSwitchUI = gameplayMenuUIRoot.GetComponentInChildren<StyleSwitchMenu>(includeInactive: true);
			outfitSwitchUI = gameplayMenuUIRoot.GetComponentInChildren<OutfitSwitchMenu>(includeInactive: true);
			pauseMenu.GetRoot().SetActive(value: false);
			dialogueUI.Init(gameTextLocalizer, this, gameInput);
			effects.Init();
			danceAbilityUI.Init(gameInput, saveManager.CurrentSaveSlot);
			taxiUI.Init();
			gameplay.Init();
			characterSelectUI.Init();
			styleSwitchUI.Init();
			outfitSwitchUI.Init();
			pauseMenu.Init(this, gameInput, baseModule, saveManager, gameTextLocalizer);
			dieMenu.Init(this, baseModule);
			gameNotificationUI.Init(gameInput, baseModule, gameTextLocalizer, this);
		}

		private void DisposeOverlay()
		{
			Object.Destroy(overlayUIRoot);
			overlayUIRoot = null;
		}

		private void DisposeMainMenu()
		{
			if (mainMenu != null)
			{
				Object.Destroy(mainMenu.GetRoot());
			}
			mainMenu = null;
		}

		private void PollForButtonPressed()
		{
			if (gameInput != null && !baseModule.IsLoading)
			{
				if (baseModule.IsPlayingInStage && gameInput.GetButtonNew(4))
				{
					OnPressedPauseButton();
				}
				if (menuNavigationController.IsShowingAnyMenu && gameInput.GetButtonNew(3))
				{
					OnPressedMenuCancelButton();
				}
			}
		}

		private void OnPressedPauseButton()
		{
			if (pauseMenu != null)
			{
				TogglePauseMenu();
			}
		}

		private void TogglePauseMenu()
		{
			if (menuNavigationController.IsMenuPartOfMenuStack(pauseMenu))
			{
				if (menuNavigationController.IsMenuTopMenu(pauseMenu) && !menuNavigationController.IsTransitioning && baseModule.IsInGamePaused)
				{
					if (uiEventSystem.sendNavigationEvents)
					{
						Core.Instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.cancel);
					}
					pauseMenu.ResumeCurrentGame();
				}
			}
			else if (menuNavigationController.CurrentMenuAllowsPauseMenu && !baseModule.IsInGamePaused)
			{
				ShowPauseMenu();
			}
		}

		private void OnPressedMenuCancelButton()
		{
			menuNavigationController.OnPressedBackButton();
		}

		internal void UpdateTextMeshProCurrentControllerButtonGlyph(TextMeshProUGUI textMesh)
		{
			buttonSpriteHandler.SetCurrentControllerUITextButtonSpriteAsset(textMesh);
		}

		private void UpdateTextMeshProJoystickButtonGlyph(TextMeshProUGUI textMesh)
		{
			buttonSpriteHandler.SetUITextButtonJoystickSpriteAsset(textMesh);
		}

		private void SetTextMeshProCurrentControllerButtonGlyph(TextMeshProUGUI textMesh, int actionId)
		{
			ControllerType currentControllerType = gameInput.GetCurrentControllerType();
			int currentControllerActionElementId = gameInput.RewiredMappingHandler.GetCurrentControllerActionElementId(actionId, currentControllerType);
			textMesh.text = buttonGlyphFormatter.CreateTagForControllerType(currentControllerActionElementId, currentControllerType);
			UpdateTextMeshProCurrentControllerButtonGlyph(textMesh);
		}

		private void SetTextMeshProGamepadButtonGlyph(TextMeshProUGUI textMesh, int elementId)
		{
			textMesh.text = buttonGlyphFormatter.CreateTagForControllerType(elementId, ControllerType.Joystick);
			UpdateTextMeshProJoystickButtonGlyph(textMesh);
		}
	}
}
