using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "ChapterMusic", menuName = "Reptile/Audio/Chapter Music")]
	public class ChapterMusic : ScriptableObject
	{
		[SerializeField]
		private SerializableDictionary<Story.Chapter, MusicTrack> chapterMusicDictionary;

		public MusicTrack GetChapterMusic(Story.Chapter chapter)
		{
			MusicTrack value = null;
			chapterMusicDictionary.TryGetValue(chapter, out value);
			return value;
		}

		public bool IsChapterMusic(MusicTrack musicTrack)
		{
			return chapterMusicDictionary.ContainsValue(musicTrack);
		}
	}
}
