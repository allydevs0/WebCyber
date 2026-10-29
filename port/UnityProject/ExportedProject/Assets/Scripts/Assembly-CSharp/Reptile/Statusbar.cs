using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class Statusbar : MonoBehaviour
	{
		private Coroutine updateClockCoroutine;

		[SerializeField]
		private Image m_CellularInner;

		[SerializeField]
		private Image m_BatteryInner;

		[SerializeField]
		private TextMeshProUGUI m_ClockLabel;

		public float CellularAmount
		{
			get
			{
				return m_CellularInner.fillAmount;
			}
			set
			{
				m_CellularInner.fillAmount = value;
			}
		}

		public float BatteryAmount
		{
			get
			{
				return m_BatteryInner.fillAmount;
			}
			set
			{
				m_BatteryInner.fillAmount = value;
			}
		}

		private void OnEnable()
		{
			if (updateClockCoroutine == null)
			{
				updateClockCoroutine = StartCoroutine(UpdateClockRealtime());
			}
		}

		private void OnDisable()
		{
			if (updateClockCoroutine != null)
			{
				StopCoroutine(updateClockCoroutine);
				updateClockCoroutine = null;
			}
		}

		public void SetTime(int hr, int min)
		{
			m_ClockLabel.text = $"{hr}:{min}";
		}

		private IEnumerator UpdateClockRealtime()
		{
			_ = DateTime.Now.Hour;
			_ = DateTime.Now.Minute;
			while (true)
			{
				int currentHour = DateTime.Now.Hour;
				int currentMinute = DateTime.Now.Minute;
				string arg = "FF";
				m_ClockLabel.text = $"{currentHour:00}<alpha=#{arg}>:<alpha=#FF>{currentMinute:00}";
				yield return new WaitForSeconds(0.5f);
				arg = "00";
				m_ClockLabel.text = $"{currentHour:00}<alpha=#{arg}>:<alpha=#FF>{currentMinute:00}";
				yield return new WaitForSeconds(0.5f);
			}
		}
	}
}
