using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class GraffitiScrollButtonContainer : MonoBehaviour
	{
		[SerializeField]
		private Sprite[] m_ContainerSprites;

		[SerializeField]
		private Image m_ContainerImage;

		[SerializeField]
		private Image m_DimOverlay;

		[SerializeField]
		private RawImage m_RawImage;

		[SerializeField]
		private AspectRatioFitter m_AspectRatioFitter;

		public void SetImage(Texture texture)
		{
			m_RawImage.texture = texture;
			m_AspectRatioFitter.aspectRatio = (float)texture.width / (float)texture.height;
		}

		public void SetSelected(bool isSelected)
		{
			m_ContainerImage.sprite = m_ContainerSprites[Convert.ToInt32(isSelected)];
		}

		public void SetDimOpacity(float alpha, float fadeDuration = 0f)
		{
			m_DimOverlay.DOFade(alpha, fadeDuration).SetEase(Ease.Linear);
		}
	}
}
