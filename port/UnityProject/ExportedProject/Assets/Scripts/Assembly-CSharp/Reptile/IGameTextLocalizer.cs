using System;
using UnityEngine;

namespace Reptile
{
	public interface IGameTextLocalizer : IDisposable
	{
		public struct LanguagesLocalizedOptionText
		{
			public string[] languagesNames;

			public string[] languagesApplyText;
		}

		SystemLanguage Language { get; }

		SystemLanguage DefaultLanguage { get; }

		LanguagesLocalizedOptionText AllAvailableLanguages { get; }

		event OnLanguageChangedHandler OnLanguageChanged;

		string GetText(string localizationKey);

		string GetDialogueText(string localizationKey, params string[] textsToInsert);

		string GetCharacterName(string localizationKey, params string[] textsToInsert);

		string GetCharacterName(int localizationKeyID);

		string GetCharacterName(Characters localizationKeyID);

		bool CharacterEqualsCharacterID(Characters character, int characterID);

		string GetStageName(string localizationKey, params string[] textsToInsert);

		string GetStageName(Stage stageID);

		string GetStageNameKey(Stage stageID);

		string GetObjectiveText(Story.ObjectiveID objectiveID);

		string GetEmailText(string localizationKey, params string[] textsToInsert);

		string GetNotificationText(string localizationKey, params string[] textsToInsert);

		string GetSkinText(string localizationKey, params string[] textsToInsert);

		int GetButtonSize(string localizationKey);

		string GetUserInterfaceText(string localizationKey, params string[] textsToInsert);

		string GetCreditsText(string localizationKey, int index);

		void UpdateLocalization(SystemLanguage systemLanguage);

		bool ApplyNewLocalizationLanguage(string languageName);

		string ConvertToLanguageName(SystemLanguage systemLanguage);

		SystemLanguage ConvertToLanguage(string languageName);

		TextMeshProButtonGlyphFormatter GetButtonGlyphTextFormatter();
	}
}
