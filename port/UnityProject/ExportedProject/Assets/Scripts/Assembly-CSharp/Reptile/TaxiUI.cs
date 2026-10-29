using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Reptile
{
	public class TaxiUI : MonoBehaviour, IUIMenuController, IUIElement, IMenuAnimationTransitionController
	{
		[SerializeField]
		private GameObject firstButton;

		[SerializeField]
		private MenuTimelineButton cancelButton;

		[SerializeField]
		private float nonSelectableAlphaValue = 0.3294118f;

		[SerializeField]
		private GameFontType normalGameFontType;

		[SerializeField]
		private GameFontType selectedGameFontType;

		private MenuTimelineButton[] buttons;

		private TMProLocalizationAddOn[] texts;

		private BaseModule baseModule;

		private NPC taxiNPC;

		private const int waitDialogue = 5;

		private const int cancelDialogue = 4;

		private const int preLoadStageDialogue = 2;

		private const int postLoadStageDialogue = 3;

		private Stage stageToLoad = Stage.NONE;

		private Coroutine travelButtonCoroutine;

		private TaxiBehaviour taxiBehaviour;

		private MenuTimelineButton buttonClicked;

		private readonly Stage[] stagesToShow = new Stage[7]
		{
			Stage.hideout,
			Stage.downhill,
			Stage.square,
			Stage.tower,
			Stage.Mall,
			Stage.osaka,
			Stage.pyramid
		};

		public bool IsEnabled { get; set; }

		public bool AlwaysShowOnTop => false;

		public Menu MenuType => Menu.TAXI_MENU;

		public bool AllowPauseMenu => false;

		public bool AllowPhone => false;

		public bool AllowMouseInput => true;

		public bool IsTransitioning => false;

		public static void TaxiFound()
		{
			Core.Instance.SaveManager.CurrentSaveSlot.GetCurrentStageProgress().taxiFound = true;
		}

		public void Init()
		{
			baseModule = Core.Instance.BaseModule;
			Transform parent = firstButton.transform.parent;
			int num = stagesToShow.Length;
			buttons = new MenuTimelineButton[num];
			texts = new TMProLocalizationAddOn[num];
			buttons[0] = firstButton.GetComponent<MenuTimelineButton>();
			texts[0] = firstButton.GetComponentInChildren<TMProLocalizationAddOn>();
			for (int i = 1; i < num; i++)
			{
				GameObject obj = Object.Instantiate(firstButton, parent);
				MenuTimelineButton component = obj.GetComponent<MenuTimelineButton>();
				TMProLocalizationAddOn componentInChildren = obj.GetComponentInChildren<TMProLocalizationAddOn>();
				buttons[i] = component;
				texts[i] = componentInChildren;
			}
			cancelButton.SetButtonVariables(null, canBeSelected: true, buttons[num - 1], null, normalGameFontType, selectedGameFontType, nonSelectableAlphaValue);
			Core.Instance.SaveManager.CurrentSaveSlot.GetStageProgress(Stage.hideout).taxiFound = true;
			base.gameObject.SetActive(value: false);
		}

		public void Show(NPC taxiNPC, TaxiBehaviour taxiBehaviour)
		{
			if (!IsEnabled)
			{
				this.taxiNPC = taxiNPC;
				this.taxiBehaviour = taxiBehaviour;
				buttonClicked = null;
				Core.Instance.SaveManager.CurrentSaveSlot.GetAllStageProgress();
				stageToLoad = Stage.NONE;
				Core.Instance.UIManager.PushNewMenu(this);
			}
		}

		public void Activate()
		{
			StageProgress[] allStageProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetAllStageProgress();
			MenuTimelineButton nextButtonUp = null;
			for (int i = 0; i < stagesToShow.Length; i++)
			{
				Stage stageID = stagesToShow[i];
				MenuTimelineButton button = buttons[i];
				StageProgress obj = allStageProgress[(int)stagesToShow[i]];
				string stageNameKey = Core.Instance.Localizer.GetStageNameKey(stagesToShow[i]);
				bool canBeSelected = false;
				if (obj.taxiFound)
				{
					string tagAtBegin = null;
					if (stageID == baseModule.CurrentStage)
					{
						tagAtBegin = "<u>";
					}
					else
					{
						canBeSelected = true;
					}
					texts[i].AssignAndUpdateTextWithTags(stageNameKey, GroupOptions.Stage, tagAtBegin, null);
				}
				else
				{
					texts[i].AssignAndUpdateText("???", GroupOptions.Stage);
				}
				button.gameObject.SetActive(value: true);
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(() =>
				{
					StageButtonClicked(button, stageID);
				});
				MenuTimelineButton nextButtonDown = cancelButton;
				if (i < stagesToShow.Length - 1)
				{
					nextButtonDown = buttons[i + 1];
				}
				button.SetButtonVariables(null, canBeSelected, nextButtonUp, nextButtonDown, normalGameFontType, selectedGameFontType, nonSelectableAlphaValue);
				button.interactable = true;
				nextButtonUp = button;
			}
			cancelButton.SetButtonVariables(null, canBeSelected: true, nextButtonUp, null, normalGameFontType, selectedGameFontType, nonSelectableAlphaValue);
		}

		public void Hide()
		{
			taxiBehaviour.ExitMenu();
		}

		private static void SetOnEndSequenceForDialogue(NPC npc, int dialogueIndex, UnityAction onEndCall)
		{
			NPC.Dialogue dialogueByNumber = npc.GetDialogueByNumber(dialogueIndex);
			dialogueByNumber.OnEndSequence.RemoveAllListeners();
			dialogueByNumber.OnEndSequence.AddListener(onEndCall);
			SequenceHandler.instance.disabledExit = false;
		}

		public void StageButtonClicked(MenuTimelineButton clickedButton, Stage stageIndex)
		{
			if (stageIndex == Stage.NONE || IsTransitioning || buttonClicked != null)
			{
				return;
			}
			if (!clickedButton.canBeSelected)
			{
				if (travelButtonCoroutine == null)
				{
					travelButtonCoroutine = StartCoroutine(CanNotTravelButtonEffect(clickedButton));
				}
				return;
			}
			Core.Instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.confirm);
			stageToLoad = stageIndex;
			buttonClicked = clickedButton;
			StartCoroutine(taxiBehaviour.FlickerDelayedButtonPress(clickedButton, () =>
			{
				NPC npc = taxiNPC;
				SetOnEndSequenceForDialogue(npc, 5, () =>
				{
					npc.StartDialogueByNumber(2);
				});
				SetOnEndSequenceForDialogue(npc, 2, TeleportToStage);
				taxiBehaviour.ExitMenu();
			}));
		}

		private IEnumerator CanNotTravelButtonEffect(MenuTimelineButton button)
		{
			Core.Instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.cancel);
			RectTransform buttonRectTransform = button.RectTransform();
			float originalX = buttonRectTransform.localPosition.x;
			float duration = 0.25f;
			float times = 0.0625f;
			float passedTime = 0f;
			float maxOffset = 300f;
			do
			{
				float num = maxOffset * (times * 0.5f - Mathf.PingPong(passedTime, times));
				buttonRectTransform.localPosition = new Vector3(originalX + num, buttonRectTransform.localPosition.y, buttonRectTransform.localPosition.z);
				yield return null;
				passedTime += Core.dt;
			}
			while (passedTime < duration);
			buttonRectTransform.localPosition = new Vector3(originalX, buttonRectTransform.localPosition.y, buttonRectTransform.localPosition.z);
			travelButtonCoroutine = null;
		}

		public void ClickSelectedButton()
		{
			if (!(EventSystem.current.currentSelectedGameObject == null))
			{
				Button component = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
				if (!(component == null))
				{
					component.onClick.Invoke();
				}
			}
		}

		public void TeleportToStage()
		{
			StageManager.OnStageInitialized += StageManager_OnStageInitialized;
			Core.Instance.StartCoroutine(TeleportDelayed());
		}

		private IEnumerator TeleportDelayed()
		{
			yield return null;
			Core.Instance.UIManager.PopAllMenusInstant();
			Core.Instance.BaseModule.SwitchStage(stageToLoad);
		}

		private static void StageManager_OnStageInitialized()
		{
			StageManager.OnStageInitialized -= StageManager_OnStageInitialized;
			Taxi[] array = Object.FindObjectsOfType<Taxi>();
			if (array != null && array.Length != 0)
			{
				array[0].GetComponent<NPC>().StartDialogueByNumber(3);
			}
		}

		public void CancelTaxi()
		{
			if (IsTransitioning || buttonClicked != null)
			{
				return;
			}
			Core.Instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.cancel);
			stageToLoad = Stage.NONE;
			buttonClicked = cancelButton;
			StartCoroutine(taxiBehaviour.FlickerDelayedButtonPress(cancelButton, () =>
			{
				NPC npc = taxiNPC;
				SetOnEndSequenceForDialogue(npc, 5, () =>
				{
					npc.StartDialogueByNumber(4);
				});
				taxiBehaviour.ExitMenu();
			}));
		}

		public GameObject GetFirstFocusGameObject()
		{
			return buttons[0].gameObject;
		}

		public void Deactivate()
		{
			taxiNPC = null;
			taxiBehaviour = null;
			buttonClicked = null;
			if (travelButtonCoroutine != null)
			{
				travelButtonCoroutine = null;
			}
		}

		protected void HandlePressedBackButton()
		{
			CancelTaxi();
		}

		public GameObject GetLastSelectedObject()
		{
			return GetFirstFocusGameObject();
		}

		public void SetLastSelectedObject(GameObject selectedObject)
		{
		}

		public void OnPressedBackButton()
		{
			CancelTaxi();
		}

		public IMenuAnimationTransitionController ShowMenu()
		{
			base.gameObject.SetActive(value: true);
			return this;
		}

		public IMenuAnimationTransitionController HideMenu()
		{
			base.gameObject.SetActive(value: false);
			return this;
		}

		public GameObject GetRoot()
		{
			return base.gameObject;
		}

		public void ShowOpeningMenuTransitionAnimation(Menu fromMenu)
		{
		}

		public void ShowClosingMenuTransitionAnimation(Menu toMenu)
		{
		}

		public void PlayCustomMenuAnimation(int index)
		{
		}

		public void PlayCustomMenuAnimationBackwards(int index)
		{
		}
	}
}
