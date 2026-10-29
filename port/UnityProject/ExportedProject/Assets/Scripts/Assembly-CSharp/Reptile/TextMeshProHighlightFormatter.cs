namespace Reptile
{
	public class TextMeshProHighlightFormatter : ATextMeshProFormatter
	{
		private const string TEXT_MESH_PRO_HIGHLIGHT_FORMAT = "<color=\"yellow\">{0}</color>";

		protected override string CreateTag(string textToFormat)
		{
			return CreateHighlightTag(textToFormat);
		}

		private string CreateHighlightTag(string textToHighlight)
		{
			return $"<color=\"yellow\">{textToHighlight}</color>";
		}
	}
}
