using System;
using TMPro;
using UnityEngine;

namespace Reptile.Phone
{
	public class UnreadIndicator : MonoBehaviour
	{
		private const float SPEED = 5f;

		private const float MAX_DISTANCE = 0.2f;

		private const float CHAR_OFFSET = 0.1f;

		private TextMeshProUGUI m_Label;

		private TMProLocalizationAddOn localizationAddon;

		private string m_RawString;

		private float m_TimeOffset;

		private bool m_IsEnabled;

		public void SetEnabled(bool isEnabled)
		{
			m_IsEnabled = isEnabled;
			base.gameObject.SetActive(isEnabled);
		}

		private void Awake()
		{
			m_Label = GetComponentInChildren<TextMeshProUGUI>();
			localizationAddon = GetComponentInChildren<TMProLocalizationAddOn>();
			m_RawString = m_Label.text;
			m_TimeOffset = UnityEngine.Random.Range(0f, MathF.PI);
		}

		private void OnEnable()
		{
			localizationAddon.OnTextLocalizationChanged += OnTextLocalizationChanged;
		}

		private void OnDisable()
		{
			localizationAddon.OnTextLocalizationChanged -= OnTextLocalizationChanged;
		}

		private void Update()
		{
			if (m_IsEnabled)
			{
				string text = "";
				for (int i = 0; i < m_RawString.Length; i++)
				{
					float num = Mathf.Abs(Mathf.Sin((Time.time + m_TimeOffset - 0.1f * (float)i) * 5f)) * 0.2f;
					text += $"<voffset={num}em>{m_RawString[i]}";
				}
				text += "</voffset>";
				m_Label.text = text;
			}
		}

		private void OnTextLocalizationChanged(SystemLanguage language, string localizedText)
		{
			m_RawString = m_Label.text;
		}
	}
}
