namespace Reptile
{
	public class TextMeshProBoldFormatter : ATextMeshProFormatter
	{
		private const string TEXT_MESH_PRO_BOLD_FORMAT = "<b>{0}</b>";

		protected override string CreateTag(string textToFormat)
		{
			return CreateBoldTag(textToFormat);
		}

		private string CreateBoldTag(string textToHighlight)
		{
			return $"<b>{textToHighlight}</b>";
		}
	}
}
