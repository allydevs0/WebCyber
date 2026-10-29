using System;

namespace Reptile
{
	public class MusicLibraryPlayer : IMusicLibraryPlayer, IDisposable
	{
		private MusicLibrary musicLibrary;

		private IMusicPlayer musicPlayer;

		public MusicLibraryPlayer(MusicLibrary musicLibrary, IMusicPlayer musicPlayer)
		{
			this.musicLibrary = musicLibrary;
			this.musicPlayer = musicPlayer;
		}

		public void Dispose()
		{
			musicPlayer = null;
			musicLibrary = null;
		}

		public void AddTrack(MusicTrack musicTrack)
		{
			musicPlayer.AddMusicTrack(musicTrack);
		}

		public void AddTrackByID(MusicTrackID musicTrackID)
		{
			MusicTrack musicTrackByID = musicLibrary.GetMusicTrackByID(musicTrackID);
			AddTrack(musicTrackByID);
		}

		public void AddTrackFromPlaylist(MusicTrackID musicTrackID, PlaylistID playlistID)
		{
			MusicTrack musicTrackFromPlayList = musicLibrary.GetMusicTrackFromPlayList(musicTrackID, playlistID);
			AddTrack(musicTrackFromPlayList);
		}

		public void StartTrack(MusicTrack musicTrack, int playbackSamples = 0)
		{
			int num = FindMusicTrackIndex(musicTrack);
			if (num >= 0)
			{
				musicPlayer.PlayFrom(num, playbackSamples);
			}
		}

		private int FindMusicTrackIndex(MusicTrack musicTrack)
		{
			for (int i = 0; i < musicPlayer.AmountOfTracks; i++)
			{
				if (musicPlayer.GetMusicTrack(i).Equals(musicTrack))
				{
					return i;
				}
			}
			return -1;
		}

		public MusicTrack GetMusicTrackByID(MusicTrackID musicTrackID)
		{
			return musicLibrary.GetMusicTrackByID(musicTrackID);
		}

		public void StartTrackByID(MusicTrackID musicTrackID, int playbackSamples = 0)
		{
			MusicTrack musicTrackByID = musicLibrary.GetMusicTrackByID(musicTrackID);
			if (musicTrackByID != null)
			{
				StartTrack(musicTrackByID);
			}
		}

		public void StartPlaylist(Playlist playlist)
		{
			musicPlayer.PlayPlaylist(playlist);
		}

		private void HandleNoMusicTracksInPlaylist(Playlist playlist)
		{
		}

		private void AddTracks(MusicTrack[] musicTracks)
		{
			for (int i = 0; i < musicTracks.Length; i++)
			{
				AddTrack(musicTracks[i]);
			}
		}

		public void StartPlaylistByID(PlaylistID playlistID)
		{
			Playlist playlist = musicLibrary.GetPlaylist(playlistID);
			StartPlaylist(playlist);
		}

		public void StartTrackFromPlaylist(MusicTrackID musicTrackID, PlaylistID playlistID)
		{
			MusicTrack musicTrackFromPlayList = musicLibrary.GetMusicTrackFromPlayList(musicTrackID, playlistID);
			StartTrack(musicTrackFromPlayList);
		}

		public void AddPlaylist(Playlist playlist)
		{
			MusicTrack[] allMusicTracks = playlist.GetAllMusicTracks();
			if (allMusicTracks.Length == 0)
			{
				HandleNoMusicTracksInPlaylist(playlist);
			}
			else
			{
				AddTracks(allMusicTracks);
			}
		}

		public void AddPlaylistByID(PlaylistID playlistID)
		{
			Playlist playlist = musicLibrary.GetPlaylist(playlistID);
			AddPlaylist(playlist);
		}
	}
}
