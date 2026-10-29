using System.Collections;

namespace Reptile
{
	public class DownloadableContentWindows : ADownloadableContent
	{
		public override IEnumerator CheckDownloadableContentLicenses()
		{
			yield return null;
		}

		public override void ClearDownloadableContent()
		{
		}

		public override bool IsDownloadableContentUnlocked(DownloadableContentItem downloadableContentItem)
		{
			return false;
		}
	}
}
