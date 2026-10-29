using System.Text.RegularExpressions;

namespace Reptile
{
	public class RegexGameTextFormatter : AGameTextFormatter
	{
		private string regexPattern = string.Empty;

		private string[] textsToInsert;

		private int textsInserted;

		private Regex regex;

		private MatchEvaluator matchEvaluator;

		public RegexGameTextFormatter(FormatterData formatterData, ITextMatcher[] matchers)
			: base(formatterData, matchers)
		{
			regexPattern = CreateRegexPattern();
			regex = new Regex(regexPattern);
			matchEvaluator = EvaluateRegexMatches;
		}

		private string CreateRegexPattern()
		{
			if (formatterData is IRegexPatternCreator)
			{
				return (formatterData as IRegexPatternCreator).CreateRegexPattern();
			}
			HandleNoRegexFormatterFound();
			return string.Empty;
		}

		private void HandleNoRegexFormatterFound()
		{
		}

		public override string FormatGameText(string textToFormat, string[] textsToInsert)
		{
			SetTextsToInsert(textsToInsert);
			string result = regex.Replace(textToFormat, matchEvaluator);
			ResetTextsToInsert();
			return result;
		}

		private string EvaluateRegexMatches(Match match)
		{
			return FormatMatchesInText(match.Value);
		}

		protected override string ReplaceMarkerInMatch(string matchFound)
		{
			if (textsInserted >= textsToInsert.Length)
			{
				return matchFound;
			}
			string result = matchFound.Replace(replacementMarker, textsToInsert[textsInserted]);
			textsInserted++;
			return result;
		}

		private void SetTextsToInsert(string[] textsToInsert)
		{
			textsInserted = 0;
			this.textsToInsert = textsToInsert;
		}

		private void ResetTextsToInsert()
		{
			textsToInsert = null;
		}
	}
}
