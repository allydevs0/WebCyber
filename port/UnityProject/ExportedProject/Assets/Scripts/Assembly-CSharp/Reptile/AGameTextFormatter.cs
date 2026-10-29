using System;

namespace Reptile
{
	public abstract class AGameTextFormatter : IGameTextFormatter, IDisposable
	{
		protected ITextMatcher[] matchers;

		protected string replacementMarker = string.Empty;

		protected FormatterData formatterData;

		public AGameTextFormatter(FormatterData formatterData, ITextMatcher[] matchers)
		{
			this.matchers = matchers;
			replacementMarker = formatterData.replacementMarker;
			this.formatterData = formatterData;
		}

		public void Dispose()
		{
			matchers = null;
			formatterData = null;
		}

		protected string FormatMatchesInText(string match)
		{
			for (int i = 0; i < matchers.Length; i++)
			{
				if (matchers[i].IsMatch(match))
				{
					return FormatMatch(matchers[i], match);
				}
			}
			return string.Empty;
		}

		private string FormatMatch(ITextMatcher matcher, string match)
		{
			if (match.Contains(replacementMarker))
			{
				return FormatReplacement(matcher, match);
			}
			return FormatAsExactMatch(matcher, match);
		}

		private string FormatReplacement(ITextMatcher matcher, string matchWithMarker)
		{
			string match = ReplaceMarkerInMatch(matchWithMarker);
			return FormatAsExactMatch(matcher, match);
		}

		private string FormatAsExactMatch(ITextMatcher matcher, string match)
		{
			string exactMatch = matcher.GetExactMatch(match);
			ITextFormatter formatter = matcher.GetFormatter();
			return FormatText(formatter, exactMatch);
		}

		private string FormatText(ITextFormatter formatter, string matchFound)
		{
			return formatter.FormatText(matchFound, string.Empty);
		}

		public abstract string FormatGameText(string textToFormat, string[] textsToInsert);

		protected abstract string ReplaceMarkerInMatch(string textMatchWithMarker);
	}
}
