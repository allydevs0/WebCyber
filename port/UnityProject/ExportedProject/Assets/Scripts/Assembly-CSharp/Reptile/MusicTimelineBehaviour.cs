using UnityEngine.Playables;

namespace Reptile
{
	public class MusicTimelineBehaviour : PlayableBehaviour
	{
		internal MusicTimelineClip clip;

		internal float playbackSeconds;

		internal bool isPlaying;
	}
}
