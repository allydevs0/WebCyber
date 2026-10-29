using System;
using System.Xml;
using UnityEngine;

namespace Reptile
{
	public class FodsCharacterNamesParser : AFodsLocalizationFileParser
	{
		private const int NAME_COLUMN_INDEX = 1;

		public FodsCharacterNamesParser(XmlDocument xmlDocument, string characterSheetName)
			: base(xmlDocument, characterSheetName)
		{
		}

		public override LocalizationKeyValuePair GetKeyValueForLanguage(int keyIndex, SystemLanguage systemLanguage)
		{
			LocalizationKeyToRowEntry localizationKeyToRowEntry = localizationKeyToRowEntries[keyIndex];
			if (localizationKeyToRowEntry.rowIndex < 0)
			{
				throw new Exception("No valid key index was provided");
			}
			string textValueForCell = fodsTable.GetTextValueForCell(localizationKeyToRowEntry.rowIndex, 1);
			return CreateLocalizationKeyValuePair(localizationKeyToRowEntry.localizationKey, textValueForCell);
		}

		public static int GetCharacterIDFromCharacterString(string characterNameKey)
		{
			int result = 0;
			if (characterNameKey.Length == 12)
			{
				result = Convert.ToInt32(characterNameKey.Substring(characterNameKey.Length - 3));
			}
			return result;
		}
	}
}
