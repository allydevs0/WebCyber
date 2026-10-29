using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "Playlist", menuName = "Reptile/Audio/Music/Playlist")]
	public class Playlist : ScriptableObject, ICollectionGenerator
	{
		[SerializeField]
		private MusicTrackDictionary musicTrackDictionary = new MusicTrackDictionary();

		private Dictionary<MusicTrackID, MusicTrack> musicTrackIDDictionary;

		public void GenerateEnumDictionary()
		{
			musicTrackIDDictionary = SerializedLibraryUtility.GenerateCollection(default(MusicTrackIDComparer), musicTrackDictionary);
		}

		public MusicTrack[] GetAllMusicTracks()
		{
			return musicTrackIDDictionary.Values.ToArray();
		}

		public MusicTrack GetMusicTrack(MusicTrackID musicTrackID)
		{
			MusicTrack value = null;
			if (!musicTrackIDDictionary.TryGetValue(musicTrackID, out value))
			{
				HandleNoMusicTrackFound(musicTrackID);
			}
			return value;
		}

		public bool IsPartOfPlaylist(MusicTrack musicTrack)
		{
			return musicTrackIDDictionary.ContainsValue(musicTrack);
		}

		private void HandleNoMusicTrackFound(MusicTrackID musicTrackID)
		{
		}
	}
}
