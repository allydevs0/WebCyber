using DG.Tweening;
using UnityEngine;

namespace Reptile.Phone
{
	public class CameraButton : MonoBehaviour
	{
		[SerializeField]
		private Vector3 m_HoldScale = Vector3.one * 0.7f;

		[SerializeField]
		private float m_HoldDuraion = 0.5f;

		[SerializeField]
		private Ease m_HoldEase = Ease.OutExpo;

		[SerializeField]
		private Vector3 m_ReleaseScale = Vector3.one;

		[SerializeField]
		private float m_ReleaseDuraion = 0.2f;

		[SerializeField]
		private Ease m_ReleaseEase = Ease.OutExpo;

		[SerializeField]
		private RectTransform m_InnerPart;

		public void Hold()
		{
			m_InnerPart.DOComplete();
			m_InnerPart.DOScale(m_HoldScale, m_HoldDuraion).SetEase(m_HoldEase);
		}

		public void Release()
		{
			m_InnerPart.DOComplete();
			m_InnerPart.DOScale(m_ReleaseScale, m_ReleaseDuraion).SetEase(m_ReleaseEase);
		}
	}
}
