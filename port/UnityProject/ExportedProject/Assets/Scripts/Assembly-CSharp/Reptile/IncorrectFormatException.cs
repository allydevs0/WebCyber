using System;

namespace Reptile
{
	public class IncorrectFormatException : Exception
	{
		private string fileName = string.Empty;

		private DataSerializerFactory.FormatType foundFormat;

		public IncorrectFormatException(string fileName, DataSerializerFactory.FormatType foundFormat)
			: base("Could not read file: " + fileName + ", unexpected format encountered: " + foundFormat)
		{
		}

		public override string ToString()
		{
			return "Could not read file: " + fileName + ", unexpected format encountered: " + foundFormat.ToString() + "\n" + StackTrace.ToString();
		}
	}
}
