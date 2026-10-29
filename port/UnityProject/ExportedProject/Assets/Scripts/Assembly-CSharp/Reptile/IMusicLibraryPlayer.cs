using System;

namespace Reptile
{
	public interface IMusicLibraryPlayer : IDisposable
	{
		void AddTrack(MusicTrack musicTrack);

		void AddTrackByID(MusicTrackID musicTrackID);

		void AddTrackFromPlaylist(MusicTrackID musicTrackID, PlaylistID playlistID);

		void AddPlaylist(Playlist playlist);

		void AddPlaylistByID(PlaylistID playlistID);

		void StartTrack(MusicTrack musicTrack, int playbackSamples = 0);

		void StartTrackByID(MusicTrackID musicTrackID, int playbackSamples = 0);

		void StartTrackFromPlaylist(MusicTrackID musicTrackID, PlaylistID playlistID);

		void StartPlaylist(Playlist playlist);

		void StartPlaylistByID(PlaylistID playlistID);

		MusicTrack GetMusicTrackByID(MusicTrackID musicTrackID);
	}
}
