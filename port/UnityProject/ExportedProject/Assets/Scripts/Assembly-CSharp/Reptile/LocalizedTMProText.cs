using TMPro;
using UnityEngine;

namespace Reptile
{
	public class LocalizedTMProText : TextMeshProUGUI
	{
		[SerializeField]
		[HideInInspector]
		private string localizationKey = string.Empty;

		[SerializeField]
		[HideInInspector]
		private GroupOptions localizationGroup;

		private static TextMeshProButtonSpriteHandler buttonSpriteHandler;

		protected override void Awake()
		{
			base.Awake();
			if (buttonSpriteHandler == null)
			{
				buttonSpriteHandler = new TextMeshProButtonSpriteHandler(Core.Instance.platformData.buttonSpriteData, Core.Instance.GameInput);
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			IGameTextLocalizer localizer = Core.Instance.Localizer;
			OnLanguageChanged(localizer);
			localizer.OnLanguageChanged += OnLanguageChanged;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			Core.Instance.Localizer.OnLanguageChanged -= OnLanguageChanged;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (Core.Instance != null && Core.Instance.Localizer != null)
			{
				Core.Instance.Localizer.OnLanguageChanged -= OnLanguageChanged;
			}
		}

		private void OnLanguageChanged(IGameTextLocalizer localizer)
		{
			SetLocalizationOnText(this, localizationKey, localizationGroup, localizer);
		}

		public void SetLocalizationOnText(TextMeshProUGUI textMesh, string localizedKey, GroupOptions groupOption, IGameTextLocalizer localizer, params string[] textsToInsert)
		{
			switch (groupOption)
			{
			case GroupOptions.Text:
				textMesh.text = localizer.GetUserInterfaceText(localizedKey, textsToInsert);
				break;
			case GroupOptions.Dialogue:
				textMesh.text = localizer.GetDialogueText(localizedKey, textsToInsert);
				break;
			case GroupOptions.Credits:
				textMesh.text = localizer.GetCreditsText(localizationKey, 2);
				break;
			case GroupOptions.Character:
				textMesh.text = localizer.GetCharacterName(localizedKey, textsToInsert);
				break;
			case GroupOptions.Stage:
				textMesh.text = localizer.GetStageName(localizedKey, textsToInsert);
				break;
			}
			buttonSpriteHandler.SetCurrentControllerUITextButtonSpriteAsset(textMesh);
		}
	}
}
