using System.Globalization;

namespace Reptile
{
	public static class FormattingUtility
	{
		public static string FormatPlayerScore(CultureInfo cultureInfo, float score)
		{
			return string.Format(cultureInfo, "{0:#,#,#,#}", score);
		}
	}
}
