using System;
using System.Xml;
using UnityEngine;

namespace Reptile
{
	public class FodsObjectiveTextParser : AFodsLocalizationFileParser
	{
		private const int TEXT_COLUMN_INDEX = 1;

		public FodsObjectiveTextParser(XmlDocument xmlDocument, string objectiveTextSheetName)
			: base(xmlDocument, objectiveTextSheetName)
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
