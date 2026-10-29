using UnityEngine;

namespace Reptile
{
	public class TalkIcon : MonoBehaviour
	{
		[SerializeField]
		private RectTransform[] m_Blobs;

		[SerializeField]
		private float m_LoopDuration = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_RelativeBlobDuration = 1f;

		[Space]
		[SerializeField]
		private float m_ScaleSmall = 0.5f;

		[SerializeField]
		private float m_ScaleLarge = 1f;

		private void Update()
		{
			for (int i = 0; i < m_Blobs.Length; i++)
			{
				float num = m_LoopDuration / (float)m_Blobs.Length;
				float num2 = num * m_RelativeBlobDuration;
				float num3 = (Time.time - num * (float)i) % m_LoopDuration;
				float num4 = ((!(num3 < num)) ? (1f - Mathf.Clamp01((num3 - num) / num2)) : Mathf.Clamp01(num3 / num2));
				float num5 = num4 * (m_ScaleLarge - m_ScaleSmall) + m_ScaleSmall;
				m_Blobs[i].localScale = new Vector3(num5, num5, 1f);
			}
		}
	}
}
