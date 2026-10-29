using UnityEngine;

namespace Reptile
{
	public class AudioAnalyzer : MonoBehaviour
	{
		[SerializeField]
		private AudioSource _audioSource;

		private bool _isInitalized;

		private float[] m_Samples = new float[512];

		private float[] m__FreqBand = new float[8];

		private float[] m_BandBuffer = new float[8];

		private float[] m_BufferDecrease = new float[8];

		private float[] m_FreqBandHighest = new float[8];

		[SerializeField]
		private float m_DecreaseStart = 0.005f;

		[SerializeField]
		private float m_DecreaseMultiplier = 1.2f;

		public float[] AudioBands { get; private set; }

		public float[] AudioBandsBuffer { get; private set; }

		public void Initalize()
		{
			if (_audioSource != null)
			{
				Initalize(_audioSource);
			}
		}

		public void Initalize(AudioSource audioSource)
		{
			_audioSource = audioSource;
			AudioBands = new float[8];
			AudioBandsBuffer = new float[8];
			_isInitalized = true;
		}

		private void Update()
		{
			if (_isInitalized && _audioSource != null)
			{
				GetSpectrumAudioSource();
				MakeFrequencyBands();
				BandBuffer();
				CreateAudioBands();
			}
		}

		private void CreateAudioBands()
		{
			for (int i = 0; i < 8; i++)
			{
				if (m__FreqBand[i] > m_FreqBandHighest[i])
				{
					m_FreqBandHighest[i] = m__FreqBand[i];
				}
				if (m_FreqBandHighest[i] != 0f)
				{
					AudioBands[i] = m__FreqBand[i] / m_FreqBandHighest[i];
				}
				if (m_FreqBandHighest[i] != 0f)
				{
					AudioBandsBuffer[i] = m_BandBuffer[i] / m_FreqBandHighest[i];
				}
			}
		}

		private void GetSpectrumAudioSource()
		{
			_audioSource.GetSpectrumData(m_Samples, 0, FFTWindow.Triangle);
		}

		private void BandBuffer()
		{
			for (int i = 0; i < 8; i++)
			{
				if (m__FreqBand[i] > m_BandBuffer[i])
				{
					m_BandBuffer[i] = m__FreqBand[i];
					m_BufferDecrease[i] = m_DecreaseStart;
				}
				if (m__FreqBand[i] < m_BandBuffer[i])
				{
					m_BandBuffer[i] -= m_BufferDecrease[i];
					m_BufferDecrease[i] *= m_DecreaseMultiplier;
				}
			}
		}

		private void MakeFrequencyBands()
		{
			int num = 0;
			for (int i = 0; i < 8; i++)
			{
				float num2 = 0f;
				int num3 = (int)Mathf.Pow(2f, i) * 2;
				if (i == 7)
				{
					num3 += 2;
				}
				for (int j = 0; j < num3; j++)
				{
					num2 += m_Samples[num] * (float)(num + 1);
					num++;
				}
				num2 /= (float)num;
				m__FreqBand[i] = num2 * 10f;
			}
		}
	}
}
