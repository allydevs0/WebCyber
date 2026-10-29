using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "DelimiterRegexFormatterData", menuName = "Reptile/Localization/Delimter regex formatting data", order = 2)]
	public class DelimiterRegexFormatterData : FormatterData, IRegexPatternCreator
	{
		[Tooltip("The format of the regex pattern used to determine how to format texts, add '{0}', '{1}', etc to indicate symbol insertion. Example: \"[{0}][{1}]\"")]
		public string delimiterRegexPattern = "";

		public string CreateRegexPattern(params string[] symbols)
		{
			string[] allSymbolsForRegex = GetAllSymbolsForRegex(symbols);
			string text = string.Empty;
			string text2 = string.Empty;
			for (int i = 0; i < allSymbolsForRegex.Length; i++)
			{
				text = text + "\\" + GetStartDelimiter(allSymbolsForRegex[i]);
				text2 = text2 + "\\" + GetEndDelimiter(allSymbolsForRegex[i]);
			}
			return string.Format(delimiterRegexPattern, text, text2);
		}

		private string[] GetAllSymbolsForRegex(string[] symbols)
		{
			List<string> list = new List<string>(symbols);
			string[] allTextMarkupMarkers = GetAllTextMarkupMarkers();
			list.AddRange(allTextMarkupMarkers);
			return list.ToArray();
		}

		private string GetStartDelimiter(string delimiters)
		{
			return delimiters.Substring(0, 1);
		}

		private string GetEndDelimiter(string delimiters)
		{
			return delimiters.Substring(1, 1);
		}
	}
}
