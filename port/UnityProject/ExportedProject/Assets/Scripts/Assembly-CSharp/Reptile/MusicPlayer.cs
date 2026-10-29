using System;
using Reptile.Phone;
using UnityEngine;

namespace Reptile
{
	public class MusicPlayer : IMusicPlayer, IDisposable
	{
		public const int NO_TRACK = -1;

		private MusicTrackQueue musicTrackQueue;

		private AudioSource audioSource;

		private bool isPlaying;

		private bool wasPlayingLastFrame;

		private bool isPlayingFromPlaylist;

		private bool isDisposed;

		private bool shuffle;

		internal int playbackSamples;

		public int CurrentTrackIndex => musicTrackQueue.CurrentTrackIndex;

		public float CurrentTrackTime
		{
			get
			{
				if (audioSource.clip == null || !isPlaying)
				{
					return 0f;
				}
				return audioSource.time;
			}
		}

		public int CurrentTrackSamples
		{
			get
			{
				if (audioSource.clip == null || !isPlaying)
				{
					return 0;
				}
				return audioSource.timeSamples;
			}
		}

		public float CurrentTrackMaxTime
		{
			get
			{
				if (audioSource.clip == null || !isPlaying)
				{
					return 0f;
				}
				return audioSource.clip.length;
			}
		}

		public int AmountOfTracks => musicTrackQueue.AmountOfTracks;

		public bool IsPlaying => isPlaying;

		public MusicPlayer(AudioSource audioSource, MusicTrackQueue musicTrackQueue)
		{
			this.audioSource = audioSource;
			this.musicTrackQueue = musicTrackQueue;
			audioSource.loop = false;
			Core.OnAlwaysUpdate += Update;
		}

		public void Dispose()
		{
			Clear();
			musicTrackQueue.Dispose();
			musicTrackQueue = null;
			audioSource = null;
			Core.OnAlwaysUpdate -= Update;
			isDisposed = true;
		}

		public void Clear()
		{
			if (!isDisposed)
			{
				musicTrackQueue.Clear();
			}
		}

		internal void SetAudioSource(AudioSource audioSource)
		{
			this.audioSource = audioSource;
		}

		public void Play()
		{
			if (musicTrackQueue.HasCurrentActiveTrack)
			{
				StartMusicPlayer();
			}
			else
			{
				PlayFromStart();
			}
		}

		public void Pause()
		{
			isPlaying = false;
		}

		public void PlayFrom(int index, int playbackSamples = 0)
		{
			if (musicTrackQueue.AmountOfTracks > 0)
			{
				isPlayingFromPlaylist = false;
				wasPlayingLastFrame = false;
				this.playbackSamples = playbackSamples;
				musicTrackQueue.SetCurrentTrack(index);
				musicTrackQueue.ClearBuffer();
				musicTrackQueue.ClearMusicQueue();
				musicTrackQueue.AddAllCurrentTracksToQueue();
				musicTrackQueue.BufferTracksInQueue();
				StartMusicPlayer();
			}
		}

		public void PlayNext()
		{
			if (isPlaying || musicTrackQueue.HasCurrentActiveTrack)
			{
				AttemptSelectNextTrack();
			}
			else
			{
				PlayFromStart();
			}
		}

		public void PlayPrevious()
		{
			if (isPlaying || musicTrackQueue.HasCurrentActiveTrack)
			{
				AttemptSelectPreviousTrack();
			}
			else
			{
				PlayFromStart();
			}
		}

		private void Update()
		{
			if (Application.isFocused)
			{
				UpdateMusicPlayer();
			}
		}

		private void UpdateMusicPlayer()
		{
			if (isPlaying)
			{
				UpdateIsPlayingMusic();
			}
			else
			{
				UpdateHasStoppedPlayingMusic();
			}
		}

		private void UpdateIsPlayingMusic()
		{
			if (!audioSource.isPlaying)
			{
				EvaluateAudioSourceStoppedPlaying();
			}
			wasPlayingLastFrame = audioSource.isPlaying;
		}

		private void EvaluateAudioSourceStoppedPlaying()
		{
			if (wasPlayingLastFrame)
			{
				PlayNext();
			}
			UpdateMusicTrackQueueLoading();
		}

		internal void UpdateMusicTrackQueueLoading()
		{
			musicTrackQueue.UpdateLoadingCurrentTrack();
			AttemptStartCurrentTrack();
		}

		public void AttemptStartCurrentTrack()
		{
			MusicPlayerData currentMusicPlayerData = musicTrackQueue.GetCurrentMusicPlayerData();
			if (currentMusicPlayerData == null)
			{
				HandleNoMusicPlayerDataFound();
				return;
			}
			PlayFromAudioSource(currentMusicPlayerData, playbackSamples);
			playbackSamples = 0;
		}

		private void HandleNoMusicPlayerDataFound()
		{
			Pause();
			if (!musicTrackQueue.HasCurrentActiveTrack)
			{
				AttemptSelectNextTrack();
			}
		}

		private void PlayFromAudioSource(MusicPlayerData musicPlayerData, int playbackSamples)
		{
			audioSource.clip = musicPlayerData.AudioClip;
			playbackSamples = ((playbackSamples < musicPlayerData.AudioClip.samples) ? playbackSamples : 0);
			audioSource.timeSamples = playbackSamples;
			audioSource.Play();
		}

