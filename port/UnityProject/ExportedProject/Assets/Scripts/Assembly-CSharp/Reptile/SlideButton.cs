using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reptile
{
	public class SlideButton : Button
	{
		public Image ButtonNormal;

		public Image ButtonHighlight;

		public Image Slider;

		public TextMeshProUGUI Label;

		public Sprite ButtonSpriteNormal;

		public Sprite ButtonSpriteHighlight;

		public Sprite SliderSpriteNormal;

		public Sprite SliderSpriteHighlight;

		public Color TextColorNormal;

		public Color TextColorHighlight;

		public float SlideDuration;

		public Ease SlideEase;

		public float SliderOffset;

		public float SlideDistance;

		private Vector2 m_StartPos;

		protected override void Awake()
		{
			Slider = base.transform.Find("Slider").GetComponent<Image>();
			Label = base.transform.GetComponentInChildren<TextMeshProUGUI>();
			base.navigation = base.navigation;
			SliderSpriteNormal = Slider.sprite;
			TextColorNormal = Label.color;
			m_StartPos = (base.transform as RectTransform).anchoredPosition;
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			Select();
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);
		}

		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			OnSelectTransition();
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			base.OnDeselect(eventData);
			OnDeselectTransition();
		}

		private void OnSelectTransition()
		{
			ButtonNormal.fillOrigin = 1;
			ButtonHighlight.fillOrigin = 0;
			Sequence s = DOTween.Sequence();
			float endValue = (base.transform as RectTransform).sizeDelta.x / 2f - (Slider.rectTransform.sizeDelta.x / 2f + SliderOffset);
			s.AppendCallback(() =>
			{
				Slider.sprite = SliderSpriteHighlight;
			});
			s.Append(Slider.rectTransform.DOAnchorPosX(endValue, SlideDuration).SetEase(SlideEase));
			s.Join(Label.DOColor(TextColorHighlight, SlideDuration).SetEase(SlideEase));
			s.Join((base.transform as RectTransform).DOAnchorPosX(m_StartPos.x + SlideDistance, SlideDuration).SetEase(SlideEase));
			s.Join(ButtonNormal.DOFillAmount(0f, SlideDuration).SetEase(SlideEase));
			s.Join(ButtonHighlight.DOFillAmount(1f, SlideDuration).SetEase(SlideEase));
		}

		private void OnDeselectTransition()
		{
			ButtonNormal.fillOrigin = 1;
			ButtonHighlight.fillOrigin = 0;
			Sequence s = DOTween.Sequence();
			float endValue = (0f - (base.transform as RectTransform).sizeDelta.x) / 2f + (Slider.rectTransform.sizeDelta.x / 2f + SliderOffset);
			s.AppendCallback(() =>
			{
				Slider.sprite = SliderSpriteNormal;
			});
			s.Append(Slider.rectTransform.DOAnchorPosX(endValue, SlideDuration).SetEase(SlideEase));
			s.Join(Label.DOColor(TextColorNormal, SlideDuration).SetEase(SlideEase));
			s.Join((base.transform as RectTransform).DOAnchorPosX(m_StartPos.x, SlideDuration).SetEase(SlideEase));
			s.Join(ButtonNormal.DOFillAmount(1f, SlideDuration).SetEase(SlideEase));
			s.Join(ButtonHighlight.DOFillAmount(0f, SlideDuration).SetEase(SlideEase));
		}
	}
}
