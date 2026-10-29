using System;
using System.Xml;
using UnityEngine;

namespace Reptile
{
	public class FodsDialogueParser : AFodsLocalizationFileParser
	{
		private const int DIALOGUE_STAGE_COLUMN_INDEX = 1;

		private const int TRANSLATION_COLUMN_INDEX = 2;

		private const string DIALOGUE_LOCALIZATION_KEY_FORMAT = "{0}_{1}_{2}";

		public FodsDialogueParser(XmlDocument xmlDocument, string dialogueSheetName)
			: base(xmlDocument, dialogueSheetName)
		{
		}

		public override LocalizationKeyValuePair GetKeyValueForLanguage(int keyIndex, SystemLanguage systemLanguage)
		{
			int rowIndex = localizationKeyToRowEntries[keyIndex].rowIndex;
			if (rowIndex < 0)
			{
				throw new Exception("No valid key index was provided");
			}
			string localizationKey = localizationKeyToRowEntries[keyIndex].localizationKey;
			string dialogueStageKey = GetDialogueStageKey(rowIndex);
			string localizationValue = GetLocalizationValue(rowIndex, 2);
			string localizationKey2 = FormatLocalizationKey(localizationKey, dialogueStageKey);
			return CreateLocalizationKeyValuePair(localizationKey2, localizationValue);
		}

		private string FormatLocalizationKey(string localizationKey, string dialogueStageKey)
		{
			if (dialogueStageKey.Equals(string.Empty))
			{
				return localizationKey;
			}
			return $"{fodsTableName}_{dialogueStageKey}_{localizationKey}";
		}

		private string GetDialogueStageKey(int rowIndex)
		{
			return fodsTable.GetTextValueForCell(rowIndex, 1);
		}
	}
}
