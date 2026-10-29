using System;
using UnityEngine;

namespace Reptile
{
	public interface ILocalizationFileReader : IDisposable
	{
		LocalizationGroup[] GetAllLocalizationGroups(SystemLanguage language);
	}
}
