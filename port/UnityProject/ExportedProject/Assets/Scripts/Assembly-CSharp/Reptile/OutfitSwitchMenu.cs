using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Reptile
{
	public class OutfitSwitchMenu : MonoBehaviour, IUIMenuController, IUIElement, IMenuAnimationTransitionController
	{
		[SerializeField]
		private GameObject firstButton;

		[SerializeField]
		private GameFontType normalGameFontType;

		[SerializeField]
		private GameFontType selectedGameFontType;

		[SerializeField]
		private float nonSelectableAlphaValue = 0.3294118f;

		[SerializeField]
		private OutfitUnlockable[] outfitUnlockables;

		private int[] characterIndexMap;

		private OutfitSwitchBehaviour clipBehaviour;

		private CharacterVisual previewCharacterVisual;

		private PublicToilet toilet;

		private MenuTimelineButton[] buttons;

		private TMProLocalizationAddOn[] texts;

		private Coroutine buttonCoroutine;

		private const string lockedSkinButtonLabel = "???";

		private Player player;

		private CharacterProgress charProgress;

		private MenuTimelineButton buttonClicked;

		private bool isTransitioning;

		public bool IsEnabled { get; set; }

		public bool AlwaysShowOnTop => false;

		public Menu MenuType => Menu.OUTFIT_SWITCH_MENU;

		public bool AllowPauseMenu => false;

		public bool AllowPhone => false;

		public bool AllowMouseInput => true;

		public bool IsTransitioning => isTransitioning;

		private static void SortUnlockables(OutfitUnlockable[] outfitUnlockables)
		{
			for (int i = 0; i < outfitUnlockables.Length; i++)
			{
				bool flag = false;
				for (int j = 0; j < outfitUnlockables.Length - 1; j++)
				{
					if (outfitUnlockables[j].character > outfitUnlockables[j + 1].character)
					{
						OutfitUnlockable outfitUnlockable = outfitUnlockables[j];
						outfitUnlockables[j] = outfitUnlockables[j + 1];
						outfitUnlockables[j + 1] = outfitUnlockable;
						flag = true;
					}
					else if (outfitUnlockables[j].character == outfitUnlockables[j + 1].character && outfitUnlockables[j].outfitIndex > outfitUnlockables[j + 1].outfitIndex)
					{
						OutfitUnlockable outfitUnlockable2 = outfitUnlockables[j];
						outfitUnlockables[j] = outfitUnlockables[j + 1];
						outfitUnlockables[j + 1] = outfitUnlockable2;
						flag = true;
					}
				}
				if (!flag)
				{
					break;
				}
			}
		}

		public void Init()
		{
			base.gameObject.SetActive(value: false);
			int num = 4;
			buttons = new MenuTimelineButton[num];
			texts = new TMProLocalizationAddOn[num];
			buttons[0] = firstButton.GetComponent<MenuTimelineButton>();
			texts[0] = firstButton.GetComponentInChildren<TMProLocalizationAddOn>();
			Transform parent = firstButton.transform.parent;
			for (int i = 1; i < num; i++)
			{
				GameObject obj = Object.Instantiate(firstButton, parent);
				MenuTimelineButton component = obj.GetComponent<MenuTimelineButton>();
				TMProLocalizationAddOn componentInChildren = obj.GetComponentInChildren<TMProLocalizationAddOn>();
				buttons[i] = component;
				texts[i] = componentInChildren;
			}
			SortUnlockables(outfitUnlockables);
			characterIndexMap = new int[26];
			for (int j = 0; j < outfitUnlockables.Length; j += 4)
			{
				int character = (int)outfitUnlockables[j].character;
				characterIndexMap[character] = j;
			}
		}

		public void StartMenu(OutfitSwitchBehaviour clipBehaviour, CharacterVisual characterVisual, PublicToilet toilet)
		{
			this.toilet = toilet;
			this.clipBehaviour = clipBehaviour;
			previewCharacterVisual = characterVisual;
			player = WorldHandler.instance.GetCurrentPlayer();
			charProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(player.character);
			buttonClicked = null;
			Core.Instance.UIManager.PushNewMenu(Core.Instance.UIManager.outfitSwitchUI);
			toilet.StartMenuVisuals();
		}

		private static OutfitUnlockable GetOutfitUnlockable(OutfitUnlockable[] outfitUnlockables, int[] characterIndexMap, int characterIndex, int outfitIndex)
		{
			int num = characterIndexMap[characterIndex];
			return outfitUnlockables[num + outfitIndex];
		}

		public void Activate()
		{
			int character = (int)player.character;
			CharacterProgress characterProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(player.character);
			_ = player.CharacterConstructor;
			AUser user = Core.Instance.Platform.User;
			MenuTimelineButton nextButtonUp = null;
			for (int i = 0; i < 4; i++)
			{
				MenuTimelineButton button = buttons[i];
				int skinIndex = i;
				OutfitUnlockable outfitUnlockable = GetOutfitUnlockable(outfitUnlockables, characterIndexMap, character, i);
				UnlockableSaveData unlockableSaveDataFor = user.GetUnlockableSaveDataFor(outfitUnlockable);
				if (unlockableSaveDataFor.IsUnlocked)
				{
					string tagAtBegin = null;
					if (characterProgress.outfit == skinIndex)
					{
						tagAtBegin = "<u>";
					}
					texts[i].AssignAndUpdateTextWithTags(outfitUnlockable.outfitName, GroupOptions.Skin, tagAtBegin, null);
				}
				else
				{
					texts[i].AssignAndUpdateText("???", GroupOptions.Text);
				}
				MenuTimelineButton nextButtonDown = null;
				if (i < 3)
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
		}

		public void Deactivate()
		{
			toilet.EndMenuVisuals();
			toilet = null;
			previewCharacterVisual = null;
			clipBehaviour = null;
			buttonClicked = null;
			for (int i = 0; i < buttons.Length; i++)
			{
				if (!(buttons[i] == null))
				{
					buttons[i].onClick.RemoveAllListeners();
				}
			}
		}

		private void OnDestroy()
		{
			clipBehaviour = null;
			if (buttonCoroutine != null)
			{
				buttonCoroutine = null;
			}
			for (int i = 0; i < buttons.Length; i++)
			{
				if (!(buttons[i] == null))
				{
					buttons[i].onClick.RemoveAllListeners();
				}
			}
		}

		public void SkinButtonSelected(MenuTimelineButton clickedButton, int skinIndex)
		{
			if (!IsTransitioning && buttonClicked == null)
			{
				int character = (int)player.character;
				Material material = player.CharacterConstructor.GetCharacterMaterials()[character, skinIndex];
				if (material != null)
				{
					previewCharacterVisual.mainRenderer.material = material;
				}
			}
		}

		public void SkinButtonClicked(MenuTimelineButton clickedButton, int skinIndex)
		{
			if (IsTransitioning || !(buttonClicked == null))
			{
				return;
			}
			if (clickedButton.canBeSelected)
			{
				buttonClicked = clickedButton;
				Core.Instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.confirm);
				int character = (int)player.character;
				if (player.CharacterConstructor.GetCharacterMaterials()[character, skinIndex] != null)
				{
					player.SetOutfit(skinIndex);
					WorldHandler.instance.SetNPCOutfit(player.character, skinIndex);
					WantedManager.instance.StopPlayerWantedStatus(showMessage: true, increaseLosePoliceCount: true);
					StartCoroutine(clipBehaviour.FlickerDelayedButtonPress(clickedButton, clipBehaviour.ExitMenu));
				}
				else
				{
					if (buttonCoroutine == null)
					{
						buttonCoroutine = StartCoroutine(CanNotPerformButtonActionAnimation(clickedButton));
					}
					buttonClicked = null;
				}
			}
			else if (buttonCoroutine == null)
			{
				buttonCoroutine = StartCoroutine(CanNotPerformButtonActionAnimation(clickedButton));
			}
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
			buttonCoroutine = null;
		}

		public GameObject GetLastSelectedObject()
		{
			return GetFirstFocusGameObject();
		}

		public void SetLastSelectedObject(GameObject selectedButton)
		{
		}

		public void OnPressedBackButton()
		{
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
			buttons[charProgress.outfit].SetSelectedColor();
			return buttons[charProgress.outfit].gameObject;
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
