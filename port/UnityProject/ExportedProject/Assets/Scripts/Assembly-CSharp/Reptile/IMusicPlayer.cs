using System;

namespace Reptile
{
	public interface IMusicPlayer : IDisposable
	{
		int CurrentTrackIndex { get; }

		float CurrentTrackTime { get; }

		int CurrentTrackSamples { get; }

		float CurrentTrackMaxTime { get; }

		int AmountOfTracks { get; }

		bool IsPlaying { get; }

		void Play();

		void PlayFrom(int index, int playbackSamples = 0);

		void Pause();

		void PlayNext();

		void PlayPrevious();

		void PlayPlaylist(Playlist playlist);

		MusicTrack GetMusicTrack(int index);

		void AddMusicTrack(MusicTrack musicTrack);

		void AddAndBufferAsCurrentTrack(MusicTrack musicTrack);

		bool IsCurrentPlayingTrack(MusicTrack track);

		void AttemptStartCurrentTrack();

		void ClearExceptCurrentTrack();

		void Clear();

		void ForcePaused();

		bool GetShuffle();

		void SetShuffle(bool value);

		void RefreshMusicQueueForStage(ChapterMusic chapterMusic, MusicTrack trackToPlay, Stage stage);
	}
}
