using UnityEngine;

namespace Reptile.Phone
{
	public class AppPhotos : App
	{
		[SerializeField]
		private PhotosAlbumMenu albumMenu;

		public PhotosManager PhotosManager { get; private set; }

		protected override void OnAppInit()
		{
			PhotosManager = new PhotosManager();
			Core instance = Core.Instance;
			PhotosManager.Init(instance.Platform.Storage, instance.Platform.ErrorHandler, instance.UIManager, instance.SaveManager, instance.Localizer);
			albumMenu.Init(PhotosManager);
		}

		public override void OnAppEnable()
		{
			if (!Core.Instance.UIManager.IsTopMenu(albumMenu))
			{
				albumMenu.Enable(OnAlbumMenuDisable);
			}
		}

		public void OnAlbumMenuDisable()
		{
			base.MyPhone.CloseCurrentApp();
		}

		protected override void OnAppTerminate()
		{
			albumMenu.Terminate();
			PhotosManager.Terminate();
			PhotosManager = null;
		}
	}
}
