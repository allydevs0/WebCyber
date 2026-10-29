using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "MusicLibrary", menuName = "Reptile/Audio/Music/MusicLibrary")]
	public class MusicLibrary : ScriptableObject
	{
		[SerializeField]
		private PlayListDictionary playlistDictionary = new PlayListDictionary();

		private Dictionary<PlaylistID, Playlist> playlistIDDictionary;

		[SerializeField]
		private MusicTrackDictionary musicTrackDictionary = new MusicTrackDictionary();

		private Dictionary<MusicTrackID, MusicTrack> musicTrackIDDictionary;

		public void Init()
		{
			GenerateEnumDictionaries();
		}

		private void GenerateEnumDictionaries()
		{
			playlistIDDictionary = SerializedLibraryUtility.GenerateCollection(default(PlaylistIDComparer), playlistDictionary);
			musicTrackIDDictionary = SerializedLibraryUtility.GenerateCollection(default(MusicTrackIDComparer), musicTrackDictionary);
		}

		public Playlist GetPlaylist(PlaylistID playlistID)
		{
			Playlist value = null;
			if (!playlistIDDictionary.TryGetValue(playlistID, out value))
			{
				HandlePlaylistNotFound(playlistID);
			}
			return value;
		}

		private void HandlePlaylistNotFound(PlaylistID playlistID)
		{
		}

		public MusicTrack GetMusicTrackFromPlayList(MusicTrackID musicTrackID, PlaylistID playlistID)
		{
			Playlist playlist = GetPlaylist(playlistID);
			if (playlist == null)
			{
				return null;
			}
			return playlist.GetMusicTrack(musicTrackID);
		}

		public MusicTrack GetMusicTrackByID(MusicTrackID musicTrackID)
		{
			MusicTrack musicTrack = GetMusicTrackFromDictionary(musicTrackID);
			if (musicTrack == null)
			{
				musicTrack = GetMusicTrackFromAllPlaylists(musicTrackID);
			}
			if (musicTrack == null)
			{
				HandleNoMusicTrackFound(musicTrackID);
			}
			return musicTrack;
		}

		private MusicTrack GetMusicTrackFromDictionary(MusicTrackID musicTrackID)
		{
			MusicTrack value = null;
			musicTrackIDDictionary.TryGetValue(musicTrackID, out value);
			return value;
		}

		private MusicTrack GetMusicTrackFromAllPlaylists(MusicTrackID musicTrackID)
		{
			foreach (Playlist value in playlistDictionary.Values)
			{
				MusicTrack musicTrack = value.GetMusicTrack(musicTrackID);
				if (musicTrack != null)
				{
					return musicTrack;
				}
			}
			return null;
		}

		private void HandleNoMusicTrackFound(MusicTrackID musicTrackID)
		{
		}
	}
}
