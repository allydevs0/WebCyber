using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class Pictureframe : MonoBehaviour
	{
		[SerializeField]
		private RawImage m_Picture;

		public void SetPicture(Texture2D picture)
		{
			m_Picture.texture = picture;
		}

		public void StartAnimation()
		{
			Mathf.Sign(Random.Range(-4, 4));
			Sequence sequence = DOTween.Sequence();
			sequence.Append(base.transform.DOScale(0.65f, 0f).SetEase(Ease.OutQuad));
			sequence.AppendInterval(0.6f);
			sequence.Append((base.transform as RectTransform).DOAnchorPosY(1600f, 0.7f).SetEase(Ease.InOutBack));
			sequence.OnComplete(() =>
			{
				Object.Destroy(base.gameObject);
			});
			sequence.Play();
		}
	}
}
