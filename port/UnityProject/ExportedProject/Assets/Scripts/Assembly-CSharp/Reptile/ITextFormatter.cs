namespace Reptile
{
	public interface ITextFormatter
	{
		bool HasInsertedText { get; }

		string FormatText(string textToFormat, string textToInsert);
	}
}
