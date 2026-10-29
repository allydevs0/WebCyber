using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class MusicPlayerStatusPanel : MonoBehaviour
	{
		private const float TEXT_TRAVEL = 100f;

		private const float TEXT_DURATION = 0.15f;

		private const Ease TEXT_EASEIN = Ease.Linear;

		private const Ease TEXT_EASEOUT = Ease.Linear;

		private const float ARTIST_DELAY = 0.07f;

		private const float ICON_BOUNCE = 0.5f;

		[SerializeField]
		private TextMeshProUGUI m_TitleLabel;

		[SerializeField]
		private TextMeshProUGUI m_ArtistLabel;

		[SerializeField]
		private Image m_ProgressBar;

		[SerializeField]
		private Image m_StatusImage;

		[SerializeField]
		private Sprite[] m_StatusIcons;

		private AppMusicPlayer m_MusicApp;

		private IMusicPlayer m_MusicPlayer;

		private MusicTrack m_CurrentTrack;

		private int m_TrackIndex = -1;

		private int trackInstanceId = -1;

		public void Initalize(AppMusicPlayer musicApp)
		{
			m_MusicApp = musicApp;
			m_MusicPlayer = Core.Instance.AudioManager.MusicPlayer;
			m_TitleLabel.text = "";
			m_ArtistLabel.text = "";
		}

		public void UpdatePanel()
		{
			EventListenerUpdate();
			ProgressbarUpdate();
			StatusIconUpdate();
		}

		private void EventListenerUpdate()
		{
			if (m_MusicApp.GetMusicTrack(m_MusicPlayer.CurrentTrackIndex).GetInstanceID() != trackInstanceId)
			{
				m_TrackIndex = m_MusicPlayer.CurrentTrackIndex;
				trackInstanceId = m_MusicApp.GetMusicTrack(m_TrackIndex).GetInstanceID();
				OnNewTrack(m_MusicApp.GetMusicTrack(m_TrackIndex));
			}
		}

		private void ProgressbarUpdate()
		{
			m_ProgressBar.fillAmount = m_MusicPlayer.CurrentTrackTime / m_MusicPlayer.CurrentTrackMaxTime;
		}

		private void StatusIconUpdate()
		{
			if (m_MusicPlayer.IsPlaying)
			{
				m_StatusImage.sprite = m_StatusIcons[0];
				m_StatusImage.rectTransform.localScale = Vector3.one * (1f + m_MusicApp.AudioVisualizer.AudioBandsBuffer[0] * 0.5f - 0.25f);
			}
			else
			{
				m_StatusImage.sprite = m_StatusIcons[1];
				m_StatusImage.rectTransform.localScale = Vector3.Lerp(m_StatusImage.rectTransform.localScale, Vector3.one, 15f * Time.deltaTime);
			}
		}

		private void OnNewTrack(MusicTrack newTrack)
		{
			if (m_CurrentTrack == null || newTrack.Title != m_CurrentTrack.Title)
			{
				Sequence sequence = DOTween.Sequence();
				sequence.Append(m_TitleLabel.rectTransform.DOAnchorPosX(100f, 0.075f).SetEase(Ease.Linear));
				sequence.Join(m_TitleLabel.DOFade(0f, 0.075f).SetEase(Ease.Linear));
				sequence.AppendCallback(() =>
				{
					m_TitleLabel.rectTransform.anchoredPosition = new Vector2(-100f, m_TitleLabel.rectTransform.anchoredPosition.y);
				});
				sequence.AppendCallback(() =>
				{
					m_TitleLabel.text = newTrack.Title;
				});
				sequence.Append(m_TitleLabel.rectTransform.DOAnchorPosX(-780f, 0.075f).SetEase(Ease.Linear));
				sequence.Join(m_TitleLabel.DOFade(1f, 0.075f).SetEase(Ease.Linear));
				sequence.Play();
			}
			if (m_CurrentTrack == null || newTrack.Artist != m_CurrentTrack.Artist)
			{
				Sequence sequence2 = DOTween.Sequence();
				sequence2.Append(m_ArtistLabel.rectTransform.DOAnchorPosX(100f, 0.075f).SetEase(Ease.Linear));
				sequence2.Join(m_ArtistLabel.DOFade(0f, 0.075f).SetEase(Ease.Linear));
				sequence2.AppendCallback(() =>
				{
					m_ArtistLabel.rectTransform.anchoredPosition = new Vector2(-100f, m_ArtistLabel.rectTransform.anchoredPosition.y);
				});
				sequence2.AppendCallback(() =>
				{
					m_ArtistLabel.text = newTrack.Artist;
				});
				sequence2.Append(m_ArtistLabel.rectTransform.DOAnchorPosX(-795f, 0.075f).SetEase(Ease.Linear));
				sequence2.Join(m_ArtistLabel.DOFade(1f, 0.075f).SetEase(Ease.Linear));
				sequence2.Play().SetDelay(0.07f);
			}
			m_CurrentTrack = newTrack;
		}
	}
}
