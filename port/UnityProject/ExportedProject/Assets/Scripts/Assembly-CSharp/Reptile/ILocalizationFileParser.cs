using System;
using UnityEngine;

namespace Reptile
{
	public interface ILocalizationFileParser : IDisposable
	{
		int AmountOfLocalizationKeys { get; }

		string Name { get; }

		LocalizationKeyValuePair GetKeyValueForLanguage(int keyIndex, SystemLanguage systemLanguage);
	}
}
