using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Reptile
{
	public class TextMeshProMenuButton : Button, ITextSelectable
	{
		[Serializable]
		public class DelayedButtonClick : UnityEvent
		{
		}

		public DelayedButtonClick onDelayedButtonClick;

		public bool extendLastCharacter = true;

		public int amountOfExtensions = 2;

		public bool useAnimatedExtensions = true;

		public float totalExtensionAnimationTime = 0.33f;

		public bool useDelayedButtonClick = true;

		public bool disableInputDuringDelay = true;

		public float buttonPressTime = 0.5f;

		public bool useBlinkingTextOnPress = true;

		public int amountOfBlinks = 3;

		public AudioClipID confirmSound = AudioClipID.confirm;

		private TextMeshProMenuButtonFont textMeshProMenuButtonFont;

		private TMProLocalizationAddOn textMeshProLocalizationAddon;

		private TextMeshProUGUI textMeshProButton;

		private EventSystem currentEventSystem;

		private Coroutine extendLastCharacterAnimationRoutine;

		private AudioManager audioManager;

		private bool playSoundOnSelect = true;

		private string originalButtonText = string.Empty;

		private string extendedButtonText = string.Empty;

		private string[] extendedButtonTexts;

		private bool isHighlighted;

		private float blinkFrequency;

		private float extensionAnimationFrequency;

		private bool isBlinking;

		private bool isPressed;

		private float blinkTime;

		public bool UseDelayedButtonClick => useDelayedButtonClick;

		public bool DisableInputDuringDelay => disableInputDuringDelay;

		public bool IsBlinking => isBlinking;

		private bool HasLocalizationAddon => textMeshProLocalizationAddon != null;

		public void ResetSelectableState()
		{
			if (isHighlighted)
			{
				OnHandleRemoveButtonHighlight();
			}
		}

		public void DisableSelectableAsRaycastTarget()
		{
			if (!(base.image == null))
			{
				base.image.raycastTarget = false;
			}
		}

		public void EnableSelectableAsRaycastTarget()
		{
			if (!(base.image == null))
			{
				base.image.raycastTarget = true;
			}
		}

		public override void OnSelect(BaseEventData eventData)
		{
			if (!isPressed && !isHighlighted)
			{
				base.OnSelect(eventData);
				HighlightText();
				if (playSoundOnSelect)
				{
					audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.selectionMove);
				}
			}
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			if (!isPressed && isHighlighted)
			{
				base.OnDeselect(eventData);
				OnHandleRemoveButtonHighlight();
			}
			playSoundOnSelect = true;
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (!isPressed && !isHighlighted)
			{
				if (eventData.selectedObject == base.gameObject)
				{
					HighlightText();
				}
				else
				{
					playSoundOnSelect = false;
					Select();
				}
				audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.selectionMove);
			}
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			if (!isPressed && isHighlighted)
			{
				base.OnPointerExit(eventData);
				OnHandleRemoveButtonHighlight();
			}
			playSoundOnSelect = true;
		}

		public override void OnMove(AxisEventData eventData)
		{
			playSoundOnSelect = false;
			base.OnMove(eventData);
		}

		public void SetText(string newText)
		{
			originalButtonText = newText;
			if (extendLastCharacter)
			{
				SetExtendedText();
			}
			else
			{
				SetOriginalText();
			}
		}

		protected override void Awake()
		{
			base.Awake();
			textMeshProMenuButtonFont = GetComponentInChildren<TextMeshProMenuButtonFont>();
			textMeshProButton = GetComponentInChildren<TextMeshProUGUI>();
			textMeshProLocalizationAddon = GetComponentInChildren<TMProLocalizationAddOn>();
			currentEventSystem = EventSystem.current;
			audioManager = Core.Instance.AudioManager;
			if (HasLocalizationAddon)
			{
				textMeshProLocalizationAddon.OnTextLocalizationChanged += OnTextLocalizationChanged;
			}
			InitializeLastCharacterExtension();
			InitializeOnClickListeners();
			InitializeBlinkFrequency();
			InitializeExtensionAnimationFrequency();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			base.onClick.RemoveAllListeners();
			if (HasLocalizationAddon)
			{
				textMeshProLocalizationAddon.OnTextLocalizationChanged -= OnTextLocalizationChanged;
			}
			StopAllCoroutines();
		}

		private void Update()
		{
			if (isBlinking)
			{
				UpdateBlinking();
			}
		}

		private void UpdateBlinking()
		{
			blinkTime += Time.deltaTime;
			if (blinkTime >= blinkFrequency)
			{
				textMeshProMenuButtonFont.ToggleTextVisibility();
				blinkTime = 0f;
			}
		}

		private void HighlightText()
		{
			isHighlighted = true;
			textMeshProMenuButtonFont.HighlightText();
			if (extendLastCharacter)
			{
				ExtendLastCharacter();
			}
		}

		private void OnTextLocalizationChanged(SystemLanguage language, string localizedText)
		{
			InitializeLastCharacterExtension();
			if (extendLastCharacter && isHighlighted)
			{
				textMeshProButton.text = extendedButtonText;
			}
		}

		private void InitializeLastCharacterExtension()
		{
			originalButtonText = textMeshProButton.text;
			if (extendLastCharacter && textMeshProButton.text.Length > 0)
			{
				CreateExtendedButtonTexts();
			}
		}

		internal void InitializeOnClickListeners()
		{
			if (useDelayedButtonClick)
			{
				base.onClick.AddListener(ShowButtonPress);
			}
		}

		private void InitializeBlinkFrequency()
		{
			if (amountOfBlinks != 0)
			{
				blinkFrequency = buttonPressTime / (float)amountOfBlinks;
			}
		}

		private void InitializeExtensionAnimationFrequency()
		{
			if (amountOfExtensions != 0)
			{
				extensionAnimationFrequency = totalExtensionAnimationTime / (float)amountOfExtensions;
			}
		}

		internal void ShowButtonPress()
		{
			ShowButtonPress(playSound: true);
		}

		internal void ShowButtonPress(bool playSound)
		{
			if (currentEventSystem.sendNavigationEvents)
			{
				if (disableInputDuringDelay)
				{
					DisableInput();
				}
				if (playSound)
				{
					audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, confirmSound);
				}
				StartCoroutine(ShowButtonPressASync());
			}
		}

		internal IEnumerator ShowButtonPressASync()
		{
			isPressed = true;
			DisableSelectableAsRaycastTarget();
			if (useBlinkingTextOnPress)
			{
				textMeshProMenuButtonFont.ToggleTextVisibility();
				isBlinking = true;
			}
			yield return new WaitForSeconds(buttonPressTime);
			isBlinking = false;
			textMeshProMenuButtonFont.ShowText();
			textMeshProMenuButtonFont.ShowTextAlpha();
			EnableInput();
			EnableSelectableAsRaycastTarget();
			isPressed = false;
			if (onDelayedButtonClick != null)
			{
				onDelayedButtonClick.Invoke();
			}
		}

		private void OnHandleRemoveButtonHighlight()
		{
			DeHighlightText();
			if (extendLastCharacter)
			{
				RemoveExtendedCharacter();
			}
		}

		internal void DeHighlightText()
		{
			isHighlighted = false;
			textMeshProMenuButtonFont.DeHighlightText();
		}

		private void RemoveExtendedCharacter()
		{
			SetOriginalText();
			if (useAnimatedExtensions && extendLastCharacterAnimationRoutine != null)
			{
				StopCoroutine(extendLastCharacterAnimationRoutine);
			}
		}

		private void ExtendLastCharacter()
		{
			if (useAnimatedExtensions)
			{
				extendLastCharacterAnimationRoutine = StartCoroutine(ShowLastCharacterExtensions());
			}
			else
			{
				textMeshProButton.text = extendedButtonText;
			}
		}

		private IEnumerator ShowLastCharacterExtensions()
		{
			for (int i = 0; i < extendedButtonTexts.Length; i++)
			{
				yield return new WaitForSeconds(extensionAnimationFrequency);
				textMeshProButton.text = extendedButtonTexts[i];
			}
			extendLastCharacterAnimationRoutine = null;
		}

		private void SetOriginalText()
		{
			textMeshProButton.text = originalButtonText;
		}

		private string CreateExtendedButtonText(int amountOfExtensions)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(originalButtonText);
			for (int i = 0; i < amountOfExtensions; i++)
			{
				stringBuilder.Append(originalButtonText[originalButtonText.Length - 1]);
			}
			return stringBuilder.ToString();
		}

		private void CreateExtendedButtonTexts()
		{
			extendedButtonTexts = new string[amountOfExtensions];
			for (int i = 0; i < extendedButtonTexts.Length; i++)
			{
				extendedButtonTexts[i] = CreateExtendedButtonText(i + 1);
			}
			extendedButtonText = extendedButtonTexts[amountOfExtensions - 1];
		}

		private void SetExtendedText()
		{
			CreateExtendedButtonTexts();
			if (isHighlighted)
			{
				ExtendLastCharacter();
			}
			else
			{
				SetOriginalText();
			}
		}

		private void DisableInput()
		{
			currentEventSystem.sendNavigationEvents = false;
		}

		private void EnableInput()
		{
			currentEventSystem.sendNavigationEvents = true;
		}
	}
}
