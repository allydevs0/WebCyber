using System;
using System.IO;
using System.Xml;

namespace Reptile
{
	public class FodsDocumentLoader : ILocalizationFileLoader
	{
		private const string FILE_COPY_NAME_AFFIX = "_copy";

		private string filePath = string.Empty;

		private bool hasFileCopy;

		private LocalizationData localizerData;

		public FodsDocumentLoader(string filePath, LocalizationData localizerData)
		{
			this.filePath = filePath;
			this.localizerData = localizerData;
		}

		public void Dispose()
		{
			if (hasFileCopy)
			{
				DeleteFileCopy();
			}
			localizerData = null;
		}

		private void DeleteFileCopy()
		{
			File.Delete(CreateCopyFileDestination(filePath));
		}

		public ILocalizationFileReader Load()
		{
			XmlDocument xmlDocument = new XmlDocument();
			bool num = CheckFilePath();
			bool flag = false;
			if (num)
			{
				flag = LoadXmlFromFile(xmlDocument);
			}
			ILocalizationFileReader result = null;
			if (flag)
			{
				result = new FodsLocalizationFileReader(xmlDocument, localizerData);
			}
			return result;
		}

		private bool CheckFilePath()
		{
			return File.Exists(filePath);
		}

		private bool LoadXmlFromFile(XmlDocument xmlDocument)
		{
			bool result = true;
			MemoryStream inStream = LoadXmlMemoryStream(filePath);
			try
			{
				xmlDocument.Load(inStream);
			}
			catch (Exception exception)
			{
				result = false;
				ShowException(exception);
			}
			return result;
		}

		private void ShowException(Exception exception)
		{
		}

		private MemoryStream LoadXmlMemoryStream(string filePath)
		{
			byte[] buffer = new byte[0];
			try
			{
				buffer = File.ReadAllBytes(filePath);
			}
			catch (Exception ex)
			{
				if (ex is IOException)
				{
					buffer = OpenCopyOfLocalizationFile(filePath);
				}
			}
			return new MemoryStream(buffer);
		}

		private byte[] OpenCopyOfLocalizationFile(string filePathToCopyFrom)
		{
			string text = CreateCopyFileDestination(filePathToCopyFrom);
			File.Copy(filePathToCopyFrom, text, overwrite: true);
			byte[] result = new byte[0];
			try
			{
				hasFileCopy = true;
				result = File.ReadAllBytes(text);
			}
			catch (Exception)
			{
			}
			return result;
		}

		private string CreateCopyFileDestination(string filePath)
		{
			int num = filePath.IndexOf('.');
			if (num < 0)
			{
				num = 0;
			}
			return filePath.Insert(num, "_copy");
		}
	}
}
