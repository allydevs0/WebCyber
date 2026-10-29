namespace Reptile
{
	public abstract class ATextMeshProFormatter : ITextFormatter
	{
		protected bool hasInsertedText;

		public bool HasInsertedText => hasInsertedText;

		public string FormatText(string textToFormat, string textToInsert)
		{
			textToFormat = CreateTag(textToFormat);
			return textToFormat;
		}

		protected abstract string CreateTag(string textToFormat);
	}
}
