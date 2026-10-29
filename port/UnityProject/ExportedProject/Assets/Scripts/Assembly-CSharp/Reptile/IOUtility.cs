using System;
using System.IO;

namespace Reptile
{
	public static class IOUtility
	{
		public static string GetRootDirectoryRelative(string directory)
		{
			string[] array = directory.Split(new char[2]
			{
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar
			}, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 0)
			{
				return array[0];
			}
			return string.Empty;
		}

		public static string GenerateUniqueNewFilePath(string currentFilePath, bool fileExists)
		{
			int num = 0;
			string text = currentFilePath;
			string directoryName = Path.GetDirectoryName(currentFilePath);
			string text2 = Path.GetFileNameWithoutExtension(currentFilePath);
			string extension = Path.GetExtension(currentFilePath);
			while (fileExists && num < 1000)
			{
				text2 += num;
				text = Path.Combine(directoryName, text2 + extension);
				fileExists = File.Exists(text);
			}
			return text;
		}
	}
}
