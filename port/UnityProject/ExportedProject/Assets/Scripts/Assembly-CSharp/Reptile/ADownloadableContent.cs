using System.Collections;

namespace Reptile
{
	public abstract class ADownloadableContent
	{
		protected DownloadableContentLibrary downloadableContentLibrary;

		public void Init(DownloadableContentLibrary downloadableContentLibrary)
		{
			this.downloadableContentLibrary = downloadableContentLibrary;
		}

		public abstract bool IsDownloadableContentUnlocked(DownloadableContentItem downloadableContentItem);

		public abstract void ClearDownloadableContent();

		public abstract IEnumerator CheckDownloadableContentLicenses();
	}
}
