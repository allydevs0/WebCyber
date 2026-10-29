using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class AppMusicPlayer : App
	{
		[SerializeField]
		private MusicPlayerTrackList m_TrackList;

		[Space(5f)]
		[SerializeField]
		private MusicPlayerStatusPanel m_StatusPanel;

		[SerializeField]
		private AudioAnalyzer m_AudioAnalyzer;

		[SerializeField]
		private Sprite shuffleIcon;

		[SerializeField]
		private Sprite noShuffleIcon;

		[SerializeField]
		private Image shuffleIconImageUnselected;

		[SerializeField]
		private Image shuffleIconImageSelected;

		[SerializeField]
		private GameObject unselectedShuffleButton;

		[SerializeField]
		private GameObject selectedShuffleButton;

		private float continuousScrollTimer;

		private const float initialScrollInterval = 0.4f;

		private const float continuousScrollInterval = 0.1f;

		private bool shuffleControlSelected;

		public IMusicPlayer GameMusicPlayer { get; private set; }

		public AudioAnalyzer AudioVisualizer => m_AudioAnalyzer;

		private void Update()
		{
		}

		protected override void OnAppInit()
		{
			GameMusicPlayer = Core.Instance.AudioManager.MusicPlayer;
			m_TrackList.InitalizeScrollView();
			m_StatusPanel.Initalize(this);
			m_AudioAnalyzer.Initalize(Core.Instance.musicAudioSource);
			DisableAudioAnalyzer();
		}

		public override void OnAppRefresh()
		{
			RefreshList();
		}

		public override void OnAppEnable()
		{
			base.OnAppEnable();
			RefreshList();
			EnableAudioAnalyzer();
			shuffleControlSelected = false;
			SetShuffleControlVisuals(shuffleControlSelected);
			ToggleShuffle(GameMusicPlayer.GetShuffle());
		}

		public override void OnAppDisable()
		{
			base.OnAppDisable();
			DisableAudioAnalyzer();
		}

		public void EnableAudioAnalyzer()
		{
			m_AudioAnalyzer.enabled = true;
		}

		public void DisableAudioAnalyzer()
		{
			m_AudioAnalyzer.enabled = false;
		}

		public override void OnAppUpdate()
		{
			m_StatusPanel.UpdatePanel();
			SetShuffleIconImage();
		}

		public override void OnPressRight()
		{
			if (shuffleControlSelected)
			{
				(selectedShuffleButton.transform as RectTransform).DOAnchorPosX(0f, 0.1f).SetEase(Ease.Linear);
				(unselectedShuffleButton.transform as RectTransform).DOAnchorPosX(0f, 0.1f).SetEase(Ease.Linear);
			}
			else
			{
				m_TrackList.HoldAnimationSelectedButton();
			}
		}

		public override void OnPressUp()
		{
			if (m_TrackList.GetContentIndex() == 0)
			{
				shuffleControlSelected = true;
				SetShuffleControlVisuals(shuffleControlSelected);
				for (int i = 0; i < m_TrackList.GetButtons().Length; i++)
				{
					m_TrackList.GetButtons()[i].IsSelected = false;
				}
			}
			else
			{
				m_TrackList.Move(PhoneScroll.ScrollDirection.Previous, m_AudioManager);
				continuousScrollTimer -= 0.4f;
			}
		}

		public override void OnPressDown()
		{
			if (shuffleControlSelected)
			{
				shuffleControlSelected = false;
				SetShuffleControlVisuals(shuffleControlSelected);
				m_TrackList.SelectButton(0);
			}
			else
			{
				m_TrackList.Move(PhoneScroll.ScrollDirection.Next, m_AudioManager);
				continuousScrollTimer -= 0.4f;
			}
		}

		public override void OnHoldUp()
		{
			if (!shuffleControlSelected)
			{
				continuousScrollTimer += Core.dt;
				if (continuousScrollTimer >= 0.1f)
				{
					continuousScrollTimer = 0f;
					m_TrackList.Move(PhoneScroll.ScrollDirection.Previous, m_AudioManager);
				}
			}
		}

		public override void OnHoldDown()
		{
			if (shuffleControlSelected)
			{
				shuffleControlSelected = false;
				SetShuffleControlVisuals(shuffleControlSelected);
				m_TrackList.SelectButton(0);
				return;
			}
			continuousScrollTimer += Core.dt;
			if (continuousScrollTimer >= 0.1f)
			{
				continuousScrollTimer = 0f;
				m_TrackList.Move(PhoneScroll.ScrollDirection.Next, m_AudioManager);
			}
		}

		public override void OnReleaseUp()
		{
			continuousScrollTimer = 0f;
		}

		public override void OnReleaseDown()
		{
			continuousScrollTimer = 0f;
		}

		public override void OnReleaseRight()
		{
			if (shuffleControlSelected)
			{
				ToggleShuffle(!GameMusicPlayer.GetShuffle());
				(selectedShuffleButton.transform as RectTransform).DOAnchorPosX(-30f, 0.1f).SetEase(Ease.Linear);
				(unselectedShuffleButton.transform as RectTransform).DOAnchorPosX(-30f, 0.1f).SetEase(Ease.Linear);
				return;
			}
			if (m_TrackList.GetContentIndex() == GameMusicPlayer.CurrentTrackIndex)
			{
				TogglePause();
			}
			else
			{
				PlaySong(m_TrackList.GetContentIndex());
				(m_TrackList.SelectedButtton as PhoneScrollUnlockableButton).SetUnread(isUnread: false);
			}
			m_TrackList.ActivateAnimationSelectedButton();
		}

		private void SetShuffleControlVisuals(bool value)
		{
			selectedShuffleButton.SetActive(value);
			unselectedShuffleButton.SetActive(!value);
			(selectedShuffleButton.transform as RectTransform).DOAnchorPosX(value ? (-30) : 0, 0.1f).SetEase(Ease.Linear);
			(unselectedShuffleButton.transform as RectTransform).DOAnchorPosX(value ? (-30) : 0, 0.1f).SetEase(Ease.Linear);
		}

		private void ToggleShuffle(bool value)
		{
			GameMusicPlayer.SetShuffle(value);
			SetShuffleIconImage();
		}

		private void SetShuffleIconImage()
		{
			Image image = shuffleIconImageSelected;
			Sprite sprite = (shuffleIconImageUnselected.sprite = (GameMusicPlayer.GetShuffle() ? shuffleIcon : noShuffleIcon));
			image.sprite = sprite;
		}

		private void RefreshList()
		{
			m_TrackList.musicAlfabeticList = new List<int>();
			for (int i = 0; i < GameMusicPlayer.AmountOfTracks; i++)
			{
				m_TrackList.musicAlfabeticList.Add(i);
			}
			if (GameMusicPlayer.AmountOfTracks <= 1)
			{
				m_TrackList.SetListContent(GameMusicPlayer.AmountOfTracks);
			}
			else
			{
				m_TrackList.UpdateListContent(GameMusicPlayer.AmountOfTracks);
			}
		}

		public void PlaySong(int index)
		{
			GameMusicPlayer.PlayFrom(index);
			if (GameMusicPlayer.GetShuffle())
			{
				GameMusicPlayer.SetShuffle(value: true);
			}
			MusicTrack musicTrack = GetMusicTrack(index);
			Core.Instance.Platform.User.GetUnlockableSaveDataFor(musicTrack).HasSeen = true;
		}

		public void TogglePause()
		{
			if (GameMusicPlayer.IsPlaying)
			{
				GameMusicPlayer.Pause();
			}
			else
			{
				GameMusicPlayer.Play();
			}
		}

		public MusicTrack GetMusicTrack(int index)
		{
			return GameMusicPlayer.GetMusicTrack(index);
		}

		public MusicTrack GetCurrentMusicTrack()
		{
			return GameMusicPlayer.GetMusicTrack(GameMusicPlayer.CurrentTrackIndex);
		}

		public override void OpenContent(AUnlockable unlockable, bool appAlreadyOpen)
		{
			if (!(unlockable == null))
			{
				if (appAlreadyOpen)
				{
					RefreshList();
				}
				m_TrackList.FindAndMoveTo(unlockable, m_AudioManager);
			}
		}
	}
}
