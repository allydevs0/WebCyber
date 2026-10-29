using Rewired;
using TMPro;
using UnityEngine;

namespace Reptile
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class TMProLocalizationAddOn : MonoBehaviour
	{
		public delegate void TextLocalizationChangedDelegate(SystemLanguage language, string localizedText);

		[SerializeField]
		[HideInInspector]
		private string localizationKey = string.Empty;

		[SerializeField]
		[HideInInspector]
		private GroupOptions localizationGroup;

		[SerializeField]
		[HideInInspector]
		private TextMeshProUGUI tmpComponent;

		private static TextMeshProButtonSpriteHandler buttonSpriteHandler;

		private GameInput gameInput;

		private SystemLanguage currentLanguage = SystemLanguage.Unknown;

		private ControllerType currentControllerType = ControllerType.Custom;

		private string[] textsToInsert;

		private string tagAtBegin;

		private string tagAtEnd;

		private IGameTextLocalizer localizer;

		public SystemLanguage CurrentLanguage => currentLanguage;

		public event TextLocalizationChangedDelegate OnTextLocalizationChanged;

		protected void Awake()
		{
			gameInput = Core.Instance.GameInput;
			localizer = Core.Instance.Localizer;
			if (buttonSpriteHandler == null)
			{
				buttonSpriteHandler = new TextMeshProButtonSpriteHandler(Core.Instance.platformData.buttonSpriteData, Core.Instance.GameInput);
			}
			if (gameInput != null)
			{
				currentControllerType = gameInput.GetCurrentControllerType();
			}
			GameInput.OnControllerConnected += OnControllerConnected;
		}

		internal void OnEnable()
		{
			if (gameInput != null)
			{
				ControllerType controllerType = gameInput.GetCurrentControllerType();
				if (currentLanguage != localizer.Language || controllerType != currentControllerType)
				{
					OnLanguageChanged(localizer);
				}
				currentControllerType = controllerType;
				localizer.OnLanguageChanged += OnLanguageChanged;
				gameInput.ListenToLastActiveControllerChanged(OnActiveControllerChanged);
				buttonSpriteHandler.SetCurrentControllerUITextButtonSpriteAsset(tmpComponent);
			}
		}

		protected void OnDisable()
		{
			textsToInsert = null;
			tagAtBegin = null;
			tagAtEnd = null;
			if (localizer != null)
			{
				localizer.OnLanguageChanged -= OnLanguageChanged;
			}
			if (gameInput != null)
			{
				gameInput.StopListenToLastActiveControllerChanged(OnActiveControllerChanged);
			}
		}

		protected void OnDestroy()
		{
			textsToInsert = null;
			tagAtBegin = null;
			tagAtEnd = null;
			if (localizer != null)
			{
				localizer.OnLanguageChanged -= OnLanguageChanged;
			}
			if (gameInput != null)
			{
				gameInput.StopListenToLastActiveControllerChanged(OnActiveControllerChanged);
			}
			GameInput.OnControllerConnected -= OnControllerConnected;
		}

		private void OnLanguageChanged(IGameTextLocalizer localizer)
		{
			currentLanguage = localizer.Language;
			SetLocalizationOnText(tmpComponent, localizationKey, localizationGroup, localizer, tagAtBegin, tagAtEnd, textsToInsert);
		}

		public void AssignAndUpdateText(string localizationKey, GroupOptions localizationGroup, params string[] textsToInsert)
		{
			AssignAndUpdateText(localizationKey, localizationGroup, tmpComponent.fontStyle, textsToInsert);
		}

		public void AssignAndUpdateText(string localizationKey, GroupOptions localizationGroup, FontStyles fontStyle, params string[] textsToInsert)
		{
			AssignAndUpdateTextWithTags(localizationKey, localizationGroup, fontStyle, null, null, textsToInsert);
		}

		public void AssignAndUpdateTextWithTags(string localizationKey, GroupOptions localizationGroup, string tagAtBegin, string tagAtEnd, params string[] textsToInsert)
		{
			AssignAndUpdateTextWithTags(localizationKey, localizationGroup, tmpComponent.fontStyle, tagAtBegin, tagAtEnd, textsToInsert);
		}

		public void AssignAndUpdateTextWithTags(string localizationKey, GroupOptions localizationGroup, FontStyles fontStyle, string tagAtBegin, string tagAtEnd, params string[] textsToInsert)
		{
			this.textsToInsert = textsToInsert;
			this.localizationKey = localizationKey;
			this.localizationGroup = localizationGroup;
			this.tagAtBegin = tagAtBegin;
			this.tagAtEnd = tagAtEnd;
			tmpComponent.fontStyle = fontStyle;
			SetLocalizationOnText(tmpComponent, localizationKey, localizationGroup, Core.Instance.Localizer, tagAtBegin, tagAtEnd, textsToInsert);
		}

		private void SetLocalizationOnText(TextMeshProUGUI textMesh, string localizedKey, GroupOptions groupOption, IGameTextLocalizer localizer, string tagAtBegin, string tagAtEnd, params string[] textsToInsert)
		{
			string text = null;
			switch (groupOption)
			{
			case GroupOptions.Text:
				text = localizer.GetUserInterfaceText(localizedKey, textsToInsert);
				break;
			case GroupOptions.Dialogue:
				text = localizer.GetDialogueText(localizedKey, textsToInsert);
				break;
			case GroupOptions.Credits:
				text = localizer.GetCreditsText(localizedKey, 2);
				break;
			case GroupOptions.Character:
				text = localizer.GetCharacterName(localizedKey, textsToInsert);
				break;
			case GroupOptions.Stage:
				text = localizer.GetStageName(localizedKey, textsToInsert);
				break;
			case GroupOptions.Skin:
				text = localizer.GetSkinText(localizedKey, textsToInsert);
				break;
			}
			if (tagAtBegin != null)
			{
				text = ((tagAtEnd == null) ? string.Format("{1}{0}", text, tagAtBegin) : string.Format("{1}{0}{2}", text, tagAtBegin, tagAtEnd));
			}
			else if (tagAtEnd != null)
			{
				text = $"{text}{tagAtEnd}";
			}
			textMesh.text = text;
			buttonSpriteHandler.SetCurrentControllerUITextButtonSpriteAsset(textMesh);
			if (OnTextLocalizationChanged != null)
			{
				OnTextLocalizationChanged(currentLanguage, text);
			}
		}

		private void OnActiveControllerChanged(int playerId, ControllerType controllerType)
		{
			currentControllerType = controllerType;
			OnLanguageChanged(localizer);
		}

		private void OnControllerConnected(ControllerType controllerType, int controllerId)
		{
			OnLanguageChanged(localizer);
		}
	}
}
