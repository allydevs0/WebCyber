using System;
using System.Xml;
using UnityEngine;

namespace Reptile
{
	public class FodsCreditsParser : AFodsLocalizationFileParser
	{
		public const int TITLES_COLUMN_INDEX = 0;

		public const int NAMES_COLUMN_INDEX = 1;

		public const int POSITION_COLUMN_INDEX = 2;

		public const int ACCUMULATION_COLUMN_INDEX = 3;

		public FodsCreditsParser(XmlDocument xmlDocument, string creditsSheetName)
			: base(xmlDocument, creditsSheetName)
		{
		}

		public override LocalizationKeyValuePair GetKeyValueForLanguage(int keyIndex, SystemLanguage systemLanguage)
		{
			int rowIndex = localizationKeyToRowEntries[keyIndex].rowIndex;
			if (rowIndex < 0)
			{
				throw new Exception("No valid key index was provided");
			}
			string localizationValue = fodsTable.GetTextValueForCell(rowIndex, 0) + ":" + fodsTable.GetTextValueForCell(rowIndex, 1) + ":" + fodsTable.GetTextValueForCell(rowIndex, 2) + ":" + fodsTable.GetTextValueForCell(rowIndex, 3);
			string localizationKey = rowIndex.ToString();
			return CreateLocalizationKeyValuePair(localizationKey, localizationValue);
		}
	}
}
