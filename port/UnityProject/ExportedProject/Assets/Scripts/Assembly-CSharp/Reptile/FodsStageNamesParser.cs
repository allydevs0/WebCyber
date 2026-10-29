using System;
using System.Xml;
using UnityEngine;

namespace Reptile
{
	public class FodsStageNamesParser : AFodsLocalizationFileParser
	{
		private const int NAME_COLUMN_INDEX = 1;

		public FodsStageNamesParser(XmlDocument xmlDocument, string stageSheetName)
			: base(xmlDocument, stageSheetName)
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
	}
}
