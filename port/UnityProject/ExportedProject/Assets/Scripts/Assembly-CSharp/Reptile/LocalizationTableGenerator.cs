using UnityEngine;

namespace Reptile
{
	public class LocalizationTableGenerator
	{
		public LocalizationLookupTable GetLocalizationLookupTable(string localizationFilePath, LocalizationData localizerData, SystemLanguage systemLanguage)
		{
			ILocalizationFileLoader localizationFileLoader = new FodsDocumentLoader(localizationFilePath, localizerData);
			ILocalizationFileReader localizationFileReader = localizationFileLoader.Load();
			LocalizationLookupTable result = GenerateLocalizationLookupTable(localizationFileReader, systemLanguage);
			DisposeLocalizationFileReader(localizationFileReader);
			DisposeLocalizationFileLoader(localizationFileLoader);
			return result;
		}

		private LocalizationLookupTable GenerateLocalizationLookupTable(ILocalizationFileReader localizationReader, SystemLanguage systemLanguage)
		{
			if (localizationReader == null)
			{
				return null;
			}
			return new LocalizationLookupTable(localizationReader.GetAllLocalizationGroups(systemLanguage));
		}

		private void DisposeLocalizationFileReader(ILocalizationFileReader localizationFileReader)
		{
			localizationFileReader?.Dispose();
		}

		private void DisposeLocalizationFileLoader(ILocalizationFileLoader localizationFileLoader)
		{
			localizationFileLoader?.Dispose();
		}
	}
}
