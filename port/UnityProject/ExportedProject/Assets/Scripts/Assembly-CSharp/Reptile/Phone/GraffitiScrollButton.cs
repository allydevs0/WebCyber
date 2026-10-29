using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class GraffitiScrollButton : PhoneScrollUnlockableButton
	{
		[SerializeField]
		private Sprite[] m_BaseSprites;

		[SerializeField]
		private Color[] m_TextColors;

		[SerializeField]
		private Image m_BaseImage;

		[SerializeField]
		private Image m_DimOverlay;

		[SerializeField]
		private RawImage m_GraffitiImage;

		[SerializeField]
		private AspectRatioFitter m_AspectRatioFitter;

		[SerializeField]
		private TextMeshProUGUI m_TitleLabel;

		[SerializeField]
		private TextMeshProUGUI m_SizeLabel;

		protected override void OnCreate()
		{
			m_BaseImage.sprite = m_BaseSprites[0];
			m_TitleLabel.color = m_TextColors[0];
			m_SizeLabel.color = m_TextColors[1];
		}

		public override void SetContent(AUnlockable content)
		{
			base.SetContent(content);
			GraffitiAppEntry graffitiAppEntry = base.AssignedContent as GraffitiAppEntry;
			m_TitleLabel.text = graffitiAppEntry.Title;
			m_SizeLabel.text = graffitiAppEntry.Size.ToString();
			switch (graffitiAppEntry.Size)
			{
			case GraffitiSize.M:
				m_GraffitiImage.rectTransform.sizeDelta = new Vector2(256f, 256f);
				break;
			case GraffitiSize.L:
				m_GraffitiImage.rectTransform.sizeDelta = new Vector2(256f, 256f);
				break;
			case GraffitiSize.XL:
				m_GraffitiImage.rectTransform.sizeDelta = new Vector2(256f, 175f);
				break;
			}
			Texture graffitiTexture = (base.AssignedContent as GraffitiAppEntry).GraffitiTexture;
			m_GraffitiImage.texture = graffitiTexture;
			m_AspectRatioFitter.aspectRatio = (float)graffitiTexture.width / (float)graffitiTexture.height;
		}

		protected override void OnSelect(bool skipAnimations = false)
		{
			m_BaseImage.sprite = m_BaseSprites[1];
			m_TitleLabel.color = m_TextColors[1];
			m_SizeLabel.color = m_TextColors[0];
		}

		protected override void OnDeselect(bool skipAnimations = false)
		{
			m_BaseImage.sprite = m_BaseSprites[0];
			m_TitleLabel.color = m_TextColors[0];
			m_SizeLabel.color = m_TextColors[1];
		}

		public void SetDimOpacity(float alpha, float fadeDuration = 0f)
		{
			m_DimOverlay.DOFade(alpha, fadeDuration).SetEase(Ease.Linear);
		}
	}
}
