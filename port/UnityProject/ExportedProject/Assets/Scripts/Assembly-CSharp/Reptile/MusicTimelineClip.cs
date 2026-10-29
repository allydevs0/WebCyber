using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	[Serializable]
	public class MusicTimelineClip : PlayableAsset
	{
		internal MusicTimelineBehaviour musicBehaviour = new MusicTimelineBehaviour();

		[SerializeField]
		public MusicTrack musicTrack;

		public int playbackSamples;

		public bool playAsStageMusic;

		public bool refreshMusicQueue = true;

		public override double duration
		{
			get
			{
				if (musicTrack == null)
				{
					return base.duration;
				}
				return (double)musicTrack.AudioClip.samples / (double)musicTrack.AudioClip.frequency;
			}
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			musicBehaviour.clip = this;
			return ScriptPlayable<MusicTimelineBehaviour>.Create(graph, musicBehaviour);
		}
	}
}
