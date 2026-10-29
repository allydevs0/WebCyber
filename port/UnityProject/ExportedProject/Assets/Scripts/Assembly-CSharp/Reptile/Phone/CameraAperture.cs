using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Reptile.Phone
{
	public class CameraAperture : MonoBehaviour
	{
		public const float CLOSED_ANGLE = 1.8f;

		public const float OPEN_ANGLE = 45f;

		private List<RectTransform> m_Slices;

		public float AptertureAngle { get; set; }

		private void Awake()
		{
			AptertureAngle = 45f;
			m_Slices = GetComponentsInChildren<RectTransform>().ToList();
			m_Slices.Remove(base.transform as RectTransform);
			for (int i = 0; i < m_Slices.Count; i++)
			{
				float f = (360f / (float)m_Slices.Count * (float)(i + 1) + 180f) * (MathF.PI / 180f);
				float num = Mathf.Sqrt(Mathf.Pow(m_Slices[i].sizeDelta.x, 2f) + Mathf.Pow(m_Slices[i].sizeDelta.y, 2f));
				Vector2 anchoredPosition = new Vector2
				{
					x = Mathf.Cos(f) * num / 2f,
					y = Mathf.Sin(f) * num / 2f
				};
				m_Slices[i].anchoredPosition = anchoredPosition;
			}
		}

		private void Update()
		{
			for (int i = 0; i < m_Slices.Count; i++)
			{
				float z = AptertureAngle + 360f / (float)m_Slices.Count * (float)i;
				m_Slices[i].localEulerAngles = new Vector3(m_Slices[i].localEulerAngles.x, m_Slices[i].localEulerAngles.y, z);
			}
		}
	}
}
