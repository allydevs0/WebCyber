namespace Reptile
{
	public class TextMeshProNormalFormatter : ATextMeshProFormatter
	{
		protected override string CreateTag(string textToFormat)
		{
			return textToFormat;
		}
	}
}
