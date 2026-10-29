using System;
using UnityEngine;

namespace Reptile.Phone
{
	public class MusicPlayerVisualizer : MonoBehaviour
	{
		[Header("Components")]
		[SerializeField]
		private AudioAnalyzer m_AudioAnalyzer;

		[SerializeField]
		private GameObject m_BarPrefab;

		[SerializeField]
		private GameObject m_ConnectorPrefab;

		[SerializeField]
		private RectTransform m_BarContainter;

		[SerializeField]
		private RectTransform m_ConnectorContainter;

		private const int BarAmount = 8;

		private const int ConnectorAmount = 7;

		private const float MaxBarHeight = 700f;

		private RectTransform[] m_Bars;

		private RectTransform[] m_Connectors;

		private void Awake()
		{
			Core.OnUpdate += UpdateVisuals;
			Initialze();
		}

		private void OnDestroy()
		{
			Core.OnUpdate -= UpdateVisuals;
		}

		private void Initialze()
		{
			m_Bars = new RectTransform[8];
			for (int i = 0; i < 8; i++)
			{
				m_Bars[i] = UnityEngine.Object.Instantiate(m_BarPrefab, m_BarContainter).transform as RectTransform;
				m_Bars[i].anchoredPosition = new Vector2
				{
					x = m_BarContainter.sizeDelta.x / 8f * ((float)i - 4f + 0.5f)
				};
			}
			m_Connectors = new RectTransform[7];
			for (int j = 0; j < 7; j++)
			{
				m_Connectors[j] = UnityEngine.Object.Instantiate(m_ConnectorPrefab, m_ConnectorContainter).transform as RectTransform;
			}
		}

		private void UpdateVisuals()
		{
			for (int i = 0; i < 8; i++)
			{
				m_Bars[i].sizeDelta = new Vector2(m_Bars[i].sizeDelta.x, m_AudioAnalyzer.AudioBandsBuffer[i] * 700f);
			}
			for (int j = 0; j < 7; j++)
			{
				Vector2 a = m_Bars[j].anchoredPosition + new Vector2(0f, m_Bars[j].sizeDelta.y);
				Vector2 b = m_Bars[j + 1].anchoredPosition + new Vector2(0f, m_Bars[j + 1].sizeDelta.y);
				m_Connectors[j].anchoredPosition = Vector2.Lerp(a, b, 0.5f);
				m_Connectors[j].sizeDelta = new Vector2(Vector2.Distance(a, b), m_Connectors[j].sizeDelta.y);
				float x = b.x - a.x;
				float z = Mathf.Atan2(b.y - a.y, x) * 180f / MathF.PI;
				m_Connectors[j].localEulerAngles = new Vector3(m_Connectors[j].localEulerAngles.x, m_Connectors[j].localEulerAngles.y, z);
			}
		}
	}
}