		private void UpdateHasStoppedPlayingMusic()
		{
			if (wasPlayingLastFrame)
			{
				audioSource.Pause();
				wasPlayingLastFrame = false;
			}
		}

		public void ForcePaused()
		{
			isPlaying = false;
			wasPlayingLastFrame = false;
			if ((bool)audioSource)
			{
				audioSource.Pause();
			}
		}

		private void StartMusicPlayer()
		{
			isPlaying = true;
		}

		public MusicTrack GetMusicTrack(int index)
		{
			return musicTrackQueue.GetMusicTrack(index);
		}

		public bool IsCurrentPlayingTrack(MusicTrack track)
		{
			if (track.AudioClip == audioSource.clip)
			{
				return audioSource.isPlaying;
			}
			return false;
		}

		public void PlayPlaylist(Playlist playlist)
		{
			musicTrackQueue.ClearBuffer();
			musicTrackQueue.ClearMusicQueue();
			PlayFromPlaylist(playlist);
			isPlayingFromPlaylist = true;
		}

		public void AddMusicTrack(MusicTrack musicTrack)
		{
			musicTrackQueue.AddMusicTrack(musicTrack);
		}

		public void AddAndBufferAsCurrentTrack(MusicTrack musicTrack)
		{
			musicTrackQueue.AddMusicTrack(musicTrack);
			musicTrackQueue.SetCurrentTrack(musicTrack);
			musicTrackQueue.BufferTracksInQueue();
		}

		private void PlayFromPlaylist(Playlist playlist)
		{
			MusicTrack[] allMusicTracks = playlist.GetAllMusicTracks();
			musicTrackQueue.AddMusicTracksToQueue(allMusicTracks);
			musicTrackQueue.SetCurrentTrack(allMusicTracks[0]);
			musicTrackQueue.BufferTracksInQueue();
			Play();
		}

		private void PlayFromStart()
		{
			PlayFrom(0);
		}

		private void AttemptSelectNextTrack()
		{
			bool flag = false;
			if (musicTrackQueue.CurrentMusicTrack != null)
			{
				flag = EvaluateRepeatingMusicTrack();
			}
			if (!flag)
			{
				musicTrackQueue.SelectNextTrack();
			}
			HandleSelectingTrack(flag);
		}

		private void HandleSelectingTrack(bool repeating = false)
		{
			wasPlayingLastFrame = repeating;
			if (!repeating && !musicTrackQueue.HasCurrentActiveTrack)
			{
				Pause();
			}
			else
			{
				StartMusicPlayer();
			}
		}

		private bool EvaluateRepeatingMusicTrack()
		{
			if (isPlayingFromPlaylist || !musicTrackQueue.CurrentMusicTrack.isRepeatable)
			{
				return false;
			}
			return true;
		}

		private void AttemptSelectPreviousTrack()
		{
			musicTrackQueue.SelectPreviousTrack();
			HandleSelectingTrack();
		}

		public void ClearExceptCurrentTrack()
		{
			isPlayingFromPlaylist = false;
			wasPlayingLastFrame = false;
			musicTrackQueue.ClearExceptCurrent();
		}

		public void RefreshMusicQueueForStage(ChapterMusic chapterMusic, MusicTrack trackToPlay, Stage stage)
		{
			musicTrackQueue.ClearTracks();
			Story.ObjectiveInfo currentObjectiveInfo = Story.GetCurrentObjectiveInfo();
			if (stage == Stage.hideout)
			{
				MusicTrack musicTrackByID = Core.Instance.AudioManager.MusicLibraryPlayer.GetMusicTrackByID(MusicTrackID.Hideout_Mixtape);
				AddMusicTrack(musicTrackByID);
			}
			else
			{
				MusicTrack chapterMusic2 = chapterMusic.GetChapterMusic(currentObjectiveInfo.chapter);
				AddMusicTrack(chapterMusic2);
			}
			AUnlockable[] unlockables = WorldHandler.instance.GetCurrentPlayer().phone.GetAppInstance<AppMusicPlayer>().Unlockables;
			for (int i = 0; i < unlockables.Length; i++)
			{
				MusicTrack musicTrack = unlockables[i] as MusicTrack;
				if (Core.Instance.Platform.User.GetUnlockableSaveDataFor(musicTrack).IsUnlocked)
				{
					musicTrack.isRepeatable = false;
					AddMusicTrack(musicTrack);
				}
			}
			musicTrackQueue.UpdateMusicQueueForStage(trackToPlay);
		}

		public void SetShuffle(bool value)
		{
			if (shuffle != value)
			{
				if (value)
				{
					musicTrackQueue.ReplaceIndexQueue(new ShuffleIndexQueue(-1));
				}
				else
				{
					musicTrackQueue.ReplaceIndexQueue(new DefaultIndexQueue(-1));
				}
				musicTrackQueue.AddAllCurrentTracksToQueue();
			}
			musicTrackQueue.RefreshIndexQueue();
			shuffle = value;
		}

		public bool GetShuffle()
		{
			return shuffle;
		}
	}
}
