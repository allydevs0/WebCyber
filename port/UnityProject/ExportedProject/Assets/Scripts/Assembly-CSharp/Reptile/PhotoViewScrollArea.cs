using UnityEngine;
using UnityEngine.EventSystems;

namespace Reptile
{
	public class PhotoViewScrollArea : MonoBehaviour, IScrollHandler, IEventSystemHandler
	{
		[SerializeField]
		internal PhotosAlbumMenu menu;

		public void OnScroll(PointerEventData eventData)
		{
			menu.OnPhotoViewScroll(eventData.scrollDelta.y);
		}
	}
}
