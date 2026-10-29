using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Reptile
{
	public static class StringExtensions
	{
		public static Color32 ToRGB32Color(this string hex)
		{
			byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
			return new Color32(r, g, b, byte.MaxValue);
		}

		public static string RemoveDiacritics(this string text)
		{
			string text2 = text.Normalize(NormalizationForm.FormD);
			StringBuilder stringBuilder = new StringBuilder();
			string text3 = text2;
			foreach (char c in text3)
			{
				if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
		}

		public static string FirstCharToUpper(this string input)
		{
			if (input != null)
			{
				if (input == "")
				{
					throw new ArgumentException("input cannot be empty", "input");
				}
				return input[0].ToString().ToUpper() + input.Substring(1);
			}
			throw new ArgumentNullException("input");
		}
	}
}
