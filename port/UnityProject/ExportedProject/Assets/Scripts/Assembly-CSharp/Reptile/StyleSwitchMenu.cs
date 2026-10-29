using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Reptile
{
	public class StyleSwitchMenu : MonoBehaviour, IUIMenuController, IUIElement, IMenuAnimationTransitionController, IDisposable
	{
		[SerializeField]
		private GameObject firstButton;

		[SerializeField]
		private MenuTimelineButton cancelButton;

		[SerializeField]
		private GameFontType normalGameFontType;

		[SerializeField]
		private GameFontType selectedGameFontType;

		[SerializeField]
		private float nonSelectableAlphaValue = 0.3294118f;

		[SerializeField]
		private TMProLocalizationAddOn menuTitleText;

		[SerializeField]
		private TMProLocalizationAddOn menuHeaderText;

		[SerializeField]
		private MoveStyleSkin[] skateboardUnlockables;

		[SerializeField]
		private MoveStyleSkin[] bmxUnlockables;

		[SerializeField]
		private MoveStyleSkin[] rollerSkatesUnlockables;

		private MoveStyleSkin[] currentUnlockables;

		private Material[] moveStyleMaterials;

		private Material[] previewMaterials;

		private const string skateboardTitle = "MOVESTYLE_TITLE_BOARD";

		private const string bmxTitle = "MOVESTYLE_TITLE_BMX";

		private const string rollerSkatesTitle = "MOVESTYLE_TITLE_INLINE";

		private const string skateboardHeader = "MOVESTYLE_HEADER_BOARD";

		private const string bmxHeader = "MOVESTYLE_HEADER_BMX";

		private const string rollerSkatesHeader = "MOVESTYLE_HEADER_INLINE";

		private StyleSwitchBehaviour clipBehaviour;

		private MoveStyle moveStyleType = MoveStyle.SKATEBOARD;

		private MenuTimelineButton[] buttons;

		private TMProLocalizationAddOn[] texts;

		private Coroutine travelButtonCoroutine;

		private const string lockedSkinButtonLabel = "???";

		private GameObject lastSelectedGameObject;

		private MenuTimelineButton buttonClicked;

		private bool isTransitioning;

		public bool IsEnabled { get; set; }

		public bool AlwaysShowOnTop => false;

		public Menu MenuType => Menu.SWITCH_STYLE_MENU;

		public bool AllowPauseMenu => false;

		public bool AllowPhone => false;

		public bool AllowMouseInput => true;

		public bool IsTransitioning => isTransitioning;

		private static void SortSkinUnlockables(MoveStyleSkin[] skinUnlockables)
		{
			for (int i = 0; i < skinUnlockables.Length; i++)
			{
				if (skinUnlockables[i].skinTexture == null)
				{
					skinUnlockables[i].skinIndex = i;
					continue;
				}
				string text = skinUnlockables[i].skinTexture.name;
				int skinIndex = Convert.ToInt32(text.Substring(text.Length - 1));
				skinUnlockables[i].skinIndex = skinIndex;
			}
			for (int j = 0; j < skinUnlockables.Length; j++)
			{
				int skinIndex2 = skinUnlockables[j].skinIndex;
				if (skinIndex2 != j && (skinUnlockables[skinIndex2].skinIndex != skinIndex2 || !(skinUnlockables[skinIndex2].skinTexture != null)))
				{
					MoveStyleSkin moveStyleSkin = skinUnlockables[j];
					skinUnlockables[j] = skinUnlockables[skinIndex2];
					skinUnlockables[skinIndex2] = moveStyleSkin;
				}
			}
		}

		public void Init()
		{
			base.gameObject.SetActive(value: false);
			SortSkinUnlockables(skateboardUnlockables);
			SortSkinUnlockables(bmxUnlockables);
			SortSkinUnlockables(rollerSkatesUnlockables);
			int num = 10;
			buttons = new MenuTimelineButton[num];
			texts = new TMProLocalizationAddOn[num];
			buttons[0] = firstButton.GetComponent<MenuTimelineButton>();
			texts[0] = firstButton.GetComponentInChildren<TMProLocalizationAddOn>();
			Transform parent = firstButton.transform.parent;
			for (int i = 1; i < num; i++)
			{
				GameObject obj = UnityEngine.Object.Instantiate(firstButton, parent);
				MenuTimelineButton component = obj.GetComponent<MenuTimelineButton>();
				TMProLocalizationAddOn componentInChildren = obj.GetComponentInChildren<TMProLocalizationAddOn>();
				buttons[i] = component;
				texts[i] = componentInChildren;
			}
			cancelButton.SetButtonVariables(null, canBeSelected: true, buttons[num - 1], null, normalGameFontType, selectedGameFontType, nonSelectableAlphaValue);
		}

		public void Dispose()
		{
			lastSelectedGameObject = null;
			currentUnlockables = null;
			clipBehaviour = null;
			moveStyleMaterials = null;
			previewMaterials = null;
			if (travelButtonCoroutine != null)
			{
				travelButtonCoroutine = null;
			}
		}

		public void StartMenu(StyleSwitchBehaviour clipBehaviour, MoveStyle moveStyle)
		{
			if (moveStyle != MoveStyle.SKATEBOARD && moveStyle != MoveStyle.BMX && moveStyle != MoveStyle.INLINE)
			{
				clipBehaviour.ExitMenu();
				return;
			}
			moveStyleType = moveStyle;
			this.clipBehaviour = clipBehaviour;
			buttonClicked = null;
			previewMaterials = clipBehaviour.GetPreviewMaterials();
			switch (moveStyle)
			{
			case MoveStyle.SKATEBOARD:
				menuTitleText.AssignAndUpdateText("MOVESTYLE_TITLE_BOARD", GroupOptions.Text);
				menuHeaderText.AssignAndUpdateText("MOVESTYLE_HEADER_BOARD", GroupOptions.Text);
				currentUnlockables = skateboardUnlockables;
				break;
			case MoveStyle.BMX:
				menuTitleText.AssignAndUpdateText("MOVESTYLE_TITLE_BMX", GroupOptions.Text);
				menuHeaderText.AssignAndUpdateText("MOVESTYLE_HEADER_BMX", GroupOptions.Text);
				currentUnlockables = bmxUnlockables;
				break;
			case MoveStyle.INLINE:
				menuTitleText.AssignAndUpdateText("MOVESTYLE_TITLE_INLINE", GroupOptions.Text);
				menuHeaderText.AssignAndUpdateText("MOVESTYLE_HEADER_INLINE", GroupOptions.Text);
				currentUnlockables = rollerSkatesUnlockables;
				break;
			}
			moveStyleMaterials = MoveStyleLoader.GetMoveStyleMaterials(WorldHandler.instance.GetCurrentPlayer(), moveStyle);
			Core.Instance.UIManager.PushNewMenu(Core.Instance.UIManager.styleSwitchUI);
		}

		public void Activate()
		{
			AUser user = Core.Instance.Platform.User;
			Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			int moveStyleSkin = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(currentPlayer.character).moveStyleSkin;
			MenuTimelineButton nextButtonUp = null;
			for (int i = 0; i < 10; i++)
			{
				MenuTimelineButton button = buttons[i];
				int skinIndex = i;
				UnlockableSaveData unlockableSaveDataFor = user.GetUnlockableSaveDataFor(currentUnlockables[i]);
				if (unlockableSaveDataFor.IsUnlocked)
				{
					string tagAtBegin = null;
					if (skinIndex == moveStyleSkin)
					{
						tagAtBegin = "<u>";
					}
					texts[i].AssignAndUpdateTextWithTags(currentUnlockables[i].Title, GroupOptions.Skin, tagAtBegin, null);
				}
				else
				{
					texts[i].AssignAndUpdateText("???", GroupOptions.Text);
				}
				MenuTimelineButton nextButtonDown = cancelButton;
				if (i < 9)
				{
					nextButtonDown = buttons[i + 1];
				}
				button.SetButtonVariables(() =>
				{
					SkinButtonSelected(button, skinIndex);
				}, unlockableSaveDataFor.IsUnlocked, nextButtonUp, nextButtonDown, normalGameFontType, selectedGameFontType, nonSelectableAlphaValue);
				button.interactable = true;
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(() =>
				{
					SkinButtonClicked(button, skinIndex);
				});
				button.gameObject.SetActive(value: true);
				nextButtonUp = button;
			}
			cancelButton.SetButtonVariables(null, canBeSelected: true, nextButtonUp, null, normalGameFontType, selectedGameFontType, nonSelectableAlphaValue);
		}

		public void Deactivate()
		{
			currentUnlockables = null;
			clipBehaviour = null;
			moveStyleMaterials = null;
			previewMaterials = null;
			buttonClicked = null;
		}

		public void SkinButtonSelected(MenuTimelineButton clickedButton, int skinIndex)
		{
			if (IsTransitioning || buttonClicked != null)
			{
				return;
			}
			Texture moveStyleSkinTexture = WorldHandler.instance.GetCurrentPlayer().CharacterConstructor.GetMoveStyleSkinTexture(moveStyleType, skinIndex);
			if (previewMaterials != null)
			{
				for (int i = 0; i < moveStyleMaterials.Length; i++)
				{
					previewMaterials[i].mainTexture = moveStyleSkinTexture;
				}
			}
		}

		public void SkinButtonClicked(MenuTimelineButton clickedButton, int skinIndex)
		{
			if (IsTransitioning || !(buttonClicked == null))
			{
				return;
			}
			if (!clickedButton.canBeSelected)
			{
				if (travelButtonCoroutine == null)
				{
					travelButtonCoroutine = StartCoroutine(CanNotPerformButtonActionAnimation(clickedButton));
				}
				return;
			}
			buttonClicked = clickedButton;
			Core instance = Core.Instance;
			instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.confirm);
			Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(currentPlayer.character).moveStyleSkin = skinIndex;
			Texture moveStyleSkinTexture = currentPlayer.CharacterConstructor.GetMoveStyleSkinTexture(moveStyleType, skinIndex);
			for (int i = 0; i < moveStyleMaterials.Length; i++)
			{
				moveStyleMaterials[i].mainTexture = moveStyleSkinTexture;
			}
			StartCoroutine(clipBehaviour.FlickerDelayedButtonPress(clickedButton, clipBehaviour.ExitMenu));
		}

		private IEnumerator CanNotPerformButtonActionAnimation(MenuTimelineButton button)
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
				Mathf.Lerp(maxOffset, 0f, passedTime / duration);
				float num = maxOffset * (times * 0.5f - Mathf.PingPong(passedTime, times));
				buttonRectTransform.localPosition = new Vector3(originalX + num, buttonRectTransform.localPosition.y, buttonRectTransform.localPosition.z);
				yield return null;
				passedTime += Core.dt;
			}
			while (passedTime < duration);
			buttonRectTransform.localPosition = new Vector3(originalX, buttonRectTransform.localPosition.y, buttonRectTransform.localPosition.z);
			travelButtonCoroutine = null;
		}

		public GameObject GetLastSelectedObject()
		{
			if (lastSelectedGameObject != null)
			{
				return lastSelectedGameObject;
			}
			return firstButton;
		}

		public void SetLastSelectedObject(GameObject selectedButton)
		{
			lastSelectedGameObject = selectedButton;
		}

		public void OnPressedBackButton()
		{
			if (!IsTransitioning && !(buttonClicked != null))
			{
				Core.Instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.cancel);
				buttonClicked = cancelButton;
				StartCoroutine(clipBehaviour.FlickerDelayedButtonPress(cancelButton, clipBehaviour.ExitMenu));
			}
		}

		private IEnumerator FadeIntoMenu()
		{
			float fadeDuration = 0.4f;
			float shutDuration = 0.2f;
			isTransitioning = true;
			yield return Core.Instance.UIManager.effects.FadeToBlack(fadeDuration).WaitForCompletion();
			base.gameObject.SetActive(value: true);
			yield return new WaitForSeconds(shutDuration);
			yield return Core.Instance.UIManager.effects.FadeOpen(fadeDuration).WaitForCompletion();
			isTransitioning = false;
		}

		public IMenuAnimationTransitionController ShowMenu()
		{
			Core.Instance.StartCoroutine(FadeIntoMenu());
			return this;
		}

		public IMenuAnimationTransitionController HideMenu()
		{
			base.gameObject.SetActive(value: false);
			return this;
		}

		public GameObject GetFirstFocusGameObject()
		{
			buttons[0].SetSelectedColor();
			return firstButton;
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
