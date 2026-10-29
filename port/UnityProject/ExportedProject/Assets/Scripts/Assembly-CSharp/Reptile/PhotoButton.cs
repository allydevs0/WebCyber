using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace Reptile
{
	public class PhotoButton : Button
	{
		[SerializeField]
		internal Image thumbnailImage;

		[SerializeField]
		private Image loadIcon;

		[SerializeField]
		private Image selectionImage;

		internal int index;

		private PhotosAlbumMenu menu;

		private PhonePictureData pictureData;

		internal void HideSelectionImage()
		{
			selectionImage.enabled = false;
		}

		internal void ShowSelectionImage()
		{
			selectionImage.enabled = true;
		}

		internal void Init(int buttonIndex, PhotosAlbumMenu menu)
		{
			this.menu = menu;
			index = buttonIndex;
			ClearPhoto();
			HideLoadingIcon();
			HideSelectionImage();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			pictureData = null;
		}

		protected override void OnDestroy()
		{
			if (thumbnailImage.sprite != null)
			{
				Object.Destroy(thumbnailImage.sprite);
				thumbnailImage.sprite = null;
			}
		}

		private bool IsSelectable()
		{
			if (base.interactable)
			{
				return menu.PhotoButtonsInteractable();
			}
			return false;
		}

		public override void OnMove(AxisEventData eventData)
		{
			if (IsSelectable())
			{
				base.OnMove(eventData);
				menu.OnUIMove(index, eventData.moveDir);
			}
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (IsSelectable())
			{
				base.OnPointerEnter(eventData);
				menu.OnUISelect(index);
				EventSystem.current.SetSelectedGameObject(base.gameObject);
			}
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			if (IsSelectable())
			{
				base.OnPointerClick(eventData);
				menu.ShowPhotoFullscreen();
			}
		}

		public override void OnSelect(BaseEventData eventData)
		{
			if (IsSelectable())
			{
				base.OnSelect(eventData);
				menu.OnUISelect(index);
			}
		}

		internal bool HasPhotoData()
		{
			return pictureData != null;
		}

		internal PhonePictureData MarkPhotoForDeletion()
		{
			PhonePictureData result = pictureData;
			ClearPhoto();
			ShowLoadingIcon();
			return result;
		}

		internal void SetPhoto(PhotoButton sourceButton)
		{
			SetPhoto(sourceButton.pictureData);
		}

		internal void SetPhoto(PhonePictureData pictureData)
		{
			this.pictureData = pictureData;
			if (thumbnailImage.sprite == null)
			{
				Vector2Int vector2Int = new Vector2Int(540, 720);
				if (PlayerPhoneCameras.Instance != null)
				{
					vector2Int = PlayerPhoneCameras.Instance.TextureSize;
				}
				Texture2D texture = new Texture2D(vector2Int.x, vector2Int.y, DefaultFormat.LDR, TextureCreationFlags.Crunch);
				Rect rect = new Rect(0f, 0f, vector2Int.x, vector2Int.y);
				Vector2 pivot = 0.5f * Vector2.one;
				float pixelsPerUnit = 100f;
				uint extrude = 0u;
				Vector4 zero = Vector4.zero;
				thumbnailImage.sprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit, extrude, SpriteMeshType.FullRect, zero, generateFallbackPhysicsShape: false);
			}
			if (pictureData != null)
			{
				thumbnailImage.sprite.texture.LoadImage(pictureData.GetPictureData());
				thumbnailImage.enabled = true;
				loadIcon.enabled = false;
			}
		}

		internal void ClearPhoto()
		{
			pictureData = null;
			thumbnailImage.enabled = false;
		}

		internal void ShowLoadingIcon()
		{
			thumbnailImage.enabled = false;
			loadIcon.enabled = true;
		}

		internal void HideLoadingIcon()
		{
			loadIcon.enabled = false;
		}

		internal Sprite GetPhoto()
		{
			return thumbnailImage.sprite;
		}
	}
}
