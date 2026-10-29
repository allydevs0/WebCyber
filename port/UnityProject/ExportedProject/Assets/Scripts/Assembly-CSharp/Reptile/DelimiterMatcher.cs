namespace Reptile
{
	public class DelimiterMatcher : ITextMatcher
	{
		private ITextFormatter textFormatter;

		private string startDelimiter = string.Empty;

		private string endDelimiter = string.Empty;

		public DelimiterMatcher(ITextFormatter textFormatter, string delimiters)
		{
			this.textFormatter = textFormatter;
			startDelimiter = GetStartDelimiter(delimiters);
			endDelimiter = GetEndDelimiter(delimiters);
		}

		private string GetStartDelimiter(string delimiters)
		{
			return delimiters.Substring(0, 1);
		}

		private string GetEndDelimiter(string delimiters)
		{
			return delimiters.Substring(1, 1);
		}

		public string GetExactMatch(string text)
		{
			return text.Substring(1, text.Length - 2);
		}

		public ITextFormatter GetFormatter()
		{
			return textFormatter;
		}

		public bool IsMatch(string text)
		{
			if (text.IndexOf(startDelimiter) == 0)
			{
				return text.LastIndexOf(endDelimiter) == text.Length - 1;
			}
			return false;
		}
	}
}
