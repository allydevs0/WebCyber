using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class MusicPlayerTrackButton : PhoneScrollUnlockableButton
	{
		[SerializeField]
		private GameObject m_ArrowObject;

		[SerializeField]
		private Image m_ButtonImage;

		[SerializeField]
		private Image m_Disc;

		[SerializeField]
		private TextMeshProUGUI m_TitleLabel;

		[SerializeField]
		private TextMeshProUGUI m_ArtistLabel;

		[SerializeField]
		private Sprite m_DefaultSprite;

		[SerializeField]
		private Sprite m_HighlightSprite;

		[SerializeField]
		private Sprite[] m_DiscSpritesDefault;

		[SerializeField]
		private Sprite[] m_DiscSpritesBlurred;

		[SerializeField]
		private Color m_TitleColorNormal = Color.white;

		[SerializeField]
		private Color m_TitleColorSelected = Color.white;

		[SerializeField]
		private Color m_ArtistColorNormal = Color.white;

		[SerializeField]
		private Color m_ArtistColorSelected = Color.white;

		[SerializeField]
		private float m_DiscSpeed = 100f;

		private AppMusicPlayer m_MusicApp;

		private bool m_TrackIsPlaying;

		public void SetMusicApp(AppMusicPlayer musicApp)
		{
			m_MusicApp = musicApp;
		}

		public override void SetContent(AUnlockable content)
		{
			base.SetContent(content);
			MusicTrack musicTrack = content as MusicTrack;
			m_TitleLabel.text = musicTrack.Title;
			m_ArtistLabel.text = musicTrack.Artist;
			if (!IsMyTrackPlaying())
			{
				OnTrackEnds(instantStop: true);
			}
		}

		protected override void OnSelect(bool skipAnimations = false)
		{
			base.OnSelect();
			m_ButtonImage.sprite = m_HighlightSprite;
			m_TitleLabel.color = m_TitleColorSelected;
			m_ArtistLabel.color = m_ArtistColorSelected;
			m_ArrowObject.SetActive(value: true);
		}

		protected override void OnDeselect(bool skipAnimations = false)
		{
			base.OnDeselect();
			m_ButtonImage.sprite = m_DefaultSprite;
			m_TitleLabel.color = m_TitleColorNormal;
			m_ArtistLabel.color = m_ArtistColorNormal;
			m_ArrowObject.SetActive(value: false);
		}

		protected override void ConstantUpdate()
		{
			if (m_TrackIsPlaying && !IsMyTrackPlaying())
			{
				OnTrackEnds(instantStop: false);
			}
			if (!m_TrackIsPlaying && IsMyTrackPlaying())
			{
				OnTracksPlays();
			}
			DiscUpdate();
		}

		private bool IsMyTrackPlaying()
		{
			MusicTrack currentMusicTrack = m_MusicApp.GetCurrentMusicTrack();
			if (currentMusicTrack != null)
			{
				return currentMusicTrack == base.AssignedContent as MusicTrack;
			}
			return false;
		}

		private void OnTracksPlays()
		{
			m_TrackIsPlaying = true;
		}

		private void OnTrackEnds(bool instantStop)
		{
			m_TrackIsPlaying = false;
			if (instantStop)
			{
				m_Disc.rectTransform.eulerAngles = Vector3.zero;
			}
			else
			{
				m_Disc.rectTransform.DORotate(Vector3.zero, 1f, RotateMode.FastBeyond360).SetEase(Ease.OutExpo);
			}
		}

		private void DiscUpdate()
		{
			if (m_TrackIsPlaying)
			{
				m_Disc.rectTransform.localEulerAngles = new Vector3
				{
					z = m_MusicApp.GameMusicPlayer.CurrentTrackTime * m_DiscSpeed
				};
			}
			if (m_TrackIsPlaying && m_MusicApp.GameMusicPlayer.IsPlaying)
			{
				m_Disc.sprite = m_DiscSpritesBlurred[Convert.ToInt32(base.IsSelected)];
			}
			else
			{
				m_Disc.sprite = m_DiscSpritesDefault[Convert.ToInt32(base.IsSelected)];
			}
		}
	}
}
