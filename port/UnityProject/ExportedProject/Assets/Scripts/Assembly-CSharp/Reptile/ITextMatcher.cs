namespace Reptile
{
	public interface ITextMatcher
	{
		bool IsMatch(string text);

		string GetExactMatch(string text);

		ITextFormatter GetFormatter();
	}
}
