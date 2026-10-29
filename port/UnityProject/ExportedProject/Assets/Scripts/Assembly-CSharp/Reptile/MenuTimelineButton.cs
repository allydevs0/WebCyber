using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reptile
{
	public class MenuTimelineButton : Button
	{
		public delegate void OnSelectButtonAction();

		internal TextMeshProUGUI buttonText;

		internal bool canBeSelected;

		private GameFontType normalGameFontType;

		private GameFontType selectedGameFontType;

		private TextMeshProFont normalFont;

		private TextMeshProFont selectedFont;

		private TextMeshProFont currentFont;

		private float nonSelectableAlphaValue;

		private string originalText;

		private string selectedText;

		private bool isSelected;

		private OnSelectButtonAction selectAction;

		private const string selectedFormatString = "• {0}";

		private void SetButtonVisual(TextMeshProFont font, bool playSound, bool isSelected = true)
		{
			Color color = buttonText.color;
			currentFont = font;
			buttonText.fontSharedMaterial = font.FontMaterial;
			if (playSound)
			{
				Core.Instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.selectionMove);
			}
			if (canBeSelected)
			{
				color.a = 1f;
			}
			else
			{
				color.a = nonSelectableAlphaValue;
			}
			buttonText.text = (isSelected ? selectedText : originalText);
			this.isSelected = isSelected;
			buttonText.color = color;
		}

		public void SetButtonVariables(OnSelectButtonAction selectButtonAction, bool canBeSelected, Button nextButtonUp, Button nextButtonDown, GameFontType normalGameFontType, GameFontType selectedGameFontType, float nonSelectableAlphaValue)
		{
			this.canBeSelected = canBeSelected;
			this.normalGameFontType = normalGameFontType;
			this.selectedGameFontType = selectedGameFontType;
			this.nonSelectableAlphaValue = nonSelectableAlphaValue;
			isSelected = false;
			IGameTextLocalizer localizer = Core.Instance.Localizer;
			GetComponentInChildren<TMProLocalizationAddOn>().OnTextLocalizationChanged += MenuTimelineButton_OnTextLocalizationChanged;
			buttonText = GetComponentInChildren<TextMeshProUGUI>();
			currentFont = normalGameFontType.GetFont(localizer.Language);
			SetTextMeshProFonts(localizer.Language);
			originalText = buttonText.text;
			selectedText = $"• {buttonText.text}";
			UpdateTextMesh(normalFont);
			SetButtonVisual(normalFont, playSound: false, isSelected: false);
			Navigation navigation = base.navigation;
			navigation.mode = Navigation.Mode.Explicit;
			navigation.selectOnUp = nextButtonUp;
			navigation.selectOnDown = nextButtonDown;
			navigation.selectOnLeft = null;
			navigation.selectOnRight = null;
			base.navigation = navigation;
			selectAction = null;
			if (canBeSelected)
			{
				selectAction = selectButtonAction;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			buttonText.text = originalText;
			GetComponentInChildren<TMProLocalizationAddOn>().OnTextLocalizationChanged -= MenuTimelineButton_OnTextLocalizationChanged;
		}

		private void MenuTimelineButton_OnTextLocalizationChanged(SystemLanguage language, string localizedText)
		{
			originalText = localizedText;
			selectedText = $"• {buttonText.text}";
			SetTextMeshProFonts(language);
			if (isSelected && buttonText.text[0] != "• {0}"[0])
			{
				SetButtonVisual(selectedFont, playSound: false);
			}
		}

		private void SetTextMeshProFonts(SystemLanguage language)
		{
			bool num = currentFont.Equals(normalFont);
			normalFont = normalGameFontType.GetFont(language);
			selectedFont = selectedGameFontType.GetFont(language);
			if (num)
			{
				UpdateTextMesh(normalFont);
			}
			else
			{
				UpdateTextMesh(selectedFont);
			}
		}

		private void UpdateTextMesh(TextMeshProFont font)
		{
			buttonText.font = font.FontAsset;
			buttonText.fontSharedMaterial = font.FontMaterial;
			buttonText.ForceMeshUpdate();
		}

		public void SetSelectedColor()
		{
			SetButtonVisual(selectedFont, playSound: false);
		}

		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			SetButtonVisual(selectedFont, playSound: true);
			if (selectAction != null)
			{
				selectAction();
			}
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (eventData.selectedObject == base.gameObject)
			{
				SetButtonVisual(selectedFont, playSound: true);
				return;
			}
			Select();
			SetButtonVisual(selectedFont, playSound: false);
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);
			SetButtonVisual(normalFont, playSound: false, isSelected: false);
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			base.OnDeselect(eventData);
			SetButtonVisual(normalFont, playSound: false, isSelected: false);
		}

		public static void EnableText(MenuTimelineButton button, float isActive)
		{
			button.buttonText.alpha = isActive;
		}
	}
}
