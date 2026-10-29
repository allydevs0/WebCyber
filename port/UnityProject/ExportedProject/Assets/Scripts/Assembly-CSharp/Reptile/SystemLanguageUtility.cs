using System;
using UnityEngine;

namespace Reptile
{
	public static class SystemLanguageUtility
	{
		public static void FormatLanguageTextValues(string[] languageTextValuesToFormat)
		{
			for (int i = 0; i < languageTextValuesToFormat.Length; i++)
			{
				if (!languageTextValuesToFormat[i].Equals(string.Empty))
				{
					languageTextValuesToFormat[i] = languageTextValuesToFormat[i].ToLower();
					languageTextValuesToFormat[i] = languageTextValuesToFormat[i].FirstCharToUpper();
				}
			}
		}

		public static SystemLanguage GetLanguageFromTextValue(string languageTextValue)
		{
			SystemLanguage result = SystemLanguage.Unknown;
			Enum.TryParse<SystemLanguage>(languageTextValue, out result);
			return result;
		}
	}
}
