using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Reptile
{
	[TrackColor(0.855f, 0.903f, 0.87f)]
	[TrackClipType(typeof(LookAtIKClip))]
	[TrackBindingType(typeof(LookAtIKComponent))]
	public class LookAtIKTrack : TrackAsset
	{
		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			return ScriptPlayable<LookAtIKMixerBehaviour>.Create(graph, inputCount);
		}
	}
}
