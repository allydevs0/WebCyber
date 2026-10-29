using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reptile
{
	public class SaveSlotMenu : AMenuController
	{
		private const string FILLED_SLOT_FORMAT = "{0} {1}";

		private const string EMPTY_SLOT_LOCALIZATION_KEY = "SAVESLOT_EMPTY_SLOT";

		private const string FILLED_SLOT_LOCALIZATION_KEY = "SAVESLOT_CHAPTER";

		private const string DELETE_MSG_POPUP_LOCALIZATION_KEY = "SAVESLOT_DELETE_MSG";

		private const string DELETE_BUTTON_LOCALIZATION_KEY = "BT_DELETE";

		private int deletingSlotId = -1;

		private int startingFromSlot = -1;

		private bool isStartingGame;

		private bool canRefreshMenu;

		private SaveManager saveManager;

		private IGameTextLocalizer localizer;

		private BaseModule baseModule;

		private AErrorHandler errorHandler;

		private AStorage storage;

		private Coroutine waitForSaveSlotDeletionRoutine;

		[SerializeField]
		private Button[] slotButtons;

		[SerializeField]
		private TextMeshProMenuButton backButton;

		public override IMenuAnimationTransitionController ShowMenu()
		{
			RefreshMenu();
			return base.ShowMenu();
		}

		public void OnPressedSlotButton(int slotId)
		{
			StartGameFromSlot(slotId);
		}

		public void OnPressedDeleteSlotButton(int slotId)
		{
			if (saveManager.IsSaveSlotOccupied(slotId))
			{
				deletingSlotId = slotId;
				lastSelectedButton = slotButtons[slotId].gameObject;
				ShowDeleteSaveSlotPopup();
			}
		}

		protected override void Start()
		{
			base.Start();
			RefreshMenu();
		}

		protected override void Awake()
		{
			base.Awake();
			uIManager = Core.Instance.UIManager;
			saveManager = Core.Instance.SaveManager;
			localizer = Core.Instance.Localizer;
			baseModule = Core.Instance.BaseModule;
			errorHandler = Core.Instance.Platform.ErrorHandler;
			storage = Core.Instance.Platform.Storage;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (waitForSaveSlotDeletionRoutine != null)
			{
				StopCoroutine(waitForSaveSlotDeletionRoutine);
			}
			waitForSaveSlotDeletionRoutine = null;
		}

		protected override void HandlePressedBackButton()
		{
			base.HandlePressedBackButton();
			if (backButton != null)
			{
				if (EventSystem.current.sendNavigationEvents)
				{
					audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.cancel);
				}
				backButton.ShowButtonPress(playSound: false);
			}
			else
			{
				BackButtonDelayedClick(playSound: true);
			}
		}

		public void BackButtonDelayedClick(bool playSound = false)
		{
			if (playSound)
			{
				audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.cancel);
			}
			uIManager.ShowPreviousMenu();
		}

		protected override void OnMenuAnimationEnded()
		{
			base.OnMenuAnimationEnded();
			if (isStartingGame)
			{
				StartGame(startingFromSlot);
			}
			isStartingGame = false;
			startingFromSlot = -1;
		}

		private void RefreshMenu()
		{
			FillSlotButtons();
		}

		private void FillSlotButtons()
		{
			for (int i = 0; i < slotButtons.Length; i++)
			{
				GameObject slotButton = slotButtons[i].gameObject;
				SaveSlotData saveSlotData = saveManager.GetSaveSlotData(i);
				FillSlotButton(slotButton, saveSlotData);
			}
		}

		private void FillSlotButton(GameObject slotButton, SaveSlotData saveSlotData)
		{
			if (saveSlotData == null)
			{
				FillEmptySlotButton(slotButton);
			}
			else
			{
				FillSlotButtonWithData(slotButton, saveSlotData);
			}
		}

		private void FillEmptySlotButton(GameObject slotButton)
		{
			slotButton.GetComponentInChildren<TextMeshProMenuButton>().SetText(localizer.GetUserInterfaceText("SAVESLOT_EMPTY_SLOT"));
		}

		private void FillSlotButtonWithData(GameObject slotButton, SaveSlotData saveSlotData)
		{
			Story.ObjectiveInfo objectiveInfo = Story.GetObjectiveInfo(saveSlotData);
			TextMeshProMenuButton componentInChildren = slotButton.GetComponentInChildren<TextMeshProMenuButton>();
			string userInterfaceText = localizer.GetUserInterfaceText("SAVESLOT_CHAPTER");
			string text = FormatFilledSlotText(userInterfaceText, (int)objectiveInfo.chapter);
			componentInChildren.SetText(text);
		}

		private void StartGameFromSlot(int slotId)
		{
			isStartingGame = true;
			startingFromSlot = slotId;
			uIManager.HideSlotsMenu();
			uIManager.RemoveMainMenu();
		}

		private void StartGame(int slotId)
		{
			baseModule.StopMainMenuSceneMusic();
			baseModule.UnloadMainMenuScene();
			if (saveManager.IsSaveSlotOccupied(slotId))
			{
				baseModule.StartGameFromSaveSlot(slotId);
			}
			else
			{
				baseModule.StartNewGameForSaveSlot(slotId);
			}
		}

		private void ShowDeleteSaveSlotPopup()
		{
			uIManager.overlay.ShowPopup("SAVESLOT_DELETE_MSG", "BT_DELETE", "BT_CANCEL", OnPressedPopupDeleteButton, OnPressedPopupCancelButton);
		}

		private void OnPressedPopupDeleteButton()
		{
			DeleteSlot(deletingSlotId);
			deletingSlotId = -1;
		}

		private void OnPressedPopupCancelButton()
		{
			deletingSlotId = -1;
		}

		private void DeleteSlot(int slotId)
		{
			if (saveManager.IsSaveSlotOccupied(slotId))
			{
				StartDeleteSaveSlot(slotId);
			}
		}

		private string FormatFilledSlotText(string localizedText, int chapterId)
		{
			return $"{localizedText} {chapterId}";
		}

		private void StartDeleteSaveSlot(int slotId)
		{
			uIManager.overlay.popup.ShowLoading();
			canRefreshMenu = false;
			waitForSaveSlotDeletionRoutine = StartCoroutine(WaitForSaveslotDeletion());
			saveManager.DeleteSaveSlot(slotId).OnTransactionDone += OnSaveSlotDeleteDone;
		}

		private IEnumerator WaitForSaveslotDeletion()
		{
			yield return new WaitUntil(() => canRefreshMenu);
			RefreshMenu();
			uIManager.overlay.popup.Hide();
		}

		private void OnSaveSlotDeleteDone(ATransaction transaction)
		{
			transaction.OnTransactionDone -= OnSaveSlotDeleteDone;
			if (transaction.IsSuccess)
			{
				FinishSaveSlotDeletion();
			}
			else if (transaction.Status == ATransaction.TransactionStatus.RETRY)
			{
				RetryDeleteSaveSlot(transaction);
			}
			else
			{
				HandleDeleteSaveSlotError(transaction);
			}
		}

		private void HandleDeleteSaveSlotError(ATransaction transaction)
		{
			IError error = errorHandler.HandleDeleteTransactionError(transaction, uIManager.overlay);
			if (error == null)
			{
				FinishSaveSlotDeletion();
			}
			else
			{
				error.OnErrorHandlerFinished += OnSaveSlotDeleteErrorHandlingFinished;
			}
		}

		private void OnSaveSlotDeleteErrorHandlingFinished(IError error)
		{
			if (error is ITransactionError)
			{
				if (error.ErrorHandlingResult == ErrorHandlingResult.RETRY)
				{
					RetryDeleteSaveSlot((error as ITransactionError).Transaction);
				}
				else
				{
					FinishSaveSlotDeletion();
				}
			}
		}

		private void RetryDeleteSaveSlot(ATransaction transaction)
		{
			uIManager.overlay.popup.ShowLoading();
			transaction.ScheduleForReset();
			storage.EnqueueTransaction(transaction);
			transaction.OnTransactionDone += OnSaveSlotDeleteDone;
		}

		private void FinishSaveSlotDeletion()
		{
			canRefreshMenu = true;
		}
	}
}
